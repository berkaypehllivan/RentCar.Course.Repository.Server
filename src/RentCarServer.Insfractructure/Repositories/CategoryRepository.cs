using RentCarServer.Domain.Categories;
using RentCarServer.Insfractructure.Abstractions;
using RentCarServer.Insfractructure.Context;

namespace RentCarServer.Insfractructure.Repositories;

internal sealed class CategoryRepository : AuditableRepository<Category, ApplicationDbContext>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }
}