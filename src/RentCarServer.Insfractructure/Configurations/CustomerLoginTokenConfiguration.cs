using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarServer.Domain.LoginTokens;

namespace RentCarServer.Insfractructure.Configurations;

internal sealed class CustomerLoginTokenConfiguration : IEntityTypeConfiguration<CustomerLoginToken>
{
    public void Configure(EntityTypeBuilder<CustomerLoginToken> builder)
    {
        builder.ToTable("CustomerLoginTokens");
        builder.HasKey(x => x.Id);
        builder.ComplexProperty(p => p.Token);
        builder.ComplexProperty(p => p.ExpiresDate);
        builder.ComplexProperty(p => p.IsActive);
    }
}
