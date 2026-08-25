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
// Register IHttpContextAccessor and workspace provider
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Turnus.Services.ICurrentWorkspaceProvider, Turnus.Services.CurrentWorkspaceProvider>();
// Authorization policies for workspace tenancy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WorkspaceMember", policy =>
        policy.Requirements.Add(new Turnus.Services.Authorization.WorkspaceMemberRequirement()));

    options.AddPolicy("WorkspaceManager", policy =>
        policy.Requirements.Add(new Turnus.Services.Authorization.WorkspaceManagerRequirement()));
});

// Register authorization handlers
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Turnus.Services.Authorization.WorkspaceMemberHandler>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Turnus.Services.Authorization.WorkspaceManagerHandler>();

var app = builder.Build();


try
{
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

    // Ensure a global SuperAdmin role exists for cross-workspace administration (hybrid approach)
    if (!await roleManager.RoleExistsAsync("SuperAdmin"))
    {
        await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
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

    // Optionally seed a SuperAdmin user (configured separately)
    var superAdminEmail = builder.Configuration["Seeding:SuperAdminEmail"];
    if (!string.IsNullOrEmpty(superAdminEmail))
    {
        var superUser = await userManager.FindByEmailAsync(superAdminEmail);
        if (superUser != null && !await userManager.IsInRoleAsync(superUser, "SuperAdmin"))
        {
            await userManager.AddToRoleAsync(superUser, "SuperAdmin");
        }
    }
    // Tenant onboarding is now user-driven. We no longer create a default workspace
    // or backfill existing entities into a default workspace at startup. This
    // prevents automatically assigning every user to a single workspace.
    Console.WriteLine("Workspace auto-seeding/backfill is disabled. Workspaces must be created or joined by users.");
}
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization failed: {ex.Message}");
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
// Resolve workspace early in the pipeline so DbContext query filters can use the value
app.UseMiddleware<Turnus.Middleware.WorkspaceResolutionMiddleware>();
app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
