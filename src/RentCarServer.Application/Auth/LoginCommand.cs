using FluentValidation;
using GenericRepository;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Auth;

public sealed record LoginCommand(
    string EmailOrUserName,
    string Password
    ) : IRequest<Result<LoginCommandResponse>>;

public sealed record LoginCommandResponse
{
    public string? Token { get; set; }
    public string? TFACode { get; set; }
}
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(p => p.EmailOrUserName).NotEmpty().WithMessage("Geçerli bir kullanıcı adı veya email adresi giriniz!");
        RuleFor(p => p.Password).NotEmpty().WithMessage("Geçerli bir şifre giriniz!");
    }
}

public sealed class LoginCommandHandler(
    IUserRepository userRepository, IJwtProvider jwtProvider, IMailService mailService, IUnitOfWork unitOfWork) : IRequestHandler<LoginCommand, Result<LoginCommandResponse>>
{
    public async Task<Result<LoginCommandResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(p =>
        p.Email.Value == request.EmailOrUserName
        || p.UserName.Value == request.EmailOrUserName);

        if (user is null)
            return Result<LoginCommandResponse>.Failure("Kullanıcı adı veya şifre hatalı!");

        var checkPassword = user.VerifyPasswordHash(request.Password);

        if (!checkPassword)
            return Result<LoginCommandResponse>.Failure("Kullanıcı adı veya şifre hatalı!");

        if (!user.TFAStatus.Value)
        {
            var token = await jwtProvider.CreateTokenAsync(user, cancellationToken);
            var res = new LoginCommandResponse() { Token = token };

            return res;
        }
        else
        {
            user.CreateTFACode();

            userRepository.Update(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            string to = user.Email.Value;
            string subject = "Giriş onayı";
            string body = $@"Uygulamaya girmek için aşağıdaki kodu kullanabilirsiniz. Kodun geçerlilik süresi 5 dakikadır. <p><h4>{user.TFAConfirmCode!.Value}<h4/><p/>";
            await mailService.SendAsync(to, subject, body);

            var res = new LoginCommandResponse() { TFACode = user.TFACode!.Value };
            return res;
        }


    }
}