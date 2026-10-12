using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarServer.Domain.Vehicles;

namespace RentCarServer.Insfractructure.Configurations
{
    internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("Vehicles");
            builder.HasKey(x => x.Id);

            builder.ComplexProperty(x => x.Brand);
            builder.ComplexProperty(x => x.Model);
            builder.ComplexProperty(x => x.ModelYear);
            builder.ComplexProperty(x => x.Color);
            builder.ComplexProperty(x => x.Plate);
            builder.ComplexProperty(x => x.CategoryId);
            builder.ComplexProperty(x => x.BranchId);
            builder.ComplexProperty(x => x.VinNumber);
            builder.ComplexProperty(x => x.EngineNumber);
            builder.ComplexProperty(x => x.Description);
            builder.ComplexProperty(x => x.ImageUrl);
            builder.ComplexProperty(x => x.FuelType);
            builder.ComplexProperty(x => x.Transmission);

            builder.ComplexProperty(x => x.EngineVolume, y =>
            {
                y.Property(p => p.Value).HasColumnType("decimal(18,2)");
            });
            builder.ComplexProperty(x => x.EnginePower);
            builder.ComplexProperty(x => x.TractionType);

            builder.ComplexProperty(x => x.FuelConsumption, y =>
            {
                y.Property(p => p.Value).HasColumnType("decimal(18,2)");
            });
            builder.ComplexProperty(x => x.SeatCount);
            builder.ComplexProperty(x => x.Kilometer);
            builder.ComplexProperty(x => x.DailyPrice);

            builder.ComplexProperty(x => x.WeeklyDiscountRate, y =>
            {
                y.Property(p => p.Value).HasColumnType("decimal(18,2)");
            });

            builder.ComplexProperty(x => x.MonthlyDiscountRate, y =>
            {
                y.Property(p => p.Value).HasColumnType("decimal(18,2)");
            });
            builder.ComplexProperty(x => x.InsuranceType);
            builder.ComplexProperty(x => x.LastMaintanceDate);
            builder.ComplexProperty(x => x.LastMaintanceKm);
            builder.ComplexProperty(x => x.NextMaintanceKm);
            builder.ComplexProperty(x => x.InspectionDate);
            builder.ComplexProperty(x => x.InsuranceEndDate);
            builder.ComplexProperty(x => x.CascoEndDate);
            builder.ComplexProperty(x => x.TireStatus);
            builder.ComplexProperty(x => x.GeneralStatus);
            builder.OwnsMany(x => x.Features);
        }
    }
}
