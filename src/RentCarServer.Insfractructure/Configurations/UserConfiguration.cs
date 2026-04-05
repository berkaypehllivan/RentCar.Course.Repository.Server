using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarServer.Domain.Users;

namespace RentCarServer.Insfractructure.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // 1. Sadece ID Birincil Anahtardır
        builder.HasKey(i => i.Id);

        // 2. Value Object olanları ComplexProperty olarak tanımlıyoruz
        // (Eğer .NET 8 altındaysanız OwnsOne kullanmalısınız)
        builder.ComplexProperty(i => i.FirstName);
        builder.ComplexProperty(i => i.LastName);
        builder.ComplexProperty(i => i.Password);

        // 3. Eğer bunlar da Value Object ise ComplexProperty yapın, 
        // Yok eğer sadece string ise Property() kullanın veya hiç yazmanıza gerek yok.
        builder.ComplexProperty(i => i.FullName);
        builder.ComplexProperty(i => i.Email);
        builder.ComplexProperty(i => i.UserName);
    }
}