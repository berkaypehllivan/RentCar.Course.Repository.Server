using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarServer.Domain.Customers;

namespace RentCarServer.Insfractructure.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.Id);

        builder.ComplexProperty(x => x.FirstName);
        builder.ComplexProperty(x => x.LastName);
        builder.ComplexProperty(x => x.IdentityNumber);
        builder.ComplexProperty(x => x.DateOfBirth);
        builder.ComplexProperty(x => x.Email);
        builder.ComplexProperty(x => x.PhoneNumber);
        builder.ComplexProperty(x => x.Address);
        builder.ComplexProperty(x => x.DriverLicenceIssueDate);
    }
}
