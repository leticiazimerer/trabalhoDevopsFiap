using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ESGMonitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComplianceAlertsController : ControllerBase
{
    private readonly IComplianceAlertService _complianceAlertService;
    private readonly ILogger<ComplianceAlertsController> _logger;

    public ComplianceAlertsController(IComplianceAlertService complianceAlertService, ILogger<ComplianceAlertsController> logger)
    {
        _complianceAlertService = complianceAlertService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves paginated compliance alerts with advanced filtering and prioritization
    /// </summary>
    /// <param name="filter">Filter parameters including severity, status, category, and date range</param>
    /// <returns>Paginated list of compliance alerts with detailed violation information</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ComplianceAlertDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PagedResult<ComplianceAlertDto>>> GetComplianceAlerts([FromQuery] ComplianceAlertFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Retrieving compliance alerts with filter: {@Filter}", filter);
            
            var result = await _complianceAlertService.GetComplianceAlertsAsync(filter);
            
            _logger.LogInformation("Retrieved {Count} compliance alerts out of {Total}", 
                result.Data.Count(), result.TotalCount);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance alerts");
            return BadRequest($"Error retrieving compliance alerts: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific compliance alert
    /// </summary>
    /// <param name="id">Compliance alert unique identifier</param>
    /// <returns>Detailed compliance alert information including resolution history</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceAlertDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ComplianceAlertDto>> GetComplianceAlert(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving compliance alert with ID: {AlertId}", id);
            
            var alert = await _complianceAlertService.GetComplianceAlertByIdAsync(id);
            
            if (alert == null)
            {
                _logger.LogWarning("Compliance alert not found: {AlertId}", id);
                return NotFound($"Compliance alert with ID '{id}' not found");
            }

            return Ok(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance alert {AlertId}", id);
            return BadRequest($"Error retrieving compliance alert: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new compliance alert with automatic severity assessment and notification
    /// </summary>
    /// <param name="createDto">Compliance alert data including violation details and impact assessment</param>
    /// <returns>Created compliance alert with assigned priority and due date</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ComplianceAlertDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<ComplianceAlertDto>> CreateComplianceAlert([FromBody] CreateComplianceAlertDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating new compliance alert for supplier: {SupplierId}, Severity: {Severity}", 
                createDto.SupplierId, createDto.Severity);
            
            var alert = await _complianceAlertService.CreateComplianceAlertAsync(createDto);
            
            _logger.LogInformation("Successfully created compliance alert: {AlertId} with severity: {Severity}", 
                alert.Id, alert.Severity);
            
            return CreatedAtAction(nameof(GetComplianceAlert), new { id = alert.Id }, alert);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating compliance alert");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating compliance alert");
            return BadRequest($"Error creating compliance alert: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates existing compliance alert with status tracking and resolution notes
    /// </summary>
    /// <param name="id">Compliance alert unique identifier</param>
    /// <param name="updateDto">Updated compliance alert data</param>
    /// <returns>Updated compliance alert information</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceAlertDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<ComplianceAlertDto>> UpdateComplianceAlert(Guid id, [FromBody] UpdateComplianceAlertDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating compliance alert: {AlertId}", id);
            
            var alert = await _complianceAlertService.UpdateComplianceAlertAsync(id, updateDto);
            
            _logger.LogInformation("Successfully updated compliance alert: {AlertId}", id);
            
            return Ok(alert);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Compliance alert not found for update: {AlertId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating compliance alert {AlertId}", id);
            return BadRequest($"Error updating compliance alert: {ex.Message}");
        }
    }

    /// <summary>
    /// Resolves a compliance alert with resolution details and corrective actions
    /// </summary>
    /// <param name="id">Compliance alert unique identifier</param>
    /// <param name="resolutionDto">Resolution details including corrective actions and responsible party</param>
    /// <returns>Resolved compliance alert with updated status</returns>
    [HttpPost("{id:guid}/resolve")]
    [ProducesResponseType(typeof(ComplianceAlertDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<ComplianceAlertDto>> ResolveComplianceAlert(Guid id, [FromBody] ResolveAlertDto resolutionDto)
    {
        try
        {
            _logger.LogInformation("Resolving compliance alert: {AlertId} by {ResolvedBy}", id, resolutionDto.ResolvedBy);
            
            var alert = await _complianceAlertService.ResolveComplianceAlertAsync(id, resolutionDto);
            
            _logger.LogInformation("Successfully resolved compliance alert: {AlertId}", id);
            
            return Ok(alert);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Compliance alert not found for resolution: {AlertId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving compliance alert {AlertId}", id);
            return BadRequest($"Error resolving compliance alert: {ex.Message}");
        }
    }

    /// <summary>
    /// Escalates a compliance alert to higher priority with notification to management
    /// </summary>
    /// <param name="id">Compliance alert unique identifier</param>
    /// <param name="escalationReason">Reason for escalation</param>
    /// <returns>Escalated compliance alert with updated priority</returns>
    [HttpPost("{id:guid}/escalate")]
    [ProducesResponseType(typeof(ComplianceAlertDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<ComplianceAlertDto>> EscalateComplianceAlert(Guid id, [FromBody] string escalationReason)
    {
        try
        {
            _logger.LogInformation("Escalating compliance alert: {AlertId}, Reason: {Reason}", id, escalationReason);
            
            var alert = await _complianceAlertService.EscalateComplianceAlertAsync(id, escalationReason);
            
            _logger.LogInformation("Successfully escalated compliance alert: {AlertId}", id);
            
            return Ok(alert);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Compliance alert not found for escalation: {AlertId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating compliance alert {AlertId}", id);
            return BadRequest($"Error escalating compliance alert: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves compliance alerts for a specific supplier with trend analysis
    /// </summary>
    /// <param name="supplierId">Supplier unique identifier</param>
    /// <param name="filter">Additional filtering parameters</param>
    /// <returns>Supplier-specific compliance alerts with violation patterns</returns>
    [HttpGet("by-supplier/{supplierId:guid}")]
    [ProducesResponseType(typeof(PagedResult<ComplianceAlertDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PagedResult<ComplianceAlertDto>>> GetComplianceAlertsBySupplier(
        Guid supplierId, 
        [FromQuery] ComplianceAlertFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Retrieving compliance alerts for supplier: {SupplierId}", supplierId);
            
            filter.SupplierId = supplierId;
            var result = await _complianceAlertService.GetComplianceAlertsAsync(filter);
            
            _logger.LogInformation("Retrieved {Count} compliance alerts for supplier {SupplierId}", 
                result.Data.Count(), supplierId);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance alerts for supplier {SupplierId}", supplierId);
            return BadRequest($"Error retrieving compliance alerts for supplier: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves critical compliance alerts requiring immediate attention
    /// </summary>
    /// <param name="daysOverdue">Number of days overdue to consider critical (default: 0)</param>
    /// <returns>List of critical compliance alerts sorted by priority and due date</returns>
    [HttpGet("critical")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceAlertDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<IEnumerable<ComplianceAlertDto>>> GetCriticalComplianceAlerts([FromQuery] int daysOverdue = 0)
    {
        try
        {
            _logger.LogInformation("Retrieving critical compliance alerts with {DaysOverdue} days overdue threshold", daysOverdue);
            
            var alerts = await _complianceAlertService.GetCriticalComplianceAlertsAsync(daysOverdue);
            
            _logger.LogInformation("Found {Count} critical compliance alerts", alerts.Count());
            
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving critical compliance alerts");
            return BadRequest($"Error retrieving critical compliance alerts: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates comprehensive compliance dashboard with key metrics and insights
    /// </summary>
    /// <returns>Dashboard data including alert statistics, resolution rates, and trend analysis</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ComplianceAlertDashboardDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<ComplianceAlertDashboardDto>> GetComplianceAlertDashboard()
    {
        try
        {
            _logger.LogInformation("Retrieving compliance alert dashboard data");
            
            var dashboard = await _complianceAlertService.GetComplianceAlertDashboardAsync();
            
            _logger.LogInformation("Successfully retrieved compliance alert dashboard data");
            
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance alert dashboard");
            return BadRequest($"Error retrieving compliance alert dashboard: {ex.Message}");
        }
    }

    /// <summary>
    /// Performs automated compliance monitoring and generates alerts based on predefined rules
    /// </summary>
    /// <param name="monitoringRequest">Monitoring parameters and thresholds</param>
    /// <returns>Results of automated monitoring including newly generated alerts</returns>
    [HttpPost("automated-monitoring")]
    [ProducesResponseType(typeof(AutomatedMonitoringResultDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<AutomatedMonitoringResultDto>> RunAutomatedMonitoring([FromBody] AutomatedMonitoringRequestDto monitoringRequest)
    {
        try
        {
            _logger.LogInformation("Running automated compliance monitoring with {RuleCount} rules", 
                monitoringRequest.MonitoringRules?.Count ?? 0);
            
            var result = await _complianceAlertService.RunAutomatedMonitoringAsync(monitoringRequest);
            
            _logger.LogInformation("Automated monitoring completed. Generated {AlertCount} new alerts", 
                result.NewAlertsGenerated);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running automated compliance monitoring");
            return BadRequest($"Error running automated compliance monitoring: {ex.Message}");
        }
    }
}

// Additional DTOs for Compliance Alerts
public class ComplianceAlertDto
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public string DetectionMethod { get; set; } = string.Empty;
    public decimal? ThresholdValue { get; set; }
    public decimal? ActualValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime? ResolvedAt { get; set; }
    public string ResolvedBy { get; set; } = string.Empty;
    public string ResolutionNotes { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string ImpactLevel { get; set; } = string.Empty;
    public string ImpactDescription { get; set; } = string.Empty;
    public string CorrectiveActions { get; set; } = string.Empty;
    public bool RequiresFollowUp { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string FollowUpNotes { get; set; } = string.Empty;
    public bool IsOverdue => DateTime.UtcNow > DueDate && Status != "Resolved" && Status != "Closed";
    public int DaysOverdue => IsOverdue ? (DateTime.UtcNow - DueDate).Days : 0;
}

public class CreateComplianceAlertDto
{
    public Guid SupplierId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DetectionMethod { get; set; } = string.Empty;
    public decimal? ThresholdValue { get; set; }
    public decimal? ActualValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string ImpactLevel { get; set; } = string.Empty;
    public string ImpactDescription { get; set; } = string.Empty;
    public string CorrectiveActions { get; set; } = string.Empty;
    public bool RequiresFollowUp { get; set; }
    public DateTime? FollowUpDate { get; set; }
}

public class UpdateComplianceAlertDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string ImpactLevel { get; set; } = string.Empty;
    public string ImpactDescription { get; set; } = string.Empty;
    public string CorrectiveActions { get; set; } = string.Empty;
    public bool RequiresFollowUp { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string FollowUpNotes { get; set; } = string.Empty;
}

public class ResolveAlertDto
{
    public string ResolvedBy { get; set; } = string.Empty;
    public string ResolutionNotes { get; set; } = string.Empty;
    public string CorrectiveActions { get; set; } = string.Empty;
}

public class ComplianceAlertFilterDto : PaginationParameters
{
    public Guid? SupplierId { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public string? Category { get; set; }
    public DateTime? DetectedAfter { get; set; }
    public DateTime? DetectedBefore { get; set; }
    public DateTime? DueBefore { get; set; }
    public bool? IsOverdue { get; set; }
    public string? ImpactLevel { get; set; }
}

public class ComplianceAlertDashboardDto
{
    public int TotalAlerts { get; set; }
    public int OpenAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int OverdueAlerts { get; set; }
    public decimal ResolutionRate { get; set; }
    public decimal AverageResolutionTime { get; set; }
    public Dictionary<string, int> AlertsBySeverity { get; set; } = new();
    public Dictionary<string, int> AlertsByCategory { get; set; } = new();
    public Dictionary<string, int> AlertsByStatus { get; set; } = new();
    public List<ComplianceAlertDto> RecentCriticalAlerts { get; set; } = new();
    public List<SupplierAlertSummaryDto> TopViolatingSuppliers { get; set; } = new();
}

public class SupplierAlertSummaryDto
{
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int TotalAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int OpenAlerts { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
}

public class AutomatedMonitoringRequestDto
{
    public List<MonitoringRuleDto> MonitoringRules { get; set; } = new();
    public List<Guid> SupplierIds { get; set; } = new();
    public DateTime? MonitoringPeriodStart { get; set; }
    public DateTime? MonitoringPeriodEnd { get; set; }
}

public class MonitoringRuleDto
{
    public string RuleName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public decimal ThresholdValue { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AutomatedMonitoringResultDto
{
    public DateTime MonitoringRunDate { get; set; }
    public int SuppliersMonitored { get; set; }
    public int RulesEvaluated { get; set; }
    public int NewAlertsGenerated { get; set; }
    public List<ComplianceAlertDto> GeneratedAlerts { get; set; } = new();
    public Dictionary<string, int> AlertsByRule { get; set; } = new();
    public List<string> MonitoringErrors { get; set; } = new();
}