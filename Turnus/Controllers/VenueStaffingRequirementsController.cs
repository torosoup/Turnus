using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class VenueStaffingRequirementsController : Controller
{
    private readonly TurnusContext _context;

    public VenueStaffingRequirementsController(TurnusContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.VenueStaffingRequirement
            .Include(v => v.Venue)
            .Include(v => v.Role)
            .ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var requirement = await _context.VenueStaffingRequirement
            .Include(v => v.Venue)
            .Include(v => v.Role)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (requirement == null) return NotFound();

        return View(requirement);
    }

    public IActionResult Create()
    {
        ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name");
        ViewData["RoleId"] = new SelectList(_context.Role, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,VenueId,RoleId,RequiredCount,IsShiftScoped")] VenueStaffingRequirement requirement)
    {
        if (ModelState.IsValid)
        {
            _context.Add(requirement);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", requirement.VenueId);
        ViewData["RoleId"] = new SelectList(_context.Role, "Id", "Name", requirement.RoleId);
        return View(requirement);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var requirement = await _context.VenueStaffingRequirement.FindAsync(id);
        if (requirement == null) return NotFound();

        ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", requirement.VenueId);
        ViewData["RoleId"] = new SelectList(_context.Role, "Id", "Name", requirement.RoleId);
        return View(requirement);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,VenueId,RoleId,RequiredCount,IsShiftScoped")] VenueStaffingRequirement requirement)
    {
        if (id != requirement.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(requirement);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VenueStaffingRequirementExists(requirement.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", requirement.VenueId);
        ViewData["RoleId"] = new SelectList(_context.Role, "Id", "Name", requirement.RoleId);
        return View(requirement);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var requirement = await _context.VenueStaffingRequirement
            .Include(v => v.Venue)
            .Include(v => v.Role)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (requirement == null) return NotFound();

        return View(requirement);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var requirement = await _context.VenueStaffingRequirement.FindAsync(id);
        if (requirement != null)
        {
            _context.VenueStaffingRequirement.Remove(requirement);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool VenueStaffingRequirementExists(int id)
    {
        return _context.VenueStaffingRequirement.Any(e => e.Id == id);
    }
}