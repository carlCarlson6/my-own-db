using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MyOwnDb.Server.Stores;

public class StoreWriter(AppDbContext db)
{
    private StoreId? _storeId;
    private List<AddRecordToStore> _records = [];
    
    public StoreWriter WithStoreId(StoreId storeId)
    {
        _storeId = storeId;
        return this;
    }
    
    public StoreWriter WithRecords(List<AddRecordToStore> records)
    {
        _records = records;
        return this;
    }
    
    public async Task Execute(CancellationToken ct)
    {
        if (_storeId is null)
            throw new InvalidOperationException("invalid store id");
        
        await using var connection = new SqliteConnection(db.Database.GetDbConnection().ConnectionString);
        await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        
        var sql = $"""
                   create table if not exists '{_storeId}' (
                       id varchar(64) primary key not null,
                       payload text not null,
                       collection text not null
                   );

                   insert into '{_storeId}' (id, payload, collection) values (@id, @payload, @collection);
                   """;
        var valuesToInsert = _records.Select(x =>
        { 
            x.Payload.TryGetPropertyValue("id", out var idJsonNode);
            var id = string.IsNullOrWhiteSpace(idJsonNode?.ToString())
                ? Guid.NewGuid().ToString()
                : idJsonNode.ToString(); 
            x.Payload.TryAdd("id", id);
            return new
            {
                id,
                payload = JsonSerializer.Serialize(x.Payload),
                collection = string.IsNullOrWhiteSpace(x.Collection) ? string.Empty : x.Collection
            };
        }).ToArray<object>();
        
        await connection.ExecuteAsync(sql, valuesToInsert, transaction);
        await transaction.CommitAsync(ct);
    }
}
