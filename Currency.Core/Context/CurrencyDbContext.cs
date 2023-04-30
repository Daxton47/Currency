using CurrencyApp.EFCore.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyApp.EFCore.Context;

public class CurrencyDbContext : DbContext
{
    public required DbSet<Currency> Currencies { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Filename=Test.db");
    }
}
