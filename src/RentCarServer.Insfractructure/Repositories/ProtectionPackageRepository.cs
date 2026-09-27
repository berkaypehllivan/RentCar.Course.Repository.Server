using RentCarServer.Domain.ProtectionPackages;
using RentCarServer.Insfractructure.Abstractions;
using RentCarServer.Insfractructure.Context;

namespace RentCarServer.Insfractructure.Repositories;

internal sealed class ProtectionPackageRepository : AuditableRepository<ProtectionPackage, ApplicationDbContext>, IProtectionPackageRepository
{
    public ProtectionPackageRepository(ApplicationDbContext context) : base(context)
    {
    }
}
