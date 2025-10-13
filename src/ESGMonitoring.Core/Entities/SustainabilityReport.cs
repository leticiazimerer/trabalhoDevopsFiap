using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESGMonitoring.Core.Entities;

public class SustainabilityReport : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public ReportType ReportType { get; set; }
    
    public ReportPeriod Period { get; set; }
    
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    
    // Report Generation Details
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(100)]
    public string GeneratedBy { get; set; } = string.Empty;
    
    public ReportStatus Status { get; set; } = ReportStatus.Draft;
    
    // Scope Filters
    public List<Guid> SupplierIds { get; set; } = new();
    
    [MaxLength(500)]
    public string ProductCategories { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string GeographicScope { get; set; } = string.Empty;
    
    // Key Metrics Summary
    [Column(TypeName = "decimal(18,4)")]
    public decimal TotalCarbonEmissions { get; set; }
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal AverageESGScore { get; set; }
    
    public int TotalSuppliersAssessed { get; set; }
    public int CompliantSuppliers { get; set; }
    public int NonCompliantSuppliers { get; set; }
    public int CriticalAlerts { get; set; }
    public int HighRiskSuppliers { get; set; }
    
    // Renewable Energy Metrics
    [Column(TypeName = "decimal(5,2)")]
    public decimal RenewableEnergyPercentage { get; set; }
    
    // Waste Reduction Metrics
    [Column(TypeName = "decimal(18,4)")]
    public decimal WasteReduced { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal WaterSaved { get; set; }
    
    // Social Impact Metrics
    public int WorkersImpacted { get; set; }
    public int SafetyIncidents { get; set; }
    public int TrainingHoursProvided { get; set; }
    
    // File Information
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string FileName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string FileFormat { get; set; } = string.Empty;
    
    public long FileSizeBytes { get; set; }
    
    // Report Content (JSON format for flexibility)
    public string ReportData { get; set; } = string.Empty;
    
    // Executive Summary
    [MaxLength(5000)]
    public string ExecutiveSummary { get; set; } = string.Empty;
    
    // Key Findings
    [MaxLength(5000)]
    public string KeyFindings { get; set; } = string.Empty;
    
    // Recommendations
    [MaxLength(5000)]
    public string Recommendations { get; set; } = string.Empty;
    
    // Approval Workflow
    public bool RequiresApproval { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    [MaxLength(100)]
    public string ApprovedBy { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string ApprovalNotes { get; set; } = string.Empty;
    
    // Distribution
    public DateTime? PublishedAt { get; set; }
    
    [MaxLength(1000)]
    public string DistributionList { get; set; } = string.Empty;
    
    // Version Control
    public int Version { get; set; } = 1;
    
    [MaxLength(500)]
    public string VersionNotes { get; set; } = string.Empty;
}

public enum ReportType
{
    ESGOverview = 1,
    CarbonFootprint = 2,
    SupplierCompliance = 3,
    SustainabilityMetrics = 4,
    RiskAssessment = 5,
    ComplianceAudit = 6,
    ImpactAssessment = 7,
    Benchmarking = 8
}

public enum ReportPeriod
{
    Monthly = 1,
    Quarterly = 2,
    SemiAnnual = 3,
    Annual = 4,
    Custom = 5
}

public enum ReportStatus
{
    Draft = 1,
    InReview = 2,
    PendingApproval = 3,
    Approved = 4,
    Published = 5,
    Archived = 6
}