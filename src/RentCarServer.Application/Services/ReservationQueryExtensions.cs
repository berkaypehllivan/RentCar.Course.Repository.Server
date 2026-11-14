using RentCarServer.Domain.Reservations;

namespace RentCarServer.Application.Services;

public static class ReservationQueryExtensions
{
    ///<summary>
    ///sys_admin tüm şubelerin rezervasyonlarını görebilir,
    ///diğer kullanıcılar sadece kendi şubelerinin rezervasyonlarını görebilir.
    /// </summary>

    public static IQueryable<Reservation> ForCurrentBranch(
        this IQueryable<Reservation> query,
        IClaimContext claimContext)
    {
        if (claimContext.IsSysAdmin())
            return query;

        var branchId = claimContext.GetBranchId();
        return query.Where(r => r.PickUpLocationId == branchId);
    }
}
