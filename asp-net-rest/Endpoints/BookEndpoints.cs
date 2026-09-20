using AspNetRest.Contracts;
using AspNetRest.Services;
using AspNetRest.Validation;
using Microsoft.AspNetCore.Http.HttpResults;
using static AspNetRest.Endpoints.EndpointResults;

namespace AspNetRest.Endpoints;

public static class BookEndpoints
{
    private const string AllowedMethods = "GET, HEAD, POST, PUT, PATCH, DELETE, OPTIONS";

    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var books = endpoints.MapGroup("/api/books").WithTags("Bücher");

        books.MapGet("", GetPage)
            .WithName(nameof(GetPage))
            .WithSummary("Bücher seitenweise und sortiert lesen")
            .AddEndpointFilter<ValidationEndpointFilter<BookPageQuery>>();

        books.MapGet("/{id:int}", GetById)
            .WithName(nameof(GetById))
            .WithSummary("Ein Buch anhand seiner Id lesen");

        books.MapMethods("/{id:int}", [HttpMethods.Head], HeadById)
            .WithName(nameof(HeadById))
            .WithSummary("Prüfen, ob ein Buch existiert – ohne Body");

        books.MapPost("", Create)
            .WithName(nameof(Create))
            .WithSummary("Ein neues Buch anlegen")
            .AddEndpointFilter<ValidationEndpointFilter<CreateBookRequest>>();

        books.MapPut("/{id:int}", Replace)
            .WithName(nameof(Replace))
            .WithSummary("Ein Buch vollständig ersetzen")
            .AddEndpointFilter<ValidationEndpointFilter<UpdateBookRequest>>();

        books.MapPatch("/{id:int}", Patch)
            .WithName(nameof(Patch))
            .WithSummary("Einzelne Felder eines Buches ändern")
            .AddEndpointFilter<ValidationEndpointFilter<PatchBookRequest>>();

        books.MapDelete("/{id:int}", Delete)
            .WithName(nameof(Delete))
            .WithSummary("Ein Buch löschen");

        books.MapMethods("", [HttpMethods.Options], GetOptions)
            .WithName(nameof(GetOptions))
            .WithSummary("Die erlaubten HTTP-Methoden erfragen");

        return endpoints;
    }

    private static Ok<BookPageResponse> GetPage([AsParameters] BookPageQuery query, IBookService books) =>
        TypedResults.Ok(books.GetPage(query));

    private static Results<Ok<BookResponse>, NotFound> GetById(int id, IBookService books) =>
        OkOrNotFound(books.Get(id));

    private static Results<Ok, NotFound> HeadById(int id, IBookService books) =>
        OkOrNotFound(books.Exists(id));

    private static Created<BookResponse> Create(CreateBookRequest request, IBookService books)
    {
        var created = books.Create(request);

        return TypedResults.Created($"/api/books/{created.Id}", created);
    }

    private static Results<Ok<BookResponse>, NotFound> Replace(
        int id,
        UpdateBookRequest request,
        IBookService books) =>
        OkOrNotFound(books.Replace(id, request));

    private static Results<Ok<BookResponse>, NotFound> Patch(
        int id,
        PatchBookRequest request,
        IBookService books) =>
        OkOrNotFound(books.Patch(id, request));

    private static Results<NoContent, NotFound> Delete(int id, IBookService books) =>
        NoContentOrNotFound(books.Delete(id));

    private static NoContent GetOptions(HttpResponse response)
    {
        response.Headers.Allow = AllowedMethods;

        return TypedResults.NoContent();
    }
}
