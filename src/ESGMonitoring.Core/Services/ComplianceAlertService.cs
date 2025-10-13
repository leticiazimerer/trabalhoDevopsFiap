using AutoMapper;
using ESGMonitoring.Core.DTOs.Common;
using ESGMonitoring.Core.Entities;
using ESGMonitoring.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ESGMonitoring.Core.Services;

public class ComplianceAlertService : IComplianceAlertService
{
    private readonly IComplianceAlertRepository _complianceAlertRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ComplianceAlertService> _logger;

    public ComplianceAlertService(
        IComplianceAlertRepository complianceAlertRepository,
        ISupplierRepository supplierRepository,
        IMapper mapper,
        ILogger<ComplianceAlertService> logger)
    {
        _complianceAlertRepository = complianceAlertRepository;
        _supplierRepository = supplierRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<ComplianceAlertDto>> GetComplianceAlertsAsync(ComplianceAlertFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Getting compliance alerts with filter: {@Filter}", filter);

            var query = await _complianceAlertRepository.GetAllAsync();

            // Apply filters
            if (filter.SupplierId.HasValue)
            {
                query = query.Where(a => a.SupplierId == filter.SupplierId.Value);
            }

            if (!string.IsNullOrEmpty(filter.Severity))
            {
                query = query.Where(a => a.Severity.ToLower().Contains(filter.Severity.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(a => a.Category.ToLower().Contains(filter.Category.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(a => a.Status.ToLower().Contains(filter.Status.ToLower()));
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= filter.DateTo.Value);
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "title" => filter.SortDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
                "severity" => filter.SortDescending ? query.OrderByDescending(a => a.Severity) : query.OrderBy(a => a.Severity),
                "createdat" => filter.SortDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
                _ => query.OrderByDescending(a => a.CreatedAt)
            };

            var totalCount = query.Count();
            var alerts = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var alertDtos = _mapper.Map<List<ComplianceAlertDto>>(alerts);

            return new PagedResult<ComplianceAlertDto>
            {
                Data = alertDtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance alerts");
            throw;
        }
    }

    public async Task<ComplianceAlertDto?> GetComplianceAlertByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting compliance alert by ID: {Id}", id);

            var alert = await _complianceAlertRepository.GetByIdAsync(id);
            return alert != null ? _mapper.Map<ComplianceAlertDto>(alert) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance alert by ID: {Id}", id);
            throw;
        }
    }

    public async Task<ComplianceAlertDto> CreateComplianceAlertAsync(CreateComplianceAlertDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating compliance alert: {@CreateDto}", createDto);

            // Validate supplier exists
            var supplier = await _supplierRepository.GetByIdAsync(createDto.SupplierId);
            if (supplier == null)
            {
                throw new ArgumentException($"Supplier with ID {createDto.SupplierId} not found");
            }

            var alert = _mapper.Map<ComplianceAlert>(createDto);
            alert.Id = Guid.NewGuid();
            alert.CreatedAt = DateTime.UtcNow;
            alert.Status = "Open";

            var createdAlert = await _complianceAlertRepository.AddAsync(alert);
            return _mapper.Map<ComplianceAlertDto>(createdAlert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating compliance alert");
            throw;
        }
    }

    public async Task<ComplianceAlertDto> UpdateComplianceAlertAsync(Guid id, UpdateComplianceAlertDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating compliance alert {Id}: {@UpdateDto}", id, updateDto);

            var existingAlert = await _complianceAlertRepository.GetByIdAsync(id);
            if (existingAlert == null)
            {
                throw new ArgumentException($"Compliance alert with ID {id} not found");
            }

            _mapper.Map(updateDto, existingAlert);
            existingAlert.UpdatedAt = DateTime.UtcNow;

            var updatedAlert = await _complianceAlertRepository.UpdateAsync(existingAlert);
            return _mapper.Map<ComplianceAlertDto>(updatedAlert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating compliance alert {Id}", id);
            throw;
        }
    }

    public async Task<ComplianceAlertDto> ResolveComplianceAlertAsync(Guid id, ResolveAlertDto resolveDto)
    {
        try
        {
            _logger.LogInformation("Resolving compliance alert {Id}: {@ResolveDto}", id, resolveDto);

            var existingAlert = await _complianceAlertRepository.GetByIdAsync(id);
            if (existingAlert == null)
            {
                throw new ArgumentException($"Compliance alert with ID {id} not found");
            }

            existingAlert.Status = "Resolved";
            existingAlert.ResolvedBy = resolveDto.ResolvedBy;
            existingAlert.ResolutionNotes = resolveDto.ResolutionNotes;
            existingAlert.ResolvedAt = DateTime.UtcNow;
            existingAlert.UpdatedAt = DateTime.UtcNow;

            var updatedAlert = await _complianceAlertRepository.UpdateAsync(existingAlert);
            return _mapper.Map<ComplianceAlertDto>(updatedAlert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving compliance alert {Id}", id);
            throw;
        }
    }

    public async Task<ComplianceAlertDto> EscalateComplianceAlertAsync(Guid id, EscalateAlertDto escalateDto)
    {
        try
        {
            _logger.LogInformation("Escalating compliance alert {Id}: {@EscalateDto}", id, escalateDto);

            var existingAlert = await _complianceAlertRepository.GetByIdAsync(id);
            if (existingAlert == null)
            {
                throw new ArgumentException($"Compliance alert with ID {id} not found");
            }

            existingAlert.Status = "Escalated";
            existingAlert.EscalatedTo = escalateDto.EscalatedTo;
            existingAlert.EscalationReason = escalateDto.EscalationReason;
            existingAlert.EscalatedAt = DateTime.UtcNow;
            existingAlert.UpdatedAt = DateTime.UtcNow;

            var updatedAlert = await _complianceAlertRepository.UpdateAsync(existingAlert);
            return _mapper.Map<ComplianceAlertDto>(updatedAlert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating compliance alert {Id}", id);
            throw;
        }
    }

    public async Task DeleteComplianceAlertAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting compliance alert: {Id}", id);

            var existingAlert = await _complianceAlertRepository.GetByIdAsync(id);
            if (existingAlert == null)
            {
                throw new ArgumentException($"Compliance alert with ID {id} not found");
            }

            await _complianceAlertRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting compliance alert {Id}", id);
            throw;
        }
    }

    public async Task<List<ComplianceAlertDto>> GetComplianceAlertsBySupplierAsync(Guid supplierId, int limit = 10)
    {
        try
        {
            _logger.LogInformation("Getting compliance alerts for supplier: {SupplierId}", supplierId);

            var alerts = await _complianceAlertRepository.GetAllAsync();
            var supplierAlerts = alerts
                .Where(a => a.SupplierId == supplierId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToList();

            return _mapper.Map<List<ComplianceAlertDto>>(supplierAlerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance alerts for supplier {SupplierId}", supplierId);
            throw;
        }
    }

    public async Task<List<ComplianceAlertDto>> GetCriticalComplianceAlertsAsync(int limit = 10)
    {
        try
        {
            _logger.LogInformation("Getting critical compliance alerts");

            var alerts = await _complianceAlertRepository.GetAllAsync();
            var criticalAlerts = alerts
                .Where(a => a.Severity.ToLower() == "critical" && a.Status != "Resolved")
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToList();

            return _mapper.Map<List<ComplianceAlertDto>>(criticalAlerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting critical compliance alerts");
            throw;
        }
    }

    public async Task<ComplianceAlertDashboardDto> GetComplianceAlertDashboardAsync()
    {
        try
        {
            _logger.LogInformation("Getting compliance alert dashboard data");

            var alerts = await _complianceAlertRepository.GetAllAsync();

            var totalAlerts = alerts.Count();
            var openAlerts = alerts.Count(a => a.Status == "Open");
            var resolvedAlerts = alerts.Count(a => a.Status == "Resolved");
            var criticalAlerts = alerts.Count(a => a.Severity.ToLower() == "critical" && a.Status != "Resolved");

            var resolutionRate = totalAlerts > 0 ? (decimal)resolvedAlerts / totalAlerts * 100 : 0;

            return new ComplianceAlertDashboardDto
            {
                TotalAlerts = totalAlerts,
                OpenAlerts = openAlerts,
                ResolvedAlerts = resolvedAlerts,
                CriticalAlerts = criticalAlerts,
                ResolutionRate = Math.Round(resolutionRate, 2)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance alert dashboard data");
            throw;
        }
    }

    public async Task<List<ComplianceAlertDto>> RunAutomatedMonitoringAsync()
    {
        try
        {
            _logger.LogInformation("Running automated compliance monitoring");

            var suppliers = await _supplierRepository.GetAllAsync();
            var newAlerts = new List<ComplianceAlert>();

            foreach (var supplier in suppliers)
            {
                // Check ESG scores and create alerts for low scores
                if (supplier.EnvironmentalScore < 3.0m)
                {
                    newAlerts.Add(new ComplianceAlert
                    {
                        Id = Guid.NewGuid(),
                        SupplierId = supplier.Id,
                        Title = "Low Environmental Score Alert",
                        Description = $"Supplier {supplier.Name} has a low environmental score of {supplier.EnvironmentalScore}",
                        Severity = "High",
                        Category = "Environmental",
                        Status = "Open",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (supplier.SocialScore < 3.0m)
                {
                    newAlerts.Add(new ComplianceAlert
                    {
                        Id = Guid.NewGuid(),
                        SupplierId = supplier.Id,
                        Title = "Low Social Score Alert",
                        Description = $"Supplier {supplier.Name} has a low social score of {supplier.SocialScore}",
                        Severity = "High",
                        Category = "Social",
                        Status = "Open",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (supplier.GovernanceScore < 3.0m)
                {
                    newAlerts.Add(new ComplianceAlert
                    {
                        Id = Guid.NewGuid(),
                        SupplierId = supplier.Id,
                        Title = "Low Governance Score Alert",
                        Description = $"Supplier {supplier.Name} has a low governance score of {supplier.GovernanceScore}",
                        Severity = "High",
                        Category = "Governance",
                        Status = "Open",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // Check for upcoming audit dates
                if (supplier.NextAuditDate.HasValue && supplier.NextAuditDate.Value <= DateTime.UtcNow.AddDays(30))
                {
                    newAlerts.Add(new ComplianceAlert
                    {
                        Id = Guid.NewGuid(),
                        SupplierId = supplier.Id,
                        Title = "Upcoming Audit Alert",
                        Description = $"Supplier {supplier.Name} has an audit scheduled for {supplier.NextAuditDate:yyyy-MM-dd}",
                        Severity = "Medium",
                        Category = "Audit",
                        Status = "Open",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Save new alerts
            foreach (var alert in newAlerts)
            {
                await _complianceAlertRepository.AddAsync(alert);
            }

            return _mapper.Map<List<ComplianceAlertDto>>(newAlerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running automated compliance monitoring");
            throw;
        }
    }
}