using ESGMonitoring.API.Controllers;
using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ESGMonitoring.Tests.Controllers;

public class ComplianceAlertsControllerTests
{
    private readonly Mock<IComplianceAlertService> _mockComplianceAlertService;
    private readonly Mock<ILogger<ComplianceAlertsController>> _mockLogger;
    private readonly ComplianceAlertsController _controller;

    public ComplianceAlertsControllerTests()
    {
        _mockComplianceAlertService = new Mock<IComplianceAlertService>();
        _mockLogger = new Mock<ILogger<ComplianceAlertsController>>();
        _controller = new ComplianceAlertsController(_mockComplianceAlertService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetComplianceAlerts_ReturnsOkResult_WithPagedData()
    {
        // Arrange
        var filter = new ComplianceAlertFilterDto { PageNumber = 1, PageSize = 10 };
        var expectedResult = new PagedResult<ComplianceAlertDto>
        {
            Data = new List<ComplianceAlertDto>
            {
                new ComplianceAlertDto 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Test Alert", 
                    Severity = "High" 
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _mockComplianceAlertService
            .Setup(s => s.GetComplianceAlertsAsync(It.IsAny<ComplianceAlertFilterDto>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetComplianceAlerts(filter);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedData = okResult.Value.Should().BeOfType<PagedResult<ComplianceAlertDto>>().Subject;
        returnedData.Data.Should().HaveCount(1);
        returnedData.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetComplianceAlert_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var expectedAlert = new ComplianceAlertDto 
        { 
            Id = alertId, 
            Title = "Test Alert", 
            Severity = "High" 
        };

        _mockComplianceAlertService
            .Setup(s => s.GetComplianceAlertByIdAsync(alertId))
            .ReturnsAsync(expectedAlert);

        // Act
        var result = await _controller.GetComplianceAlert(alertId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedAlert = okResult.Value.Should().BeOfType<ComplianceAlertDto>().Subject;
        returnedAlert.Id.Should().Be(alertId);
        returnedAlert.Title.Should().Be("Test Alert");
    }

    [Fact]
    public async Task GetComplianceAlert_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();

        _mockComplianceAlertService
            .Setup(s => s.GetComplianceAlertByIdAsync(alertId))
            .ReturnsAsync((ComplianceAlertDto?)null);

        // Act
        var result = await _controller.GetComplianceAlert(alertId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateComplianceAlert_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new CreateComplianceAlertDto
        {
            SupplierId = Guid.NewGuid(),
            Title = "New Alert",
            Severity = "Critical",
            Category = "Environmental"
        };

        var createdAlert = new ComplianceAlertDto
        {
            Id = Guid.NewGuid(),
            SupplierId = createDto.SupplierId,
            Title = createDto.Title,
            Severity = createDto.Severity,
            Category = createDto.Category
        };

        _mockComplianceAlertService
            .Setup(s => s.CreateComplianceAlertAsync(It.IsAny<CreateComplianceAlertDto>()))
            .ReturnsAsync(createdAlert);

        // Act
        var result = await _controller.CreateComplianceAlert(createDto);

        // Assert
        result.Should().NotBeNull();
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        
        var returnedAlert = createdResult.Value.Should().BeOfType<ComplianceAlertDto>().Subject;
        returnedAlert.Title.Should().Be(createDto.Title);
    }

    [Fact]
    public async Task UpdateComplianceAlert_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var updateDto = new UpdateComplianceAlertDto
        {
            Title = "Updated Alert",
            Status = "In Progress"
        };

        var updatedAlert = new ComplianceAlertDto
        {
            Id = alertId,
            Title = updateDto.Title,
            Status = updateDto.Status
        };

        _mockComplianceAlertService
            .Setup(s => s.UpdateComplianceAlertAsync(alertId, It.IsAny<UpdateComplianceAlertDto>()))
            .ReturnsAsync(updatedAlert);

        // Act
        var result = await _controller.UpdateComplianceAlert(alertId, updateDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedAlert = okResult.Value.Should().BeOfType<ComplianceAlertDto>().Subject;
        returnedAlert.Title.Should().Be(updateDto.Title);
    }

    [Fact]
    public async Task ResolveComplianceAlert_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var resolutionDto = new ResolveAlertDto
        {
            ResolvedBy = "Test Resolver",
            ResolutionNotes = "Issue resolved"
        };

        var resolvedAlert = new ComplianceAlertDto
        {
            Id = alertId,
            Status = "Resolved",
            ResolvedBy = resolutionDto.ResolvedBy,
            ResolutionNotes = resolutionDto.ResolutionNotes
        };

        _mockComplianceAlertService
            .Setup(s => s.ResolveComplianceAlertAsync(alertId, It.IsAny<ResolveAlertDto>()))
            .ReturnsAsync(resolvedAlert);

        // Act
        var result = await _controller.ResolveComplianceAlert(alertId, resolutionDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedAlert = okResult.Value.Should().BeOfType<ComplianceAlertDto>().Subject;
        returnedAlert.Status.Should().Be("Resolved");
        returnedAlert.ResolvedBy.Should().Be(resolutionDto.ResolvedBy);
    }

    [Fact]
    public async Task GetCriticalComplianceAlerts_ReturnsOkResult_WithCriticalAlerts()
    {
        // Arrange
        var expectedAlerts = new List<ComplianceAlertDto>
        {
            new ComplianceAlertDto 
            { 
                Id = Guid.NewGuid(), 
                Title = "Critical Alert", 
                Severity = "Critical" 
            }
        };

        _mockComplianceAlertService
            .Setup(s => s.GetCriticalComplianceAlertsAsync(It.IsAny<int>()))
            .ReturnsAsync(expectedAlerts);

        // Act
        var result = await _controller.GetCriticalComplianceAlerts(0);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedAlerts = okResult.Value.Should().BeAssignableTo<IEnumerable<ComplianceAlertDto>>().Subject;
        returnedAlerts.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetComplianceAlertDashboard_ReturnsOkResult_WithDashboardData()
    {
        // Arrange
        var expectedDashboard = new ComplianceAlertDashboardDto
        {
            TotalAlerts = 100,
            OpenAlerts = 25,
            ResolvedAlerts = 70,
            CriticalAlerts = 5,
            ResolutionRate = 70.0m
        };

        _mockComplianceAlertService
            .Setup(s => s.GetComplianceAlertDashboardAsync())
            .ReturnsAsync(expectedDashboard);

        // Act
        var result = await _controller.GetComplianceAlertDashboard();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var dashboard = okResult.Value.Should().BeOfType<ComplianceAlertDashboardDto>().Subject;
        dashboard.TotalAlerts.Should().Be(100);
        dashboard.ResolutionRate.Should().Be(70.0m);
    }

    [Fact]
    public async Task GetComplianceAlerts_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var filter = new ComplianceAlertFilterDto();
        _mockComplianceAlertService
            .Setup(s => s.GetComplianceAlertsAsync(It.IsAny<ComplianceAlertFilterDto>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetComplianceAlerts(filter);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }
}