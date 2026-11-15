using GenericRepository;
using RentCarServer.Domain.LoginTokens;
using RentCarServer.Insfractructure.Context;

namespace RentCarServer.Insfractructure.Repositories;

internal sealed class CustomerLoginTokenRepository : Repository<CustomerLoginToken, ApplicationDbContext>, ICustomerLoginTokenRepository
{
    public CustomerLoginTokenRepository(ApplicationDbContext context) : base(context)
    {
    }
}
