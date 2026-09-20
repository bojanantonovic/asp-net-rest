using AspNetRest.Endpoints;
using AspNetRest.Middleware;

namespace AspNetRest.Composition;

public static class WebApplicationExtensions
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        app.UseExceptionHandler()
            .UseStatusCodePages()
            .UseMiddleware<AuditTrailMiddleware>();

        return app;
    }

    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapHomeEndpoints()
            .MapBookEndpoints()
            .MapDiagnosticsEndpoints();

        return app;
    }
}
