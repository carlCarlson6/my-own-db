using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MyOwnDb.Server.Stores;

public class StoreBuilder(AppDbContext db)
{
    private readonly string[] _sqlKeywords =
        ["SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "ALTER", "DROP", "FROM", "WHERE"];

    private string _storeName = string.Empty;
    private Guid? _tenantId;
    private List<AddRecordToStore> _records = [];

    public StoreBuilder WithName(string storeName)
    {
        _storeName = HasSql(storeName)
            ? throw new InvalidOperationException("invalid name")
            : storeName;
        return this;
    }

    public StoreBuilder WithTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public StoreBuilder WithRecords(List<AddRecordToStore> records)
    {
        _records = records;
        return this;
    }
    
    public async Task Build()
    {
        await using var connection = new SqliteConnection(db.Database.GetDbConnection().ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        
        var storeTableName = BuildStoreTableName(_tenantId, _storeName);
        var sql = $"""
                   create table if not exists '{storeTableName}' (
                       id varchar(64) primary key not null,
                       payload text not null,
                       collection text not null
                   );

                   insert into '{storeTableName}' (id, payload, collection) values (@id, @payload, @collection);
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
        await transaction.CommitAsync();
    }
    
    public static string BuildStoreTableName(Guid? tenantId, string? storeName)
    {
        if (string.IsNullOrWhiteSpace(storeName))
            throw new InvalidOperationException("invalid name");
        if (tenantId is null)
            throw new InvalidOperationException("invalid tenant");
        return $"store_{tenantId}_{storeName}";
    }

    private bool HasSql(string input) => _sqlKeywords.Any(keyword => input.Contains(keyword, StringComparison.InvariantCultureIgnoreCase));
    
}
