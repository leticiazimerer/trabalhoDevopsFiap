using ESGMonitoring.Core.Entities;
using ESGMonitoring.Core.Interfaces;
using ESGMonitoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ESGMonitoring.Infrastructure.Repositories;

public class CarbonFootprintRepository : Repository<CarbonFootprint>, ICarbonFootprintRepository
{
    public CarbonFootprintRepository(ESGMonitoringDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CarbonFootprint>> GetBySupplierIdAsync(Guid supplierId)
    {
        return await _context.CarbonFootprints
            .Where(cf => cf.SupplierId == supplierId)
            .Include(cf => cf.Supplier)
            .OrderByDescending(cf => cf.AssessmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<CarbonFootprint>> GetByProductCategoryAsync(string productCategory)
    {
        return await _context.CarbonFootprints
            .Where(cf => cf.ProductCategory.ToLower().Contains(productCategory.ToLower()))
            .Include(cf => cf.Supplier)
            .OrderByDescending(cf => cf.AssessmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<CarbonFootprint>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.CarbonFootprints
            .Where(cf => cf.AssessmentDate >= startDate && cf.AssessmentDate <= endDate)
            .Include(cf => cf.Supplier)
            .OrderByDescending(cf => cf.AssessmentDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalEmissionsBySupplierAsync(Guid supplierId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.CarbonFootprints
            .Where(cf => cf.SupplierId == supplierId);

        if (startDate.HasValue)
            query = query.Where(cf => cf.AssessmentDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(cf => cf.AssessmentDate <= endDate.Value);

        return await query.SumAsync(cf => cf.TotalEmissions);
    }

    public async Task<decimal> GetAverageEmissionsByProductAsync(string productName)
    {
        var emissions = await _context.CarbonFootprints
            .Where(cf => cf.ProductName.ToLower().Contains(productName.ToLower()))
            .Select(cf => cf.TotalEmissions)
            .ToListAsync();

        return emissions.Any() ? emissions.Average() : 0;
    }

    public async Task<IEnumerable<CarbonFootprint>> GetUnverifiedFootprintsAsync()
    {
        return await _context.CarbonFootprints
            .Where(cf => !cf.IsVerified)
            .Include(cf => cf.Supplier)
            .OrderByDescending(cf => cf.AssessmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<CarbonFootprint>> GetFootprintsAboveThresholdAsync(decimal threshold)
    {
        return await _context.CarbonFootprints
            .Where(cf => cf.TotalEmissions > threshold)
            .Include(cf => cf.Supplier)
            .OrderByDescending(cf => cf.TotalEmissions)
            .ToListAsync();
    }
}