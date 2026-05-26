using GenericRepository;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Branches;
using RentCarServer.Domain.Branchs;
using RentCarServer.Domain.Roles;
using RentCarServer.Domain.Shared;
using RentCarServer.Domain.Users;
using RentCarServer.Domain.Users.ValueObjects;

namespace RentCarServer.WebAPI;

public static class ExtensionMethod
{
    public static async Task CreateFirstUser(this WebApplication app)
    {
        using var scoped = app.Services.CreateScope();
        var srv = scoped.ServiceProvider;

        var userRepository = srv.GetRequiredService<IUserRepository>();
        var roleRepository = srv.GetRequiredService<IRoleRepository>();
        var branchRepository = srv.GetRequiredService<IBranchRepository>();

        var unitOfWork = scoped.ServiceProvider.GetRequiredService<IUnitOfWork>();

        Branch? branch = await branchRepository.FirstOrDefaultAsync(i => i.Name.Value == "Merkez Şube");
        Role? role = await roleRepository.FirstOrDefaultAsync(i => i.Name.Value == "sys_admin");

        if (branch is null)
        {
            Name name = new("Merkez Şube");
            Address address = new("Manisa", "Saruhanlı", "Saruhanlı Merkez");
            Contact contact = new(
                "5071073662",
                "5060668882",
                "info@rentcar.com");
            branch = new(name, address, contact, true);
            branchRepository.Add(branch);
        }

        if (role is null)
        {
            Name name = new("sys_admin");
            role = new(name, true);
            roleRepository.Add(role);

        }

        if (!(await userRepository.AnyAsync(p => p.UserName.Value == "admin")))
        {
            FirstName firstName = new("Berkay");
            LastName lastName = new("Pehlivan");
            Email email = new("berkaypehlivan75@gmail.com");
            UserName userName = new("admin");
            Password password = new("123");
            IdentityId branchId = branch.Id;
            IdentityId roleId = role.Id;

            var user = new User(
                firstName,
                lastName,
                email,
                userName,
                password,
                branchId,
                roleId,
                true);

            userRepository.Add(user);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public static async Task CleanRemovedPermissionsFromRoleAsync(this WebApplication application)
    {
        using var scoped = application.Services.CreateScope();
        var srv = scoped.ServiceProvider;
        var service = srv.GetRequiredService<PermissionCleanerService>();
        await service.CleanRemovedPermissionsFromRolesAsync();
    }
}
