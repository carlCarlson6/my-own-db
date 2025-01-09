using MyOwnDb.Server;
using MyOwnDb.Server.Stores;
using MyOwnDb.Server.Stores.Query;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddTransient<AppDbContext>()
    .AddTransient<StoreWriter>()
    .AddTransient<StoreReader>()
    .AddTransient<QueryBuilder>();

var webApp = builder.Build();

if (webApp.Environment.IsDevelopment())
{
    webApp.UseSwagger();
    webApp.UseSwaggerUI();
}

webApp.UseHttpsRedirection();

webApp
    .MapStore()
    .MapQueryStore();

webApp.Run();

internal static class HttpContextExtensions
{
    public static Guid GetTenantId(this HttpContext _)
    {
        // TODO
        return new Guid("cfd5b1f6-e4ee-45c2-9ef8-10fea235ac3a");
    }
}