using FluentValidation;
using GenericRepository;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Auth;

public sealed record ForgotPasswordCommand(string Email) : IRequest<Result<string>>;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Geçerli bir email adresi giriniz!")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz!");

    }
}

internal sealed class ForgotPasswordHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IMailService mailService) : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(p => p.Email.Value == request.Email, cancellationToken);

        if (user == null)
            return Result<string>.Failure("Kullanıcı bulunamadı!");

        user.CreateForgotPasswordId();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        string to = user.Email.Value;
        string subject = "Şifre Sıfırla";
        string body = @"<!DOCTYPE html>
<html lang=""tr"">

<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Şifre Sıfırlama - RentCar</title>
    <style>
        /* Reset */
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333333;
            background-color: #f8f9fa;
        }

        /* Container */
        .email-container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
        }

        /* Header */
        .email-header {
            background: linear-gradient(135deg, #ff6b35 0%, #e85a2f 100%);
            padding: 30px 20px;
            text-align: center;
        }

        .logo {
            color: white;
            font-size: 28px;
            font-weight: bold;
            margin-bottom: 10px;
        }

        .header-text {
            color: rgba(255, 255, 255, 0.9);
            font-size: 16px;
        }

        /* Content */
        .email-content {
            padding: 40px 30px;
        }

        .greeting {
            font-size: 18px;
            font-weight: 600;
            color: #2c3e50;
            margin-bottom: 20px;
        }

        .message {
            font-size: 16px;
            color: #555555;
            margin-bottom: 30px;
            line-height: 1.6;
        }

        /* Security Alert */
        .security-alert {
            background: #fff3cd;
            border: 1px solid #ffeaa7;
            border-radius: 8px;
            padding: 15px;
            margin: 25px 0;
            display: flex;
            align-items: flex-start;
            gap: 10px;
        }

        .alert-icon {
            color: #856404;
            font-size: 18px;
            margin-top: 2px;
        }

        .alert-content {
            flex: 1;
        }

        .alert-title {
            font-weight: 600;
            color: #856404;
            margin-bottom: 5px;
        }

        .alert-text {
            font-size: 14px;
            color: #856404;
            line-height: 1.4;
        }

        /* Reset Button */
        .reset-button-container {
            text-align: center;
            margin: 35px 0;
        }

        .reset-button {
            display: inline-block;
            background: linear-gradient(135deg, #ff6b35 0%, #e85a2f 100%);
            color: white;
            text-decoration: none;
            padding: 15px 40px;
            border-radius: 50px;
            font-size: 16px;
            font-weight: 600;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(255, 107, 53, 0.3);
        }

        .reset-button:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(255, 107, 53, 0.4);
            color: white;
            text-decoration: none;
        }

        /* Alternative Link */
        .alternative-link {
            background: #f8f9fa;
            border: 1px solid #e9ecef;
            border-radius: 8px;
            padding: 20px;
            margin: 25px 0;
            text-align: center;
        }

        .alternative-title {
            font-size: 14px;
            color: #6c757d;
            margin-bottom: 10px;
        }

        .link-text {
            font-size: 12px;
            color: #495057;
            word-break: break-all;
            background: white;
            padding: 10px;
            border-radius: 4px;
            border: 1px solid #dee2e6;
        }

        /* Info Section */
        .info-section {
            background: #e3f2fd;
            border-radius: 8px;
            padding: 20px;
            margin: 25px 0;
        }

        .info-title {
            font-weight: 600;
            color: #1565c0;
            margin-bottom: 10px;
            font-size: 16px;
        }

        .info-list {
            margin: 0;
            padding-left: 20px;
        }

        .info-list li {
            color: #1976d2;
            font-size: 14px;
            margin-bottom: 5px;
        }

        /* Footer */
        .email-footer {
            background: #2c3e50;
            color: white;
            padding: 30px 20px;
            text-align: center;
        }

        .footer-content {
            margin-bottom: 20px;
        }

        .company-info {
            font-size: 16px;
            font-weight: 600;
            margin-bottom: 10px;
        }

        .contact-info {
            font-size: 14px;
            color: #bdc3c7;
            margin-bottom: 15px;
        }

        .social-links {
            margin: 20px 0;
        }

        .social-links a {
            color: #bdc3c7;
            text-decoration: none;
            margin: 0 10px;
            font-size: 14px;
        }

        .footer-note {
            font-size: 12px;
            color: #95a5a6;
            border-top: 1px solid #34495e;
            padding-top: 15px;
            margin-top: 20px;
        }

        /* Responsive */
        @media only screen and (max-width: 600px) {
            .email-container {
                margin: 10px;
                border-radius: 5px;
            }

            .email-content {
                padding: 25px 20px;
            }

            .reset-button {
                padding: 12px 30px;
                font-size: 15px;
            }

            .greeting {
                font-size: 16px;
            }

            .message {
                font-size: 15px;
            }
        }
    </style>
</head>

<body>
    <div class=""email-container"">
        <!-- Header -->
        <div class=""email-header"">
            <div class=""logo"">🚗 RentCar</div>
            <div class=""header-text"">Güvenli Araç Kiralama Platformu</div>
        </div>

        <!-- Content -->
        <div class=""email-content"">
            <div class=""greeting"">Merhaba {UserName},</div>

            <div class=""message"">
                Hesabınız için şifre sıfırlama talebinde bulundunuz. Aşağıdaki butona tıklayarak yeni şifrenizi
                belirleyebilirsiniz.
            </div>

            <!-- Security Alert -->
            <div class=""security-alert"">
                <div class=""alert-icon"">⚠️</div>
                <div class=""alert-content"">
                    <div class=""alert-title"">Güvenlik Uyarısı</div>
                    <div class=""alert-text"">
                        Eğer bu işlemi siz yapmadıysanız, bu e-postayı görmezden gelin ve derhal müşteri hizmetlerimizle
                        iletişime geçin.
                    </div>
                </div>
            </div>

            <!-- Reset Button -->
            <div class=""reset-button-container"">
                <a href=""{ResetPasswordUrl}"" target=""_blank"" class=""reset-button"">
                    🔒 Şifremi Sıfırla
                </a>
            </div>

            <!-- Alternative Link -->
            <div class=""alternative-link"">
                <div class=""alternative-title"">Butona tıklayamıyorsanız, aşağıdaki bağlantıyı kopyalayıp tarayıcınıza
                    yapıştırın:</div>
                <div class=""link-text"">{ResetPasswordUrl}</div>
            </div>

            <!-- Info Section -->
            <div class=""info-section"">
                <div class=""info-title"">📋 Önemli Bilgiler:</div>
                <ul class=""info-list"">
                    <li>Bu bağlantı güvenlik nedeniyle <strong>24 saat</strong> sonra geçersiz olacaktır</li>
                    <li>Bağlantı sadece <strong>tek kullanımlık</strong>tır</li>
                    <li>Şifrenizi değiştirdikten sonra tüm cihazlardan çıkış yapmanızı öneririz</li>
                    <li>Güvenli bir şifre seçmeyi unutmayın (en az 8 karakter, büyük/küçük harf, rakam)</li>
                </ul>
            </div>

            <div class=""message"">
                Herhangi bir sorunuz varsa, müşteri hizmetlerimizle <strong>7/24</strong> iletişime geçebilirsiniz.
            </div>
        </div>

        <!-- Footer -->
        <div class=""email-footer"">
            <div class=""footer-content"">
                <div class=""company-info"">RentCar - Güvenli Araç Kiralama</div>
                <div class=""contact-info"">
                    📞 0850 222 3344 | 📧 destek@rentcar.com<br>
                    📍 Cumhuriyet Mah. Atatürk Cad. No:123, Kayseri
                </div>
                <div class=""social-links"">
                    <a href=""#"">Facebook</a> |
                    <a href=""#"">Instagram</a> |
                    <a href=""#"">Twitter</a> |
                    <a href=""#"">LinkedIn</a>
                </div>
            </div>
            <div class=""footer-note"">
                Bu e-posta otomatik olarak gönderilmiştir. Lütfen bu e-postaya yanıt vermeyin.<br>
                © 2025 RentCar. Tüm hakları saklıdır. | <a href=""#"" style=""color: #95a5a6;"">Gizlilik Politikası</a> | <a
                    href=""#"" style=""color: #95a5a6;"">İletişim</a>
            </div>
        </div>
    </div>
</body>

</html>";

        body = body.Replace("{UserName}", user.FirstName.Value + " " + user.LastName.Value);
        body = body.Replace("{ResetPasswordUrl}", $"http://localhost:4200/forgot-password/{user.ForgotPasswordId!.Value}");
        await mailService.SendAsync(to, subject, body, cancellationToken = default);

        return "Şifre sıfırlama mailiniz gönderilmiştir. Lütfen email adresinizi kontrol ediniz!";
    }
}