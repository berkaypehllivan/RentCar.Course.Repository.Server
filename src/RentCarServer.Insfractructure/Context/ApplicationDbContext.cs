using GenericRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Roles;
using RentCarServer.Domain.Users;
using System.Security.Claims;

namespace RentCarServer.Insfractructure.Context;

internal sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    private static IdentityId? _systemUserId;
    private static readonly SemaphoreSlim _systemUserLock = new(1, 1);

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.ApplyGlobalFilters();
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<IdentityId>().HaveConversion<IdentityIdValueConverter>();
        configurationBuilder.Properties<decimal>().HaveColumnType("money");
        configurationBuilder.Properties<string>().HaveColumnType("nvarchar(MAX)");
        configurationBuilder.Properties<TimeOnly>().HaveColumnType("time(7)");
        base.ConfigureConventions(configurationBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Entity>().ToList();

        HttpContextAccessor httpContextAccessor = new();
        string? userIdString =
            httpContextAccessor
            .HttpContext?
            .User
            .Claims
            .FirstOrDefault(p => p.Type == ClaimTypes.NameIdentifier)?
            .Value;

        IdentityId? loggedInUserId = userIdString is not null ? new IdentityId(Guid.Parse(userIdString)) : null;

        bool needSystemUser = loggedInUserId is null && entries.Any(e => e.State is EntityState.Added or EntityState.Modified);

        IdentityId? effectiveUserId = loggedInUserId;
        if (needSystemUser)
            effectiveUserId = await GetSystemUserIdAsync(cancellationToken);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(p => p.CreatedAt).CurrentValue = DateTimeOffset.Now;
                entry.Property(p => p.CreatedBy).CurrentValue = effectiveUserId;
            }
            if (entry.State == EntityState.Modified)
            {
                if (entry.Property(p => p.IsDeleted).CurrentValue == true)
                {
                    entry.Property(p => p.DeletedAt).CurrentValue = DateTimeOffset.Now;
                    entry.Property(p => p.DeletedBy).CurrentValue = effectiveUserId;
                }
                else
                {
                    entry.Property(p => p.UpdatedAt).CurrentValue = DateTimeOffset.Now;
                    entry.Property(p => p.UpdatedBy).CurrentValue = effectiveUserId;
                }
            }

            if (entry.State == EntityState.Deleted)
            {
                throw new ArgumentException("Db'den direkt silme işlemi yapamazsınız");
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task<IdentityId?> GetSystemUserIdAsync(CancellationToken cancellationToken)
    {
        if (_systemUserId is not null) return _systemUserId;

        await _systemUserLock.WaitAsync(cancellationToken);
        try
        {
            if (_systemUserId is not null) return _systemUserId;

            var systemUserId = await (
                from user in Set<User>().AsNoTracking()
                join role in Set<Role>().AsNoTracking() on user.RoleId equals role.Id
                where role.Name.Value == "sys_admin"
                select user.Id
                ).FirstOrDefaultAsync(cancellationToken);

            _systemUserId = systemUserId;
            return _systemUserId;
        }
        finally
        {
            _systemUserLock.Release();
        }
    }
}


internal sealed class IdentityIdValueConverter : ValueConverter<IdentityId, Guid>
{
    public IdentityIdValueConverter() : base(m => m.Value, m => new IdentityId(m)) { }
}