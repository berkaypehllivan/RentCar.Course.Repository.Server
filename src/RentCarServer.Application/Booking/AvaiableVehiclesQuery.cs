using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Branchs;
using RentCarServer.Domain.Categories;
using RentCarServer.Domain.Vehicles;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Booking;

public sealed record AvaiableVehiclesQuery(
    Guid BranchId,
    DateOnly PickUpDate,
    TimeOnly PickUpTime,
    DateOnly DeliveryDate,
    TimeOnly DeliveryTime) : IRequest<Result<List<AvaiableVehicleDto>>>;

public sealed record AvaiableVehicleDto(
    Guid Id,
    string Brand,
    string Model,
    int ModelYear,
    string CategoryName,
    string Transmission,
    string FuelType,
    int SeatCount,
    string ImageUrl,
    decimal DailyPrice,
    int TotalDay,
    decimal TotalPrice
    );

public sealed class AvaiableVehiclesQueryValidator : AbstractValidator<AvaiableVehiclesQuery>
{
    public AvaiableVehiclesQueryValidator()
    {
        RuleFor(p => p.PickUpDate).NotEmpty().WithMessage("Geçerli bir alış tarihi giriniz.");
        RuleFor(p => p.DeliveryDate).NotEmpty().WithMessage("Geçerli bir teslim tarihi giriniz.");
        RuleFor(p => p)
            .Must(p => p.DeliveryDate.ToDateTime(p.DeliveryTime) > p.PickUpDate.ToDateTime(p.PickUpTime))
            .WithMessage("Teslim tarihi, alış tarihinden sonra olmalıdır.");
    }
}

internal sealed class AvaiableVehiclesQueryHandler(
    IBranchRepository branchRepository,
    IVehicleRepository vehicleRepository,
    ICategoryRepository categoryRepository,
    IVehicleAvailabilityService vehicleAvailabilityService) : IRequestHandler<AvaiableVehiclesQuery, Result<List<AvaiableVehicleDto>>>
{
    public async Task<Result<List<AvaiableVehicleDto>>> Handle(AvaiableVehiclesQuery request, CancellationToken cancellationToken)
    {
        var isBranchExists = await branchRepository.AnyAsync(i => i.Id == request.BranchId, cancellationToken);

        if (!isBranchExists)
            return Result<List<AvaiableVehicleDto>>.Failure("Şube bilgisi bulunamadı.");

        var pickUpDateTime = request.PickUpDate.ToDateTime(request.PickUpTime);
        var deliveryDateTime = request.DeliveryDate.ToDateTime(request.DeliveryTime);
        var branchId = new IdentityId(request.BranchId);
        var vehicles = await vehicleRepository
            .Where(v => v.BranchId == branchId && v.IsActive)
            .ToListAsync(cancellationToken);

        if (vehicles.Count == 0)
            return new List<AvaiableVehicleDto>();

        var vehicleIds = vehicles.Select(v => v.Id).ToList();
        var unavailableVehicleIds = await vehicleAvailabilityService.GetUnavailableVehicleIdsAsync(
            vehicleIds, pickUpDateTime, deliveryDateTime, cancellationToken);

        var availableVehicles = vehicles
            .Where(v => !unavailableVehicleIds.Contains(v.Id))
            .ToList();

        if (availableVehicles.Count == 0)
            return new List<AvaiableVehicleDto>();

        var categoryIds = availableVehicles.Select(v => v.CategoryId).Distinct().ToList();
        var categories = await categoryRepository
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var totalDay = CalculateTotalDay(pickUpDateTime, deliveryDateTime);

        var result = availableVehicles.Select(
            v =>
            {
                var category = categories.FirstOrDefault(c => c.Id == v.CategoryId);
                var dailyPrice = v.DailyPrice.Value;

                return new AvaiableVehicleDto(
                    v.Id,
                    v.Brand.Value,
                    v.Model.Value,
                    v.ModelYear.Value,
                    category?.Name.Value ?? string.Empty,
                    v.Transmission.Value,
                    v.FuelType.Value,
                    v.SeatCount.Value,
                    v.ImageUrl.Value,
                    dailyPrice,
                    totalDay,
                    dailyPrice * totalDay);
            }).ToList();

        return result;
    }

    private static int CalculateTotalDay(DateTime pickUpDateTime, DateTime deliveryDateTime)
    {
        var totalDays = (deliveryDateTime - pickUpDateTime).Days;
        var sameDayExtraAllowed = deliveryDateTime.TimeOfDay <= pickUpDateTime.TimeOfDay.Add(TimeSpan.FromHours(2));

        if (totalDays == 0 || (totalDays == 1 && sameDayExtraAllowed))
            return 1;

        return sameDayExtraAllowed ? totalDays : totalDays + 1;
    }
}