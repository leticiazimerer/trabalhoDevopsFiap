using ESGMonitoring.Core.Entities;
using ESGMonitoring.Core.Interfaces;
using ESGMonitoring.Core.DTOs.Supplier;
using ESGMonitoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ESGMonitoring.Infrastructure.Repositories;

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(ESGMonitoringDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByTaxIdAsync(string taxId, Guid? excludeId = null)
    {
        var query = _dbSet.Where(s => s.TaxId == taxId && s.IsActive);
        
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Supplier>> GetSuppliersWithUpcomingAuditsAsync(int daysAhead = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        
        return await _dbSet
            .Where(s => s.IsActive && s.NextAuditDate <= cutoffDate)
            .OrderBy(s => s.NextAuditDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Supplier>> GetSuppliersByRiskLevelAsync(SupplierRiskLevel riskLevel)
    {
        return await _dbSet
            .Where(s => s.IsActive && s.RiskLevel == riskLevel)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Supplier> Items, int TotalCount)> GetFilteredSuppliersAsync(SupplierFilterDto filter)
    {
        var query = _dbSet.Where(s => s.IsActive);

        // Apply filters
        if (filter.RiskLevel.HasValue)
        {
            query = query.Where(s => s.RiskLevel == filter.RiskLevel.Value);
        }

        if (filter.MinESGScore.HasValue)
        {
            query = query.Where(s => (s.EnvironmentalScore + s.SocialScore + s.GovernanceScore) / 3 >= filter.MinESGScore.Value);
        }

        if (filter.MaxESGScore.HasValue)
        {
            query = query.Where(s => (s.EnvironmentalScore + s.SocialScore + s.GovernanceScore) / 3 <= filter.MaxESGScore.Value);
        }

        if (filter.HasRenewableEnergy.HasValue)
        {
            query = query.Where(s => s.HasRenewableEnergyProgram == filter.HasRenewableEnergy.Value);
        }

        if (filter.HasFairLaborCertification.HasValue)
        {
            query = query.Where(s => s.HasFairLaborCertification == filter.HasFairLaborCertification.Value);
        }

        if (filter.AuditDueBefore.HasValue)
        {
            query = query.Where(s => s.NextAuditDate <= filter.AuditDueBefore.Value);
        }

        if (!string.IsNullOrEmpty(filter.Certification))
        {
            query = query.Where(s => s.Certifications.Contains(filter.Certification));
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(s => 
                s.Name.ToLower().Contains(searchTerm) ||
                s.TaxId.Contains(searchTerm) ||
                s.ContactEmail.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDescending);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Dictionary<SupplierRiskLevel, int>> GetSupplierCountByRiskLevelAsync()
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .GroupBy(s => s.RiskLevel)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<decimal> GetAverageESGScoreAsync()
    {
        var suppliers = await _dbSet
            .Where(s => s.IsActive)
            .Select(s => new { s.EnvironmentalScore, s.SocialScore, s.GovernanceScore })
            .ToListAsync();

        if (!suppliers.Any())
            return 0;

        return suppliers.Average(s => (s.EnvironmentalScore + s.SocialScore + s.GovernanceScore) / 3);
    }

    public async Task<IEnumerable<Supplier>> GetTopPerformingSuppliersAsync(int count = 10)
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderByDescending(s => (s.EnvironmentalScore + s.SocialScore + s.GovernanceScore) / 3)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Supplier>> GetSuppliersRequiringAttentionAsync()
    {
        var now = DateTime.UtcNow;
        
        return await _dbSet
            .Where(s => s.IsActive && (
                s.RiskLevel >= SupplierRiskLevel.High ||
                s.NextAuditDate < now ||
                (s.EnvironmentalScore + s.SocialScore + s.GovernanceScore) / 3 < 50
            ))
            .OrderByDescending(s => s.RiskLevel)
            .ThenBy(s => s.NextAuditDate)
            .ToListAsync();
    }

    private static IQueryable<Supplier> ApplySorting(IQueryable<Supplier> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return query.OrderBy(s => s.Name);
        }

        var sortExpression = sortBy.ToLower() switch
        {
            "name" => sortDescending 
                ? query.OrderByDescending(s => s.Name)
                : query.OrderBy(s => s.Name),
            "esgscore" => sortDescending
                ? query.OrderByDescending(s => (s.EnvironmentalScore + s.SocialScore + s.GovernanceScore) / 3)
                : query.OrderBy(s => (s.EnvironmentalScore + s.SocialScore + s.GovernanceScore) / 3),
            "risklevel" => sortDescending
                ? query.OrderByDescending(s => s.RiskLevel)
                : query.OrderBy(s => s.RiskLevel),
            "nextauditdate" => sortDescending
                ? query.OrderByDescending(s => s.NextAuditDate)
                : query.OrderBy(s => s.NextAuditDate),
            "createdat" => sortDescending
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt),
            _ => query.OrderBy(s => s.Name)
        };

        return sortExpression;
    }
}