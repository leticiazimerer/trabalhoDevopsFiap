using ESGMonitoring.Core.Entities;
using ESGMonitoring.Core.DTOs.Supplier;

namespace ESGMonitoring.Core.Interfaces;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<bool> ExistsByTaxIdAsync(string taxId, Guid? excludeId = null);
    Task<IEnumerable<Supplier>> GetSuppliersWithUpcomingAuditsAsync(int daysAhead = 30);
    Task<IEnumerable<Supplier>> GetSuppliersByRiskLevelAsync(SupplierRiskLevel riskLevel);
    Task<(IEnumerable<Supplier> Items, int TotalCount)> GetFilteredSuppliersAsync(SupplierFilterDto filter);
    Task<Dictionary<SupplierRiskLevel, int>> GetSupplierCountByRiskLevelAsync();
    Task<decimal> GetAverageESGScoreAsync();
    Task<IEnumerable<Supplier>> GetTopPerformingSuppliersAsync(int count = 10);
    Task<IEnumerable<Supplier>> GetSuppliersRequiringAttentionAsync();
}