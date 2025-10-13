using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ESGMonitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SustainabilityReportsController : ControllerBase
{
    private readonly ISustainabilityReportService _sustainabilityReportService;
    private readonly ILogger<SustainabilityReportsController> _logger;

    public SustainabilityReportsController(ISustainabilityReportService sustainabilityReportService, ILogger<SustainabilityReportsController> logger)
    {
        _sustainabilityReportService = sustainabilityReportService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves paginated sustainability reports with advanced filtering and categorization
    /// </summary>
    /// <param name="filter">Filter parameters including report type, period, status, and date range</param>
    /// <returns>Paginated list of sustainability reports with comprehensive ESG metrics</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SustainabilityReportDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PagedResult<SustainabilityReportDto>>> GetSustainabilityReports([FromQuery] SustainabilityReportFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Retrieving sustainability reports with filter: {@Filter}", filter);
            
            var result = await _sustainabilityReportService.GetSustainabilityReportsAsync(filter);
            
            _logger.LogInformation("Retrieved {Count} sustainability reports out of {Total}", 
                result.Data.Count(), result.TotalCount);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sustainability reports");
            return BadRequest($"Error retrieving sustainability reports: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific sustainability report
    /// </summary>
    /// <param name="id">Sustainability report unique identifier</param>
    /// <returns>Detailed sustainability report with complete ESG analysis and recommendations</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SustainabilityReportDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<SustainabilityReportDto>> GetSustainabilityReport(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving sustainability report with ID: {ReportId}", id);
            
            var report = await _sustainabilityReportService.GetSustainabilityReportByIdAsync(id);
            
            if (report == null)
            {
                _logger.LogWarning("Sustainability report not found: {ReportId}", id);
                return NotFound($"Sustainability report with ID '{id}' not found");
            }

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sustainability report {ReportId}", id);
            return BadRequest($"Error retrieving sustainability report: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a new comprehensive sustainability report with advanced analytics and insights
    /// </summary>
    /// <param name="createDto">Report generation parameters including scope, period, and analysis criteria</param>
    /// <returns>Generated sustainability report with executive summary and detailed metrics</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(SustainabilityReportDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<SustainabilityReportDto>> GenerateSustainabilityReport([FromBody] CreateSustainabilityReportDto createDto)
    {
        try
        {
            _logger.LogInformation("Generating new sustainability report: {ReportTitle}, Type: {ReportType}", 
                createDto.Title, createDto.ReportType);
            
            var report = await _sustainabilityReportService.GenerateSustainabilityReportAsync(createDto);
            
            _logger.LogInformation("Successfully generated sustainability report: {ReportId} with {SupplierCount} suppliers analyzed", 
                report.Id, report.TotalSuppliersAssessed);
            
            return CreatedAtAction(nameof(GetSustainabilityReport), new { id = report.Id }, report);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while generating sustainability report");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sustainability report");
            return BadRequest($"Error generating sustainability report: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates existing sustainability report with new data and analysis
    /// </summary>
    /// <param name="id">Sustainability report unique identifier</param>
    /// <param name="updateDto">Updated report data and metadata</param>
    /// <returns>Updated sustainability report information</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SustainabilityReportDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<SustainabilityReportDto>> UpdateSustainabilityReport(Guid id, [FromBody] UpdateSustainabilityReportDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating sustainability report: {ReportId}", id);
            
            var report = await _sustainabilityReportService.UpdateSustainabilityReportAsync(id, updateDto);
            
            _logger.LogInformation("Successfully updated sustainability report: {ReportId}", id);
            
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Sustainability report not found for update: {ReportId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sustainability report {ReportId}", id);
            return BadRequest($"Error updating sustainability report: {ex.Message}");
        }
    }

    /// <summary>
    /// Downloads sustainability report in specified format (PDF, Excel, CSV)
    /// </summary>
    /// <param name="id">Sustainability report unique identifier</param>
    /// <param name="format">Download format (pdf, excel, csv)</param>
    /// <returns>Report file in requested format</returns>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(typeof(FileResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> DownloadSustainabilityReport(Guid id, [FromQuery] string format = "pdf")
    {
        try
        {
            _logger.LogInformation("Downloading sustainability report: {ReportId} in format: {Format}", id, format);
            
            var fileResult = await _sustainabilityReportService.DownloadSustainabilityReportAsync(id, format);
            
            _logger.LogInformation("Successfully prepared download for sustainability report: {ReportId}", id);
            
            return File(fileResult.FileContent, fileResult.ContentType, fileResult.FileName);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Sustainability report not found for download: {ReportId}", id);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid format requested for report download: {Format}", format);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading sustainability report {ReportId}", id);
            return BadRequest($"Error downloading sustainability report: {ex.Message}");
        }
    }

    /// <summary>
    /// Publishes sustainability report for external distribution and stakeholder access
    /// </summary>
    /// <param name="id">Sustainability report unique identifier</param>
    /// <param name="publishDto">Publication parameters including distribution list and access settings</param>
    /// <returns>Published report with access information</returns>
    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(SustainabilityReportDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<SustainabilityReportDto>> PublishSustainabilityReport(Guid id, [FromBody] PublishReportDto publishDto)
    {
        try
        {
            _logger.LogInformation("Publishing sustainability report: {ReportId} to {RecipientCount} recipients", 
                id, publishDto.DistributionList?.Count ?? 0);
            
            var report = await _sustainabilityReportService.PublishSustainabilityReportAsync(id, publishDto);
            
            _logger.LogInformation("Successfully published sustainability report: {ReportId}", id);
            
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Sustainability report not found for publication: {ReportId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing sustainability report {ReportId}", id);
            return BadRequest($"Error publishing sustainability report: {ex.Message}");
        }
    }

    /// <summary>
    /// Approves sustainability report for publication with management sign-off
    /// </summary>
    /// <param name="id">Sustainability report unique identifier</param>
    /// <param name="approvalDto">Approval details including approver information and notes</param>
    /// <returns>Approved sustainability report</returns>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(SustainabilityReportDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<SustainabilityReportDto>> ApproveSustainabilityReport(Guid id, [FromBody] ApproveReportDto approvalDto)
    {
        try
        {
            _logger.LogInformation("Approving sustainability report: {ReportId} by {ApprovedBy}", id, approvalDto.ApprovedBy);
            
            var report = await _sustainabilityReportService.ApproveSustainabilityReportAsync(id, approvalDto);
            
            _logger.LogInformation("Successfully approved sustainability report: {ReportId}", id);
            
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Sustainability report not found for approval: {ReportId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving sustainability report {ReportId}", id);
            return BadRequest($"Error approving sustainability report: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves sustainability reports by type with performance analytics
    /// </summary>
    /// <param name="reportType">Type of sustainability report (ESGOverview, CarbonFootprint, etc.)</param>
    /// <param name="filter">Additional filtering parameters</param>
    /// <returns>Filtered sustainability reports by type</returns>
    [HttpGet("by-type/{reportType}")]
    [ProducesResponseType(typeof(PagedResult<SustainabilityReportDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PagedResult<SustainabilityReportDto>>> GetSustainabilityReportsByType(
        string reportType, 
        [FromQuery] SustainabilityReportFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Retrieving sustainability reports by type: {ReportType}", reportType);
            
            filter.ReportType = reportType;
            var result = await _sustainabilityReportService.GetSustainabilityReportsAsync(filter);
            
            _logger.LogInformation("Retrieved {Count} sustainability reports of type {ReportType}", 
                result.Data.Count(), reportType);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sustainability reports by type {ReportType}", reportType);
            return BadRequest($"Error retrieving sustainability reports by type: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates comprehensive sustainability dashboard with key performance indicators
    /// </summary>
    /// <returns>Dashboard data including report statistics, compliance metrics, and trend analysis</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(SustainabilityReportDashboardDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<SustainabilityReportDashboardDto>> GetSustainabilityReportDashboard()
    {
        try
        {
            _logger.LogInformation("Retrieving sustainability report dashboard data");
            
            var dashboard = await _sustainabilityReportService.GetSustainabilityReportDashboardAsync();
            
            _logger.LogInformation("Successfully retrieved sustainability report dashboard data");
            
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sustainability report dashboard");
            return BadRequest($"Error retrieving sustainability report dashboard: {ex.Message}");
        }
    }

    /// <summary>
    /// Performs comparative analysis between multiple sustainability reports
    /// </summary>
    /// <param name="compareDto">Comparison parameters including report IDs and analysis criteria</param>
    /// <returns>Comparative analysis results with trend insights and performance benchmarks</returns>
    [HttpPost("compare")]
    [ProducesResponseType(typeof(SustainabilityReportComparisonDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<SustainabilityReportComparisonDto>> CompareSustainabilityReports([FromBody] CompareSustainabilityReportsDto compareDto)
    {
        try
        {
            _logger.LogInformation("Comparing {ReportCount} sustainability reports", compareDto.ReportIds.Count);
            
            var comparison = await _sustainabilityReportService.CompareSustainabilityReportsAsync(compareDto);
            
            _logger.LogInformation("Successfully completed sustainability report comparison");
            
            return Ok(comparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing sustainability reports");
            return BadRequest($"Error comparing sustainability reports: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates automated sustainability insights and recommendations based on report data
    /// </summary>
    /// <param name="id">Sustainability report unique identifier</param>
    /// <returns>AI-generated insights and actionable recommendations for ESG improvement</returns>
    [HttpGet("{id:guid}/insights")]
    [ProducesResponseType(typeof(SustainabilityInsightsDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<SustainabilityInsightsDto>> GetSustainabilityInsights(Guid id)
    {
        try
        {
            _logger.LogInformation("Generating sustainability insights for report: {ReportId}", id);
            
            var insights = await _sustainabilityReportService.GenerateSustainabilityInsightsAsync(id);
            
            _logger.LogInformation("Successfully generated sustainability insights for report: {ReportId}", id);
            
            return Ok(insights);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Sustainability report not found for insights generation: {ReportId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sustainability insights for report {ReportId}", id);
            return BadRequest($"Error generating sustainability insights: {ex.Message}");
        }
    }
}

// Additional DTOs for Sustainability Reports
public class SustainabilityReportDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<Guid> SupplierIds { get; set; } = new();
    public string ProductCategories { get; set; } = string.Empty;
    public string GeographicScope { get; set; } = string.Empty;
    public decimal TotalCarbonEmissions { get; set; }
    public decimal AverageESGScore { get; set; }
    public int TotalSuppliersAssessed { get; set; }
    public int CompliantSuppliers { get; set; }
    public int NonCompliantSuppliers { get; set; }
    public int CriticalAlerts { get; set; }
    public int HighRiskSuppliers { get; set; }
    public decimal RenewableEnergyPercentage { get; set; }
    public decimal WasteReduced { get; set; }
    public decimal WaterSaved { get; set; }
    public int WorkersImpacted { get; set; }
    public int SafetyIncidents { get; set; }
    public int TrainingHoursProvided { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string KeyFindings { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public bool RequiresApproval { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string ApprovedBy { get; set; } = string.Empty;
    public string ApprovalNotes { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public string DistributionList { get; set; } = string.Empty;
    public int Version { get; set; }
    public string VersionNotes { get; set; } = string.Empty;
}

public class CreateSustainabilityReportDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public List<Guid> SupplierIds { get; set; } = new();
    public string ProductCategories { get; set; } = string.Empty;
    public string GeographicScope { get; set; } = string.Empty;
    public bool RequiresApproval { get; set; }
}

public class UpdateSustainabilityReportDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string KeyFindings { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public string VersionNotes { get; set; } = string.Empty;
}

public class SustainabilityReportFilterDto : PaginationParameters
{
    public string? ReportType { get; set; }
    public string? Period { get; set; }
    public string? Status { get; set; }
    public DateTime? GeneratedAfter { get; set; }
    public DateTime? GeneratedBefore { get; set; }
    public string? GeneratedBy { get; set; }
    public bool? RequiresApproval { get; set; }
    public bool? IsPublished { get; set; }
}

public class PublishReportDto
{
    public List<string> DistributionList { get; set; } = new();
    public bool IsPublic { get; set; }
    public string AccessLevel { get; set; } = string.Empty;
}

public class ApproveReportDto
{
    public string ApprovedBy { get; set; } = string.Empty;
    public string ApprovalNotes { get; set; } = string.Empty;
}

public class SustainabilityReportDashboardDto
{
    public int TotalReports { get; set; }
    public int PublishedReports { get; set; }
    public int PendingApprovalReports { get; set; }
    public int DraftReports { get; set; }
    public Dictionary<string, int> ReportsByType { get; set; } = new();
    public Dictionary<string, int> ReportsByPeriod { get; set; } = new();
    public List<SustainabilityReportDto> RecentReports { get; set; } = new();
    public SustainabilityMetricsSummaryDto OverallMetrics { get; set; } = new();
}

public class SustainabilityMetricsSummaryDto
{
    public decimal TotalCarbonEmissions { get; set; }
    public decimal AverageESGScore { get; set; }
    public decimal ComplianceRate { get; set; }
    public decimal RenewableEnergyUsage { get; set; }
    public int TotalSuppliersMonitored { get; set; }
    public int ActiveAlerts { get; set; }
}

public class CompareSustainabilityReportsDto
{
    public List<Guid> ReportIds { get; set; } = new();
    public List<string> ComparisonMetrics { get; set; } = new();
}

public class SustainabilityReportComparisonDto
{
    public List<SustainabilityReportDto> Reports { get; set; } = new();
    public Dictionary<string, List<decimal>> MetricComparisons { get; set; } = new();
    public List<string> KeyDifferences { get; set; } = new();
    public List<string> Trends { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public class SustainabilityInsightsDto
{
    public Guid ReportId { get; set; }
    public List<string> KeyInsights { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public List<string> RiskAreas { get; set; } = new();
    public List<string> Opportunities { get; set; } = new();
    public Dictionary<string, decimal> BenchmarkComparisons { get; set; } = new();
    public List<string> NextSteps { get; set; } = new();
    public decimal OverallSustainabilityScore { get; set; }
    public string SustainabilityRating { get; set; } = string.Empty;
}

public class ReportFileResultDto
{
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}