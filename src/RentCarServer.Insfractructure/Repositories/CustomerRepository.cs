using RentCarServer.Domain.Customers;
using RentCarServer.Insfractructure.Abstractions;
using RentCarServer.Insfractructure.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace RentCarServer.Insfractructure.Repositories
{
    internal sealed class CustomerRepository : AuditableRepository<Customer, ApplicationDbContext>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
