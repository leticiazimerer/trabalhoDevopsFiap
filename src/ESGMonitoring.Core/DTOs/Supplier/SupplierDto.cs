using ESGMonitoring.Core.Entities;

namespace ESGMonitoring.Core.DTOs.Supplier;

public class SupplierDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public decimal EnvironmentalScore { get; set; }
    public decimal SocialScore { get; set; }
    public decimal GovernanceScore { get; set; }
    public decimal OverallESGScore => (EnvironmentalScore + SocialScore + GovernanceScore) / 3;
    public bool HasRenewableEnergyProgram { get; set; }
    public bool HasWasteReductionProgram { get; set; }
    public bool HasWaterConservationProgram { get; set; }
    public bool HasCarbonNeutralityPlan { get; set; }
    public bool HasFairLaborCertification { get; set; }
    public bool HasChildLaborPolicy { get; set; }
    public bool HasSafeWorkingConditions { get; set; }
    public bool HasLivingWagePolicy { get; set; }
    public string Certifications { get; set; } = string.Empty;
    public DateTime LastAuditDate { get; set; }
    public DateTime NextAuditDate { get; set; }
    public SupplierRiskLevel RiskLevel { get; set; }
    public string RiskNotes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSupplierDto
{
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public decimal EnvironmentalScore { get; set; }
    public decimal SocialScore { get; set; }
    public decimal GovernanceScore { get; set; }
    public bool HasRenewableEnergyProgram { get; set; }
    public bool HasWasteReductionProgram { get; set; }
    public bool HasWaterConservationProgram { get; set; }
    public bool HasCarbonNeutralityPlan { get; set; }
    public bool HasFairLaborCertification { get; set; }
    public bool HasChildLaborPolicy { get; set; }
    public bool HasSafeWorkingConditions { get; set; }
    public bool HasLivingWagePolicy { get; set; }
    public string Certifications { get; set; } = string.Empty;
    public DateTime LastAuditDate { get; set; }
    public DateTime NextAuditDate { get; set; }
    public SupplierRiskLevel RiskLevel { get; set; }
    public string RiskNotes { get; set; } = string.Empty;
}

public class UpdateSupplierDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public decimal EnvironmentalScore { get; set; }
    public decimal SocialScore { get; set; }
    public decimal GovernanceScore { get; set; }
    public bool HasRenewableEnergyProgram { get; set; }
    public bool HasWasteReductionProgram { get; set; }
    public bool HasWaterConservationProgram { get; set; }
    public bool HasCarbonNeutralityPlan { get; set; }
    public bool HasFairLaborCertification { get; set; }
    public bool HasChildLaborPolicy { get; set; }
    public bool HasSafeWorkingConditions { get; set; }
    public bool HasLivingWagePolicy { get; set; }
    public string Certifications { get; set; } = string.Empty;
    public DateTime LastAuditDate { get; set; }
    public DateTime NextAuditDate { get; set; }
    public SupplierRiskLevel RiskLevel { get; set; }
    public string RiskNotes { get; set; } = string.Empty;
}

public class SupplierFilterDto : PaginationParameters
{
    public SupplierRiskLevel? RiskLevel { get; set; }
    public decimal? MinESGScore { get; set; }
    public decimal? MaxESGScore { get; set; }
    public bool? HasRenewableEnergy { get; set; }
    public bool? HasFairLaborCertification { get; set; }
    public DateTime? AuditDueBefore { get; set; }
    public string? Certification { get; set; }
}