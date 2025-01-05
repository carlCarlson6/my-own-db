using MyOwnDb.Server;
using MyOwnDb.Server.Stores;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddTransient<AppDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapStore();

app.Run();

internal static class HttpContextExtensions
{
    public static Guid GetTenantId(this HttpContext _)
    {
        // TODO
        return new Guid("cfd5b1f6-e4ee-45c2-9ef8-10fea235ac3a");
    }
}