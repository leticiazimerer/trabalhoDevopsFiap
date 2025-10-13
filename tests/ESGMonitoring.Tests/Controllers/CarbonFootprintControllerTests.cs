using ESGMonitoring.API.Controllers;
using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.DTOs.CarbonFootprint;
using ESGMonitoring.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ESGMonitoring.Tests.Controllers;

public class CarbonFootprintControllerTests
{
    private readonly Mock<ICarbonFootprintService> _mockCarbonFootprintService;
    private readonly Mock<ILogger<CarbonFootprintController>> _mockLogger;
    private readonly CarbonFootprintController _controller;

    public CarbonFootprintControllerTests()
    {
        _mockCarbonFootprintService = new Mock<ICarbonFootprintService>();
        _mockLogger = new Mock<ILogger<CarbonFootprintController>>();
        _controller = new CarbonFootprintController(_mockCarbonFootprintService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCarbonFootprints_ReturnsOkResult_WithPagedData()
    {
        // Arrange
        var filter = new CarbonFootprintFilterDto { PageNumber = 1, PageSize = 10 };
        var expectedResult = new PagedResult<CarbonFootprintDto>
        {
            Data = new List<CarbonFootprintDto>
            {
                new CarbonFootprintDto 
                { 
                    Id = Guid.NewGuid(), 
                    ProductName = "Test Product", 
                    TotalEmissions = 100.5m 
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _mockCarbonFootprintService
            .Setup(s => s.GetCarbonFootprintsAsync(It.IsAny<CarbonFootprintFilterDto>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetCarbonFootprints(filter);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedData = okResult.Value.Should().BeOfType<PagedResult<CarbonFootprintDto>>().Subject;
        returnedData.Data.Should().HaveCount(1);
        returnedData.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetCarbonFootprint_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var footprintId = Guid.NewGuid();
        var expectedFootprint = new CarbonFootprintDto 
        { 
            Id = footprintId, 
            ProductName = "Test Product", 
            TotalEmissions = 100.5m 
        };

        _mockCarbonFootprintService
            .Setup(s => s.GetCarbonFootprintByIdAsync(footprintId))
            .ReturnsAsync(expectedFootprint);

        // Act
        var result = await _controller.GetCarbonFootprint(footprintId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedFootprint = okResult.Value.Should().BeOfType<CarbonFootprintDto>().Subject;
        returnedFootprint.Id.Should().Be(footprintId);
        returnedFootprint.ProductName.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetCarbonFootprint_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var footprintId = Guid.NewGuid();

        _mockCarbonFootprintService
            .Setup(s => s.GetCarbonFootprintByIdAsync(footprintId))
            .ReturnsAsync((CarbonFootprintDto?)null);

        // Act
        var result = await _controller.GetCarbonFootprint(footprintId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateCarbonFootprint_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new CreateCarbonFootprintDto
        {
            SupplierId = Guid.NewGuid(),
            ProductName = "New Product",
            ProductionEmissions = 50.0m,
            TransportationEmissions = 30.0m,
            PackagingEmissions = 20.0m
        };

        var createdFootprint = new CarbonFootprintDto
        {
            Id = Guid.NewGuid(),
            SupplierId = createDto.SupplierId,
            ProductName = createDto.ProductName,
            TotalEmissions = 100.0m
        };

        _mockCarbonFootprintService
            .Setup(s => s.CreateCarbonFootprintAsync(It.IsAny<CreateCarbonFootprintDto>()))
            .ReturnsAsync(createdFootprint);

        // Act
        var result = await _controller.CreateCarbonFootprint(createDto);

        // Assert
        result.Should().NotBeNull();
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        
        var returnedFootprint = createdResult.Value.Should().BeOfType<CarbonFootprintDto>().Subject;
        returnedFootprint.ProductName.Should().Be(createDto.ProductName);
    }

    [Fact]
    public async Task UpdateCarbonFootprint_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var footprintId = Guid.NewGuid();
        var updateDto = new UpdateCarbonFootprintDto
        {
            ProductName = "Updated Product",
            ProductionEmissions = 60.0m
        };

        var updatedFootprint = new CarbonFootprintDto
        {
            Id = footprintId,
            ProductName = updateDto.ProductName,
            ProductionEmissions = updateDto.ProductionEmissions
        };

        _mockCarbonFootprintService
            .Setup(s => s.UpdateCarbonFootprintAsync(footprintId, It.IsAny<UpdateCarbonFootprintDto>()))
            .ReturnsAsync(updatedFootprint);

        // Act
        var result = await _controller.UpdateCarbonFootprint(footprintId, updateDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedFootprint = okResult.Value.Should().BeOfType<CarbonFootprintDto>().Subject;
        returnedFootprint.ProductName.Should().Be(updateDto.ProductName);
    }

    [Fact]
    public async Task DeleteCarbonFootprint_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var footprintId = Guid.NewGuid();

        _mockCarbonFootprintService
            .Setup(s => s.DeleteCarbonFootprintAsync(footprintId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteCarbonFootprint(footprintId);

        // Assert
        result.Should().NotBeNull();
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        noContentResult.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task GetCarbonFootprintSummary_ReturnsOkResult_WithSummaryData()
    {
        // Arrange
        var filter = new CarbonFootprintFilterDto();
        var expectedSummary = new CarbonFootprintSummaryDto
        {
            TotalRecords = 100,
            TotalEmissions = 5000.0m,
            AverageEmissions = 50.0m,
            VerifiedRecords = 80,
            UnverifiedRecords = 20
        };

        _mockCarbonFootprintService
            .Setup(s => s.GetCarbonFootprintSummaryAsync(It.IsAny<CarbonFootprintFilterDto>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _controller.GetCarbonFootprintSummary(filter);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var summary = okResult.Value.Should().BeOfType<CarbonFootprintSummaryDto>().Subject;
        summary.TotalRecords.Should().Be(100);
        summary.TotalEmissions.Should().Be(5000.0m);
    }

    [Fact]
    public async Task VerifyCarbonFootprint_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var footprintId = Guid.NewGuid();
        var verifierName = "Test Verifier";
        var verifiedFootprint = new CarbonFootprintDto
        {
            Id = footprintId,
            IsVerified = true,
            VerifiedBy = verifierName
        };

        _mockCarbonFootprintService
            .Setup(s => s.VerifyCarbonFootprintAsync(footprintId, verifierName))
            .ReturnsAsync(verifiedFootprint);

        // Act
        var result = await _controller.VerifyCarbonFootprint(footprintId, verifierName);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedFootprint = okResult.Value.Should().BeOfType<CarbonFootprintDto>().Subject;
        returnedFootprint.IsVerified.Should().BeTrue();
        returnedFootprint.VerifiedBy.Should().Be(verifierName);
    }

    [Fact]
    public async Task GetCarbonFootprints_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var filter = new CarbonFootprintFilterDto();
        _mockCarbonFootprintService
            .Setup(s => s.GetCarbonFootprintsAsync(It.IsAny<CarbonFootprintFilterDto>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetCarbonFootprints(filter);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }
}