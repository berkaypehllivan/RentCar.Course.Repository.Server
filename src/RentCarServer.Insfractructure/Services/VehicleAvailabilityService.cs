using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.ValueObjects;

namespace RentCarServer.Insfractructure.Services;

internal sealed class VehicleAvailabilityService(IReservationRepository reservationRepository) : IVehicleAvailabilityService
{
    public async Task<bool> IsAvailableAsync(IdentityId vehicleId, DateTime pickUpDateTime, DateTime deliveryDateTime, CancellationToken cancellationToken = default)
    {
        var unavailable = await GetUnavailableVehicleIdsAsync(
            new[] { vehicleId }, pickUpDateTime, deliveryDateTime, cancellationToken);

        return !unavailable.Contains(vehicleId);
    }
    public async Task<IReadOnlyCollection<IdentityId>> GetUnavailableVehicleIdsAsync(IEnumerable<IdentityId> vehicleIds, DateTime pickUpDateTime, DateTime deliveryDateTime, CancellationToken cancellationToken = default)
    {
        var vehicleIdList = vehicleIds.ToList();

        var possibleOverlaps = await reservationRepository
            .Where(r => vehicleIdList.Contains(r.VehicleId)
            && (r.Status.Value == Status.Pending.Value || r.Status.Value == Status.Delivered.Value))
            .Select(s => new
            {
                s.VehicleId,
                DeliveyDate = s.DeliveryDate.Value,
                DeliveryTime = s.DeliveryTime.Value,
                PickUpDate = s.PickUpDate.Value,
                PickUpTime = s.PickUpTime.Value,
            })
            .ToListAsync(cancellationToken);

        var unavailableVehicleIds = possibleOverlaps
            .Where(r =>
            pickUpDateTime < r.DeliveyDate.ToDateTime(r.DeliveryTime).AddHours(1) &&
            deliveryDateTime > r.PickUpDate.ToDateTime(r.PickUpTime))
            .Select(r => r.VehicleId)
            .Distinct()
            .ToList();

        return unavailableVehicleIds;
    }
}
