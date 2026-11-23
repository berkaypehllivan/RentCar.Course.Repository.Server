using FluentValidation;
using GenericRepository;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Customers;
using RentCarServer.Domain.Customers.ValueObjects;
using RentCarServer.Domain.Shared;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.CustomerPortal;

public sealed record CustomerProfileUpdateCommand(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Email,
    string PhoneNumber,
    string City,
    string District,
    string FullAddress,
    DateOnly DriverLicenceIssueDate) : IRequest<Result<string>>;

public sealed class CustomerProfileUpdateCommandValidator : AbstractValidator<CustomerProfileUpdateCommand>
{
    public CustomerProfileUpdateCommandValidator()
    {
        RuleFor(p => p.FirstName).NotEmpty().WithMessage("Geçerli bir ad girin.");
        RuleFor(p => p.LastName).NotEmpty().WithMessage("Geçerli bir soyad girin.");
        RuleFor(p => p.DateOfBirth).NotEmpty().WithMessage("Geçerli bir doğum tarihi girin.");
        RuleFor(p => p.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta adresi girin.");
        RuleFor(p => p.PhoneNumber).NotEmpty().WithMessage("Geçerli bir telefon numarası girin.");
        RuleFor(p => p.City).NotEmpty().WithMessage("Şehir bilgisi boş olamaz.");
        RuleFor(p => p.District).NotEmpty().WithMessage("İlçe bilgisi boş olamaz.");
        RuleFor(p => p.FullAddress).NotEmpty().WithMessage("Açık adres bilgisi boş olamaz.");
        RuleFor(p => p.DriverLicenceIssueDate)
            .NotEmpty()
            .LessThan(DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("Geçerli bir ehliyet veriliş tarihi giriniz!");
    }
}

internal sealed class CustomerProfileUpdateCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    IClaimContext claimContext) : IRequestHandler<CustomerProfileUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerProfileUpdateCommand request, CancellationToken cancellationToken)
    {
        var customerId = claimContext.GetUserId();
        Customer? customer = await customerRepository.FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);

        if (customer is null)
            return Result<string>.Failure("Müşteri bulunamadı.");

        if (!string.Equals(customer.Email.Value, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            bool emailExists = await customerRepository.AnyAsync(
                x => x.Id != customerId && x.Email.Value == request.Email,
                cancellationToken);
            if (emailExists)
                return Result<string>.Failure("Bu e-posta adresi ile bir müşteri zaten kayıtlı. Farklı bir adres deneyin.");
        }

        customer.SetFirstName(new FirstName(request.FirstName));
        customer.SetLastName(new LastName(request.LastName));
        customer.SetDateOfBirth(new DateOfBirth(request.DateOfBirth));
        customer.SetEmail(new Email(request.Email));
        customer.SetPhoneNumber(new PhoneNumber(request.PhoneNumber));
        customer.SetAddress(new Address(request.City, request.District, request.FullAddress));
        customer.SetDriverLicenceIssueDate(new DriverLicenceIssueDate(request.DriverLicenceIssueDate));

        customerRepository.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Profil bilgileriniz başarıyla güncellendi.";
    }
}