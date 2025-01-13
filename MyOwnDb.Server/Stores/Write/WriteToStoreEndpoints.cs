using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace MyOwnDb.Server.Stores.Write;

public static class WriteToStoreEndpoints
{
    public static IEndpointRouteBuilder MapWriteToStores(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/stores", CreateStore);
        app.MapPost("/api/stores/{storeName}", AddRecordToStore);
        app.MapPut("/api/stores/{storeName}", AddRecordToStore);
        app.MapPatch("/api/stores/{storeName}", AddRecordToStore);
        return app;
    }
    
    private static async Task<IResult> CreateStore(
        [FromBody] CreateStoreRequest request,
        [FromServices] StoreWriter writer,
        HttpContext ctx)
    {
        await writer
            .WithStoreId(new StoreId(request.NameIdentifier, ctx.GetTenantId()))
            .Execute(ctx.RequestAborted);
        return Results.Created();
    }

    private static async Task<IResult> AddRecordToStore(
        [FromRoute(Name = nameof(storeName))] string storeName,
        [FromBody] List<AddRecordToStore> request,
        [FromServices] StoreWriter writer,
        HttpContext ctx)
    {
        await writer
            .WithStoreId(new StoreId(storeName, ctx.GetTenantId()))
            .WithRecords(request)
            .Execute(ctx.RequestAborted);
        return Results.Created();
    }
}

public record CreateStoreRequest(string NameIdentifier);
public record AddRecordToStore(JsonObject Payload, string Collection = "");