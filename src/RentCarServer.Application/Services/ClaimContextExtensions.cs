namespace RentCarServer.Application.Services;

public static class ClaimContextExtensions
{
    public const string SysAdminRoleName = "sys_admin";

    /// <summary>Giriş yapan kullanıcı sys_admin rolünde mi?</summary>
    public static bool IsSysAdmin(this IClaimContext claimContext) => claimContext.GetRoleName() == SysAdminRoleName;

    ///<summary>sys_admin her şubeye, diğer kullanıcılar sadece kendi şubelerine erişebilir.</summary>
    public static bool CanAccessBranch(this IClaimContext claimContext, Guid branchId)
    => claimContext.IsSysAdmin() || claimContext.GetBranchId() == branchId;

}
