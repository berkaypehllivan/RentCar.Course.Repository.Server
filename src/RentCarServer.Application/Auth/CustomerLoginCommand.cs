using FluentValidation;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Customers;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Auth;

public sealed record CustomerLoginCommand(
    string Email,
    string Password) : IRequest<Result<CustomerLoginCommandResponse>>;

public sealed record CustomerLoginCommandResponse
{
    public string? Token { get; set; }
}


public sealed class CustomerLoginCommandValidator : AbstractValidator<CustomerLoginCommand>
{
    public CustomerLoginCommandValidator()
    {
        RuleFor(p => p.Email).NotEmpty().WithMessage("Geçerli bir e-posta adresi giriniz.");
        RuleFor(p => p.Password).NotEmpty().WithMessage("Geçerli bir şifre giriniz.");
    }

}
public sealed class CustomerLoginCommandHandler(
    ICustomerRepository customerRepository,
    IJwtProvider jwtProvider) : IRequestHandler<CustomerLoginCommand, Result<CustomerLoginCommandResponse>>
{
    public async Task<Result<CustomerLoginCommandResponse>> Handle(CustomerLoginCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.FirstOrDefaultAsync(p => p.Email.Value == request.Email, cancellationToken);

        if (customer is null || !customer.VerifyPasswordHash(request.Password))
            return Result<CustomerLoginCommandResponse>.Failure(401, "E-posta veya şifre hatalı.");

        var token = await jwtProvider.CreateTokenAsync(customer, cancellationToken);

        return new CustomerLoginCommandResponse { Token = token };
    }
}