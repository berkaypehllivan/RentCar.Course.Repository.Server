using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Vehicles;

namespace RentCarServer.Application.Services;

public static class VehicleQueryExtensions
{
    ///<summary>
    ///sys_admin tüm şubelerin araçlarını görür,
    ///diğer kullanıcılar sadece kendi şubelerindeki araçları görür.
    /// </summary>
    public static IQueryable<Vehicle> ForCurrentBranch(this IQueryable<Vehicle> query, IClaimContext claimContext)
    {
        if (claimContext.IsSysAdmin())
            return query;

        var branchId = claimContext.GetBranchId();
        return query.Where(p => p.BranchId == branchId);
    }

    ///<summary>GetAllWithAudit() ile gelen sorgular için ayrı kural (MapTo'dan önce çağırılmalı.)</summary>
    public static IQueryable<EntityWithAuditDto<Vehicle>> ForCurrentBranch(this IQueryable<EntityWithAuditDto<Vehicle>> query, IClaimContext claimContext)
    {
        if (claimContext.IsSysAdmin())
            return query;

        var branchId = claimContext.GetBranchId();
        return query.Where(p => p.Entity.BranchId.Value == branchId);
    }
}
