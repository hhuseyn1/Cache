using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Server.Models;

namespace Server.Contexts;

public class KVDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory() + "../../../../")
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
        string? connectionString = configuration.GetConnectionString("MyDB");

        optionsBuilder.UseSqlServer(connectionString);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KeyValue>().HasKey(e => e.Key);
        base.OnModelCreating(modelBuilder);
    }

    DbSet<KeyValue> KeyValues { get; set; }
}