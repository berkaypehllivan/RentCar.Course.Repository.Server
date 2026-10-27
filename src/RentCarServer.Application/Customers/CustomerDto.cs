using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Customers;

namespace RentCarServer.Application.Customers;

public sealed class CustomerDto : EntityDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string IdentityNumber { get; set; } = default!;
    public DateOnly DateOfBirth { get; set; }
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Address { get; set; } = default!;
    public DateOnly DriverLicenceIssueDate { get; set; } = default!;
}

public static class CustomerExtensions
{
    public static IQueryable<CustomerDto> MapTo(this IQueryable<EntityWithAuditDto<Customer>> entities)
    {
        var res = entities.Select(s => new CustomerDto
        {
            Id = s.Entity.Id,
            FirstName = s.Entity.FirstName.Value,
            LastName = s.Entity.LastName.Value,
            FullName = s.Entity.FirstName.Value + " " + s.Entity.LastName.Value,
            IdentityNumber = s.Entity.IdentityNumber.Value,
            DateOfBirth = s.Entity.DateOfBirth.Value,
            Email = s.Entity.Email.Value,
            PhoneNumber = s.Entity.PhoneNumber.Value,
            Address = s.Entity.Address.Value,
            DriverLicenceIssueDate = s.Entity.DriverLicenceIssueDate.Value,
            IsActive = s.Entity.IsActive,
            CreatedAt = s.Entity.CreatedAt,
            CreatedBy = s.Entity.CreatedBy,
            CreatedFullName = s.CreatedUser.FullName.Value,
            UpdatedAt = s.Entity.UpdatedAt,
            UpdatedBy = s.Entity.UpdatedBy != null ? s.Entity.UpdatedBy!.Value : null,
            UpdatedFullName = s.UpdatedUser != null ? s.UpdatedUser.FullName.Value : null,
        })
        .AsQueryable();

        return res;
    }
}