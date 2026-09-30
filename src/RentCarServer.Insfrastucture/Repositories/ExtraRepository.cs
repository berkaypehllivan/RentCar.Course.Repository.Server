using RentCarServer.Domain.Extras;
using RentCarServer.Insfractructure.Abstractions;
using RentCarServer.Insfractructure.Context;

namespace RentCarServer.Insfractructure.Repositories;

internal sealed class ExtraRepository : AuditableRepository<Extra, ApplicationDbContext>, IExtraRepository
{
    public ExtraRepository(ApplicationDbContext context) : base(context)
    {
    }
}
