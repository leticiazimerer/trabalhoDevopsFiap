using ESGMonitoring.Core.Entities;
using ESGMonitoring.Core.Interfaces;
using ESGMonitoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ESGMonitoring.Infrastructure.Repositories;

public class ComplianceAlertRepository : Repository<ComplianceAlert>, IComplianceAlertRepository
{
    public ComplianceAlertRepository(ESGMonitoringDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ComplianceAlert>> GetBySupplierIdAsync(Guid supplierId)
    {
        return await _context.ComplianceAlerts
            .Where(ca => ca.SupplierId == supplierId)
            .Include(ca => ca.Supplier)
            .OrderByDescending(ca => ca.DetectedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ComplianceAlert>> GetBySeverityAsync(string severity)
    {
        return await _context.ComplianceAlerts
            .Where(ca => ca.Severity.ToLower() == severity.ToLower())
            .Include(ca => ca.Supplier)
            .OrderByDescending(ca => ca.DetectedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ComplianceAlert>> GetByStatusAsync(string status)
    {
        return await _context.ComplianceAlerts
            .Where(ca => ca.Status.ToLower() == status.ToLower())
            .Include(ca => ca.Supplier)
            .OrderByDescending(ca => ca.DetectedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ComplianceAlert>> GetByCategoryAsync(string category)
    {
        return await _context.ComplianceAlerts
            .Where(ca => ca.Category.ToLower() == category.ToLower())
            .Include(ca => ca.Supplier)
            .OrderByDescending(ca => ca.DetectedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ComplianceAlert>> GetOverdueAlertsAsync()
    {
        var currentDate = DateTime.UtcNow;
        return await _context.ComplianceAlerts
            .Where(ca => ca.DueDate < currentDate && 
                        ca.Status != "Resolved" && 
                        ca.Status != "Closed")
            .Include(ca => ca.Supplier)
            .OrderBy(ca => ca.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ComplianceAlert>> GetCriticalAlertsAsync(int daysOverdue = 0)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-daysOverdue);
        return await _context.ComplianceAlerts
            .Where(ca => (ca.Severity == "Critical" || ca.Severity == "High") &&
                        (ca.Status == "Open" || ca.Status == "In Progress") &&
                        (daysOverdue == 0 || ca.DueDate < thresholdDate))
            .Include(ca => ca.Supplier)
            .OrderBy(ca => ca.DueDate)
            .ThenByDescending(ca => ca.Severity)
            .ToListAsync();
    }

    public async Task<IEnumerable<ComplianceAlert>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ComplianceAlerts
            .Where(ca => ca.DetectedAt >= startDate && ca.DetectedAt <= endDate)
            .Include(ca => ca.Supplier)
            .OrderByDescending(ca => ca.DetectedAt)
            .ToListAsync();
    }

    public async Task<int> GetAlertCountBySupplierAsync(Guid supplierId, string? severity = null)
    {
        var query = _context.ComplianceAlerts
            .Where(ca => ca.SupplierId == supplierId);

        if (!string.IsNullOrEmpty(severity))
            query = query.Where(ca => ca.Severity.ToLower() == severity.ToLower());

        return await query.CountAsync();
    }

    public async Task<decimal> GetResolutionRateAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ComplianceAlerts.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(ca => ca.DetectedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(ca => ca.DetectedAt <= endDate.Value);

        var totalAlerts = await query.CountAsync();
        if (totalAlerts == 0) return 0;

        var resolvedAlerts = await query
            .Where(ca => ca.Status == "Resolved" || ca.Status == "Closed")
            .CountAsync();

        return Math.Round((decimal)resolvedAlerts / totalAlerts * 100, 2);
    }

    public async Task<double> GetAverageResolutionTimeAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ComplianceAlerts
            .Where(ca => ca.ResolvedAt.HasValue);

        if (startDate.HasValue)
            query = query.Where(ca => ca.DetectedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(ca => ca.DetectedAt <= endDate.Value);

        var resolvedAlerts = await query
            .Select(ca => new { ca.DetectedAt, ca.ResolvedAt })
            .ToListAsync();

        if (!resolvedAlerts.Any()) return 0;

        var totalHours = resolvedAlerts
            .Where(ra => ra.ResolvedAt.HasValue)
            .Sum(ra => (ra.ResolvedAt!.Value - ra.DetectedAt).TotalHours);

        return Math.Round(totalHours / resolvedAlerts.Count, 2);
    }
}