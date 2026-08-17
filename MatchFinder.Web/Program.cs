using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MatchFinder.Domain.Entities;
using MatchFinder.Infrastructure.Data;
using MatchFinder.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection String & DbContext
builder.Services.AddDbContext<MatchFinderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<MatchFinderDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// 3. Register Application Services
builder.Services.AddScoped<IMatchService, MatchService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Matches}/{action=Index}/{id?}")
    .WithStaticAssets();

// Auto-migration & Role Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MatchFinderDbContext>();
    context.Database.Migrate();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    string[] roles = { "Admin", "Host", "Player" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }
}

app.Run();