using ESGMonitoring.Core.DTOs.CarbonFootprint;
using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.DTOs.CarbonFootprint;
using ESGMonitoring.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ESGMonitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CarbonFootprintController : ControllerBase
{
    private readonly ICarbonFootprintService _carbonFootprintService;
    private readonly ILogger<CarbonFootprintController> _logger;

    public CarbonFootprintController(ICarbonFootprintService carbonFootprintService, ILogger<CarbonFootprintController> logger)
    {
        _carbonFootprintService = carbonFootprintService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves paginated carbon footprint records with advanced filtering and analytics
    /// </summary>
    /// <param name="filter">Filter parameters including supplier, product category, date range, and emissions thresholds</param>
    /// <returns>Paginated list of carbon footprint records with detailed emission breakdowns</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CarbonFootprintDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PagedResult<CarbonFootprintDto>>> GetCarbonFootprints([FromQuery] CarbonFootprintFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Retrieving carbon footprints with filter: {@Filter}", filter);
            
            var result = await _carbonFootprintService.GetCarbonFootprintsAsync(filter);
            
            _logger.LogInformation("Retrieved {Count} carbon footprint records out of {Total}", 
                result.Data.Count(), result.TotalCount);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving carbon footprints");
            return BadRequest($"Error retrieving carbon footprints: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves detailed carbon footprint information for a specific record
    /// </summary>
    /// <param name="id">Carbon footprint record unique identifier</param>
    /// <returns>Detailed carbon footprint data including verification status and emission sources</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CarbonFootprintDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<CarbonFootprintDto>> GetCarbonFootprint(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving carbon footprint with ID: {CarbonFootprintId}", id);
            
            var carbonFootprint = await _carbonFootprintService.GetCarbonFootprintByIdAsync(id);
            
            if (carbonFootprint == null)
            {
                _logger.LogWarning("Carbon footprint not found: {CarbonFootprintId}", id);
                return NotFound($"Carbon footprint with ID '{id}' not found");
            }

            return Ok(carbonFootprint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving carbon footprint {CarbonFootprintId}", id);
            return BadRequest($"Error retrieving carbon footprint: {ex.Message}");
        }
    }

    /// <summary>
    /// Records new carbon footprint data with automatic emission calculations and validation
    /// </summary>
    /// <param name="createDto">Carbon footprint data including production, transportation, and packaging emissions</param>
    /// <returns>Created carbon footprint record with calculated total emissions and efficiency metrics</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CarbonFootprintDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<CarbonFootprintDto>> CreateCarbonFootprint([FromBody] CreateCarbonFootprintDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating new carbon footprint record for supplier: {SupplierId}, Product: {ProductName}", 
                createDto.SupplierId, createDto.ProductName);
            
            var carbonFootprint = await _carbonFootprintService.CreateCarbonFootprintAsync(createDto);
            
            _logger.LogInformation("Successfully created carbon footprint record: {CarbonFootprintId} with total emissions: {TotalEmissions} kg CO2e", 
                carbonFootprint.Id, carbonFootprint.TotalEmissions);
            
            return CreatedAtAction(nameof(GetCarbonFootprint), new { id = carbonFootprint.Id }, carbonFootprint);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating carbon footprint");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating carbon footprint");
            return BadRequest($"Error creating carbon footprint: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates existing carbon footprint data with recalculation of emission totals
    /// </summary>
    /// <param name="id">Carbon footprint record unique identifier</param>
    /// <param name="updateDto">Updated carbon footprint data</param>
    /// <returns>Updated carbon footprint record</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CarbonFootprintDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<CarbonFootprintDto>> UpdateCarbonFootprint(Guid id, [FromBody] UpdateCarbonFootprintDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating carbon footprint: {CarbonFootprintId}", id);
            
            var carbonFootprint = await _carbonFootprintService.UpdateCarbonFootprintAsync(id, updateDto);
            
            _logger.LogInformation("Successfully updated carbon footprint: {CarbonFootprintId}", id);
            
            return Ok(carbonFootprint);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Carbon footprint not found for update: {CarbonFootprintId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating carbon footprint {CarbonFootprintId}", id);
            return BadRequest($"Error updating carbon footprint: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a carbon footprint record (soft delete)
    /// </summary>
    /// <param name="id">Carbon footprint record unique identifier</param>
    /// <returns>No content on successful deletion</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DeleteCarbonFootprint(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting carbon footprint: {CarbonFootprintId}", id);
            
            await _carbonFootprintService.DeleteCarbonFootprintAsync(id);
            
            _logger.LogInformation("Successfully deleted carbon footprint: {CarbonFootprintId}", id);
            
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Carbon footprint not found for deletion: {CarbonFootprintId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting carbon footprint {CarbonFootprintId}", id);
            return BadRequest($"Error deleting carbon footprint: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves carbon footprint records for a specific supplier with trend analysis
    /// </summary>
    /// <param name="supplierId">Supplier unique identifier</param>
    /// <param name="filter">Additional filtering parameters</param>
    /// <returns>Supplier-specific carbon footprint data with emission trends</returns>
    [HttpGet("by-supplier/{supplierId:guid}")]
    [ProducesResponseType(typeof(PagedResult<CarbonFootprintDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PagedResult<CarbonFootprintDto>>> GetCarbonFootprintsBySupplier(
        Guid supplierId, 
        [FromQuery] CarbonFootprintFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Retrieving carbon footprints for supplier: {SupplierId}", supplierId);
            
            filter.SupplierId = supplierId;
            var result = await _carbonFootprintService.GetCarbonFootprintsAsync(filter);
            
            _logger.LogInformation("Retrieved {Count} carbon footprint records for supplier {SupplierId}", 
                result.Data.Count(), supplierId);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving carbon footprints for supplier {SupplierId}", supplierId);
            return BadRequest($"Error retrieving carbon footprints for supplier: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates comprehensive carbon footprint summary with analytics and insights
    /// </summary>
    /// <param name="filter">Filter parameters for summary calculation</param>
    /// <returns>Aggregated carbon footprint data with breakdowns by category, transport mode, and supplier</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(CarbonFootprintSummaryDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<CarbonFootprintSummaryDto>> GetCarbonFootprintSummary([FromQuery] CarbonFootprintFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Generating carbon footprint summary with filter: {@Filter}", filter);
            
            var summary = await _carbonFootprintService.GetCarbonFootprintSummaryAsync(filter);
            
            _logger.LogInformation("Generated carbon footprint summary with total emissions: {TotalEmissions} kg CO2e", 
                summary.TotalEmissions);
            
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating carbon footprint summary");
            return BadRequest($"Error generating carbon footprint summary: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifies carbon footprint data for accuracy and compliance
    /// </summary>
    /// <param name="id">Carbon footprint record unique identifier</param>
    /// <param name="verifierName">Name of the person/organization verifying the data</param>
    /// <returns>Updated carbon footprint record with verification status</returns>
    [HttpPost("{id:guid}/verify")]
    [ProducesResponseType(typeof(CarbonFootprintDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<CarbonFootprintDto>> VerifyCarbonFootprint(Guid id, [FromBody] string verifierName)
    {
        try
        {
            _logger.LogInformation("Verifying carbon footprint: {CarbonFootprintId} by {VerifierName}", id, verifierName);
            
            var carbonFootprint = await _carbonFootprintService.VerifyCarbonFootprintAsync(id, verifierName);
            
            _logger.LogInformation("Successfully verified carbon footprint: {CarbonFootprintId}", id);
            
            return Ok(carbonFootprint);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Carbon footprint not found for verification: {CarbonFootprintId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying carbon footprint {CarbonFootprintId}", id);
            return BadRequest($"Error verifying carbon footprint: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves carbon footprint trends and analytics for performance monitoring
    /// </summary>
    /// <param name="supplierId">Optional supplier filter</param>
    /// <param name="months">Number of months to analyze (default: 12)</param>
    /// <returns>Trend data showing emission patterns over time</returns>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(CarbonFootprintTrendsDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<CarbonFootprintTrendsDto>> GetCarbonFootprintTrends(
        [FromQuery] Guid? supplierId = null, 
        [FromQuery] int months = 12)
    {
        try
        {
            _logger.LogInformation("Retrieving carbon footprint trends for {Months} months, Supplier: {SupplierId}", 
                months, supplierId);
            
            var trends = await _carbonFootprintService.GetCarbonFootprintTrendsAsync(supplierId, months);
            
            _logger.LogInformation("Successfully retrieved carbon footprint trends");
            
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving carbon footprint trends");
            return BadRequest($"Error retrieving carbon footprint trends: {ex.Message}");
        }
    }
}

// Additional DTOs for Carbon Footprint Trends
public class CarbonFootprintTrendsDto
{
    public Guid? SupplierId { get; set; }
    public int PeriodMonths { get; set; }
    public List<MonthlyEmissionDto> MonthlyData { get; set; } = new();
    public decimal TotalEmissions { get; set; }
    public decimal AverageMonthlyEmissions { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
    public decimal TrendPercentage { get; set; }
}

public class MonthlyEmissionDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalEmissions { get; set; }
    public int RecordCount { get; set; }
    public decimal AverageEmissions { get; set; }
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
}

// Additional DTOs for the Carbon Footprint controller
public class CarbonFootprintTrendsDto
{
    public List<MonthlyEmissionDto> MonthlyEmissions { get; set; } = new();
    public Dictionary<string, decimal> EmissionsByCategory { get; set; } = new();
    public Dictionary<string, decimal> EmissionsByTransportMode { get; set; } = new();
    public decimal TotalEmissionsReduction { get; set; }
    public decimal AverageRenewableEnergyUsage { get; set; }
    public List<TopEmitterDto> TopEmitters { get; set; } = new();
}

public class MonthlyEmissionDto
{
    public DateTime Month { get; set; }
    public decimal TotalEmissions { get; set; }
    public decimal ProductionEmissions { get; set; }
    public decimal TransportationEmissions { get; set; }
    public decimal PackagingEmissions { get; set; }
    public int RecordCount { get; set; }
}

public class TopEmitterDto
{
    public string Name { get; set; } = string.Empty;
    public decimal TotalEmissions { get; set; }
    public decimal Percentage { get; set; }
}