using AutoMapper;
using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.DTOs.CarbonFootprint;
using ESGMonitoring.Core.Entities;
using ESGMonitoring.Core.Interfaces;
using ESGMonitoring.API.Controllers;
using Microsoft.Extensions.Logging;

namespace ESGMonitoring.Core.Services;

public class CarbonFootprintService : ICarbonFootprintService
{
    private readonly ICarbonFootprintRepository _carbonFootprintRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CarbonFootprintService> _logger;

    public CarbonFootprintService(
        ICarbonFootprintRepository carbonFootprintRepository,
        ISupplierRepository supplierRepository,
        IMapper mapper,
        ILogger<CarbonFootprintService> logger)
    {
        _carbonFootprintRepository = carbonFootprintRepository;
        _supplierRepository = supplierRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<CarbonFootprintDto>> GetCarbonFootprintsAsync(CarbonFootprintFilterDto filter)
    {
        _logger.LogInformation("Retrieving carbon footprints with filter: {@Filter}", filter);

        var query = await _carbonFootprintRepository.GetAllAsync();
        var carbonFootprints = query.AsQueryable();

        // Apply filters
        if (filter.SupplierId.HasValue)
            carbonFootprints = carbonFootprints.Where(cf => cf.SupplierId == filter.SupplierId.Value);

        if (!string.IsNullOrEmpty(filter.ProductCategory))
            carbonFootprints = carbonFootprints.Where(cf => cf.ProductCategory.ToLower().Contains(filter.ProductCategory.ToLower()));

        if (!string.IsNullOrEmpty(filter.ProductName))
            carbonFootprints = carbonFootprints.Where(cf => cf.ProductName.ToLower().Contains(filter.ProductName.ToLower()));

        if (filter.AssessmentDateFrom.HasValue)
            carbonFootprints = carbonFootprints.Where(cf => cf.AssessmentDate >= filter.AssessmentDateFrom.Value);

        if (filter.AssessmentDateTo.HasValue)
            carbonFootprints = carbonFootprints.Where(cf => cf.AssessmentDate <= filter.AssessmentDateTo.Value);

        if (filter.MinEmissions.HasValue)
            carbonFootprints = carbonFootprints.Where(cf => cf.TotalEmissions >= filter.MinEmissions.Value);

        if (filter.MaxEmissions.HasValue)
            carbonFootprints = carbonFootprints.Where(cf => cf.TotalEmissions <= filter.MaxEmissions.Value);

        if (filter.IsVerified.HasValue)
            carbonFootprints = carbonFootprints.Where(cf => cf.IsVerified == filter.IsVerified.Value);

        // Apply sorting
        carbonFootprints = filter.SortBy?.ToLower() switch
        {
            "totalemissions" => filter.SortDescending ? 
                carbonFootprints.OrderByDescending(cf => cf.TotalEmissions) : 
                carbonFootprints.OrderBy(cf => cf.TotalEmissions),
            "assessmentdate" => filter.SortDescending ? 
                carbonFootprints.OrderByDescending(cf => cf.AssessmentDate) : 
                carbonFootprints.OrderBy(cf => cf.AssessmentDate),
            "productname" => filter.SortDescending ? 
                carbonFootprints.OrderByDescending(cf => cf.ProductName) : 
                carbonFootprints.OrderBy(cf => cf.ProductName),
            _ => carbonFootprints.OrderByDescending(cf => cf.AssessmentDate)
        };

        var totalCount = carbonFootprints.Count();
        var pagedData = carbonFootprints
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<CarbonFootprintDto>>(pagedData);

        return new PagedResult<CarbonFootprintDto>
        {
            Data = dtos,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
        };
    }

    public async Task<CarbonFootprintDto?> GetCarbonFootprintByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving carbon footprint with ID: {CarbonFootprintId}", id);

        var carbonFootprint = await _carbonFootprintRepository.GetByIdAsync(id);
        return carbonFootprint != null ? _mapper.Map<CarbonFootprintDto>(carbonFootprint) : null;
    }

    public async Task<CarbonFootprintDto> CreateCarbonFootprintAsync(CreateCarbonFootprintDto createDto)
    {
        _logger.LogInformation("Creating new carbon footprint for supplier: {SupplierId}", createDto.SupplierId);

        // Validate supplier exists
        var supplier = await _supplierRepository.GetByIdAsync(createDto.SupplierId);
        if (supplier == null)
            throw new KeyNotFoundException($"Supplier with ID '{createDto.SupplierId}' not found");

        var carbonFootprint = _mapper.Map<CarbonFootprint>(createDto);
        carbonFootprint.Id = Guid.NewGuid();
        carbonFootprint.CreatedAt = DateTime.UtcNow;
        carbonFootprint.UpdatedAt = DateTime.UtcNow;

        // Calculate total emissions
        carbonFootprint.TotalEmissions = 
            carbonFootprint.ProductionEmissions + 
            carbonFootprint.TransportationEmissions + 
            carbonFootprint.PackagingEmissions;

        await _carbonFootprintRepository.AddAsync(carbonFootprint);

        _logger.LogInformation("Successfully created carbon footprint: {CarbonFootprintId}", carbonFootprint.Id);

        return _mapper.Map<CarbonFootprintDto>(carbonFootprint);
    }

    public async Task<CarbonFootprintDto> UpdateCarbonFootprintAsync(Guid id, UpdateCarbonFootprintDto updateDto)
    {
        _logger.LogInformation("Updating carbon footprint: {CarbonFootprintId}", id);

        var carbonFootprint = await _carbonFootprintRepository.GetByIdAsync(id);
        if (carbonFootprint == null)
            throw new KeyNotFoundException($"Carbon footprint with ID '{id}' not found");

        _mapper.Map(updateDto, carbonFootprint);
        carbonFootprint.UpdatedAt = DateTime.UtcNow;

        // Recalculate total emissions
        carbonFootprint.TotalEmissions = 
            carbonFootprint.ProductionEmissions + 
            carbonFootprint.TransportationEmissions + 
            carbonFootprint.PackagingEmissions;

        await _carbonFootprintRepository.UpdateAsync(carbonFootprint);

        _logger.LogInformation("Successfully updated carbon footprint: {CarbonFootprintId}", id);

        return _mapper.Map<CarbonFootprintDto>(carbonFootprint);
    }

    public async Task DeleteCarbonFootprintAsync(Guid id)
    {
        _logger.LogInformation("Deleting carbon footprint: {CarbonFootprintId}", id);

        var carbonFootprint = await _carbonFootprintRepository.GetByIdAsync(id);
        if (carbonFootprint == null)
            throw new KeyNotFoundException($"Carbon footprint with ID '{id}' not found");

        await _carbonFootprintRepository.DeleteAsync(id);

        _logger.LogInformation("Successfully deleted carbon footprint: {CarbonFootprintId}", id);
    }

    public async Task<CarbonFootprintSummaryDto> GetCarbonFootprintSummaryAsync(CarbonFootprintFilterDto filter)
    {
        _logger.LogInformation("Generating carbon footprint summary with filter: {@Filter}", filter);

        var pagedResult = await GetCarbonFootprintsAsync(filter);
        var allData = pagedResult.Data;

        if (!allData.Any())
        {
            return new CarbonFootprintSummaryDto
            {
                TotalRecords = 0,
                TotalEmissions = 0,
                AverageEmissions = 0,
                MaxEmissions = 0,
                MinEmissions = 0,
                VerifiedRecords = 0,
                UnverifiedRecords = 0
            };
        }

        return new CarbonFootprintSummaryDto
        {
            TotalRecords = allData.Count,
            TotalEmissions = allData.Sum(cf => cf.TotalEmissions),
            AverageEmissions = allData.Average(cf => cf.TotalEmissions),
            MaxEmissions = allData.Max(cf => cf.TotalEmissions),
            MinEmissions = allData.Min(cf => cf.TotalEmissions),
            VerifiedRecords = allData.Count(cf => cf.IsVerified),
            UnverifiedRecords = allData.Count(cf => !cf.IsVerified),
            EmissionsByCategory = allData
                .GroupBy(cf => cf.ProductCategory)
                .ToDictionary(g => g.Key, g => g.Sum(cf => cf.TotalEmissions)),
            TopEmittingProducts = allData
                .GroupBy(cf => cf.ProductName)
                .OrderByDescending(g => g.Sum(cf => cf.TotalEmissions))
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Sum(cf => cf.TotalEmissions))
        };
    }

    public async Task<CarbonFootprintDto> VerifyCarbonFootprintAsync(Guid id, string verifierName)
    {
        _logger.LogInformation("Verifying carbon footprint: {CarbonFootprintId} by {Verifier}", id, verifierName);

        var carbonFootprint = await _carbonFootprintRepository.GetByIdAsync(id);
        if (carbonFootprint == null)
            throw new KeyNotFoundException($"Carbon footprint with ID '{id}' not found");

        carbonFootprint.IsVerified = true;
        carbonFootprint.VerifiedBy = verifierName;
        carbonFootprint.VerifiedAt = DateTime.UtcNow;
        carbonFootprint.UpdatedAt = DateTime.UtcNow;

        await _carbonFootprintRepository.UpdateAsync(carbonFootprint);

        _logger.LogInformation("Successfully verified carbon footprint: {CarbonFootprintId}", id);

        return _mapper.Map<CarbonFootprintDto>(carbonFootprint);
    }

    public async Task<CarbonFootprintTrendsDto> GetCarbonFootprintTrendsAsync(Guid? supplierId, int months)
    {
        _logger.LogInformation("Generating carbon footprint trends for supplier: {SupplierId}, months: {Months}", supplierId, months);

        var startDate = DateTime.UtcNow.AddMonths(-months);
        var carbonFootprints = await _carbonFootprintRepository.GetByDateRangeAsync(startDate, DateTime.UtcNow);

        if (supplierId.HasValue)
            carbonFootprints = carbonFootprints.Where(cf => cf.SupplierId == supplierId.Value);

        var monthlyData = carbonFootprints
            .GroupBy(cf => new { cf.AssessmentDate.Year, cf.AssessmentDate.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyEmissionDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalEmissions = g.Sum(cf => cf.TotalEmissions),
                RecordCount = g.Count(),
                AverageEmissions = g.Average(cf => cf.TotalEmissions)
            })
            .ToList();

        var totalEmissions = monthlyData.Sum(md => md.TotalEmissions);
        var averageMonthlyEmissions = monthlyData.Any() ? monthlyData.Average(md => md.TotalEmissions) : 0;

        // Calculate trend (simple linear regression slope)
        var trend = CalculateTrend(monthlyData.Select(md => md.TotalEmissions).ToList());

        return new CarbonFootprintTrendsDto
        {
            SupplierId = supplierId,
            PeriodMonths = months,
            MonthlyData = monthlyData,
            TotalEmissions = totalEmissions,
            AverageMonthlyEmissions = averageMonthlyEmissions,
            TrendDirection = trend > 0 ? "Increasing" : trend < 0 ? "Decreasing" : "Stable",
            TrendPercentage = Math.Abs(trend * 100)
        };
    }

    private static decimal CalculateTrend(List<decimal> values)
    {
        if (values.Count < 2) return 0;

        var n = values.Count;
        var sumX = n * (n + 1) / 2; // Sum of 1, 2, 3, ..., n
        var sumY = values.Sum();
        var sumXY = values.Select((y, i) => (decimal)(i + 1) * y).Sum();
        var sumX2 = n * (n + 1) * (2 * n + 1) / 6; // Sum of 1², 2², 3², ..., n²

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        return slope;
    }
}