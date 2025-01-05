using System.Text.Json.Nodes;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.String;
using static System.Text.Json.JsonSerializer;

namespace MyOwnDb.Server.Stores;

public static class StoreEndpoints
{
    public static IEndpointRouteBuilder MapStore(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/db/store", CreateStore);
        
        app.MapPost("/api/db/store/{storeId}", AddRecordToStore);
        app.MapPut("/api/db/store/{storeId}", AddRecordToStore);
        app.MapPatch("/api/db/store/{storeId}", AddRecordToStore);

        app.MapGet("/api/db/store/{storeId}/collections", (
            [FromRoute(Name = nameof(storeId))] string storeId,
            [FromServices] AppDbContext db,
            HttpContext ctx) => GetStoreRecordsByCollection(storeId, Empty, db, ctx)
        );
        app.MapGet("/api/db/store/{storeId}/collections/{collection}", GetStoreRecordsByCollection);
        
        app.MapGet("api/db/store/{storeId}/records/{recordId}", GetStoreRecord);
        
        return app;
    }
    
    private static async Task<IResult> CreateStore(
        [FromBody] CreateStoreRequest request,
        [FromServices] AppDbContext db,
        HttpContext ctx)
    {
        await new StoreBuilder(db)
            .WithName(request.NameIdentifier)
            .WithTenantId(ctx.GetTenantId())
            .Build();
        return Results.Created();
    }

    private static async Task<IResult> AddRecordToStore(
        [FromRoute(Name = nameof(storeId))] string storeId,
        [FromBody] List<AddRecordToStore> request,
        [FromServices] AppDbContext db,
        HttpContext ctx)
    {
        await new StoreBuilder(db)
            .WithName(storeId)
            .WithTenantId(ctx.GetTenantId())
            .WithRecords(request)
            .Build();
        return Results.Created();
    }

    private static async Task<IResult> GetStoreRecordsByCollection(
        [FromRoute(Name = nameof(storeId))] string storeId,
        [FromRoute(Name = nameof(collection))] string collection,
        [FromServices] AppDbContext db,
        HttpContext ctx)
    {
        var tableName = StoreBuilder.BuildStoreTableName(ctx.GetTenantId(), storeId);
        await using var connection = new SqliteConnection(db.Database.GetDbConnection().ConnectionString);
        await connection.OpenAsync();
        
        var records = await connection.QueryAsync<string>(
            $"select payload from '{tableName}' where collection=@collection", 
            new { collection = IsNullOrWhiteSpace(collection) ? Empty : collection });

        return Results.Ok(records.Select(x => Deserialize<JsonObject>(x)));
    }
    
    private static async Task<IResult> GetStoreRecord(
        [FromRoute(Name = nameof(storeId))] string storeId,
        [FromRoute(Name = nameof(recordId))] string recordId,
        [FromServices] AppDbContext db,
        HttpContext ctx)
    {
        var tableName = StoreBuilder.BuildStoreTableName(ctx.GetTenantId(), storeId);
        await using var connection = new SqliteConnection(db.Database.GetDbConnection().ConnectionString);
        await connection.OpenAsync();

        var result = await connection.QueryFirstOrDefaultAsync<string>(
            $"select payload from '{tableName}' where id=@id", 
            new { id = recordId });
        
        return IsNullOrWhiteSpace(result) 
            ? Results.NotFound() 
            : Results.Ok(Deserialize<JsonObject>(result));
    }
}

public record CreateStoreRequest(string NameIdentifier);
public record AddRecordToStore(JsonObject Payload, string Collection = "");