using ESGMonitoring.Core.DTOs.Supplier;
using FluentValidation;

namespace ESGMonitoring.Core.Validators;

public class CreateSupplierDtoValidator : AbstractValidator<CreateSupplierDto>
{
    public CreateSupplierDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required")
            .MaximumLength(200).WithMessage("Supplier name cannot exceed 200 characters");

        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage("Tax ID is required")
            .MaximumLength(20).WithMessage("Tax ID cannot exceed 20 characters")
            .Matches(@"^\d{11}$").WithMessage("Tax ID must be 11 digits");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactEmail))
            .WithMessage("Invalid email format");

        RuleFor(x => x.EnvironmentalScore)
            .InclusiveBetween(0, 100).WithMessage("Environmental score must be between 0 and 100");

        RuleFor(x => x.SocialScore)
            .InclusiveBetween(0, 100).WithMessage("Social score must be between 0 and 100");

        RuleFor(x => x.GovernanceScore)
            .InclusiveBetween(0, 100).WithMessage("Governance score must be between 0 and 100");

        RuleFor(x => x.NextAuditDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Next audit date must be in the future");

        RuleFor(x => x.LastAuditDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Last audit date cannot be in the future");

        RuleFor(x => x)
            .Must(HaveValidAuditDates).WithMessage("Next audit date must be after last audit date");
    }

    private static bool HaveValidAuditDates(CreateSupplierDto dto)
    {
        return dto.NextAuditDate > dto.LastAuditDate;
    }
}

public class UpdateSupplierDtoValidator : AbstractValidator<UpdateSupplierDto>
{
    public UpdateSupplierDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required")
            .MaximumLength(200).WithMessage("Supplier name cannot exceed 200 characters");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactEmail))
            .WithMessage("Invalid email format");

        RuleFor(x => x.EnvironmentalScore)
            .InclusiveBetween(0, 100).WithMessage("Environmental score must be between 0 and 100");

        RuleFor(x => x.SocialScore)
            .InclusiveBetween(0, 100).WithMessage("Social score must be between 0 and 100");

        RuleFor(x => x.GovernanceScore)
            .InclusiveBetween(0, 100).WithMessage("Governance score must be between 0 and 100");

        RuleFor(x => x.NextAuditDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Next audit date must be in the future");

        RuleFor(x => x.LastAuditDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Last audit date cannot be in the future");

        RuleFor(x => x)
            .Must(HaveValidAuditDates).WithMessage("Next audit date must be after last audit date");
    }

    private static bool HaveValidAuditDates(UpdateSupplierDto dto)
    {
        return dto.NextAuditDate > dto.LastAuditDate;
    }
}

public class SupplierFilterDtoValidator : AbstractValidator<SupplierFilterDto>
{
    public SupplierFilterDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.MinESGScore)
            .InclusiveBetween(0, 100).When(x => x.MinESGScore.HasValue)
            .WithMessage("Minimum ESG score must be between 0 and 100");

        RuleFor(x => x.MaxESGScore)
            .InclusiveBetween(0, 100).When(x => x.MaxESGScore.HasValue)
            .WithMessage("Maximum ESG score must be between 0 and 100");

        RuleFor(x => x)
            .Must(HaveValidESGScoreRange).WithMessage("Maximum ESG score must be greater than minimum ESG score")
            .When(x => x.MinESGScore.HasValue && x.MaxESGScore.HasValue);
    }

    private static bool HaveValidESGScoreRange(SupplierFilterDto dto)
    {
        return !dto.MinESGScore.HasValue || !dto.MaxESGScore.HasValue || dto.MaxESGScore >= dto.MinESGScore;
    }
}