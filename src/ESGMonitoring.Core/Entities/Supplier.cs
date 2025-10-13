using System.ComponentModel.DataAnnotations;

namespace ESGMonitoring.Core.Entities;

public class Supplier : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string TaxId { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string ContactEmail { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string ContactPhone { get; set; } = string.Empty;
    
    // ESG Scores (0-100)
    public decimal EnvironmentalScore { get; set; }
    public decimal SocialScore { get; set; }
    public decimal GovernanceScore { get; set; }
    
    // Sustainability Practices
    public bool HasRenewableEnergyProgram { get; set; }
    public bool HasWasteReductionProgram { get; set; }
    public bool HasWaterConservationProgram { get; set; }
    public bool HasCarbonNeutralityPlan { get; set; }
    
    // Fair Labor Practices
    public bool HasFairLaborCertification { get; set; }
    public bool HasChildLaborPolicy { get; set; }
    public bool HasSafeWorkingConditions { get; set; }
    public bool HasLivingWagePolicy { get; set; }
    
    // Certifications
    [MaxLength(1000)]
    public string Certifications { get; set; } = string.Empty;
    
    public DateTime LastAuditDate { get; set; }
    public DateTime NextAuditDate { get; set; }
    
    // Risk Assessment
    public SupplierRiskLevel RiskLevel { get; set; }
    
    [MaxLength(2000)]
    public string RiskNotes { get; set; } = string.Empty;
    
    // Navigation Properties
    public virtual ICollection<CarbonFootprint> CarbonFootprints { get; set; } = new List<CarbonFootprint>();
    public virtual ICollection<ComplianceAlert> ComplianceAlerts { get; set; } = new List<ComplianceAlert>();
}

public enum SupplierRiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}