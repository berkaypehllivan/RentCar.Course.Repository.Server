using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarServer.Domain.ProtectionPackages;

namespace RentCarServer.Insfractructure.Configurations;

internal sealed class ProtectionPackageConfiguration : IEntityTypeConfiguration<ProtectionPackage>
{
    public void Configure(EntityTypeBuilder<ProtectionPackage> builder)
    {
        builder.ToTable("ProtectionPackages");
        builder.HasKey(x => x.Id);
        builder.ComplexProperty(i => i.Name);
        builder.ComplexProperty(i => i.Price);
        builder.ComplexProperty(i => i.IsRecommended);
        builder.OwnsMany(i => i.Coverages);
    }
}
