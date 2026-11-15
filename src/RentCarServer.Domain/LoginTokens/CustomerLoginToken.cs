using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.LoginTokens.ValueObjects;

namespace RentCarServer.Domain.LoginTokens;

public sealed class CustomerLoginToken
{
    private CustomerLoginToken()
    {
    }

    public CustomerLoginToken(Token token, IdentityId customerId, ExpiresDate expiresDate)
    {
        Id = new(Guid.CreateVersion7());
        SetToken(token);
        SetCustomerId(customerId);
        SetIsActive(new(true));
        SetExpiresDate(expiresDate);
    }

    public IdentityId Id { get; private set; } = default!;
    public Token Token { get; private set; } = default!;
    public IdentityId CustomerId { get; private set; } = default!;
    public IsActive IsActive { get; private set; } = default!;
    public ExpiresDate ExpiresDate { get; private set; } = default!;

    #region Behaviors 

    public void SetIsActive(IsActive isActive) => IsActive = isActive;
    public void SetToken(Token token) => Token = token;
    public void SetCustomerId(IdentityId customerId) => CustomerId = customerId;
    public void SetExpiresDate(ExpiresDate expiresDate) => ExpiresDate = expiresDate;

    #endregion
}
