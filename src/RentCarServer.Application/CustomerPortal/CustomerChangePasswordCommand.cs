using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Extentions;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Customers;
using RentCarServer.Domain.LoginTokens;
using RentCarServer.Domain.Shared;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.CustomerPortal;

public sealed record CustomerChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : IRequest<Result<string>>;

public sealed class CustomerChangePasswordCommandValidator : AbstractValidator<CustomerChangePasswordCommand>
{
    public CustomerChangePasswordCommandValidator()
    {
        RuleFor(p => p.CurrentPassword).NotEmpty().WithMessage("Mevcut şifrenizi giriniz!");
        RuleFor(p => p.NewPassword).ValidPassword();
        RuleFor(p => p.NewPassword)
            .NotEqual(p => p.CurrentPassword)
            .WithMessage("Yeni şifre mevcut şifreyle aynı olamaz!");
    }
}

internal sealed class CustomerChangePasswordCommandHandler(
    ICustomerRepository customerRepository,
    ICustomerLoginTokenRepository loginTokenRepository,
    IClaimContext claimContext,
    IUnitOfWork unitOfWork) : IRequestHandler<CustomerChangePasswordCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var customerId = claimContext.GetUserId();

        var customer = await customerRepository.FirstOrDefaultAsync(p => p.Id == customerId, cancellationToken);

        if (customer is null)
            return Result<string>.Failure("Müşteri bilgisi bulunamadı.");

        if (!customer.VerifyPasswordHash(request.CurrentPassword))
            return Result<string>.Failure("Mevcut şifreniz hatalı.");

        customer.SetPassword(new Password(request.NewPassword));
        customerRepository.Update(customer);

        var loginTokens = await loginTokenRepository
            .Where(p => p.CustomerId == customerId && p.IsActive.Value == true)
            .ToListAsync(cancellationToken);

        foreach (var item in loginTokens)
        {
            item.SetIsActive(new(false));
        }
        loginTokenRepository.UpdateRange(loginTokens);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Şifreniz değiştirildi. Güvenliğiniz için tüm cihazlardaki oturumlarınız kapatıldı, lütfen tekrar giriş yapın.";
    }
}