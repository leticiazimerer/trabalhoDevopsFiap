using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.API.Controllers;

namespace ESGMonitoring.Core.Interfaces;

public interface IComplianceAlertService
{
    Task<PagedResult<ComplianceAlertDto>> GetComplianceAlertsAsync(ComplianceAlertFilterDto filter);
    Task<ComplianceAlertDto?> GetComplianceAlertByIdAsync(Guid id);
    Task<ComplianceAlertDto> CreateComplianceAlertAsync(CreateComplianceAlertDto createDto);
    Task<ComplianceAlertDto> UpdateComplianceAlertAsync(Guid id, UpdateComplianceAlertDto updateDto);
    Task<ComplianceAlertDto> ResolveComplianceAlertAsync(Guid id, ResolveAlertDto resolutionDto);
    Task<ComplianceAlertDto> EscalateComplianceAlertAsync(Guid id, string escalationReason);
    Task<IEnumerable<ComplianceAlertDto>> GetCriticalComplianceAlertsAsync(int daysOverdue);
    Task<ComplianceAlertDashboardDto> GetComplianceAlertDashboardAsync();
    Task<AutomatedMonitoringResultDto> RunAutomatedMonitoringAsync(AutomatedMonitoringRequestDto monitoringRequest);
}