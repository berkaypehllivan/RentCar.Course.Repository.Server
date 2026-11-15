using GenericRepository;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Auth;

public sealed record UserLoginWithTFACommand(
    string EmailOrUserName,
    string TFACode,
    string TFAConfirmCode
    ) : IRequest<Result<UserLoginCommandResponse>>;

public sealed class UserLoginWithTFACommandHandler(IUserRepository userRepository, IJwtProvider jwtProvider, IUnitOfWork unitOfWork) : IRequestHandler<UserLoginWithTFACommand, Result<UserLoginCommandResponse>>
{
    public async Task<Result<UserLoginCommandResponse>> Handle(UserLoginWithTFACommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(p =>
        p.Email.Value == request.EmailOrUserName
        || p.UserName.Value == request.EmailOrUserName);

        if (user is null)
            return Result<UserLoginCommandResponse>.Failure("Kullanıcı adı veya şifre hatalı!");

        if (user.TFAIsCompleted is null
            || user.TFAExpiresDate is null
            || user.TFACode is null
            || user.TFAConfirmCode is null
            || user.TFAIsCompleted.Value
            || user.TFAExpiresDate.Value < DateTimeOffset.Now
            || (user.TFAConfirmCode.Value != request.TFAConfirmCode
            || user.TFACode.Value != request.TFACode))
            return Result<UserLoginCommandResponse>.Failure("TFA kodu geçersiz!");

        user.SetTFAIsCompleted();
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = await jwtProvider.CreateTokenAsync(user, cancellationToken);
        var res = new UserLoginCommandResponse { Token = token };
        return res;
    }
}