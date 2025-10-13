using ESGMonitoring.API.Controllers;
using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ESGMonitoring.Tests.Controllers;

public class SustainabilityReportsControllerTests
{
    private readonly Mock<ISustainabilityReportService> _mockSustainabilityReportService;
    private readonly Mock<ILogger<SustainabilityReportsController>> _mockLogger;
    private readonly SustainabilityReportsController _controller;

    public SustainabilityReportsControllerTests()
    {
        _mockSustainabilityReportService = new Mock<ISustainabilityReportService>();
        _mockLogger = new Mock<ILogger<SustainabilityReportsController>>();
        _controller = new SustainabilityReportsController(_mockSustainabilityReportService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetSustainabilityReports_ReturnsOkResult_WithPagedData()
    {
        // Arrange
        var filter = new SustainabilityReportFilterDto { PageNumber = 1, PageSize = 10 };
        var expectedResult = new PagedResult<SustainabilityReportDto>
        {
            Data = new List<SustainabilityReportDto>
            {
                new SustainabilityReportDto 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Test Report", 
                    ReportType = "ESGOverview" 
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _mockSustainabilityReportService
            .Setup(s => s.GetSustainabilityReportsAsync(It.IsAny<SustainabilityReportFilterDto>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetSustainabilityReports(filter);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedData = okResult.Value.Should().BeOfType<PagedResult<SustainabilityReportDto>>().Subject;
        returnedData.Data.Should().HaveCount(1);
        returnedData.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSustainabilityReport_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var expectedReport = new SustainabilityReportDto 
        { 
            Id = reportId, 
            Title = "Test Report", 
            ReportType = "ESGOverview" 
        };

        _mockSustainabilityReportService
            .Setup(s => s.GetSustainabilityReportByIdAsync(reportId))
            .ReturnsAsync(expectedReport);

        // Act
        var result = await _controller.GetSustainabilityReport(reportId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedReport = okResult.Value.Should().BeOfType<SustainabilityReportDto>().Subject;
        returnedReport.Id.Should().Be(reportId);
        returnedReport.Title.Should().Be("Test Report");
    }

    [Fact]
    public async Task GetSustainabilityReport_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var reportId = Guid.NewGuid();

        _mockSustainabilityReportService
            .Setup(s => s.GetSustainabilityReportByIdAsync(reportId))
            .ReturnsAsync((SustainabilityReportDto?)null);

        // Act
        var result = await _controller.GetSustainabilityReport(reportId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GenerateSustainabilityReport_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new CreateSustainabilityReportDto
        {
            Title = "New Report",
            ReportType = "CarbonFootprint",
            Period = "Q1 2024"
        };

        var generatedReport = new SustainabilityReportDto
        {
            Id = Guid.NewGuid(),
            Title = createDto.Title,
            ReportType = createDto.ReportType,
            Period = createDto.Period,
            Status = "Generated"
        };

        _mockSustainabilityReportService
            .Setup(s => s.GenerateSustainabilityReportAsync(It.IsAny<CreateSustainabilityReportDto>()))
            .ReturnsAsync(generatedReport);

        // Act
        var result = await _controller.GenerateSustainabilityReport(createDto);

        // Assert
        result.Should().NotBeNull();
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        
        var returnedReport = createdResult.Value.Should().BeOfType<SustainabilityReportDto>().Subject;
        returnedReport.Title.Should().Be(createDto.Title);
    }

    [Fact]
    public async Task UpdateSustainabilityReport_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var updateDto = new UpdateSustainabilityReportDto
        {
            Title = "Updated Report",
            ExecutiveSummary = "Updated summary"
        };

        var updatedReport = new SustainabilityReportDto
        {
            Id = reportId,
            Title = updateDto.Title,
            ExecutiveSummary = updateDto.ExecutiveSummary
        };

        _mockSustainabilityReportService
            .Setup(s => s.UpdateSustainabilityReportAsync(reportId, It.IsAny<UpdateSustainabilityReportDto>()))
            .ReturnsAsync(updatedReport);

        // Act
        var result = await _controller.UpdateSustainabilityReport(reportId, updateDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedReport = okResult.Value.Should().BeOfType<SustainabilityReportDto>().Subject;
        returnedReport.Title.Should().Be(updateDto.Title);
    }

    [Fact]
    public async Task DownloadSustainabilityReport_WithValidId_ReturnsFileResult()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var format = "pdf";
        var fileResult = new ReportFileResultDto
        {
            FileContent = new byte[] { 1, 2, 3, 4, 5 },
            ContentType = "application/pdf",
            FileName = "report.pdf"
        };

        _mockSustainabilityReportService
            .Setup(s => s.DownloadSustainabilityReportAsync(reportId, format))
            .ReturnsAsync(fileResult);

        // Act
        var result = await _controller.DownloadSustainabilityReport(reportId, format);

        // Assert
        result.Should().NotBeNull();
        var fileActionResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileActionResult.ContentType.Should().Be("application/pdf");
        fileActionResult.FileDownloadName.Should().Be("report.pdf");
    }

    [Fact]
    public async Task PublishSustainabilityReport_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var publishDto = new PublishReportDto
        {
            DistributionList = new List<string> { "stakeholder@example.com" },
            IsPublic = true
        };

        var publishedReport = new SustainabilityReportDto
        {
            Id = reportId,
            Status = "Published",
            PublishedAt = DateTime.UtcNow
        };

        _mockSustainabilityReportService
            .Setup(s => s.PublishSustainabilityReportAsync(reportId, It.IsAny<PublishReportDto>()))
            .ReturnsAsync(publishedReport);

        // Act
        var result = await _controller.PublishSustainabilityReport(reportId, publishDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedReport = okResult.Value.Should().BeOfType<SustainabilityReportDto>().Subject;
        returnedReport.Status.Should().Be("Published");
    }

    [Fact]
    public async Task ApproveSustainabilityReport_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var approvalDto = new ApproveReportDto
        {
            ApprovedBy = "Manager",
            ApprovalNotes = "Approved for publication"
        };

        var approvedReport = new SustainabilityReportDto
        {
            Id = reportId,
            ApprovedBy = approvalDto.ApprovedBy,
            ApprovalNotes = approvalDto.ApprovalNotes,
            ApprovedAt = DateTime.UtcNow
        };

        _mockSustainabilityReportService
            .Setup(s => s.ApproveSustainabilityReportAsync(reportId, It.IsAny<ApproveReportDto>()))
            .ReturnsAsync(approvedReport);

        // Act
        var result = await _controller.ApproveSustainabilityReport(reportId, approvalDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedReport = okResult.Value.Should().BeOfType<SustainabilityReportDto>().Subject;
        returnedReport.ApprovedBy.Should().Be(approvalDto.ApprovedBy);
    }

    [Fact]
    public async Task GetSustainabilityReportDashboard_ReturnsOkResult_WithDashboardData()
    {
        // Arrange
        var expectedDashboard = new SustainabilityReportDashboardDto
        {
            TotalReports = 50,
            PublishedReports = 30,
            PendingApprovalReports = 10,
            DraftReports = 10
        };

        _mockSustainabilityReportService
            .Setup(s => s.GetSustainabilityReportDashboardAsync())
            .ReturnsAsync(expectedDashboard);

        // Act
        var result = await _controller.GetSustainabilityReportDashboard();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var dashboard = okResult.Value.Should().BeOfType<SustainabilityReportDashboardDto>().Subject;
        dashboard.TotalReports.Should().Be(50);
        dashboard.PublishedReports.Should().Be(30);
    }

    [Fact]
    public async Task GetSustainabilityReports_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var filter = new SustainabilityReportFilterDto();
        _mockSustainabilityReportService
            .Setup(s => s.GetSustainabilityReportsAsync(It.IsAny<SustainabilityReportFilterDto>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetSustainabilityReports(filter);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }
}