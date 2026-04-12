using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarServer.Domain.LoginTokens;

namespace RentCarServer.Insfractructure.Configurations;

internal sealed class LoginTokenConfiguration : IEntityTypeConfiguration<LoginToken>
{
    public void Configure(EntityTypeBuilder<LoginToken> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ComplexProperty(p => p.Token);
        builder.ComplexProperty(p => p.ExpiresDate);
        builder.ComplexProperty(p => p.IsActive);
    }
}
