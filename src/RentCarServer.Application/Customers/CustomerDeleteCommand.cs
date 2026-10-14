using GenericRepository;
using RentCarServer.Application.Behaviors;
using RentCarServer.Domain.Customers;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Customers;

[Permission("customer:delete")]
public sealed record CustomerDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class CustomerDeleteCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CustomerDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerDeleteCommand request, CancellationToken cancellationToken)
    {
        Customer? customer = await customerRepository.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (customer is null)
            return Result<string>.Failure("Müşteri bulunamadı!");

        customer.Delete();
        customerRepository.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Müşteri başarıyla silindi!";
    }
}