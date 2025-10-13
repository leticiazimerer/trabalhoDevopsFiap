using ESGMonitoring.Core.Entities;
using ESGMonitoring.Core.Interfaces;
using ESGMonitoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ESGMonitoring.Infrastructure.Repositories;

public class SustainabilityReportRepository : Repository<SustainabilityReport>, ISustainabilityReportRepository
{
    public SustainabilityReportRepository(ESGMonitoringDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SustainabilityReport>> GetByReportTypeAsync(string reportType)
    {
        return await _context.SustainabilityReports
            .Where(sr => sr.ReportType.ToLower() == reportType.ToLower())
            .OrderByDescending(sr => sr.GeneratedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SustainabilityReport>> GetByPeriodAsync(string period)
    {
        return await _context.SustainabilityReports
            .Where(sr => sr.Period.ToLower() == period.ToLower())
            .OrderByDescending(sr => sr.GeneratedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SustainabilityReport>> GetByStatusAsync(string status)
    {
        return await _context.SustainabilityReports
            .Where(sr => sr.Status.ToLower() == status.ToLower())
            .OrderByDescending(sr => sr.GeneratedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SustainabilityReport>> GetByGeneratedByAsync(string generatedBy)
    {
        return await _context.SustainabilityReports
            .Where(sr => sr.GeneratedBy.ToLower().Contains(generatedBy.ToLower()))
            .OrderByDescending(sr => sr.GeneratedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SustainabilityReport>> GetPublishedReportsAsync()
    {
        return await _context.SustainabilityReports
            .Where(sr => sr.PublishedAt.HasValue)
            .OrderByDescending(sr => sr.PublishedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SustainabilityReport>> GetPendingApprovalReportsAsync()
    {
        return await _context.SustainabilityReports
            .Where(sr => sr.RequiresApproval && !sr.ApprovedAt.HasValue)
            .OrderBy(sr => sr.GeneratedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SustainabilityReport>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.SustainabilityReports
            .Where(sr => sr.GeneratedAt >= startDate && sr.GeneratedAt <= endDate)
            .OrderByDescending(sr => sr.GeneratedAt)
            .ToListAsync();
    }

    public async Task<SustainabilityReport?> GetLatestReportByTypeAsync(string reportType)
    {
        return await _context.SustainabilityReports
            .Where(sr => sr.ReportType.ToLower() == reportType.ToLower())
            .OrderByDescending(sr => sr.GeneratedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetReportCountByTypeAsync(string reportType)
    {
        return await _context.SustainabilityReports
            .Where(sr => sr.ReportType.ToLower() == reportType.ToLower())
            .CountAsync();
    }

    public async Task<IEnumerable<SustainabilityReport>> GetReportsForSupplierAsync(Guid supplierId)
    {
        return await _context.SustainabilityReports
            .Where(sr => sr.SupplierIds.Contains(supplierId.ToString()))
            .OrderByDescending(sr => sr.GeneratedAt)
            .ToListAsync();
    }
}