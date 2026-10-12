using RentCarServer.Domain.Vehicles;
using RentCarServer.Insfractructure.Abstractions;
using RentCarServer.Insfractructure.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace RentCarServer.Insfractructure.Repositories
{
    internal sealed class VehicleRepository : AuditableRepository<Vehicle, ApplicationDbContext>, IVehicleRepository
    {
        public VehicleRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
