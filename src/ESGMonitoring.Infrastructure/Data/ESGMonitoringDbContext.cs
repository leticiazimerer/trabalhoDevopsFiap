using ESGMonitoring.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ESGMonitoring.Infrastructure.Data;

public class ESGMonitoringDbContext : DbContext
{
    public ESGMonitoringDbContext(DbContextOptions<ESGMonitoringDbContext> options) : base(options)
    {
    }

    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<CarbonFootprint> CarbonFootprints { get; set; }
    public DbSet<ComplianceAlert> ComplianceAlerts { get; set; }
    public DbSet<SustainabilityReport> SustainabilityReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Supplier Configuration
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TaxId).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.RiskLevel);
            entity.HasIndex(e => e.NextAuditDate);
            
            entity.Property(e => e.EnvironmentalScore)
                .HasPrecision(5, 2);
            entity.Property(e => e.SocialScore)
                .HasPrecision(5, 2);
            entity.Property(e => e.GovernanceScore)
                .HasPrecision(5, 2);
        });

        // CarbonFootprint Configuration
        modelBuilder.Entity<CarbonFootprint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.ProductionDate);
            entity.HasIndex(e => e.ProductCategory);
            entity.HasIndex(e => e.IsVerified);
            
            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.CarbonFootprints)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ComplianceAlert Configuration
        modelBuilder.Entity<ComplianceAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DetectedAt);
            entity.HasIndex(e => e.DueDate);
            
            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.ComplianceAlerts)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SustainabilityReport Configuration
        modelBuilder.Entity<SustainabilityReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReportType);
            entity.HasIndex(e => e.Period);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.GeneratedAt);
            
            entity.Property(e => e.SupplierIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>());
        });

        // Seed Data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Suppliers
        var suppliers = new[]
        {
            new Supplier
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "GreenTech Solutions Ltd",
                TaxId = "12345678901",
                Address = "123 Sustainability Ave, Green City, GC 12345",
                ContactEmail = "contact@greentech.com",
                ContactPhone = "+1-555-0101",
                EnvironmentalScore = 85.5m,
                SocialScore = 78.2m,
                GovernanceScore = 92.1m,
                HasRenewableEnergyProgram = true,
                HasWasteReductionProgram = true,
                HasWaterConservationProgram = true,
                HasCarbonNeutralityPlan = true,
                HasFairLaborCertification = true,
                HasChildLaborPolicy = true,
                HasSafeWorkingConditions = true,
                HasLivingWagePolicy = true,
                Certifications = "ISO 14001, SA8000, B-Corp",
                LastAuditDate = DateTime.UtcNow.AddMonths(-6),
                NextAuditDate = DateTime.UtcNow.AddMonths(6),
                RiskLevel = SupplierRiskLevel.Low,
                RiskNotes = "Excellent ESG performance with strong certifications"
            },
            new Supplier
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Industrial Manufacturing Corp",
                TaxId = "23456789012",
                Address = "456 Factory St, Industrial Park, IP 23456",
                ContactEmail = "info@indmfg.com",
                ContactPhone = "+1-555-0202",
                EnvironmentalScore = 45.8m,
                SocialScore = 52.3m,
                GovernanceScore = 68.7m,
                HasRenewableEnergyProgram = false,
                HasWasteReductionProgram = true,
                HasWaterConservationProgram = false,
                HasCarbonNeutralityPlan = false,
                HasFairLaborCertification = false,
                HasChildLaborPolicy = true,
                HasSafeWorkingConditions = true,
                HasLivingWagePolicy = false,
                Certifications = "ISO 9001",
                LastAuditDate = DateTime.UtcNow.AddMonths(-12),
                NextAuditDate = DateTime.UtcNow.AddMonths(-1),
                RiskLevel = SupplierRiskLevel.High,
                RiskNotes = "Overdue audit, low ESG scores, needs immediate attention"
            }
        };

        modelBuilder.Entity<Supplier>().HasData(suppliers);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;
            
            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}