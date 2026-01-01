using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;
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
    public DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ConversionRequestEntity>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<ConversionRequestEntity>()
            .Property(x => x.Status)
            .HasConversion<int>();

        modelBuilder.Entity<ConversionRequestEntity>()
            .Property(x => x.InputSourceType)
            .HasConversion<int>()
            .HasDefaultValue(InputSourceType.Form);

        modelBuilder.Entity<ConversionRequestEntity>()
            .Property(x => x.ExtractionConfidence)
            .HasPrecision(3, 2);

        modelBuilder.Entity<FhirResourceEntity>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<UserEntity>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<UserEntity>()
            .Property(x => x.Role)
            .HasConversion<int>();

        // Seed users
        modelBuilder.Entity<UserEntity>().HasData(
            new UserEntity { Id = 1, Username = "patient1", Role = UserRole.Patient },
            new UserEntity { Id = 2, Username = "doctor1", Role = UserRole.Practitioner },
            new UserEntity { Id = 3, Username = "hospital1", Role = UserRole.Organization }
        );
    }
}
}