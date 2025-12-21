using FhirProject.Api.Models.entities;
using Microsoft.EntityFrameworkCore;

namespace FhirProject.Api.Data
{
    public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConversionRequestEntity> ConversionRequests { get; set; }
    public DbSet<FhirResourceEntity> FhirResources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ConversionRequestEntity>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<ConversionRequestEntity>()
            .Property(x => x.Status)
            .HasConversion<int>();

        modelBuilder.Entity<FhirResourceEntity>()
            .HasKey(x => x.Id);
    }
}
}