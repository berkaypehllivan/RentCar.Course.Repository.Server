using RentCarServer.Domain.Abstractions;

namespace RentCarServer.Application.Services;

public interface IVehicleAvailabilityService
{
    Task<bool> IsAvailableAsync(
        IdentityId vehicleId,
        DateTime pickUpDateTime,
        DateTime deliveryDateTime,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<IdentityId>> GetUnavailableVehicleIdsAsync(
        IEnumerable<IdentityId> vehicleIds,
        DateTime pickUpDateTime,
        DateTime deliveryDateTime,
        CancellationToken cancellationToken = default);
}
