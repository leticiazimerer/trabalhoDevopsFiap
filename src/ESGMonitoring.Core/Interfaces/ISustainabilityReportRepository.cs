using ESGMonitoring.Core.Entities;

namespace ESGMonitoring.Core.Interfaces;

public interface ISustainabilityReportRepository : IRepository<SustainabilityReport>
{
    Task<IEnumerable<SustainabilityReport>> GetByReportTypeAsync(string reportType);
    Task<IEnumerable<SustainabilityReport>> GetByStatusAsync(string status);
    Task<IEnumerable<SustainabilityReport>> GetByPeriodAsync(string period);
    Task<IEnumerable<SustainabilityReport>> GetPublishedReportsAsync();
    Task<IEnumerable<SustainabilityReport>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate);
}