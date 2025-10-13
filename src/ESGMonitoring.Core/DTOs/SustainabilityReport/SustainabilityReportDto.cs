namespace ESGMonitoring.Core.DTOs.SustainabilityReport;

public class SustainabilityReportDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty; // ESGOverview, CarbonFootprint, SupplierPerformance, ComplianceSummary
    public string Period { get; set; } = string.Empty; // Q1 2024, 2024, Jan-Mar 2024
    public string Status { get; set; } = string.Empty; // Draft, Generated, Published
    public string? ExecutiveSummary { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    public bool IsPublic { get; set; }
    public List<string> DistributionList { get; set; } = new();
}

public class CreateSustainabilityReportDto
{
    public string Title { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string? ExecutiveSummary { get; set; }
    public List<Guid>? SupplierIds { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class UpdateSustainabilityReportDto
{
    public string? Title { get; set; }
    public string? ExecutiveSummary { get; set; }
    public string? Content { get; set; }
    public string? Status { get; set; }
}

public class SustainabilityReportFilterDto : PaginationParameters
{
    public string? ReportType { get; set; }
    public string? Status { get; set; }
    public string? Period { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

public class PublishReportDto
{
    public List<string> DistributionList { get; set; } = new();
    public bool IsPublic { get; set; }
}

public class ApproveReportDto
{
    public string ApprovedBy { get; set; } = string.Empty;
    public string? ApprovalNotes { get; set; }
}

public class ReportFileResultDto
{
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class ReportComparisonDto
{
    public SustainabilityReportDto Report1 { get; set; } = new();
    public SustainabilityReportDto Report2 { get; set; } = new();
    public string ComparisonSummary { get; set; } = string.Empty;
    public DateTime ComparedAt { get; set; }
}

public class ReportInsightsDto
{
    public Guid ReportId { get; set; }
    public List<string> KeyInsights { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string TrendAnalysis { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

public class SustainabilityReportDashboardDto
{
    public int TotalReports { get; set; }
    public int PublishedReports { get; set; }
    public int PendingApprovalReports { get; set; }
    public int DraftReports { get; set; }
    public List<SustainabilityReportDto> RecentReports { get; set; } = new();
}