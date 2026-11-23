using FluentValidation;

namespace RentCarServer.Application.Extentions;

public static class PasswordRuleExtensions
{
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> rule) => rule
        .NotEmpty().WithMessage("Şifre boş olamaz!")
        .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır!")
        .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir!")
        .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir!")
        .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir!");
}
