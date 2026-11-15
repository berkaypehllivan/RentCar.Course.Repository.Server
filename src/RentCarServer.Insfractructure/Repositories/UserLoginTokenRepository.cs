using GenericRepository;
using RentCarServer.Domain.LoginTokens;
using RentCarServer.Insfractructure.Context;

namespace RentCarServer.Insfractructure.Repositories;

internal sealed class UserLoginTokenRepository : Repository<UserLoginToken, ApplicationDbContext>, IUserLoginTokenRepository
{
    public UserLoginTokenRepository(ApplicationDbContext context) : base(context)
    {
    }
}
