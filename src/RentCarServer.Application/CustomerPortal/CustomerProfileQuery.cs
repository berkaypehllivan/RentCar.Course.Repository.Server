using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Customers;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Customers;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.CustomerPortal;

public sealed record CustomerProfileQuery() : IRequest<Result<CustomerDto>>;

internal sealed class CustomerProfileQueryHandler(ICustomerRepository customerRepository, IClaimContext claimContext) : IRequestHandler<CustomerProfileQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CustomerProfileQuery request, CancellationToken cancellationToken)
    {
        var customerId = claimContext.GetUserId();

        var res = await customerRepository
            .GetAllWithAudit()
            .MapTo()
            .Where(p => p.Id == customerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (res is null)
            return Result<CustomerDto>.Failure("Müşteri bilgisi bulunamadı.");

        return res;

    }
}