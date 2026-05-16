using GenericRepository;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Users;
using RentCarServer.Domain.Users.ValueObjects;

namespace RentCarServer.WebAPI;

public static class ExtensionMethod
{
    public static async Task CreateFirstUser(this WebApplication app)
    {
        using var scoped = app.Services.CreateScope();
        var userRepository = scoped.ServiceProvider.GetRequiredService<IUserRepository>();
        var unitOfWork = scoped.ServiceProvider.GetRequiredService<IUnitOfWork>();

        if (!(await userRepository.AnyAsync(p => p.UserName.Value == "admin")))
        {
            FirstName firstName = new("Berkay");
            LastName lastName = new("Pehlivan");
            Email email = new("berkaypehlivan75@gmail.com");
            UserName userName = new("admin");
            Password password = new("123");

            var user = new User(
                firstName,
                lastName,
                email,
                userName,
                password);

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
