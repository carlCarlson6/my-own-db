namespace MyOwnDb.Server.Stores.Query;

public record QueryFilterOperation(FilterOperationType Type, string? Path, object? Value, List<string>? Values)
{
    // TODO - check input values does not contain sql injection
    public string ToSql() => Type switch
    {
        FilterOperationType.EQUALS => BuildEqualsSql().sql,
        FilterOperationType.AND => "and ",
        FilterOperationType.OR => "or ",
        FilterOperationType.IN => throw new NotImplementedException(),
        _ => throw new ArgumentOutOfRangeException()
    };

    private (string sql, string paramName, object? paramValue) BuildEqualsSql()
    {
        if (string.IsNullOrEmpty(Path))
            throw new ArgumentException("missing data error");
        return (
            sql: $"json_extract(payload, '$.{Path.Replace("__", ".")}') = @{Path} ",
            paramName: Path,
            paramValue: Value
        );
    }
    
    public List<KeyValuePair<string, object?>> GetSqlParameters() => Type switch
    {
        FilterOperationType.EQUALS => [new KeyValuePair<string, object>(BuildEqualsSql().paramName, BuildEqualsSql().paramValue)],
        FilterOperationType.AND => [],
        FilterOperationType.OR => [],
        FilterOperationType.IN => throw new NotImplementedException(),
        _ => throw new ArgumentOutOfRangeException()
    };
}

public enum FilterOperationType
{
    EQUALS = 0,
    AND = 1,
    OR = 2,
    IN = 3
}