using FluentValidation;
using GenericRepository;
using RentCarServer.Application.Behaviors;
using RentCarServer.Domain.Customers;
using RentCarServer.Domain.Customers.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Customers;

[Permission("customer:create")]
public sealed record CustomerCreateCommand(
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

public sealed class CustomerCreateCommandValidator : AbstractValidator<CustomerCreateCommand>
{
    public CustomerCreateCommandValidator()
    {
        RuleFor(p => p.FirstName).NotEmpty().WithMessage("Geçerli bir ad giriniz!");
        RuleFor(p => p.LastName).NotEmpty().WithMessage("Geçerli bir soyad giriniz!");
        RuleFor(p => p.IdentityNumber).NotEmpty().MaximumLength(11).WithMessage("Geçerli bir kimlik numarası giriniz!");
        RuleFor(p => p.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz!");
        RuleFor(p => p.PhoneNumber).NotEmpty().WithMessage("Geçerli bir telefon numarası giriniz!");
        RuleFor(p => p.DriverLicenceIssueDate).NotEmpty().LessThan(DateOnly.FromDateTime(DateTime.Now)).WithMessage("Geçerli bir ehliyet veriliş tarihi giriniz!");
    }
}

internal sealed class CustomerCreateCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork) : IRequestHandler<CustomerCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerCreateCommand request, CancellationToken cancellationToken)
    {
        if (await repository.AnyAsync(p => p.IdentityNumber.Value == request.IdentityNumber, cancellationToken))
            return Result<string>.Failure("Bu kimlik numarası ile bir müşteri zaten mevcut.");

        FirstName firstName = new(request.FirstName);
        LastName lastName = new(request.LastName);
        IdentityNumber identityNumber = new(request.IdentityNumber);
        DateOfBirth dateOfBirth = new(request.DateOfBirth);
        Email email = new(request.Email);
        PhoneNumber phoneNumber = new(request.PhoneNumber);
        Address address = new(request.Address);
        DriverLicenceIssueDate driverLicenceIssueDate = new(request.DriverLicenceIssueDate);

        Customer customer = new(
            firstName,
            lastName,
            identityNumber,
            dateOfBirth,
            email,
            phoneNumber,
            address,
            driverLicenceIssueDate,
            request.IsActive
        );

        repository.Add(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Müşteri başarıyla kaydedildi.";
    }
}