using ESGMonitoring.Core.Entities;

namespace ESGMonitoring.Core.Interfaces;

public interface IComplianceAlertRepository : IRepository<ComplianceAlert>
{
    Task<IEnumerable<ComplianceAlert>> GetBySupplierIdAsync(Guid supplierId);
    Task<IEnumerable<ComplianceAlert>> GetBySeverityAsync(string severity);
    Task<IEnumerable<ComplianceAlert>> GetByStatusAsync(string status);
    Task<IEnumerable<ComplianceAlert>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ComplianceAlert>> GetCriticalAlertsAsync();
    Task<IEnumerable<ComplianceAlert>> GetUnresolvedAlertsAsync();
}