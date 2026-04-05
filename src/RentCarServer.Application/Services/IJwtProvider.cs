using RentCarServer.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace RentCarServer.Application.Services;

public interface IJwtProvider
{
    string CreateToken(User user);
}
