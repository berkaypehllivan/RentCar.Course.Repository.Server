using FluentEmail.Core;
using RentCarServer.Application.Services;

namespace RentCarServer.Insfractructure.Services;

internal sealed class MailService(IFluentEmail fluentEmail) : IMailService
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        var sendResponse = await fluentEmail.To(to).Subject(subject).Body(body, isHtml: true).SendAsync(cancellationToken = default);

        if (!sendResponse.Successful)
            throw new ArgumentException(string.Join(", ", sendResponse.ErrorMessages));
    }
}
