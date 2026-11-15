using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarServer.Domain.LoginTokens;

namespace RentCarServer.Insfractructure.Configurations;

internal sealed class UserLoginTokenConfiguration : IEntityTypeConfiguration<UserLoginToken>
{
    public void Configure(EntityTypeBuilder<UserLoginToken> builder)
    {
        builder.ToTable("UserLoginTokens");
        builder.HasKey(x => x.Id);
        builder.ComplexProperty(p => p.Token);
        builder.ComplexProperty(p => p.ExpiresDate);
        builder.ComplexProperty(p => p.IsActive);
    }
}
