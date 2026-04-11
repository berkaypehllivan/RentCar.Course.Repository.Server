using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarServer.Domain.Users;

namespace RentCarServer.Insfractructure.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(i => i.Id);
        builder.ComplexProperty(i => i.FirstName);
        builder.ComplexProperty(i => i.LastName);
        builder.ComplexProperty(i => i.Password);
        builder.ComplexProperty(i => i.FullName);
        builder.ComplexProperty(i => i.Email);
        builder.ComplexProperty(i => i.UserName);
        builder.ComplexProperty(i => i.ForgotPasswordCode);
        builder.ComplexProperty(i => i.ForgotPasswordDate);
        builder.ComplexProperty(i => i.IsForgotPasswordCompleted);

    }
}