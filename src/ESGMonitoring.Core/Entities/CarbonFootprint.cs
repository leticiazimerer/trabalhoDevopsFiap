using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESGMonitoring.Core.Entities;

public class CarbonFootprint : BaseEntity
{
    [Required]
    public Guid SupplierId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string ProductCategory { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string BatchNumber { get; set; } = string.Empty;
    
    // Production Emissions (kg CO2e)
    [Column(TypeName = "decimal(18,4)")]
    public decimal ProductionEmissions { get; set; }
    
    // Transportation Emissions (kg CO2e)
    [Column(TypeName = "decimal(18,4)")]
    public decimal TransportationEmissions { get; set; }
    
    // Packaging Emissions (kg CO2e)
    [Column(TypeName = "decimal(18,4)")]
    public decimal PackagingEmissions { get; set; }
    
    // Total Emissions (kg CO2e)
    [Column(TypeName = "decimal(18,4)")]
    public decimal TotalEmissions { get; set; }
    
    // Production Details
    public int QuantityProduced { get; set; }
    
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;
    
    public DateTime ProductionDate { get; set; }
    
    // Transportation Details
    [MaxLength(200)]
    public string OriginLocation { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string DestinationLocation { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal DistanceKm { get; set; }
    
    public TransportMode TransportMode { get; set; }
    
    // Energy Sources
    [Column(TypeName = "decimal(5,2)")]
    public decimal RenewableEnergyPercentage { get; set; }
    
    // Verification
    public bool IsVerified { get; set; }
    
    [MaxLength(100)]
    public string VerifiedBy { get; set; } = string.Empty;
    
    public DateTime? VerificationDate { get; set; }
    
    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;
    
    // Navigation Properties
    [ForeignKey("SupplierId")]
    public virtual Supplier Supplier { get; set; } = null!;
}

public enum TransportMode
{
    Road = 1,
    Rail = 2,
    Sea = 3,
    Air = 4,
    Pipeline = 5,
    Multimodal = 6
}