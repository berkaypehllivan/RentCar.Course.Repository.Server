using RentCarServer.Domain.Reservations;
using RentCarServer.Insfractructure.Abstractions;
using RentCarServer.Insfractructure.Context;

namespace RentCarServer.Insfractructure.Repositories;

internal sealed class ReservationRepository : AuditableRepository<Reservation, ApplicationDbContext>, IReservationRepository
{
    public ReservationRepository(ApplicationDbContext context) : base(context)
    {
    }
}
