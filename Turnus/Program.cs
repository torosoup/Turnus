using Microsoft.EntityFrameworkCore;
using Turnus.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("TurnusContext") ?? throw new InvalidOperationException("Connection string 'TurnusContext' not found.");

Console.WriteLine(connectionString);

builder.Services.AddDbContext<TurnusContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TurnusContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed Manager role and assign to configured email
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<TurnusContext>();
    await db.Database.MigrateAsync();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync("Manager"))
    {
        await roleManager.CreateAsync(new IdentityRole("Manager"));
    }

    var managerEmail = builder.Configuration["Seeding:ManagerEmail"];
    if (!string.IsNullOrEmpty(managerEmail))
    {
        var managerUser = await userManager.FindByEmailAsync(managerEmail);
        if (managerUser != null && !await userManager.IsInRoleAsync(managerUser, "Manager"))
        {
            await userManager.AddToRoleAsync(managerUser, "Manager");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
