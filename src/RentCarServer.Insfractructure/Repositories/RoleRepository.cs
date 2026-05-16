using RentCarServer.Domain.Roles;
using RentCarServer.Insfractructure.Abstractions;
using RentCarServer.Insfractructure.Context;

namespace RentCarServer.Insfractructure.Repositories;

internal sealed class RoleRepository : AuditableRepository<Role, ApplicationDbContext>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

}
