using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.DTOs.Supplier;
using ESGMonitoring.Core.Entities;
using ESGMonitoring.Core.Interfaces;
using AutoMapper;

namespace ESGMonitoring.Core.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public SupplierService(ISupplierRepository supplierRepository, IMapper mapper)
    {
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<SupplierDto>> GetSuppliersAsync(SupplierFilterDto filter)
    {
        var (items, totalCount) = await _supplierRepository.GetFilteredSuppliersAsync(filter);
        var supplierDtos = _mapper.Map<IEnumerable<SupplierDto>>(items);

        return new PagedResult<SupplierDto>
        {
            Data = supplierDtos,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(Guid id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        return supplier != null ? _mapper.Map<SupplierDto>(supplier) : null;
    }

    public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createDto)
    {
        // Validate unique tax ID
        if (await _supplierRepository.ExistsByTaxIdAsync(createDto.TaxId))
        {
            throw new InvalidOperationException($"Supplier with Tax ID '{createDto.TaxId}' already exists.");
        }

        var supplier = _mapper.Map<Supplier>(createDto);
        
        // Business logic for risk assessment
        supplier.RiskLevel = CalculateRiskLevel(supplier);
        
        var createdSupplier = await _supplierRepository.AddAsync(supplier);
        return _mapper.Map<SupplierDto>(createdSupplier);
    }

    public async Task<SupplierDto> UpdateSupplierAsync(Guid id, UpdateSupplierDto updateDto)
    {
        var existingSupplier = await _supplierRepository.GetByIdAsync(id);
        if (existingSupplier == null)
        {
            throw new KeyNotFoundException($"Supplier with ID '{id}' not found.");
        }

        _mapper.Map(updateDto, existingSupplier);
        
        // Recalculate risk level based on updated data
        existingSupplier.RiskLevel = CalculateRiskLevel(existingSupplier);
        
        var updatedSupplier = await _supplierRepository.UpdateAsync(existingSupplier);
        return _mapper.Map<SupplierDto>(updatedSupplier);
    }

    public async Task DeleteSupplierAsync(Guid id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"Supplier with ID '{id}' not found.");
        }

        await _supplierRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<SupplierDto>> GetSuppliersWithUpcomingAuditsAsync(int daysAhead = 30)
    {
        var suppliers = await _supplierRepository.GetSuppliersWithUpcomingAuditsAsync(daysAhead);
        return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
    }

    public async Task<IEnumerable<SupplierDto>> GetSuppliersByRiskLevelAsync(string riskLevel)
    {
        if (!Enum.TryParse<SupplierRiskLevel>(riskLevel, true, out var parsedRiskLevel))
        {
            throw new ArgumentException($"Invalid risk level: {riskLevel}");
        }

        var suppliers = await _supplierRepository.GetSuppliersByRiskLevelAsync(parsedRiskLevel);
        return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
    }

    public async Task<SupplierDashboardDto> GetSupplierDashboardAsync()
    {
        var totalSuppliers = await _supplierRepository.CountAsync();
        var averageESGScore = await _supplierRepository.GetAverageESGScoreAsync();
        var riskLevelCounts = await _supplierRepository.GetSupplierCountByRiskLevelAsync();
        var topPerformingSuppliers = await _supplierRepository.GetTopPerformingSuppliersAsync(5);
        var suppliersRequiringAttention = await _supplierRepository.GetSuppliersRequiringAttentionAsync();
        var upcomingAudits = await _supplierRepository.GetSuppliersWithUpcomingAuditsAsync(30);
        var overdueAudits = await _supplierRepository.GetSuppliersWithUpcomingAuditsAsync(-1);

        var compliantSuppliers = await _supplierRepository.CountAsync(s => 
            (s.EnvironmentalScore + s.SocialScore + s.GovernanceScore) / 3 >= 70 &&
            s.RiskLevel <= SupplierRiskLevel.Medium);

        return new SupplierDashboardDto
        {
            TotalSuppliers = totalSuppliers,
            CompliantSuppliers = compliantSuppliers,
            NonCompliantSuppliers = totalSuppliers - compliantSuppliers,
            AverageESGScore = averageESGScore,
            SuppliersByRiskLevel = riskLevelCounts.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
            UpcomingAudits = upcomingAudits.Count(),
            OverdueAudits = overdueAudits.Count(),
            TopPerformingSuppliers = _mapper.Map<IEnumerable<SupplierDto>>(topPerformingSuppliers),
            SuppliersRequiringAttention = _mapper.Map<IEnumerable<SupplierDto>>(suppliersRequiringAttention)
        };
    }

    public async Task<bool> ValidateSupplierAsync(Guid id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null) return false;

        // Comprehensive validation logic
        var validationCriteria = new[]
        {
            supplier.EnvironmentalScore >= 50,
            supplier.SocialScore >= 50,
            supplier.GovernanceScore >= 50,
            supplier.HasChildLaborPolicy,
            supplier.HasSafeWorkingConditions,
            supplier.NextAuditDate > DateTime.UtcNow,
            supplier.RiskLevel <= SupplierRiskLevel.Medium
        };

        return validationCriteria.Count(c => c) >= 5; // At least 5 out of 7 criteria must be met
    }

    public async Task<SupplierESGAnalysisDto> GetSupplierESGAnalysisAsync(Guid id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"Supplier with ID '{id}' not found.");
        }

        var overallScore = (supplier.EnvironmentalScore + supplier.SocialScore + supplier.GovernanceScore) / 3;
        var averageESGScore = await _supplierRepository.GetAverageESGScoreAsync();

        var analysis = new SupplierESGAnalysisDto
        {
            SupplierId = supplier.Id,
            SupplierName = supplier.Name,
            EnvironmentalScore = supplier.EnvironmentalScore,
            SocialScore = supplier.SocialScore,
            GovernanceScore = supplier.GovernanceScore,
            OverallESGScore = overallScore,
            ESGRating = GetESGRating(overallScore),
            Strengths = GetStrengths(supplier),
            ImprovementAreas = GetImprovementAreas(supplier),
            Recommendations = GetRecommendations(supplier),
            BenchmarkComparison = new Dictionary<string, decimal>
            {
                { "Industry Average", averageESGScore },
                { "Your Score", overallScore },
                { "Difference", overallScore - averageESGScore }
            }
        };

        return analysis;
    }

    private static SupplierRiskLevel CalculateRiskLevel(Supplier supplier)
    {
        var overallESGScore = (supplier.EnvironmentalScore + supplier.SocialScore + supplier.GovernanceScore) / 3;
        var riskFactors = 0;

        // ESG Score Risk
        if (overallESGScore < 40) riskFactors += 3;
        else if (overallESGScore < 60) riskFactors += 2;
        else if (overallESGScore < 80) riskFactors += 1;

        // Certification Risk
        if (string.IsNullOrEmpty(supplier.Certifications)) riskFactors += 1;

        // Labor Practice Risk
        if (!supplier.HasFairLaborCertification) riskFactors += 1;
        if (!supplier.HasChildLaborPolicy) riskFactors += 2;
        if (!supplier.HasSafeWorkingConditions) riskFactors += 2;

        // Environmental Risk
        if (!supplier.HasRenewableEnergyProgram) riskFactors += 1;
        if (!supplier.HasCarbonNeutralityPlan) riskFactors += 1;

        // Audit Risk
        if (supplier.NextAuditDate < DateTime.UtcNow) riskFactors += 2;

        return riskFactors switch
        {
            <= 2 => SupplierRiskLevel.Low,
            <= 5 => SupplierRiskLevel.Medium,
            <= 8 => SupplierRiskLevel.High,
            _ => SupplierRiskLevel.Critical
        };
    }

    private static string GetESGRating(decimal score)
    {
        return score switch
        {
            >= 90 => "A+",
            >= 80 => "A",
            >= 70 => "B+",
            >= 60 => "B",
            >= 50 => "C+",
            >= 40 => "C",
            >= 30 => "D",
            _ => "F"
        };
    }

    private static List<string> GetStrengths(Supplier supplier)
    {
        var strengths = new List<string>();

        if (supplier.EnvironmentalScore >= 80)
            strengths.Add("Excellent environmental performance");
        if (supplier.SocialScore >= 80)
            strengths.Add("Strong social responsibility practices");
        if (supplier.GovernanceScore >= 80)
            strengths.Add("Robust governance framework");
        if (supplier.HasRenewableEnergyProgram)
            strengths.Add("Renewable energy program in place");
        if (supplier.HasFairLaborCertification)
            strengths.Add("Fair labor certification achieved");
        if (supplier.HasCarbonNeutralityPlan)
            strengths.Add("Carbon neutrality plan implemented");

        return strengths;
    }

    private static List<string> GetImprovementAreas(Supplier supplier)
    {
        var areas = new List<string>();

        if (supplier.EnvironmentalScore < 60)
            areas.Add("Environmental performance needs improvement");
        if (supplier.SocialScore < 60)
            areas.Add("Social responsibility practices require attention");
        if (supplier.GovernanceScore < 60)
            areas.Add("Governance framework needs strengthening");
        if (!supplier.HasRenewableEnergyProgram)
            areas.Add("Implement renewable energy program");
        if (!supplier.HasFairLaborCertification)
            areas.Add("Obtain fair labor certification");
        if (!supplier.HasCarbonNeutralityPlan)
            areas.Add("Develop carbon neutrality plan");

        return areas;
    }

    private static List<string> GetRecommendations(Supplier supplier)
    {
        var recommendations = new List<string>();

        if (supplier.EnvironmentalScore < 70)
            recommendations.Add("Invest in environmental management systems and green technologies");
        if (supplier.SocialScore < 70)
            recommendations.Add("Enhance worker safety programs and community engagement initiatives");
        if (supplier.GovernanceScore < 70)
            recommendations.Add("Strengthen board oversight and transparency reporting");
        if (supplier.RiskLevel >= SupplierRiskLevel.High)
            recommendations.Add("Immediate action required to address high-risk factors");
        if (supplier.NextAuditDate < DateTime.UtcNow.AddMonths(3))
            recommendations.Add("Schedule comprehensive ESG audit within the next quarter");

        return recommendations;
    }
}

public class ESGTrendDto
{
    public DateTime Date { get; set; }
    public decimal EnvironmentalScore { get; set; }
    public decimal SocialScore { get; set; }
    public decimal GovernanceScore { get; set; }
    public decimal OverallScore { get; set; }
}