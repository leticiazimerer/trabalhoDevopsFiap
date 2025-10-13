using ESGMonitoring.API.Controllers;
using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.DTOs.Supplier;
using ESGMonitoring.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ESGMonitoring.Tests.Controllers;

public class SuppliersControllerTests
{
    private readonly Mock<ISupplierService> _mockSupplierService;
    private readonly Mock<ILogger<SuppliersController>> _mockLogger;
    private readonly SuppliersController _controller;

    public SuppliersControllerTests()
    {
        _mockSupplierService = new Mock<ISupplierService>();
        _mockLogger = new Mock<ILogger<SuppliersController>>();
        _controller = new SuppliersController(_mockSupplierService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetSuppliers_ReturnsOkResult_WithPagedSuppliers()
    {
        // Arrange
        var filter = new SupplierFilterDto { PageNumber = 1, PageSize = 10 };
        var expectedResult = new PagedResult<SupplierDto>
        {
            Data = new List<SupplierDto>
            {
                new SupplierDto 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Test Supplier", 
                    ESGScore = 75.5m 
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _mockSupplierService
            .Setup(s => s.GetSuppliersAsync(It.IsAny<SupplierFilterDto>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetSuppliers(filter);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedData = okResult.Value.Should().BeOfType<PagedResult<SupplierDto>>().Subject;
        returnedData.Data.Should().HaveCount(1);
        returnedData.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSupplier_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var expectedSupplier = new SupplierDto 
        { 
            Id = supplierId, 
            Name = "Test Supplier", 
            ESGScore = 75.5m 
        };

        _mockSupplierService
            .Setup(s => s.GetSupplierByIdAsync(supplierId))
            .ReturnsAsync(expectedSupplier);

        // Act
        var result = await _controller.GetSupplier(supplierId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedSupplier = okResult.Value.Should().BeOfType<SupplierDto>().Subject;
        returnedSupplier.Id.Should().Be(supplierId);
        returnedSupplier.Name.Should().Be("Test Supplier");
    }

    [Fact]
    public async Task GetSupplier_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var supplierId = Guid.NewGuid();

        _mockSupplierService
            .Setup(s => s.GetSupplierByIdAsync(supplierId))
            .ReturnsAsync((SupplierDto?)null);

        // Act
        var result = await _controller.GetSupplier(supplierId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateSupplier_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new CreateSupplierDto
        {
            Name = "New Supplier",
            ContactEmail = "test@supplier.com",
            Country = "USA",
            ESGScore = 80.0m
        };

        var createdSupplier = new SupplierDto
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            ContactEmail = createDto.ContactEmail,
            Country = createDto.Country,
            ESGScore = createDto.ESGScore
        };

        _mockSupplierService
            .Setup(s => s.CreateSupplierAsync(It.IsAny<CreateSupplierDto>()))
            .ReturnsAsync(createdSupplier);

        // Act
        var result = await _controller.CreateSupplier(createDto);

        // Assert
        result.Should().NotBeNull();
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        
        var returnedSupplier = createdResult.Value.Should().BeOfType<SupplierDto>().Subject;
        returnedSupplier.Name.Should().Be(createDto.Name);
    }

    [Fact]
    public async Task UpdateSupplier_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var updateDto = new UpdateSupplierDto
        {
            Name = "Updated Supplier",
            ESGScore = 85.0m
        };

        var updatedSupplier = new SupplierDto
        {
            Id = supplierId,
            Name = updateDto.Name,
            ESGScore = updateDto.ESGScore
        };

        _mockSupplierService
            .Setup(s => s.UpdateSupplierAsync(supplierId, It.IsAny<UpdateSupplierDto>()))
            .ReturnsAsync(updatedSupplier);

        // Act
        var result = await _controller.UpdateSupplier(supplierId, updateDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedSupplier = okResult.Value.Should().BeOfType<SupplierDto>().Subject;
        returnedSupplier.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task DeleteSupplier_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var supplierId = Guid.NewGuid();

        _mockSupplierService
            .Setup(s => s.DeleteSupplierAsync(supplierId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteSupplier(supplierId);

        // Assert
        result.Should().NotBeNull();
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        noContentResult.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task GetSupplierDashboard_ReturnsOkResult_WithDashboardData()
    {
        // Arrange
        var expectedDashboard = new SupplierDashboardDto
        {
            TotalSuppliers = 100,
            HighRiskSuppliers = 15,
            AverageESGScore = 72.5m,
            ComplianceRate = 85.0m
        };

        _mockSupplierService
            .Setup(s => s.GetSupplierDashboardAsync())
            .ReturnsAsync(expectedDashboard);

        // Act
        var result = await _controller.GetSupplierDashboard();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var dashboard = okResult.Value.Should().BeOfType<SupplierDashboardDto>().Subject;
        dashboard.TotalSuppliers.Should().Be(100);
        dashboard.HighRiskSuppliers.Should().Be(15);
    }

    [Fact]
    public async Task GetSuppliers_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var filter = new SupplierFilterDto();
        _mockSupplierService
            .Setup(s => s.GetSuppliersAsync(It.IsAny<SupplierFilterDto>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetSuppliers(filter);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }
}