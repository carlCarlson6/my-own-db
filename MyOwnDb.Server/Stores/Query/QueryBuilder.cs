using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace MyOwnDb.Server.Stores.Query;

public class QueryBuilder(AppDbContext db)
{
    private StoreId? _storeId;
    private Dictionary<string, object?> _parameters = new();
    private readonly List<string> _filtersSql = [];
    
    public QueryBuilder WithStoreId(StoreId storeId)
    {
        _storeId = storeId;
        return this;
    }

    public QueryBuilder WithFilters(params QueryFilterOperation[] filters)
    {
        foreach (var filter in filters)
        {
            var sql = filter.ToSql();
            _filtersSql.Add(sql);
            
            _parameters = _parameters
                .Concat(filter
                    .GetSqlParameters())
                .ToDictionary();
        }
        
        return this;
    }
    
    public async Task<List<JsonObject>> Run(CancellationToken ct)
    {
        if (_storeId is null)
            throw new InvalidOperationException("Store Id not set");

        var strBuilder = new StringBuilder($"select payload from '{_storeId}' ");

        if (_filtersSql.Count > 0)
        {
            strBuilder = _filtersSql.Aggregate(
                strBuilder.Append("where "), 
                (current, filter) => current.Append(filter));
        }
        
        var sql = strBuilder.ToString();
        
        await using var connection = new SqliteConnection(db.Database.GetDbConnection().ConnectionString);
        await connection.OpenAsync(ct);

        var result = await connection.QueryAsync<string>(sql, param: _parameters);
        
        return result.Select(x => JsonSerializer.Deserialize<JsonObject>(x)!).ToList();
    }
}