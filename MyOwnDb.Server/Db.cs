using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using static System.Environment;

namespace MyOwnDb.Server;

public static class Db
{
    
}

public class AppDbContext : DbContext
{
    public DbSet<StoreInstance> Stores { get; set; }
    
    private string DbPath { get; } = Path.Join(
        CurrentDirectory, 
        "my-own.db"
    );
    
    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<StoreInstance>()
            .ToTable("sqlite_master")
            .HasNoKey();
    }
}

[Table("sqlite_master")]
public class StoreInstance
{
    public string Name { get; set; }
}