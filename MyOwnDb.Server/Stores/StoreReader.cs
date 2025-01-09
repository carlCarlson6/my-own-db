using System.Text.Json;
using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static System.String;

namespace MyOwnDb.Server.Stores;

public class StoreReader(AppDbContext db)
{
    public async Task<IEnumerable<JsonObject>> All(StoreId storeId, CancellationToken ct)
    {
        await using var connection = new SqliteConnection(db.Database.GetDbConnection().ConnectionString);
        await connection.OpenAsync(ct);
        
        var records = await connection.QueryAsync<string>(
            sql: $"select payload from '{storeId}'");
        
        return records.Select(x => JsonSerializer.Deserialize<JsonObject>(x)!);
    }
    
    public async Task<IEnumerable<JsonObject>> ByCollection(StoreId storeId, string collection, CancellationToken ct)
    {
        await using var connection = new SqliteConnection(db.Database.GetDbConnection().ConnectionString);
        await connection.OpenAsync(ct);
        
        var records = await connection.QueryAsync<string>(
            sql: $"select payload from '{storeId}' where collection=@collection", 
            param: new { collection = IsNullOrWhiteSpace(collection) ? Empty : collection });
        
        return records.Select(x => JsonSerializer.Deserialize<JsonObject>(x)!);
    }

    public async Task<JsonObject?> ByRecordId(StoreId storeId, string recordId, CancellationToken ct)
    {
        await using var connection = new SqliteConnection(db.Database.GetDbConnection().ConnectionString);
        await connection.OpenAsync(ct);

        var result = await connection.QueryFirstOrDefaultAsync<string>(
            $"select payload from '{storeId}' where id=@id", 
            new { id = recordId });
        
        return result != null ? JsonSerializer.Deserialize<JsonObject>(result) : null;
    }

    public async Task<List<JsonObject>> Query(StoreId storeId, string queryPath, string value, CancellationToken ct)
    {
        await using var connection = new SqliteConnection(db.Database.GetDbConnection().ConnectionString);
        await connection.OpenAsync(ct);

        var result = await connection.QueryAsync<string>(
            sql: $"""
                  select 
                      payload, 
                      json_extract(payload, '$.{queryPath}') as to_match 
                  from '{storeId}' 
                  where to_match=@to_match
                  """, 
            param: new { to_match = value });
        
        return result.Select(x => JsonSerializer.Deserialize<JsonObject>(x)!).ToList() ?? [];
    }
}