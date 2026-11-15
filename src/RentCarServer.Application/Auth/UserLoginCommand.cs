using FluentValidation;
using GenericRepository;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Auth;

public sealed record UserLoginCommand(
    string EmailOrUserName,
    string Password
    ) : IRequest<Result<UserLoginCommandResponse>>;

public sealed record UserLoginCommandResponse
{
    public string? Token { get; set; }
    public string? TFACode { get; set; }
}
public sealed class UserLoginCommandValidator : AbstractValidator<UserLoginCommand>
{
    public UserLoginCommandValidator()
    {
        RuleFor(p => p.EmailOrUserName).NotEmpty().WithMessage("Geçerli bir kullanıcı adı veya email adresi giriniz!");
        RuleFor(p => p.Password).NotEmpty().WithMessage("Geçerli bir şifre giriniz!");
    }
}

public sealed class UserLoginCommandHandler(
    IUserRepository userRepository, IJwtProvider jwtProvider, IMailService mailService, IUnitOfWork unitOfWork) : IRequestHandler<UserLoginCommand, Result<UserLoginCommandResponse>>
{
    public async Task<Result<UserLoginCommandResponse>> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(p =>
            p.Email.Value == request.EmailOrUserName
            || p.UserName.Value == request.EmailOrUserName, cancellationToken);

        if (user is null || !user.VerifyPasswordHash(request.Password))
            return Result<UserLoginCommandResponse>.Failure(401, "Kullanıcı adı veya şifre hatalı!");

        if (!user.TFAStatus.Value)
        {
            var token = await jwtProvider.CreateTokenAsync(user, cancellationToken);
            return new UserLoginCommandResponse { Token = token };
        }

        user.CreateTFACode();
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var body = $@"Uygulamaya girmek için aşağıdaki kodu kullanabilirsiniz. Kodun geçerlilik süresi 5 dakikadır.
<p><h4>{user.TFAConfirmCode!.Value}</h4></p>";
        await mailService.SendAsync(user.Email.Value, "Giriş onayı", body);

        return new UserLoginCommandResponse { TFACode = user.TFACode!.Value };
    }
}