using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using static System.String;

namespace MyOwnDb.Server.Stores;

public static class StoreEndpoints
{
    public static IEndpointRouteBuilder MapStore(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/stores", CreateStore);
        
        app.MapPost("/api/stores/{storeName}", AddRecordToStore);
        app.MapPut("/api/stores/{storeName}", AddRecordToStore);
        app.MapPatch("/api/stores/{storeName}", AddRecordToStore);

        app.MapGet("api/stores/{storeName}", GetAllStoreRecords);
        
        app.MapGet("/api/stores/{storeName}/collections", (
            [FromRoute(Name = nameof(storeName))] string storeName,
            [FromServices] StoreReader reader,
            HttpContext ctx) => GetStoreRecordsByCollection(storeName, Empty, reader, ctx)
        );
        app.MapGet("/api/stores/{storeName}/collections/{collection}", GetStoreRecordsByCollection);
        
        app.MapGet("api/stores/{storeName}/records/{recordId}", GetStoreRecord);
        
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

    private static async Task<IResult> GetAllStoreRecords(
        [FromRoute(Name = nameof(storeName))] string storeName,
        [FromServices] StoreReader reader,
        HttpContext ctx)
    {
        var records = await reader
            .All(new StoreId(storeName, ctx.GetTenantId()), ctx.RequestAborted);
        return Results.Ok(records);
    }
    
    private static async Task<IResult> GetStoreRecordsByCollection(
        [FromRoute(Name = nameof(storeName))] string storeName,
        [FromRoute(Name = nameof(collection))] string collection,
        [FromServices] StoreReader reader,
        HttpContext ctx)
    {
        var records = await reader
            .ByCollection(new StoreId(storeName, ctx.GetTenantId()), collection, ctx.RequestAborted);
        return Results.Ok(records);
    }
    
    private static async Task<IResult> GetStoreRecord(
        [FromRoute(Name = nameof(storeName))] string storeName,
        [FromRoute(Name = nameof(recordId))] string recordId,
        [FromServices] StoreReader reader,
        HttpContext ctx)
    {
        var maybeRecord = await reader
            .ByRecordId(new StoreId(storeName, ctx.GetTenantId()), recordId, ctx.RequestAborted);
        return maybeRecord is null
            ? Results.NotFound() 
            : Results.Ok(maybeRecord);
    }
}

public record CreateStoreRequest(string NameIdentifier);
public record AddRecordToStore(JsonObject Payload, string Collection = "");