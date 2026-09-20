# ASP.NET Core Minimal API – Schulungsbeispiel

Eine vollständige Minimal API zu zwei Themen:

* **FluentValidation** – Validierungsregeln als lesbare Kette, ausgeführt in einem
  Endpunkt-Filter, bevor ein Handler überhaupt startet.
* **Dependency Injection mit MEDI** (`Microsoft.Extensions.DependencyInjection`) –
  Singleton, Scoped und Transient, keyed services, Options-Pattern, Factory-Registrierung,
  Dekorator und selbst erzeugte Scopes.

Zwei Regeln ziehen sich durch das ganze Projekt:

1. **Keine Lambdas im Routing und in der Konfiguration.** Jeder Endpunkt, jede Factory und
   jede Regelbedingung ist eine benannte, dokumentierte Methode.
2. **Jede Methode und jede Lebensdauer ist kommentiert** – am Typ steht, warum er so und
   nicht anders registriert ist.

**Keine Datenbank nötig**: die Daten liegen im Arbeitsspeicher und sind nach jedem Neustart
wieder im Ausgangszustand.

## Starten

```bash
dotnet run --project asp-net-rest
```

Die API läuft danach auf <http://localhost:5080>. In Rider: Run-Konfiguration `http`
starten, oder `asp-net-rest/asp-net-rest.http` öffnen und die Requests einzeln abschicken.

## Aufbau

| Ordner / Datei | Inhalt |
|---|---|
| `Program.cs` | Die ganze Komposition als eine Kette – mehr nicht |
| `Composition/ServiceCollectionExtensions.cs` | **Der Composition Root**: alle Registrierungen, nach Lebensdauer gegliedert |
| `Composition/WebApplicationExtensions.cs` | Middleware-Pipeline und Zuordnung der Endpunkte |
| `Endpoints/BookEndpoints.cs` | Die Ressource `/api/books` – einmal jede HTTP-Methode |
| `Endpoints/DiagnosticsEndpoints.cs` | Macht Lebensdauern, Request-Scope und Fehlerbehandlung sichtbar |
| `Endpoints/HomeEndpoints.cs` | Wegweiser auf `/` |
| `Validation/` | Validatoren, eigene fluent Regeln, Endpunkt-Filter, Options-Prüfung |
| `Services/` | Fachliche Dienste – je einer pro Lebensdauer |
| `Services/Diagnostics/` | Die drei Sonden und der `LifetimeInspector` |
| `Persistence/` | Repository-Schnittstelle, In-Memory-Speicher, Logging-Dekorator |
| `Contracts/` | Die JSON-Verträge (Requests, Responses, Query-Parameter) |
| `Domain/Book.cs` | Das fachliche Modell |
| `Options/BookLibraryOptions.cs` | Konfigurierbare Grenzwerte aus `appsettings.json` |
| `asp-net-rest.http` | Fertige Requests zum Ausprobieren |

## Teil 1 – Die Endpunkte

| Methode | Pfad | Bedeutung | Status |
|---|---|---|---|
| `GET` | `/api/books` | Liste lesen (sortiert, seitenweise) | 200, 400 |
| `GET` | `/api/books/{id}` | ein Buch lesen | 200, 404 |
| `HEAD` | `/api/books/{id}` | nur prüfen, ob es das Buch gibt | 200, 404 |
| `POST` | `/api/books` | neues Buch anlegen | 201 + `Location`, 400 |
| `PUT` | `/api/books/{id}` | Buch vollständig ersetzen | 200, 400, 404 |
| `PATCH` | `/api/books/{id}` | einzelne Felder ändern | 200, 400, 404 |
| `DELETE` | `/api/books/{id}` | Buch löschen | 204, 404 |
| `OPTIONS` | `/api/books` | erlaubte Methoden erfragen | 204 + `Allow` |

Gemappt wird ausschliesslich über Methodengruppen:

```csharp
books.MapPost("", Create)
     .WithName(nameof(Create))
     .WithSummary("Ein neues Buch anlegen")
     .AddEndpointFilter<ValidationEndpointFilter<CreateBookRequest>>();
```

Der Handler dahinter ist eine gewöhnliche Methode mit typisiertem Ergebnis – der Compiler
lässt nur die dokumentierten Antworten zu:

```csharp
private static Results<Ok<BookResponse>, NotFound> GetById(int id, IBookService books)
```

## Teil 2 – FluentValidation

Die Regeln stehen im Konstruktor des Validators; jede Bedingung ist eine benannte Methode:

```csharp
RuleFor(request => request.Title).BookTitle(limits.MaxTitleLength);
RuleFor(request => request.Year).PublicationYear(limits.EarliestPublicationYear, clock.UtcNow.Year + 1);

RuleFor(request => request)
    .Must(BeUniqueEdition)
    .WithMessage(ValidationMessages.DuplicateEdition)
    .OverridePropertyName(ValidationMessages.TitleField)
    .When(HasTitleAndAuthor);
```

Was dabei zu sehen ist:

* `BookTitle(...)` und `PublicationYear(...)` sind **eigene fluent Regeln**
  (`Validation/BookRuleExtensions.cs`) – so steht jeder Meldungstext nur einmal im Projekt.
* `Must(BeUniqueEdition)` zeigt eine **fachliche** Regel mit Datenzugriff: dieselbe Ausgabe darf es nur
  einmal geben. Möglich ist das, weil Validatoren *scoped* registriert sind.
* Ausgeführt wird alles im `ValidationEndpointFilter<TRequest>`, einem Endpunkt-Filter. Kein
  Handler enthält deshalb eine einzige `if (!valid)`-Zeile.
* Validiert wird nicht nur der Body: `GET /api/books` bündelt seine Query-Parameter mit
  `[AsParameters]` in `BookPageQuery` und schickt sie durch denselben Filter.
* Auch die **Konfiguration** wird mit FluentValidation geprüft – beim Start, nicht beim
  ersten Request (`FluentValidationOptions<TOptions>` + `ValidateOnStart()`).

Fehler kommen einheitlich als Problem Details nach RFC 9457 zurück:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "title": ["Der Titel darf nicht leer sein."],
    "year": ["Das Erscheinungsjahr muss zwischen 1450 und 2027 liegen."]
  }
}
```

## Teil 3 – Dependency Injection (MEDI)

### Die drei Lebensdauern

| Lebensdauer | Bedeutung | In diesem Projekt |
|---|---|---|
| **Singleton** | eine Instanz für die gesamte Laufzeit | `InMemoryBookRepository`, `IClock`, die Sortierstrategien |
| **Scoped** | eine Instanz pro Scope, in ASP.NET Core pro Request | `IBookService`, `IAuditTrail`, alle Validatoren |
| **Transient** | bei jeder Anforderung eine neue Instanz | `IBookMapper`, `BookSorterSelector` |

**Die goldene Regel**: Ein Service darf nur Services mit *gleicher oder längerer*
Lebensdauer im Konstruktor halten. Ein Singleton mit einem Scoped-Feld ist eine
*captive dependency* – der Container weist sie beim Start ab.

Wo genau das droht, zeigt das Projekt an drei Stellen samt Lösung:

* `AuditTrailMiddleware` – eine Middleware lebt wie ein Singleton; der Scoped-Service kommt
  deshalb als Parameter von `InvokeAsync`, nicht in den Konstruktor.
* `ValidationEndpointFilter<T>` – wird einmal beim Start erzeugt; der Validator wird pro
  Request aus `HttpContext.RequestServices` aufgelöst.
* `StartupWarmupService` und `FluentValidationOptions<T>` – beide Singletons; sie öffnen
  sich mit der `IServiceScopeFactory` für die Dauer der Arbeit einen eigenen Scope.

### Zum Anfassen

```bash
curl http://localhost:5080/api/diagnostics/lifetimes
```

Zweimal aufrufen und die Ids vergleichen:

* die **Singleton**-Id bleibt über alle Requests dieselbe,
* die **Scoped**-Id ist innerhalb eines Requests stabil und ändert sich mit jedem neuen
  Request – und auch im selbst erzeugten Kind-Scope,
* die beiden **Transient**-Ids unterscheiden sich schon innerhalb desselben Scopes.

```bash
curl http://localhost:5080/api/diagnostics/audit
```

Der erste Protokolleintrag stammt aus der Middleware, gelesen wird er im Endpunkt – der
Beweis, dass beide dieselbe Scoped-Instanz benutzen.

### Weitere Registrierungsarten

* **Keyed services** – drei Sortierstrategien teilen sich `IBookSorter` und werden über die
  Schlüssel `id`, `title`, `year` unterschieden (`?sortBy=title`).
* **Factory-Registrierung + Dekorator** – `IBookRepository` wird über eine Methode
  aufgelöst, die den echten Speicher in `LoggingBookRepository` wickelt. Die Aufrufer
  merken davon nichts.
* **Options-Pattern** – `BookLibraryOptions` kommt aus `appsettings.json`, wird beim Start
  validiert und über `IOptions<T>` injiziert.
* **Hosted Service** – `StartupWarmupService` zählt beim Start den Bestand und zeigt dabei,
  wie ein Singleton an einen Scoped-Service kommt.

## Fehlerbehandlung

`GlobalExceptionHandler` (ein `IExceptionHandler`) macht aus jeder unbehandelten Ausnahme
eine 500er-Antwort im Problem-Details-Format; die technische Meldung bleibt im Log.
Ausprobieren:

```bash
curl -i http://localhost:5080/api/diagnostics/boom
```

## Fluent, wo es geht

Die Komposition der Anwendung ist eine einzige Kette:

```csharp
WebApplication.CreateBuilder(args)
    .AddApplicationServices()
    .Build()
    .UseApplicationPipeline()
    .MapApplicationEndpoints()
    .Run();
```

Dasselbe Prinzip zieht sich durch: Registrierungen (`services.AddConfiguredOptions(configuration)
.AddPersistence().AddDomainServices()…`, jede Gruppe intern ebenfalls verkettet), die Pipeline
(`app.UseExceptionHandler().UseStatusCodePages().UseMiddleware<AuditTrailMiddleware>()`), die
Endpunkt-Gruppen (`MapHomeEndpoints().MapBookEndpoints().MapDiagnosticsEndpoints()`), der Seed des
Speichers (`Seed(…).Seed(…).Seed(…)`) und `IAuditTrail.Record`, das sich selbst zurückgibt.

In den Tests gilt es genauso – Testdaten kommen aus unveränderlichen Buildern, geprüft wird mit
[AwesomeAssertions](https://github.com/AwesomeAssertions/AwesomeAssertions) (MIT-Fork von
FluentAssertions 7):

```csharp
var request = ABook().WithTitle(string.Empty).AsCreateRequest();

var result = validator.Validate(request);

result.Errors.Should()
    .ContainSingle()
    .Which.ErrorMessage.Should().Be(ValidationMessages.TitleRequired);
```

## Synchron und ohne Kommentare

Der Code ist durchgehend synchron: kein `async`, kein `await`, keine `Task`-Rückgabewerte in
eigenen Signaturen. Handler, Services und Repository geben ihre Werte direkt zurück:

```csharp
private static Results<Ok<BookResponse>, NotFound> GetById(int id, IBookService books)
{
    var book = books.Get(id);

    return book is null ? TypedResults.NotFound() : TypedResults.Ok(book);
}
```

Vier Stellen geben weiterhin `Task` bzw. `ValueTask` zurück, weil die Schnittstellen von
ASP.NET Core es vorschreiben – aber keine davon benutzt `async`/`await`:
`IExceptionHandler.TryHandleAsync`, `IEndpointFilter.InvokeAsync`, `IHostedService`
und die Middleware. Die `AuditTrailMiddleware` protokolliert deshalb über
`Response.OnCompleted(...)` statt über `await next(context)`.

Ausnahme sind die Integrationstests: der In-Memory-Testserver von ASP.NET Core weist den
synchronen Aufruf ausdrücklich ab (`NotSupportedException`), dort bleibt der `HttpClient`
asynchron.

Der Code enthält keine Kommentare – auch die Tests nicht. Die Dreiteilung Given-When-Then
steht dort im Methodennamen und in den durch Leerzeilen getrennten Blöcken.

## Tests

```bash
dotnet test
```

70 Tests in `asp-net-rest.Tests`, benannt nach `given<Vorbedingung>_when<Aktion>_then<Ergebnis>`
und in drei durch Leerzeilen getrennte Blöcke gegliedert:

| Ordner | Was geprüft wird |
|---|---|
| `Validation/` | Die Regeln einzeln: Pflichtfelder, Längen, Jahresgrenzen, Dubletten, PATCH ohne Änderung, unbekannte Sortierschlüssel |
| `Services/` | Sortierstrategien, `BookService` (Paging, Trimmen, PATCH-Semantik) und die Registrierung selbst: Lebensdauern, keyed services, Dekorator, Options |
| `Persistence/` | Der Speicher: Id-Vergabe, PATCH lässt andere Felder stehen, Dublettenprüfung ohne Beachtung der Gross-/Kleinschreibung |
| `Api/` | Integrationstests über die ganze Anwendung – jede HTTP-Methode, Status-Codes, `Location`- und `Allow`-Header, Problem Details |
| `Fixtures/` | Fluent Builder für Testdaten (`ABook()`, `APatch()`), Konstanten (`BookFixtures`), die angehaltene Uhr (`FixedClock`) und der Container ohne Webserver (`TestHost`) |

Zwei Dinge sind dabei die eigentliche Lehre:

* **Fluent Assertions**: `page.Should().BeEquivalentTo(new { TotalCount = 3, SortedBy = "id" })`
  prüft mehrere Felder in einer Zusicherung – und meldet im Fehlerfall, welches Feld abweicht.
* **Unit-Tests ohne Webserver**: Validatoren und Services entstehen mit einem `new` bzw. aus
  einem Container ohne Host. Möglich ist das nur, weil die Abhängigkeiten an Schnittstellen
  hängen – `FixedClock` hält die Zeit an, sonst wäre die Jahresregel nur bis Silvester grün.
* **Integrationstests mit `WebApplicationFactory<Program>`**: dieselbe `Program.cs`, im
  Arbeitsspeicher gestartet, ohne Port und ohne Netzwerk. Jeder Test bekommt eine eigene
  Anwendung und damit einen frischen Bestand.

Für die Tests trägt `Program.cs` am Ende eine Zeile `public partial class Program;` – bei
Top-Level-Statements ist die erzeugte Klasse sonst intern. Und `AddApplicationServices` ist
in `AddApplicationCoreServices` (alles ohne Webserver) und `AddWebInfrastructure`
(OpenAPI, Problem Details) geteilt, damit der Container im Unit-Test ohne Host baubar bleibt.

## Nächster Schritt

Soll aus dem RAM eine echte Datenbank werden, wird nur `InMemoryBookRepository` gegen ein
Repository mit EF Core ausgetauscht – eine Zeile im Composition Root. Endpunkte, Validatoren
und Services bleiben unverändert. Genau dafür gibt es die Schnittstellen.
