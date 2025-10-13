using AutoMapper;
using ESGMonitoring.Core.DTOs.Supplier;
using ESGMonitoring.Core.DTOs.CarbonFootprint;
using ESGMonitoring.Core.DTOs.ComplianceAlert;
using ESGMonitoring.Core.DTOs.SustainabilityReport;
using ESGMonitoring.Core.Entities;
using System.Text.Json;

namespace ESGMonitoring.Core.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Supplier Mappings
        CreateMap<Supplier, SupplierDto>()
            .ForMember(dest => dest.OverallESGScore, 
                opt => opt.MapFrom(src => (src.EnvironmentalScore + src.SocialScore + src.GovernanceScore) / 3))
            .ForMember(dest => dest.Certifications, opt => opt.MapFrom(src => 
                string.IsNullOrEmpty(src.Certifications) ? new List<string>() : 
                JsonSerializer.Deserialize<List<string>>(src.Certifications) ?? new List<string>()));

        CreateMap<CreateSupplierDto, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CarbonFootprints, opt => opt.Ignore())
            .ForMember(dest => dest.ComplianceAlerts, opt => opt.Ignore())
            .ForMember(dest => dest.Certifications, opt => opt.MapFrom(src => 
                JsonSerializer.Serialize(src.Certifications ?? new List<string>())));

        CreateMap<UpdateSupplierDto, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TaxId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CarbonFootprints, opt => opt.Ignore())
            .ForMember(dest => dest.ComplianceAlerts, opt => opt.Ignore())
            .ForMember(dest => dest.Certifications, opt => opt.MapFrom(src => 
                src.Certifications != null ? JsonSerializer.Serialize(src.Certifications) : null))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Carbon Footprint Mappings
        CreateMap<CarbonFootprint, CarbonFootprintDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name))
            .ForMember(dest => dest.EmissionsPerUnit, 
                opt => opt.MapFrom(src => src.QuantityProduced > 0 ? src.TotalEmissions / src.QuantityProduced : 0));

        CreateMap<CreateCarbonFootprintDto, CarbonFootprint>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.TotalEmissions, 
                opt => opt.MapFrom(src => src.ProductionEmissions + src.TransportationEmissions + src.PackagingEmissions))
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.VerifiedBy, opt => opt.MapFrom(src => string.Empty))
            .ForMember(dest => dest.VerificationDate, opt => opt.MapFrom(src => (DateTime?)null));

        CreateMap<UpdateCarbonFootprintDto, CarbonFootprint>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.TotalEmissions, 
                opt => opt.MapFrom(src => (src.ProductionEmissions ?? 0) + (src.TransportationEmissions ?? 0) + (src.PackagingEmissions ?? 0)))
            .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
            .ForMember(dest => dest.VerifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.VerificationDate, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Compliance Alert mappings
        CreateMap<ComplianceAlert, ComplianceAlertDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty));

        CreateMap<CreateComplianceAlertDto, ComplianceAlert>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.ResolvedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ResolvedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ResolutionNotes, opt => opt.Ignore())
            .ForMember(dest => dest.EscalatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.EscalatedTo, opt => opt.Ignore())
            .ForMember(dest => dest.EscalationReason, opt => opt.Ignore());

        CreateMap<UpdateComplianceAlertDto, ComplianceAlert>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Sustainability Report mappings
        CreateMap<SustainabilityReport, SustainabilityReportDto>()
            .ForMember(dest => dest.DistributionList, opt => opt.MapFrom(src => 
                string.IsNullOrEmpty(src.DistributionList) ? new List<string>() : 
                JsonSerializer.Deserialize<List<string>>(src.DistributionList) ?? new List<string>()));

        CreateMap<CreateSustainabilityReportDto, SustainabilityReport>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Content, opt => opt.Ignore())
            .ForMember(dest => dest.PublishedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovalNotes, opt => opt.Ignore())
            .ForMember(dest => dest.IsPublic, opt => opt.Ignore())
            .ForMember(dest => dest.DistributionList, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierIds, opt => opt.MapFrom(src => 
                src.SupplierIds != null ? JsonSerializer.Serialize(src.SupplierIds) : null));

        CreateMap<UpdateSustainabilityReportDto, SustainabilityReport>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}