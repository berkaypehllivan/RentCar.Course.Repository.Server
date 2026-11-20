using Microsoft.EntityFrameworkCore;
using RentCarServer.Domain.Branchs;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Booking;

public sealed record BranchListQuery() : IRequest<Result<List<BranchListDto>>>;

public sealed record BranchListDto(Guid Id, string Name);

internal sealed class BranchListQueryHandler(IBranchRepository branchRepository) : IRequestHandler<BranchListQuery, Result<List<BranchListDto>>>
{
    public async Task<Result<List<BranchListDto>>> Handle(BranchListQuery request, CancellationToken cancellationToken)
    {
        var branches = await branchRepository
            .Where(b => b.IsActive)
            .Select(b => new BranchListDto(b.Id, b.Name.Value))
            .ToListAsync(cancellationToken);

        return branches;
    }
}