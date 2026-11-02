using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarServer.Domain.Reservations;

namespace RentCarServer.Insfractructure.Configurations;

internal class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        builder.HasKey(x => x.Id);

        builder.ComplexProperty(p => p.ReservationNumber);
        builder.ComplexProperty(p => p.PickUpDate);
        builder.ComplexProperty(p => p.PickUpTime);
        builder.ComplexProperty(p => p.DeliveryDate);
        builder.ComplexProperty(p => p.DeliveryTime);
        builder.ComplexProperty(p => p.TotalDay);
        builder.ComplexProperty(p => p.VehicleDailyPrice);
        builder.ComplexProperty(p => p.ProtectionPackagePrice);
        builder.OwnsMany(p => p.ReservationExtras);
        builder.ComplexProperty(p => p.Note);
        builder.ComplexProperty(p => p.PaymentInformation);
        builder.ComplexProperty(p => p.Status);
        builder.ComplexProperty(p => p.Total);
        builder.ComplexProperty(p => p.PickUpDateTime);
        builder.ComplexProperty(p => p.DeliveryDateTime);
        builder.OwnsMany(p => p.ReservationHistories);
        builder.OwnsOne(p => p.PickUpForm, modelBuilder =>
        {
            modelBuilder.OwnsOne(i => i.Kilometer);
            modelBuilder.OwnsMany(i => i.ImageUrls, x =>
            {
                x.ToTable("PickUpForm_ImageUrls");
            });

            modelBuilder.OwnsMany(i => i.Supplies, x =>
            {
                x.ToTable("PickUpForm_Supplies");
            });
            modelBuilder.OwnsMany(i => i.Damages, x =>
            {
                x.ToTable("PickUpForm_Damages");
            });

            modelBuilder.OwnsOne(i => i.Note, x =>
            {
                x.ToTable("PickUpForm_Note");
            });
        });
        builder.OwnsOne(p => p.DeliveryForm, modelBuilder =>
        {
            modelBuilder.OwnsOne(i => i.Kilometer);
            modelBuilder.OwnsMany(i => i.ImageUrls, x =>
            {
                x.ToTable("DeliveryForm_ImageUrls");
            });

            modelBuilder.OwnsMany(i => i.Supplies, x =>
            {
                x.ToTable("DeliveryForm_Supplies");
            });

            modelBuilder.OwnsMany(i => i.Damages, x =>
            {
                x.ToTable("DeliveryForm_Damages");
            });

            modelBuilder.OwnsOne(i => i.Note, x =>
            {
                x.ToTable("DeliveryForm_Note");
            });
        });
    }
}
