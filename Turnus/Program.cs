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

    // Create a default workspace if none exists and backfill existing data to that workspace
    var defaultWorkspaceName = builder.Configuration["Seeding:DefaultWorkspaceName"] ?? "Default Workspace";
    var defaultWorkspace = await db.Set<Workspace>().FirstOrDefaultAsync();
    if (defaultWorkspace == null)
    {
        defaultWorkspace = new Workspace
        {
            Name = defaultWorkspaceName,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = managerEmail
        };
        db.Set<Workspace>().Add(defaultWorkspace);
        await db.SaveChangesAsync();
    }

    // Backfill nullable WorkspaceId columns for existing records to the default workspace
    // Venues
    var venuesToUpdate = await db.Venue.Where(v => v.WorkspaceId == null).ToListAsync();
    foreach (var v in venuesToUpdate) v.WorkspaceId = defaultWorkspace.Id;
    // Departments
    var deptsToUpdate = await db.Department.Where(d => d.WorkspaceId == null).ToListAsync();
    foreach (var d in deptsToUpdate) d.WorkspaceId = defaultWorkspace.Id;
    // Roles
    var rolesToUpdate = await db.Role.Where(r => r.WorkspaceId == null).ToListAsync();
    foreach (var r in rolesToUpdate) r.WorkspaceId = defaultWorkspace.Id;
    // ShiftDefinitions
    var sdefsToUpdate = await db.ShiftDefinition.Where(s => s.WorkspaceId == null).ToListAsync();
    foreach (var s in sdefsToUpdate) s.WorkspaceId = defaultWorkspace.Id;
    // ScheduledShifts
    var schedsToUpdate = await db.ScheduledShift.Where(s => s.WorkspaceId == null).ToListAsync();
    foreach (var s in schedsToUpdate) s.WorkspaceId = defaultWorkspace.Id;
    // VenueStaffingRequirement
    var reqsToUpdate = await db.VenueStaffingRequirement.Where(r => r.WorkspaceId == null).ToListAsync();
    foreach (var r in reqsToUpdate) r.WorkspaceId = defaultWorkspace.Id;
    // ShiftAssignments
    var assignsToUpdate = await db.ShiftAssignment.Where(a => a.WorkspaceId == null).ToListAsync();
    foreach (var a in assignsToUpdate) a.WorkspaceId = defaultWorkspace.Id;
    // Availability
    var availsToUpdate = await db.Availability.Where(a => a.WorkspaceId == null).ToListAsync();
    foreach (var a in availsToUpdate) a.WorkspaceId = defaultWorkspace.Id;

    await db.SaveChangesAsync();

    // Seed WorkspaceMember entries for existing users: make the configured manager the Owner
    var users = userManager.Users.ToList();
    foreach (var u in users)
    {
        var exists = await db.WorkspaceMember.FindAsync(defaultWorkspace.Id, u.Id);
        if (exists == null)
        {
            /*
            var memberRole = WorkspaceRole.Member;
            if (!string.IsNullOrEmpty(managerEmail) && u.Email == managerEmail)
            {
                memberRole = WorkspaceRole.Owner;
            }
            */

            // Make the configured manager email the explicit Owner; others become Admin
            var memberRole = WorkspaceRole.Admin;
            if (!string.IsNullOrEmpty(managerEmail) && u.Email == managerEmail)
            {
                memberRole = WorkspaceRole.Owner;
            }

            db.WorkspaceMember.Add(new WorkspaceMember
            {
                WorkspaceId = defaultWorkspace.Id,
                UserId = u.Id,
                Role = memberRole,
                JoinedAt = DateTime.UtcNow
            });
        }
    }

    await db.SaveChangesAsync();
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
