using RentCarServer.Application.Behaviors;
using RentCarServer.Domain.Customers;
using TS.MediatR;

namespace RentCarServer.Application.Customers;

[Permission("customer:view")]
public sealed record CustomerGetAllQuery : IRequest<IQueryable<CustomerDto>>;

internal sealed class CustomerGetAllQueryHandler(
    ICustomerRepository customerRepository)
    : IRequestHandler<CustomerGetAllQuery, IQueryable<CustomerDto>>
{
    public Task<IQueryable<CustomerDto>> Handle(CustomerGetAllQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(
            customerRepository
                .GetAllWithAudit()
                .MapTo()
                .AsQueryable()
        );
}