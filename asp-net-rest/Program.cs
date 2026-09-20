using AspNetRest.Composition;

WebApplication.CreateBuilder(args)
    .AddApplicationServices()
    .Build()
    .UseApplicationPipeline()
    .MapApplicationEndpoints()
    .Run();

public partial class Program;
