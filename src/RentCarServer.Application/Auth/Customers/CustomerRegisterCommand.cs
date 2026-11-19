using FluentValidation;
using GenericRepository;
using RentCarServer.Domain.Customers;
using RentCarServer.Domain.Customers.ValueObjects;
using RentCarServer.Domain.Shared;
using TS.MediatR;
using TS.Result;
using Email = RentCarServer.Domain.Customers.ValueObjects.Email;
using FirstName = RentCarServer.Domain.Customers.ValueObjects.FirstName;
using LastName = RentCarServer.Domain.Customers.ValueObjects.LastName;

namespace RentCarServer.Application.Auth.Customers;

public sealed record CustomerRegisterCommand(
    string FirstName,
    string LastName,
    string IdentityNumber,
    DateOnly DateOfBirth,
    string Email,
    string PhoneNumber,
    string City,
    string District,
    string FullAddress,
    DateOnly DriverLicenceIssueDate,
    string Password
) : IRequest<Result<string>>;

public sealed class CustomerRegisterCommandValidator : AbstractValidator<CustomerRegisterCommand>
{
    public CustomerRegisterCommandValidator()
    {
        RuleFor(p => p.FirstName).NotEmpty().WithMessage("Geçerli bir ad giriniz!");
        RuleFor(p => p.LastName).NotEmpty().WithMessage("Geçerli bir soyad giriniz!");
        RuleFor(p => p.IdentityNumber).NotEmpty().MaximumLength(11).WithMessage("Geçerli bir kimlik numarası giriniz!");
        RuleFor(p => p.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz!");
        RuleFor(p => p.PhoneNumber).NotEmpty().WithMessage("Geçerli bir telefon numarası giriniz!");
        RuleFor(p => p.City).NotEmpty().WithMessage("Geçerli bir şehir giriniz!");
        RuleFor(p => p.District).NotEmpty().WithMessage("Geçerli bir ilçe giriniz!");
        RuleFor(p => p.FullAddress).NotEmpty().WithMessage("Geçerli bir adres giriniz!");
        RuleFor(p => p.DriverLicenceIssueDate).NotEmpty().LessThan(DateOnly.FromDateTime(DateTime.Now)).WithMessage("Geçerli bir ehliyet veriliş tarihi giriniz!");
        RuleFor(p => p.Password).NotEmpty().MinimumLength(6).WithMessage("Şifreniz en az 6 karakter olmalıdır!");
    }
}

internal sealed class CustomerRegisterCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork) : IRequestHandler<CustomerRegisterCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerRegisterCommand request, CancellationToken cancellationToken)
    {
        if (await repository.AnyAsync(p => p.IdentityNumber.Value == request.IdentityNumber, cancellationToken))
            return Result<string>.Failure("Bu kimlik numarası ile bir müşteri zaten mevcut.");

        if (await repository.AnyAsync(p => p.Email.Value == request.Email, cancellationToken))
            return Result<string>.Failure("Bu e-posta adresi ile bir müşteri zaten mevcut.");

        FirstName firstName = new(request.FirstName);
        LastName lastName = new(request.LastName);
        IdentityNumber identityNumber = new(request.IdentityNumber);
        DateOfBirth dateOfBirth = new(request.DateOfBirth);
        Email email = new(request.Email);
        PhoneNumber phoneNumber = new(request.PhoneNumber);
        Address address = new(request.City, request.District, request.FullAddress);
        Password password = new(request.Password);
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
            password,
            true
        );

        repository.Add(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Kayıt işlemi başarılı! Giriş yapabilirsiniz.";
    }
}