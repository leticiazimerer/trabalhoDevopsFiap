using ESGMonitoring.Core.Entities;

namespace ESGMonitoring.Core.DTOs.CarbonFootprint;

public class CarbonFootprintDto
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public decimal ProductionEmissions { get; set; }
    public decimal TransportationEmissions { get; set; }
    public decimal PackagingEmissions { get; set; }
    public decimal TotalEmissions { get; set; }
    public int QuantityProduced { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal EmissionsPerUnit => QuantityProduced > 0 ? TotalEmissions / QuantityProduced : 0;
    public DateTime ProductionDate { get; set; }
    public string OriginLocation { get; set; } = string.Empty;
    public string DestinationLocation { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public TransportMode TransportMode { get; set; }
    public decimal RenewableEnergyPercentage { get; set; }
    public bool IsVerified { get; set; }
    public string VerifiedBy { get; set; } = string.Empty;
    public DateTime? VerificationDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateCarbonFootprintDto
{
    public Guid SupplierId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public decimal ProductionEmissions { get; set; }
    public decimal TransportationEmissions { get; set; }
    public decimal PackagingEmissions { get; set; }
    public int QuantityProduced { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime ProductionDate { get; set; }
    public string OriginLocation { get; set; } = string.Empty;
    public string DestinationLocation { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public TransportMode TransportMode { get; set; }
    public decimal RenewableEnergyPercentage { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class UpdateCarbonFootprintDto
{
    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public decimal ProductionEmissions { get; set; }
    public decimal TransportationEmissions { get; set; }
    public decimal PackagingEmissions { get; set; }
    public int QuantityProduced { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime ProductionDate { get; set; }
    public string OriginLocation { get; set; } = string.Empty;
    public string DestinationLocation { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public TransportMode TransportMode { get; set; }
    public decimal RenewableEnergyPercentage { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CarbonFootprintFilterDto : PaginationParameters
{
    public Guid? SupplierId { get; set; }
    public string? ProductCategory { get; set; }
    public DateTime? ProductionDateFrom { get; set; }
    public DateTime? ProductionDateTo { get; set; }
    public decimal? MinEmissions { get; set; }
    public decimal? MaxEmissions { get; set; }
    public TransportMode? TransportMode { get; set; }
    public bool? IsVerified { get; set; }
    public decimal? MinRenewableEnergyPercentage { get; set; }
}

public class CarbonFootprintSummaryDto
{
    public decimal TotalEmissions { get; set; }
    public decimal AverageEmissionsPerUnit { get; set; }
    public int TotalProducts { get; set; }
    public decimal AverageRenewableEnergyPercentage { get; set; }
    public Dictionary<TransportMode, decimal> EmissionsByTransportMode { get; set; } = new();
    public Dictionary<string, decimal> EmissionsByCategory { get; set; } = new();
    public Dictionary<string, decimal> EmissionsBySupplier { get; set; } = new();
}