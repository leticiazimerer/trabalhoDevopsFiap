using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.DTOs.Supplier;

namespace ESGMonitoring.Core.Interfaces;

public interface ISupplierService
{
    Task<PagedResult<SupplierDto>> GetSuppliersAsync(SupplierFilterDto filter);
    Task<SupplierDto?> GetSupplierByIdAsync(Guid id);
    Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createDto);
    Task<SupplierDto> UpdateSupplierAsync(Guid id, UpdateSupplierDto updateDto);
    Task DeleteSupplierAsync(Guid id);
    Task<IEnumerable<SupplierDto>> GetSuppliersWithUpcomingAuditsAsync(int daysAhead = 30);
    Task<IEnumerable<SupplierDto>> GetSuppliersByRiskLevelAsync(string riskLevel);
    Task<SupplierDashboardDto> GetSupplierDashboardAsync();
    Task<bool> ValidateSupplierAsync(Guid id);
    Task<SupplierESGAnalysisDto> GetSupplierESGAnalysisAsync(Guid id);
}

public class SupplierDashboardDto
{
    public int TotalSuppliers { get; set; }
    public int CompliantSuppliers { get; set; }
    public int NonCompliantSuppliers { get; set; }
    public decimal AverageESGScore { get; set; }
    public Dictionary<string, int> SuppliersByRiskLevel { get; set; } = new();
    public int UpcomingAudits { get; set; }
    public int OverdueAudits { get; set; }
    public IEnumerable<SupplierDto> TopPerformingSuppliers { get; set; } = new List<SupplierDto>();
    public IEnumerable<SupplierDto> SuppliersRequiringAttention { get; set; } = new List<SupplierDto>();
}

public class SupplierESGAnalysisDto
{
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal EnvironmentalScore { get; set; }
    public decimal SocialScore { get; set; }
    public decimal GovernanceScore { get; set; }
    public decimal OverallESGScore { get; set; }
    public string ESGRating { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public Dictionary<string, decimal> BenchmarkComparison { get; set; } = new();
    public List<ESGTrendDto> ESGTrends { get; set; } = new();
}