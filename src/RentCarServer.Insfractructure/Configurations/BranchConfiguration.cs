using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarServer.Domain.Branches;

namespace RentCarServer.Insfractructure.Configurations;

internal class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.HasKey(x => x.Id);
        builder.ComplexProperty(i => i.Name);
        builder.ComplexProperty(i => i.Address);
    }
}
