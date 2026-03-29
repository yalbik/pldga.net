using PLDGA.Infrastructure;
using PLDGA.Application.Interfaces;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var dataDirectory = Environment.GetEnvironmentVariable("APP_DATA_PATH")
    ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDirectory);
builder.Services.AddInfrastructure(dataDirectory);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery();

var app = builder.Build();

// Seed default season and admin if empty
using (var scope = app.Services.CreateScope())
{
    var seasonRepo = scope.ServiceProvider.GetRequiredService<ISeasonRepository>();
    var current = await seasonRepo.GetCurrentAsync();
    if (current == null)
    {
        var year = DateTime.UtcNow.Year;
        await seasonRepo.AddAsync(new Season
        {
            Year = year,
            StartDate = new DateTime(year, 1, 1),
            EndDate = new DateTime(year, 12, 31),
            IsCurrent = true
        });
    }

    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var admin = await userRepo.GetByUsernameAsync("admin");
    if (admin == null)
    {
        await authService.RegisterAsync(new PLDGA.Application.DTOs.CreateMemberDto
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@pldga.net",
            Username = "admin",
            Password = "Admin123!",
            IsAdmin = true,
            IsPaid = true
        });
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
