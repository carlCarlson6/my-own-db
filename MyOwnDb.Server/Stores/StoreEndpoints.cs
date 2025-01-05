using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using static System.String;

namespace MyOwnDb.Server.Stores;

public static class StoreEndpoints
{
    public static IEndpointRouteBuilder MapStore(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/db/store", CreateStore);
        
        app.MapPost("/api/db/store/{storeName}", AddRecordToStore);
        app.MapPut("/api/db/store/{storeName}", AddRecordToStore);
        app.MapPatch("/api/db/store/{storeName}", AddRecordToStore);

        app.MapGet("api/db/store/{storeName}", GetAllStoreRecords);
        
        app.MapGet("/api/db/store/{storeName}/collections", (
            [FromRoute(Name = nameof(storeName))] string storeName,
            [FromServices] StoreReader reader,
            HttpContext ctx) => GetStoreRecordsByCollection(storeName, Empty, reader, ctx)
        );
        app.MapGet("/api/db/store/{storeName}/collections/{collection}", GetStoreRecordsByCollection);
        
        app.MapGet("api/db/store/{storeName}/records/{recordId}", GetStoreRecord);
        
        app.MapPost("/api/db/store/{storeName}/query", QueryStore);
        
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
    
    private static async Task<IResult> QueryStore(
        [FromRoute(Name = nameof(storeName))] string storeName,
        [FromBody] QueryStoreRequest request,
        [FromServices] StoreReader reader,
        HttpContext ctx)
    {
        // TODO check path does not contain sql
        var records = await reader
            .Query(new StoreId(storeName, ctx.GetTenantId()), request.Path, request.Value, ctx.RequestAborted);
        return Results.Ok(records);
    }
}

public record CreateStoreRequest(string NameIdentifier);
public record AddRecordToStore(JsonObject Payload, string Collection = "");
public record QueryStoreRequest(string Path, string Value);