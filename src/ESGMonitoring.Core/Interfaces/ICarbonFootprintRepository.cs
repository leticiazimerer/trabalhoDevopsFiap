using ESGMonitoring.Core.Entities;

namespace ESGMonitoring.Core.Interfaces;

public interface ICarbonFootprintRepository : IRepository<CarbonFootprint>
{
    Task<IEnumerable<CarbonFootprint>> GetBySupplierIdAsync(Guid supplierId);
    Task<IEnumerable<CarbonFootprint>> GetByProductNameAsync(string productName);
    Task<IEnumerable<CarbonFootprint>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<CarbonFootprint>> GetVerifiedFootprintsAsync();
    Task<IEnumerable<CarbonFootprint>> GetUnverifiedFootprintsAsync();
    Task<decimal> GetTotalEmissionsBySupplierAsync(Guid supplierId);
    Task<decimal> GetAverageEmissionsAsync();
}