using Microsoft.AspNetCore.Http.HttpResults;

namespace AspNetRest.Endpoints;

public static class EndpointResults
{
    public static Results<Ok<TResponse>, NotFound> OkOrNotFound<TResponse>(TResponse? response)
        where TResponse : class =>
        response is null ? TypedResults.NotFound() : TypedResults.Ok(response);

    public static Results<Ok, NotFound> OkOrNotFound(bool found) =>
        found ? TypedResults.Ok() : TypedResults.NotFound();

    public static Results<NoContent, NotFound> NoContentOrNotFound(bool succeeded) =>
        succeeded ? TypedResults.NoContent() : TypedResults.NotFound();
}
