using Microsoft.EntityFrameworkCore;
using RentCarServer.Domain.ProtectionPackages;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Booking;

public sealed record ProtectionPackageListQuery() : IRequest<Result<List<ProtectionPackageListDto>>>;

public sealed record ProtectionPackageListDto(
    Guid Id,
    string Name,
    decimal Price,
    bool IsRecommended,
    List<string> Coverages);

internal sealed class ProtectionPackageListQueryHandler(
    IProtectionPackageRepository protectionPackageRepository) : IRequestHandler<ProtectionPackageListQuery, Result<List<ProtectionPackageListDto>>>
{
    public async Task<Result<List<ProtectionPackageListDto>>> Handle(ProtectionPackageListQuery request, CancellationToken cancellationToken)
    {
        var packages = await protectionPackageRepository
                .Where(p => p.IsActive)
                .OrderBy(p => p.OrderNumber.Value)
                .Select(p => new
                {
                    p.Id,
                    Name = p.Name.Value,
                    Price = p.Price.Value,
                    IsRecommended = p.IsRecommended.Value,
                    p.Coverages
                })
                .ToListAsync(cancellationToken);

        var dtos = packages
            .Select(p => new ProtectionPackageListDto
            (
                p.Id,
                p.Name,
                p.Price,
                p.IsRecommended,
                p.Coverages.Select(c => c.Name).ToList()))
            .ToList();

        return dtos;
    }
}