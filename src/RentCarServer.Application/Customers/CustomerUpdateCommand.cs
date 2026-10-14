using FluentValidation;
using GenericRepository;
using RentCarServer.Application.Behaviors;
using RentCarServer.Domain.Customers;
using RentCarServer.Domain.Customers.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Customers;

[Permission("customer:update")]
public sealed record CustomerUpdateCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string IdentityNumber,
    DateOnly DateOfBirth,
    string Email,
    string PhoneNumber,
    string Address,
    DateOnly DriverLicenceIssueDate,
    bool IsActive
) : IRequest<Result<string>>;

public sealed class CustomerUpdateCommandValidator : AbstractValidator<CustomerUpdateCommand>
{
    public CustomerUpdateCommandValidator()
    {
        RuleFor(p => p.Id).NotEmpty().WithMessage("Geçerli bir müşteri seçin.");
        RuleFor(p => p.FirstName).NotEmpty().WithMessage("Geçerli bir ad girin.");
        RuleFor(p => p.LastName).NotEmpty().WithMessage("Geçerli bir soyad girin.");
        RuleFor(p => p.IdentityNumber).NotEmpty().MaximumLength(11).WithMessage("Geçerli bir kimlik numarası girin.");
        RuleFor(p => p.DriverLicenceIssueDate).NotEmpty().LessThan(DateOnly.FromDateTime(DateTime.Now)).WithMessage("Geçerli bir ehliyet veriliş tarihi giriniz!");
    }
}

internal sealed class CustomerUpdateCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork) : IRequestHandler<CustomerUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerUpdateCommand request, CancellationToken cancellationToken)
    {
        Customer? customer = await repository.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (customer is null)
            return Result<string>.Failure("Müşteri bulunamadı.");

        if (!string.Equals(customer.IdentityNumber.Value, request.IdentityNumber, StringComparison.OrdinalIgnoreCase))
        {
            bool identityExists = await repository.AnyAsync(x => x.IdentityNumber.Value == request.IdentityNumber, cancellationToken);
            if (identityExists)
                return Result<string>.Failure("Bu kimlik numarası ile bir müşteri zaten mevcut.");
        }

        FirstName firstName = new(request.FirstName);
        LastName lastName = new(request.LastName);
        IdentityNumber identityNumber = new(request.IdentityNumber);
        DateOfBirth dateOfBirth = new(request.DateOfBirth);
        Email email = new(request.Email);
        PhoneNumber phoneNumber = new(request.PhoneNumber);
        Address address = new(request.Address);
        DriverLicenceIssueDate driverLicenceIssueDate = new(request.DriverLicenceIssueDate);

        customer.SetFirstName(firstName);
        customer.SetLastName(lastName);
        customer.SetIdentityNumber(identityNumber);
        customer.SetDateOfBirth(dateOfBirth);
        customer.SetEmail(email);
        customer.SetPhoneNumber(phoneNumber);
        customer.SetAddress(address);
        customer.SetStatus(request.IsActive);
        customer.SetDriverLicenceIssueDate(driverLicenceIssueDate);

        repository.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Müşteri başarıyla güncellendi.";
    }
}