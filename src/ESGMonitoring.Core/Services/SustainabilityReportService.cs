using AutoMapper;
using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.Entities;
using ESGMonitoring.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ESGMonitoring.Core.Services;

public class SustainabilityReportService : ISustainabilityReportService
{
    private readonly ISustainabilityReportRepository _sustainabilityReportRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly ICarbonFootprintRepository _carbonFootprintRepository;
    private readonly IComplianceAlertRepository _complianceAlertRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SustainabilityReportService> _logger;

    public SustainabilityReportService(
        ISustainabilityReportRepository sustainabilityReportRepository,
        ISupplierRepository supplierRepository,
        ICarbonFootprintRepository carbonFootprintRepository,
        IComplianceAlertRepository complianceAlertRepository,
        IMapper mapper,
        ILogger<SustainabilityReportService> logger)
    {
        _sustainabilityReportRepository = sustainabilityReportRepository;
        _supplierRepository = supplierRepository;
        _carbonFootprintRepository = carbonFootprintRepository;
        _complianceAlertRepository = complianceAlertRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<SustainabilityReportDto>> GetSustainabilityReportsAsync(SustainabilityReportFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Getting sustainability reports with filter: {@Filter}", filter);

            var query = await _sustainabilityReportRepository.GetAllAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.ReportType))
            {
                query = query.Where(r => r.ReportType.ToLower().Contains(filter.ReportType.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(r => r.Status.ToLower().Contains(filter.Status.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.Period))
            {
                query = query.Where(r => r.Period.ToLower().Contains(filter.Period.ToLower()));
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= filter.DateTo.Value);
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "title" => filter.SortDescending ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title),
                "reporttype" => filter.SortDescending ? query.OrderByDescending(r => r.ReportType) : query.OrderBy(r => r.ReportType),
                "createdat" => filter.SortDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            var totalCount = query.Count();
            var reports = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var reportDtos = _mapper.Map<List<SustainabilityReportDto>>(reports);

            return new PagedResult<SustainabilityReportDto>
            {
                Data = reportDtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sustainability reports");
            throw;
        }
    }

    public async Task<SustainabilityReportDto?> GetSustainabilityReportByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting sustainability report by ID: {Id}", id);

            var report = await _sustainabilityReportRepository.GetByIdAsync(id);
            return report != null ? _mapper.Map<SustainabilityReportDto>(report) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sustainability report by ID: {Id}", id);
            throw;
        }
    }

    public async Task<SustainabilityReportDto> GenerateSustainabilityReportAsync(CreateSustainabilityReportDto createDto)
    {
        try
        {
            _logger.LogInformation("Generating sustainability report: {@CreateDto}", createDto);

            var report = _mapper.Map<SustainabilityReport>(createDto);
            report.Id = Guid.NewGuid();
            report.CreatedAt = DateTime.UtcNow;
            report.Status = "Generated";

            // Generate report content based on type
            await GenerateReportContentAsync(report);

            var createdReport = await _sustainabilityReportRepository.AddAsync(report);
            return _mapper.Map<SustainabilityReportDto>(createdReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sustainability report");
            throw;
        }
    }

    public async Task<SustainabilityReportDto> UpdateSustainabilityReportAsync(Guid id, UpdateSustainabilityReportDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating sustainability report {Id}: {@UpdateDto}", id, updateDto);

            var existingReport = await _sustainabilityReportRepository.GetByIdAsync(id);
            if (existingReport == null)
            {
                throw new ArgumentException($"Sustainability report with ID {id} not found");
            }

            _mapper.Map(updateDto, existingReport);
            existingReport.UpdatedAt = DateTime.UtcNow;

            var updatedReport = await _sustainabilityReportRepository.UpdateAsync(existingReport);
            return _mapper.Map<SustainabilityReportDto>(updatedReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sustainability report {Id}", id);
            throw;
        }
    }

    public async Task<ReportFileResultDto> DownloadSustainabilityReportAsync(Guid id, string format)
    {
        try
        {
            _logger.LogInformation("Downloading sustainability report {Id} in format: {Format}", id, format);

            var report = await _sustainabilityReportRepository.GetByIdAsync(id);
            if (report == null)
            {
                throw new ArgumentException($"Sustainability report with ID {id} not found");
            }

            var fileContent = format.ToLower() switch
            {
                "pdf" => GeneratePdfContent(report),
                "excel" => GenerateExcelContent(report),
                "json" => GenerateJsonContent(report),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };

            var contentType = format.ToLower() switch
            {
                "pdf" => "application/pdf",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "json" => "application/json",
                _ => "application/octet-stream"
            };

            var fileName = $"{report.Title}_{DateTime.UtcNow:yyyyMMdd}.{format.ToLower()}";

            return new ReportFileResultDto
            {
                FileContent = fileContent,
                ContentType = contentType,
                FileName = fileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading sustainability report {Id}", id);
            throw;
        }
    }

    public async Task<SustainabilityReportDto> PublishSustainabilityReportAsync(Guid id, PublishReportDto publishDto)
    {
        try
        {
            _logger.LogInformation("Publishing sustainability report {Id}: {@PublishDto}", id, publishDto);

            var existingReport = await _sustainabilityReportRepository.GetByIdAsync(id);
            if (existingReport == null)
            {
                throw new ArgumentException($"Sustainability report with ID {id} not found");
            }

            existingReport.Status = "Published";
            existingReport.PublishedAt = DateTime.UtcNow;
            existingReport.IsPublic = publishDto.IsPublic;
            existingReport.DistributionList = JsonSerializer.Serialize(publishDto.DistributionList);
            existingReport.UpdatedAt = DateTime.UtcNow;

            var updatedReport = await _sustainabilityReportRepository.UpdateAsync(existingReport);
            return _mapper.Map<SustainabilityReportDto>(updatedReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing sustainability report {Id}", id);
            throw;
        }
    }

    public async Task<SustainabilityReportDto> ApproveSustainabilityReportAsync(Guid id, ApproveReportDto approveDto)
    {
        try
        {
            _logger.LogInformation("Approving sustainability report {Id}: {@ApproveDto}", id, approveDto);

            var existingReport = await _sustainabilityReportRepository.GetByIdAsync(id);
            if (existingReport == null)
            {
                throw new ArgumentException($"Sustainability report with ID {id} not found");
            }

            existingReport.ApprovedBy = approveDto.ApprovedBy;
            existingReport.ApprovalNotes = approveDto.ApprovalNotes;
            existingReport.ApprovedAt = DateTime.UtcNow;
            existingReport.UpdatedAt = DateTime.UtcNow;

            var updatedReport = await _sustainabilityReportRepository.UpdateAsync(existingReport);
            return _mapper.Map<SustainabilityReportDto>(updatedReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving sustainability report {Id}", id);
            throw;
        }
    }

    public async Task DeleteSustainabilityReportAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting sustainability report: {Id}", id);

            var existingReport = await _sustainabilityReportRepository.GetByIdAsync(id);
            if (existingReport == null)
            {
                throw new ArgumentException($"Sustainability report with ID {id} not found");
            }

            await _sustainabilityReportRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sustainability report {Id}", id);
            throw;
        }
    }

    public async Task<ReportComparisonDto> CompareSustainabilityReportsAsync(Guid reportId1, Guid reportId2)
    {
        try
        {
            _logger.LogInformation("Comparing sustainability reports {ReportId1} and {ReportId2}", reportId1, reportId2);

            var report1 = await _sustainabilityReportRepository.GetByIdAsync(reportId1);
            var report2 = await _sustainabilityReportRepository.GetByIdAsync(reportId2);

            if (report1 == null || report2 == null)
            {
                throw new ArgumentException("One or both reports not found");
            }

            return new ReportComparisonDto
            {
                Report1 = _mapper.Map<SustainabilityReportDto>(report1),
                Report2 = _mapper.Map<SustainabilityReportDto>(report2),
                ComparisonSummary = GenerateComparisonSummary(report1, report2),
                ComparedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing sustainability reports");
            throw;
        }
    }

    public async Task<ReportInsightsDto> GenerateSustainabilityReportInsightsAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Generating insights for sustainability report: {Id}", id);

            var report = await _sustainabilityReportRepository.GetByIdAsync(id);
            if (report == null)
            {
                throw new ArgumentException($"Sustainability report with ID {id} not found");
            }

            // Generate insights based on report data and historical trends
            var insights = new List<string>();
            var recommendations = new List<string>();

            // Add sample insights (in a real implementation, this would be more sophisticated)
            insights.Add("Carbon emissions have decreased by 15% compared to the previous period");
            insights.Add("Supplier ESG scores show improvement in governance metrics");
            insights.Add("Compliance alert resolution time has improved by 20%");

            recommendations.Add("Focus on renewable energy adoption to further reduce carbon footprint");
            recommendations.Add("Implement additional social responsibility programs");
            recommendations.Add("Enhance supplier audit frequency for high-risk categories");

            return new ReportInsightsDto
            {
                ReportId = id,
                KeyInsights = insights,
                Recommendations = recommendations,
                TrendAnalysis = "Overall positive trend in ESG performance metrics",
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sustainability report insights {Id}", id);
            throw;
        }
    }

    public async Task<SustainabilityReportDashboardDto> GetSustainabilityReportDashboardAsync()
    {
        try
        {
            _logger.LogInformation("Getting sustainability report dashboard data");

            var reports = await _sustainabilityReportRepository.GetAllAsync();

            var totalReports = reports.Count();
            var publishedReports = reports.Count(r => r.Status == "Published");
            var pendingApprovalReports = reports.Count(r => r.Status == "Generated" && r.ApprovedAt == null);
            var draftReports = reports.Count(r => r.Status == "Draft");

            return new SustainabilityReportDashboardDto
            {
                TotalReports = totalReports,
                PublishedReports = publishedReports,
                PendingApprovalReports = pendingApprovalReports,
                DraftReports = draftReports,
                RecentReports = _mapper.Map<List<SustainabilityReportDto>>(
                    reports.OrderByDescending(r => r.CreatedAt).Take(5).ToList())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sustainability report dashboard data");
            throw;
        }
    }

    private async Task GenerateReportContentAsync(SustainabilityReport report)
    {
        var content = new StringBuilder();
        
        content.AppendLine($"# {report.Title}");
        content.AppendLine($"**Report Type:** {report.ReportType}");
        content.AppendLine($"**Period:** {report.Period}");
        content.AppendLine($"**Generated:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        content.AppendLine();

        switch (report.ReportType.ToLower())
        {
            case "esgoverview":
                await GenerateESGOverviewContent(content);
                break;
            case "carbonfootprint":
                await GenerateCarbonFootprintContent(content);
                break;
            case "supplierperformance":
                await GenerateSupplierPerformanceContent(content);
                break;
            case "compliancesummary":
                await GenerateComplianceSummaryContent(content);
                break;
            default:
                content.AppendLine("## General Sustainability Report");
                content.AppendLine("This report provides an overview of sustainability metrics and performance.");
                break;
        }

        report.Content = content.ToString();
    }

    private async Task GenerateESGOverviewContent(StringBuilder content)
    {
        content.AppendLine("## ESG Overview");
        
        var suppliers = await _supplierRepository.GetAllAsync();
        var avgEnvironmentalScore = suppliers.Average(s => s.EnvironmentalScore);
        var avgSocialScore = suppliers.Average(s => s.SocialScore);
        var avgGovernanceScore = suppliers.Average(s => s.GovernanceScore);

        content.AppendLine($"**Average Environmental Score:** {avgEnvironmentalScore:F2}");
        content.AppendLine($"**Average Social Score:** {avgSocialScore:F2}");
        content.AppendLine($"**Average Governance Score:** {avgGovernanceScore:F2}");
        content.AppendLine();
        
        content.AppendLine("### Key Metrics");
        content.AppendLine($"- Total Suppliers: {suppliers.Count()}");
        content.AppendLine($"- High-Risk Suppliers: {suppliers.Count(s => s.RiskLevel == "High")}");
        content.AppendLine($"- Certified Suppliers: {suppliers.Count(s => !string.IsNullOrEmpty(s.Certifications))}");
    }

    private async Task GenerateCarbonFootprintContent(StringBuilder content)
    {
        content.AppendLine("## Carbon Footprint Analysis");
        
        var carbonFootprints = await _carbonFootprintRepository.GetAllAsync();
        var totalEmissions = carbonFootprints.Sum(cf => cf.TotalEmissions);
        var avgEmissions = carbonFootprints.Average(cf => cf.TotalEmissions);

        content.AppendLine($"**Total Carbon Emissions:** {totalEmissions:F2} CO2e");
        content.AppendLine($"**Average Emissions per Product:** {avgEmissions:F2} CO2e");
        content.AppendLine($"**Total Products Tracked:** {carbonFootprints.Count()}");
        content.AppendLine();
        
        content.AppendLine("### Emission Sources");
        content.AppendLine($"- Production: {carbonFootprints.Sum(cf => cf.ProductionEmissions):F2} CO2e");
        content.AppendLine($"- Transportation: {carbonFootprints.Sum(cf => cf.TransportationEmissions):F2} CO2e");
        content.AppendLine($"- Packaging: {carbonFootprints.Sum(cf => cf.PackagingEmissions):F2} CO2e");
    }

    private async Task GenerateSupplierPerformanceContent(StringBuilder content)
    {
        content.AppendLine("## Supplier Performance Analysis");
        
        var suppliers = await _supplierRepository.GetAllAsync();
        
        content.AppendLine("### Performance Summary");
        content.AppendLine($"- Total Suppliers: {suppliers.Count()}");
        content.AppendLine($"- Top Performers (ESG > 4.0): {suppliers.Count(s => (s.EnvironmentalScore + s.SocialScore + s.GovernanceScore) / 3 > 4.0)}");
        content.AppendLine($"- Needs Improvement (ESG < 3.0): {suppliers.Count(s => (s.EnvironmentalScore + s.SocialScore + s.GovernanceScore) / 3 < 3.0)}");
    }

    private async Task GenerateComplianceSummaryContent(StringBuilder content)
    {
        content.AppendLine("## Compliance Summary");
        
        var alerts = await _complianceAlertRepository.GetAllAsync();
        
        content.AppendLine("### Alert Summary");
        content.AppendLine($"- Total Alerts: {alerts.Count()}");
        content.AppendLine($"- Open Alerts: {alerts.Count(a => a.Status == "Open")}");
        content.AppendLine($"- Resolved Alerts: {alerts.Count(a => a.Status == "Resolved")}");
        content.AppendLine($"- Critical Alerts: {alerts.Count(a => a.Severity == "Critical")}");
    }

    private byte[] GeneratePdfContent(SustainabilityReport report)
    {
        // In a real implementation, you would use a PDF generation library like iTextSharp
        // For now, return the content as UTF-8 bytes
        return Encoding.UTF8.GetBytes($"PDF Report: {report.Title}\n\n{report.Content}");
    }

    private byte[] GenerateExcelContent(SustainabilityReport report)
    {
        // In a real implementation, you would use a library like EPPlus or ClosedXML
        // For now, return CSV-like content
        var csvContent = $"Report Title,{report.Title}\nReport Type,{report.ReportType}\nPeriod,{report.Period}\nGenerated,{report.CreatedAt}\n\nContent:\n{report.Content}";
        return Encoding.UTF8.GetBytes(csvContent);
    }

    private byte[] GenerateJsonContent(SustainabilityReport report)
    {
        var reportData = new
        {
            report.Id,
            report.Title,
            report.ReportType,
            report.Period,
            report.Content,
            report.CreatedAt,
            report.Status
        };
        
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(reportData, new JsonSerializerOptions { WriteIndented = true }));
    }

    private string GenerateComparisonSummary(SustainabilityReport report1, SustainabilityReport report2)
    {
        return $"Comparison between '{report1.Title}' ({report1.Period}) and '{report2.Title}' ({report2.Period}). " +
               $"Both reports are of type {report1.ReportType}. " +
               $"Report 1 was generated on {report1.CreatedAt:yyyy-MM-dd}, Report 2 on {report2.CreatedAt:yyyy-MM-dd}.";
    }
}