using Microsoft.AspNetCore.Mvc;
using MyOwnDb.Server.Stores.Read.Query;
using static System.String;

namespace MyOwnDb.Server.Stores.Read;

public static class ReadStoreEndpoints
{
    public static IEndpointRouteBuilder MapReadStores(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/stores/{storeName}", GetAllStoreRecords);
        
        app.MapGet("/api/stores/{storeName}/collections", (
                [FromRoute(Name = nameof(storeName))] string storeName,
                [FromServices] StoreReader reader,
                HttpContext ctx) => GetStoreRecordsByCollection(storeName, Empty, reader, ctx)
        );
        app.MapGet("/api/stores/{storeName}/collections/{collection}", GetStoreRecordsByCollection);
        
        app.MapGet("api/stores/{storeName}/records/{recordId}", GetStoreRecord);
        
        return app
            .MapQueryStore();
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