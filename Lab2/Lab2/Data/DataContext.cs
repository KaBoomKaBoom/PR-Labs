using Lab2.Models;
using Microsoft.EntityFrameworkCore;

public class DataContext : DbContext
{
    private readonly IConfiguration _config;

    public DataContext(IConfiguration config)
    {
        _config = config;
    }

    public virtual DbSet<Product> Product { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder
                .UseSqlServer(_config.GetConnectionString("DefaultConnection"),
                    optionsBuilder => optionsBuilder.EnableRetryOnFailure());
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Lab2");

        modelBuilder.Entity<Product>()
            .ToTable("Product", "Lab2")
            .HasKey(u => u.Id);
        modelBuilder.Entity<ProductFiltered>()
            .ToTable("ProductFiltered", "Lab2")
            .HasKey(u => u.Id);
    }
}
