using static System.String;
using static System.StringComparison;

namespace MyOwnDb.Server.Stores;

public class StoreId
{
    private readonly string[] _sqlKeywords = [ "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "ALTER", "DROP", "FROM", "WHERE" ];
    
    private readonly string _name;
    private readonly Guid _tenantId;

    public StoreId(string? name, Guid? tenantId)
    {
        if (IsNullOrWhiteSpace(name) || HasSql(name))
            throw new InvalidOperationException("invalid name");
        if (tenantId is null)
            throw new InvalidOperationException("invalid tenant");
        
        _name = name;
        _tenantId = tenantId.Value;
    }

    public override string ToString() => $"store_{_tenantId}_{_name}";
    
    private bool HasSql(string input) => _sqlKeywords
        .Any(keyword => input.Contains(keyword, InvariantCultureIgnoreCase));
}