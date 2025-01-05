using Microsoft.EntityFrameworkCore;
using static System.Environment;

namespace MyOwnDb.Server;

public class AppDbContext : DbContext
{
    private string DbPath { get; } = Path.Join(
        CurrentDirectory, 
        "my-own.db"
    );
    
    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}