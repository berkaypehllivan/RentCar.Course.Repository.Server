using GenericRepository;
using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Branches;
using System;
using System.Collections.Generic;
using System.Text;

namespace RentCarServer.Domain.Branchs;

public interface IBranchRepository : IAuditableRepository<Branch>
{
}
