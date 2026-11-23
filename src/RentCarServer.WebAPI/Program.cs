using Microsoft.AspNetCore.OData;
using RentCarServer.Application;
using RentCarServer.Insfractructure;
using RentCarServer.WebAPI;
using RentCarServer.WebAPI.Controllers;
using RentCarServer.WebAPI.Middlewares;
using RentCarServer.WebAPI.Modules;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;
using TS.Result;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVİS KAYITLARI (DEPENDENCY INJECTION) ---

builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var limitFactor = builder.Environment.IsDevelopment() ? 20 : 1;

builder.Services.AddRateLimiter(cfr =>
{
    cfr.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    cfr.OnRejected = async (context, cancellationToken) =>
    {
        var message = "Çok fazla deneme yaptınız. Lütfen biraz bekleyip tekrar deneyin.";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            var seconds = (int)Math.Ceiling(retryAfter.TotalSeconds);
            context.HttpContext.Response.Headers.RetryAfter = seconds.ToString();
            message = $"Çok fazla deneme yaptınız. Lütfen {seconds} saniye sonra tekrar deneyin.";
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            Result<string>.Failure(StatusCodes.Status429TooManyRequests, message),
            cancellationToken);
    };

    cfr.AddIpFixedWindowPolicy("fixed", 100, TimeSpan.FromSeconds(1));
    cfr.AddIpFixedWindowPolicy("login-fixed", 5 * limitFactor, TimeSpan.FromMinutes(1));
    cfr.AddIpFixedWindowPolicy("change-password-fixed", 5 * limitFactor, TimeSpan.FromMinutes(1));
    cfr.AddIpFixedWindowPolicy("register-fixed", 5 * limitFactor, TimeSpan.FromHours(1));
    cfr.AddIpFixedWindowPolicy("forgot-password-fixed", 3 * limitFactor, TimeSpan.FromMinutes(5));
    cfr.AddIpFixedWindowPolicy("reset-password-fixed", 3 * limitFactor, TimeSpan.FromMinutes(1));
    cfr.AddIpFixedWindowPolicy("check-forgot-password-code-fixed", 5 * limitFactor, TimeSpan.FromMinutes(1));

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

//UI Part
app.MapBooking();
app.MapCustomerPortal();

app.MapBranch();
app.MapRole();
app.MapPermission();
app.MapUser();
app.MapCategory();
app.MapProtectionPackage();
app.MapExtra();
app.MapVehicle();
app.MapCustomer();
app.MapReservation();
app.MapReservationForm();
app.MapDashboard();


app.MapGet("/", () => "Hello world").RequireAuthorization();


await app.CleanRemovedPermissionsFromRoleAsync();
app.Run();