# ASP.NET Core Minimal API – Training Example

A complete Minimal API built around two topics:

* **FluentValidation** – validation rules as a readable chain, executed in an endpoint filter
  before a handler even starts.
* **Dependency injection with MEDI** (`Microsoft.Extensions.DependencyInjection`) –
  singleton, scoped and transient, keyed services, the options pattern, factory
  registration, decorators and manually created scopes.

Two rules run through the whole project:

1. **No lambdas in routing and configuration.** Every endpoint, every factory and every rule
   condition is a named method.
2. **No comments anywhere** – names, types and structure have to carry the meaning on their
   own. The code is also fully synchronous.

**No database required**: the data lives in memory and is back to its initial state after
every restart.

## Running it

```bash
dotnet run --project asp-net-rest
```

The API then listens on <http://localhost:5080>. In Rider: start the `http` run
configuration, or open `asp-net-rest/asp-net-rest.http` and fire the requests one by one.

## Layout

| Folder / file | Contents |
|---|---|
| `Program.cs` | The entire composition as a single chain – nothing else |
| `Composition/ServiceCollectionExtensions.cs` | **The composition root**: every registration, grouped by lifetime |
| `Composition/WebApplicationExtensions.cs` | Middleware pipeline and endpoint mapping |
| `Endpoints/BookEndpoints.cs` | The `/api/books` resource – every HTTP method once |
| `Endpoints/DiagnosticsEndpoints.cs` | Makes lifetimes, request scope and error handling visible |
| `Endpoints/EndpointResults.cs` | Shared result helpers (`OkOrNotFound`, `NoContentOrNotFound`) |
| `Endpoints/HomeEndpoints.cs` | Signpost on `/` |
| `Validation/` | Validators, custom fluent rules, endpoint filter, options validation |
| `Services/` | Application services – one per lifetime |
| `Services/Diagnostics/` | The three probes and the `LifetimeInspector` |
| `Persistence/` | Repository interface, in-memory store, logging decorator |
| `Contracts/` | The JSON contracts (requests, responses, query parameters) |
| `Domain/Book.cs` | The domain model |
| `Options/BookLibraryOptions.cs` | Configurable limits from `appsettings.json` |
| `asp-net-rest.http` | Ready-made requests to try out |

## Part 1 – The endpoints

| Method | Path | Meaning | Status |
|---|---|---|---|
| `GET` | `/api/books` | read the list (sorted, paged) | 200, 400 |
| `GET` | `/api/books/{id}` | read one book | 200, 404 |
| `HEAD` | `/api/books/{id}` | only check whether the book exists | 200, 404 |
| `POST` | `/api/books` | create a new book | 201 + `Location`, 400 |
| `PUT` | `/api/books/{id}` | replace a book completely | 200, 400, 404 |
| `PATCH` | `/api/books/{id}` | change individual fields | 200, 400, 404 |
| `DELETE` | `/api/books/{id}` | delete a book | 204, 404 |
| `OPTIONS` | `/api/books` | ask for the allowed methods | 204 + `Allow` |

Mapping happens exclusively through method groups:

```csharp
books.MapPost("", Create)
     .WithName(nameof(Create))
     .WithSummary("Create a new book")
     .AddEndpointFilter<ValidationEndpointFilter<CreateBookRequest>>();
```

The handler behind it is an ordinary method with a typed result – the compiler only allows
the documented responses:

```csharp
private static Results<Ok<BookResponse>, NotFound> GetById(int id, IBookService books) =>
    OkOrNotFound(books.Get(id));
```

## Part 2 – FluentValidation

The rules live in the validator's constructor; every condition is a named method:

```csharp
RuleFor(request => request.Title).BookTitle(limits.MaxTitleLength);
RuleFor(request => request.Year).PublicationYear(limits.EarliestPublicationYear, clock.UtcNow.Year + 1);

RuleFor(request => request)
    .Must(BeUniqueEdition)
    .WithMessage(ValidationMessages.DuplicateEdition)
    .OverridePropertyName(ValidationMessages.TitleField)
    .When(HasTitleAndAuthor);
```

What that shows:

* `BookTitle(...)` and `PublicationYear(...)` are **custom fluent rules**
  (`Validation/BookRuleExtensions.cs`) – every message text exists exactly once in the project.
* `Must(BeUniqueEdition)` is a **domain** rule with data access: the same edition may only be
  recorded once. That works because validators are registered *scoped*.
* Everything runs inside `ValidationEndpointFilter<TRequest>`, an endpoint filter. No handler
  contains a single `if (!valid)` line.
* Not only the body is validated: `GET /api/books` bundles its query parameters into
  `BookPageQuery` with `[AsParameters]` and sends them through the same filter.
* Even the **configuration** is checked with FluentValidation – at startup, not on the first
  request (`FluentValidationOptions<TOptions>` + `ValidateOnStart()`).

Errors always come back as problem details per RFC 9457:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "title": ["The title must not be empty."],
    "year": ["The publication year must be between 1450 and 2027."]
  }
}
```

## Part 3 – Dependency injection (MEDI)

### The three lifetimes

| Lifetime | Meaning | In this project |
|---|---|---|
| **Singleton** | one instance for the whole application lifetime | `InMemoryBookRepository`, `IClock`, the sort strategies |
| **Scoped** | one instance per scope, in ASP.NET Core per request | `IBookService`, `IAuditTrail`, all validators |
| **Transient** | a new instance on every resolution | `IBookMapper`, `BookSorterSelector` |

**The golden rule**: a service may only hold services with an *equal or longer* lifetime in
its constructor. A singleton with a scoped field is a *captive dependency* – the container
rejects it at startup.

The project shows three places where that would happen, each with its solution:

* `AuditTrailMiddleware` – middleware lives like a singleton, so the scoped service arrives as
  a parameter of `InvokeAsync` instead of in the constructor.
* `ValidationEndpointFilter<T>` – created once at startup; the validator is resolved per
  request from `HttpContext.RequestServices`.
* `StartupWarmupService` and `FluentValidationOptions<T>` – both singletons; they open their
  own scope with `IServiceScopeFactory` for the duration of the work.

### See it for yourself

```bash
curl http://localhost:5080/api/diagnostics/lifetimes
```

Call it twice and compare the ids:

* the **singleton** id stays the same across all requests,
* the **scoped** id is stable within one request and changes with every new request – and in
  the manually created child scope as well,
* the two **transient** ids already differ within the same scope.

```bash
curl http://localhost:5080/api/diagnostics/audit
```

The first audit entry comes from the middleware and is read in the endpoint – proof that both
use the same scoped instance.

### More registration styles

* **Keyed services** – three sort strategies share `IBookSorter` and are told apart by the
  keys `id`, `title`, `year` (`?sortBy=title`).
* **Factory registration + decorator** – `IBookRepository` is resolved through a method that
  wraps the real store in `LoggingBookRepository`. Callers never notice.
* **Options pattern** – `BookLibraryOptions` comes from `appsettings.json`, is validated at
  startup and injected via `IOptions<T>`.
* **Hosted service** – `StartupWarmupService` counts the books at startup and shows how a
  singleton reaches a scoped service.

## Error handling

`GlobalExceptionHandler` (an `IExceptionHandler`) turns every unhandled exception into a 500
response in problem details format; the technical message stays in the log. Try it:

```bash
curl -i http://localhost:5080/api/diagnostics/boom
```

## Fluent wherever possible

The composition of the application is a single chain:

```csharp
WebApplication.CreateBuilder(args)
    .AddApplicationServices()
    .Build()
    .UseApplicationPipeline()
    .MapApplicationEndpoints()
    .Run();
```

The same principle runs through the rest: registrations (`services.AddConfiguredOptions(configuration)
.AddPersistence().AddDomainServices()…`, each group chained internally as well), the pipeline
(`app.UseExceptionHandler().UseStatusCodePages().UseMiddleware<AuditTrailMiddleware>()`), the
endpoint groups (`MapHomeEndpoints().MapBookEndpoints().MapDiagnosticsEndpoints()`), the store's
seed (`Seed(…).Seed(…).Seed(…)`) and `IAuditTrail.Record`, which returns itself.

The tests follow suit – test data comes from immutable builders, assertions use
[AwesomeAssertions](https://github.com/AwesomeAssertions/AwesomeAssertions) (MIT fork of
FluentAssertions 7):

```csharp
var request = ABook().WithTitle(string.Empty).AsCreateRequest();

var result = validator.Validate(request);

result.Errors.Should()
    .ContainSingle()
    .Which.ErrorMessage.Should().Be(ValidationMessages.TitleRequired);
```

## Synchronous and comment-free

The code is synchronous throughout: no `async`, no `await`, no `Task` return values in our own
signatures. Handlers, services and repository return their values directly:

```csharp
private static Results<Ok, NotFound> HeadById(int id, IBookService books) =>
    OkOrNotFound(books.Exists(id));
```

Four places still return `Task` or `ValueTask` because the ASP.NET Core interfaces demand it –
but none of them uses `async`/`await`: `IExceptionHandler.TryHandleAsync`,
`IEndpointFilter.InvokeAsync`, `IHostedService` and the middleware. That is why
`AuditTrailMiddleware` logs through `Response.OnCompleted(...)` instead of
`await next(context)`.

The integration tests are the exception: the in-memory test server of ASP.NET Core explicitly
rejects the synchronous call (`NotSupportedException`), so the `HttpClient` stays asynchronous
there.

The code contains no comments – the tests included. Given-When-Then shows up in the method
name and in the blocks separated by blank lines.

## Tests

```bash
dotnet test
```

70 tests in `asp-net-rest.Tests`, named `given<Precondition>_when<Action>_then<Outcome>` and
split into three blocks separated by blank lines:

| Folder | What it covers |
|---|---|
| `Validation/` | The rules one by one: required fields, lengths, year bounds, duplicates, PATCH without a change, unknown sort keys |
| `Services/` | Sort strategies, `BookService` (paging, trimming, PATCH semantics) and the registration itself: lifetimes, keyed services, decorator, options |
| `Persistence/` | The store: id assignment, PATCH leaving other fields alone, case-insensitive duplicate detection |
| `Api/` | Integration tests across the whole application – every HTTP method, status codes, `Location` and `Allow` headers, problem details |
| `Fixtures/` | Fluent builders for test data (`ABook()`, `APatch()`), constants (`BookFixtures`), the frozen clock (`FixedClock`) and the container without a web server (`TestHost`) |

Three things are the actual lesson here:

* **Fluent assertions**: `page.Should().BeEquivalentTo(new { TotalCount = 3, SortedBy = "id" })`
  checks several fields in one assertion – and reports which field differs when it fails.
* **Unit tests without a web server**: validators and services are created with a `new` or from
  a container without a host. That only works because the dependencies hang off interfaces –
  `FixedClock` freezes time, otherwise the year rule would only be green until New Year's Eve.
* **Integration tests with `WebApplicationFactory<Program>`**: the same `Program.cs`, started in
  memory, without a port and without networking. Every test gets its own application and
  therefore a fresh store.

For the tests, `Program.cs` carries a final line `public partial class Program;` – with
top-level statements the generated class would otherwise be internal. And
`AddApplicationServices` is split into `AddApplicationCoreServices` (everything that works
without a web server) and `AddWebInfrastructure` (OpenAPI, problem details) so the container
stays buildable in a unit test without a host.

## Next step

To turn the in-memory store into a real database, only `InMemoryBookRepository` is swapped for
a repository backed by EF Core – one line in the composition root. Endpoints, validators and
services stay untouched. That is exactly what the interfaces are for.
