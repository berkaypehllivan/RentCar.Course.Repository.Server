using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarServer.Domain.Extras;

namespace RentCarServer.Insfractructure.Configurations;

internal sealed class ExtraConfiguration : IEntityTypeConfiguration<Extra>
{
    public void Configure(EntityTypeBuilder<Extra> builder)
    {
        builder.ToTable("Extras");
        builder.HasKey(x => x.Id);
        builder.ComplexProperty(i => i.Name);
        builder.ComplexProperty(i => i.Price);
        builder.ComplexProperty(i => i.Description);
    }
}