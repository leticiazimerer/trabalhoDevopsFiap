using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESGMonitoring.Core.Entities;

public class ComplianceAlert : BaseEntity
{
    [Required]
    public Guid SupplierId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public AlertSeverity Severity { get; set; }
    
    public AlertCategory Category { get; set; }
    
    public AlertStatus Status { get; set; } = AlertStatus.Open;
    
    // Detection Details
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(100)]
    public string DetectionMethod { get; set; } = string.Empty;
    
    // Threshold Information
    [Column(TypeName = "decimal(18,4)")]
    public decimal? ThresholdValue { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal? ActualValue { get; set; }
    
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;
    
    // Resolution Details
    public DateTime? ResolvedAt { get; set; }
    
    [MaxLength(100)]
    public string ResolvedBy { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string ResolutionNotes { get; set; } = string.Empty;
    
    // Due Date for Resolution
    public DateTime DueDate { get; set; }
    
    // Impact Assessment
    public ImpactLevel ImpactLevel { get; set; }
    
    [MaxLength(1000)]
    public string ImpactDescription { get; set; } = string.Empty;
    
    // Corrective Actions
    [MaxLength(2000)]
    public string CorrectiveActions { get; set; } = string.Empty;
    
    // Follow-up
    public bool RequiresFollowUp { get; set; }
    public DateTime? FollowUpDate { get; set; }
    
    [MaxLength(1000)]
    public string FollowUpNotes { get; set; } = string.Empty;
    
    // Navigation Properties
    [ForeignKey("SupplierId")]
    public virtual Supplier Supplier { get; set; } = null!;
}

public enum AlertSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum AlertCategory
{
    Environmental = 1,
    Social = 2,
    Governance = 3,
    CarbonEmissions = 4,
    LaborPractices = 5,
    SafetyViolation = 6,
    EthicsViolation = 7,
    RegulatoryCompliance = 8
}

public enum AlertStatus
{
    Open = 1,
    InProgress = 2,
    Resolved = 3,
    Closed = 4,
    Escalated = 5
}

public enum ImpactLevel
{
    Minimal = 1,
    Minor = 2,
    Moderate = 3,
    Major = 4,
    Severe = 5
}