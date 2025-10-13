using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.API.Controllers;

namespace ESGMonitoring.Core.Interfaces;

public interface ISustainabilityReportService
{
    Task<PagedResult<SustainabilityReportDto>> GetSustainabilityReportsAsync(SustainabilityReportFilterDto filter);
    Task<SustainabilityReportDto?> GetSustainabilityReportByIdAsync(Guid id);
    Task<SustainabilityReportDto> GenerateSustainabilityReportAsync(CreateSustainabilityReportDto createDto);
    Task<SustainabilityReportDto> UpdateSustainabilityReportAsync(Guid id, UpdateSustainabilityReportDto updateDto);
    Task<ReportFileResultDto> DownloadSustainabilityReportAsync(Guid id, string format);
    Task<SustainabilityReportDto> PublishSustainabilityReportAsync(Guid id, PublishReportDto publishDto);
    Task<SustainabilityReportDto> ApproveSustainabilityReportAsync(Guid id, ApproveReportDto approvalDto);
    Task<SustainabilityReportDashboardDto> GetSustainabilityReportDashboardAsync();
    Task<SustainabilityReportComparisonDto> CompareSustainabilityReportsAsync(CompareSustainabilityReportsDto compareDto);
    Task<SustainabilityInsightsDto> GenerateSustainabilityInsightsAsync(Guid reportId);
}