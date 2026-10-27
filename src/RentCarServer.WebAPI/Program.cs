using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.RateLimiting;
using RentCarServer.Application;
using RentCarServer.Insfractructure;
using RentCarServer.WebAPI;
using RentCarServer.WebAPI.Controllers;
using RentCarServer.WebAPI.Middlewares;
using RentCarServer.WebAPI.Modules;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVİS KAYITLARI (DEPENDENCY INJECTION) ---

builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddRateLimiter(cfr =>
{
    cfr.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromSeconds(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    cfr.AddFixedWindowLimiter("login-fixed", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    cfr.AddFixedWindowLimiter("forgot-password-fixed", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromMinutes(5);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    cfr.AddFixedWindowLimiter("reset-password-fixed", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    cfr.AddFixedWindowLimiter("check-forgot-password-code-fixed", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services
    .AddControllers()
    .AddOData(opt =>
        opt.Select().Filter().Count().Expand().OrderBy().SetMaxTop(null).AddRouteComponents("odata", MainODataController.GetEdmModel())
    );

builder.Services.AddResponseCompression(opt =>
{
    opt.EnableForHttps = true;
});

builder.Services.AddExceptionHandler<ExceptionHandler>().AddProblemDetails();

builder.Services.AddTransient<CheckTokenMiddleware>();
builder.Services.AddHostedService<CheckLoginTokenBackgroundService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));

if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();
}
app.UseStaticFiles();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CheckTokenMiddleware>();

app.MapControllers()
    .RequireRateLimiting("fixed")
    .RequireAuthorization();
app.MapAuth();
app.MapBranch();
app.MapRole();
app.MapPermission();
app.MapUser();
app.MapCategory();
app.MapProtectionPackage();
app.MapExtra();
app.MapVehicle();
app.MapSeedData();
app.MapCustomer();
app.MapReservation();

app.MapGet("/", () => "Hello world").RequireAuthorization();


await app.CleanRemovedPermissionsFromRoleAsync();
app.Run();