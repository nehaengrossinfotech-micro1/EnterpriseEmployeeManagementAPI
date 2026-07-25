using EnterpriseEmployeeManagementAPI.Models.DTOs;
using FluentValidation;

namespace EnterpriseEmployeeManagementAPI.Validators;

public sealed class UpdateEmployeeRequestValidator
    : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(request => request.EmployeeNumber)
            .NotEmpty()
            .MaximumLength(30)
            .Matches("^[A-Za-z0-9-]+$");
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.JobTitle).NotEmpty().MaximumLength(150);
        RuleFor(request => request.DepartmentId).NotEmpty();
        RuleFor(request => request.HireDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
