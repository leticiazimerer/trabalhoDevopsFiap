using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.DTOs.CarbonFootprint;
using ESGMonitoring.API.Controllers;

namespace ESGMonitoring.Core.Interfaces;

public interface ICarbonFootprintService
{
    Task<PagedResult<CarbonFootprintDto>> GetCarbonFootprintsAsync(CarbonFootprintFilterDto filter);
    Task<CarbonFootprintDto?> GetCarbonFootprintByIdAsync(Guid id);
    Task<CarbonFootprintDto> CreateCarbonFootprintAsync(CreateCarbonFootprintDto createDto);
    Task<CarbonFootprintDto> UpdateCarbonFootprintAsync(Guid id, UpdateCarbonFootprintDto updateDto);
    Task DeleteCarbonFootprintAsync(Guid id);
    Task<CarbonFootprintSummaryDto> GetCarbonFootprintSummaryAsync(CarbonFootprintFilterDto filter);
    Task<CarbonFootprintDto> VerifyCarbonFootprintAsync(Guid id, string verifierName);
    Task<CarbonFootprintTrendsDto> GetCarbonFootprintTrendsAsync(Guid? supplierId, int months);
}