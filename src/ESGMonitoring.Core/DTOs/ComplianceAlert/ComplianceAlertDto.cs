namespace ESGMonitoring.Core.DTOs.ComplianceAlert;

public class ComplianceAlertDto
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Critical, High, Medium, Low
    public string Category { get; set; } = string.Empty; // Environmental, Social, Governance, Audit
    public string Status { get; set; } = string.Empty; // Open, In Progress, Resolved, Escalated
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public string? EscalatedTo { get; set; }
    public string? EscalationReason { get; set; }
}

public class CreateComplianceAlertDto
{
    public Guid SupplierId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class UpdateComplianceAlertDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
}

public class ComplianceAlertFilterDto : PaginationParameters
{
    public Guid? SupplierId { get; set; }
    public string? Severity { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

public class ResolveAlertDto
{
    public string ResolvedBy { get; set; } = string.Empty;
    public string ResolutionNotes { get; set; } = string.Empty;
}

public class EscalateAlertDto
{
    public string EscalatedTo { get; set; } = string.Empty;
    public string EscalationReason { get; set; } = string.Empty;
}

public class ComplianceAlertDashboardDto
{
    public int TotalAlerts { get; set; }
    public int OpenAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public decimal ResolutionRate { get; set; }
    public List<ComplianceAlertDto> RecentAlerts { get; set; } = new();
}