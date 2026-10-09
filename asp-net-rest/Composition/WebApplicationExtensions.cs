using AspNetRest.Endpoints;
using AspNetRest.Middleware;

namespace AspNetRest.Composition;

public static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        public WebApplication UseApplicationPipeline()
        {
            app.UseExceptionHandler()
                .UseStatusCodePages()
                .UseMiddleware<AuditTrailMiddleware>();

            return app;
        }

        public WebApplication MapApplicationEndpoints()
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
}
