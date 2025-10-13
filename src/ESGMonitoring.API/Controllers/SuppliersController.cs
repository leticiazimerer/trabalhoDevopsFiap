using ESGMonitoring.Core.DTOs.Supplier;
using ESGMonitoring.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ESGMonitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(ISupplierService supplierService, ILogger<SuppliersController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of suppliers with advanced filtering and ESG scoring
    /// </summary>
    /// <param name="filter">Filter parameters including pagination, ESG scores, risk levels, and certifications</param>
    /// <returns>Paginated list of suppliers with comprehensive ESG data</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SupplierDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PagedResult<SupplierDto>>> GetSuppliers([FromQuery] SupplierFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Retrieving suppliers with filter: {@Filter}", filter);
            
            var result = await _supplierService.GetSuppliersAsync(filter);
            
            _logger.LogInformation("Retrieved {Count} suppliers out of {Total}", 
                result.Data.Count(), result.TotalCount);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers");
            return BadRequest($"Error retrieving suppliers: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific supplier including ESG analysis
    /// </summary>
    /// <param name="id">Supplier unique identifier</param>
    /// <returns>Detailed supplier information with ESG metrics</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SupplierDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<SupplierDto>> GetSupplier(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving supplier with ID: {SupplierId}", id);
            
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            
            if (supplier == null)
            {
                _logger.LogWarning("Supplier not found: {SupplierId}", id);
                return NotFound($"Supplier with ID '{id}' not found");
            }

            return Ok(supplier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier {SupplierId}", id);
            return BadRequest($"Error retrieving supplier: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new supplier with comprehensive ESG assessment and risk evaluation
    /// </summary>
    /// <param name="createDto">Supplier creation data including ESG scores and sustainability practices</param>
    /// <returns>Created supplier with calculated risk level and ESG rating</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SupplierDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<SupplierDto>> CreateSupplier([FromBody] CreateSupplierDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating new supplier: {SupplierName}", createDto.Name);
            
            var supplier = await _supplierService.CreateSupplierAsync(createDto);
            
            _logger.LogInformation("Successfully created supplier: {SupplierId}", supplier.Id);
            
            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, supplier);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating supplier");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier");
            return BadRequest($"Error creating supplier: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an existing supplier with automatic risk level recalculation
    /// </summary>
    /// <param name="id">Supplier unique identifier</param>
    /// <param name="updateDto">Updated supplier data</param>
    /// <returns>Updated supplier information</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SupplierDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<SupplierDto>> UpdateSupplier(Guid id, [FromBody] UpdateSupplierDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating supplier: {SupplierId}", id);
            
            var supplier = await _supplierService.UpdateSupplierAsync(id, updateDto);
            
            _logger.LogInformation("Successfully updated supplier: {SupplierId}", id);
            
            return Ok(supplier);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Supplier not found for update: {SupplierId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier {SupplierId}", id);
            return BadRequest($"Error updating supplier: {ex.Message}");
        }
    }

    /// <summary>
    /// Soft deletes a supplier (marks as inactive)
    /// </summary>
    /// <param name="id">Supplier unique identifier</param>
    /// <returns>No content on successful deletion</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DeleteSupplier(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting supplier: {SupplierId}", id);
            
            await _supplierService.DeleteSupplierAsync(id);
            
            _logger.LogInformation("Successfully deleted supplier: {SupplierId}", id);
            
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Supplier not found for deletion: {SupplierId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier {SupplierId}", id);
            return BadRequest($"Error deleting supplier: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves suppliers with upcoming audits for proactive compliance management
    /// </summary>
    /// <param name="daysAhead">Number of days ahead to look for upcoming audits (default: 30)</param>
    /// <returns>List of suppliers with audits due within the specified timeframe</returns>
    [HttpGet("upcoming-audits")]
    [ProducesResponseType(typeof(IEnumerable<SupplierDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliersWithUpcomingAudits([FromQuery] int daysAhead = 30)
    {
        try
        {
            _logger.LogInformation("Retrieving suppliers with upcoming audits within {DaysAhead} days", daysAhead);
            
            var suppliers = await _supplierService.GetSuppliersWithUpcomingAuditsAsync(daysAhead);
            
            _logger.LogInformation("Found {Count} suppliers with upcoming audits", suppliers.Count());
            
            return Ok(suppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers with upcoming audits");
            return BadRequest($"Error retrieving suppliers with upcoming audits: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves suppliers filtered by risk level for targeted risk management
    /// </summary>
    /// <param name="riskLevel">Risk level filter (Low, Medium, High, Critical)</param>
    /// <returns>List of suppliers matching the specified risk level</returns>
    [HttpGet("by-risk-level/{riskLevel}")]
    [ProducesResponseType(typeof(IEnumerable<SupplierDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliersByRiskLevel(string riskLevel)
    {
        try
        {
            _logger.LogInformation("Retrieving suppliers with risk level: {RiskLevel}", riskLevel);
            
            var suppliers = await _supplierService.GetSuppliersByRiskLevelAsync(riskLevel);
            
            _logger.LogInformation("Found {Count} suppliers with risk level {RiskLevel}", 
                suppliers.Count(), riskLevel);
            
            return Ok(suppliers);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid risk level provided: {RiskLevel}", riskLevel);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers by risk level");
            return BadRequest($"Error retrieving suppliers by risk level: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves comprehensive supplier dashboard with key ESG metrics and insights
    /// </summary>
    /// <returns>Dashboard data including compliance statistics, risk distribution, and performance metrics</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(SupplierDashboardDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<SupplierDashboardDto>> GetSupplierDashboard()
    {
        try
        {
            _logger.LogInformation("Retrieving supplier dashboard data");
            
            var dashboard = await _supplierService.GetSupplierDashboardAsync();
            
            _logger.LogInformation("Successfully retrieved supplier dashboard data");
            
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier dashboard");
            return BadRequest($"Error retrieving supplier dashboard: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates supplier compliance against ESG criteria and industry standards
    /// </summary>
    /// <param name="id">Supplier unique identifier</param>
    /// <returns>Validation result indicating compliance status</returns>
    [HttpPost("{id:guid}/validate")]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<bool>> ValidateSupplier(Guid id)
    {
        try
        {
            _logger.LogInformation("Validating supplier compliance: {SupplierId}", id);
            
            var isValid = await _supplierService.ValidateSupplierAsync(id);
            
            _logger.LogInformation("Supplier {SupplierId} validation result: {IsValid}", id, isValid);
            
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating supplier {SupplierId}", id);
            return BadRequest($"Error validating supplier: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates comprehensive ESG analysis report for a specific supplier
    /// </summary>
    /// <param name="id">Supplier unique identifier</param>
    /// <returns>Detailed ESG analysis including scores, benchmarks, and recommendations</returns>
    [HttpGet("{id:guid}/esg-analysis")]
    [ProducesResponseType(typeof(SupplierESGAnalysisDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<SupplierESGAnalysisDto>> GetSupplierESGAnalysis(Guid id)
    {
        try
        {
            _logger.LogInformation("Generating ESG analysis for supplier: {SupplierId}", id);
            
            var analysis = await _supplierService.GetSupplierESGAnalysisAsync(id);
            
            _logger.LogInformation("Successfully generated ESG analysis for supplier: {SupplierId}", id);
            
            return Ok(analysis);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Supplier not found for ESG analysis: {SupplierId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating ESG analysis for supplier {SupplierId}", id);
            return BadRequest($"Error generating ESG analysis: {ex.Message}");
        }
    }
}