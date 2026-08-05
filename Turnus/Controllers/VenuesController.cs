using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Policy = "WorkspaceManager")]
    public class VenuesController : Controller
    {
        private readonly TurnusContext _context;
        private readonly Turnus.Services.ICurrentWorkspaceProvider _workspaceProvider;

        public VenuesController(TurnusContext context, Turnus.Services.ICurrentWorkspaceProvider workspaceProvider)
        {
            _context = context;
            _workspaceProvider = workspaceProvider;
        }

        public async Task<IActionResult> Index()
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            return View(await _context.Venue
                .Include(v => v.Departments)
                .Where(v => v.WorkspaceId == wsId.Value)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var venue = await _context.Venue
                .Include(v => v.Departments)
                .FirstOrDefaultAsync(v => v.Id == id && v.WorkspaceId == wsId.Value);

            if (venue == null) return NotFound();

            return View(venue);
        }

        public async Task<IActionResult> Create()
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            return PartialView("~/Views/Admin/Partials/Configuration/Venue/_CreateVenue.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Venue venue)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            if (ModelState.IsValid)
            {
                venue.CreatedAt = DateTime.UtcNow;
                venue.WorkspaceId = wsId.Value;
                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null || venue.WorkspaceId != wsId.Value) return NotFound();

            return PartialView("~/Views/Admin/Partials/Configuration/Venue/_EditVenue.cshtml", venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedAt")] Venue venue)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            if (id != venue.Id) return NotFound();

            var existing = await _context.Venue.FindAsync(id);
            if (existing == null || existing.WorkspaceId != wsId.Value) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    venue.WorkspaceId = wsId.Value;
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Venue.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                // Preserve venue context after editing
                return RedirectToAction("Dashboard", "Admin", new { venueId = venue.Id });
            }
            return PartialView("~/Views/Admin/Partials/Configuration/Venue/_EditVenue.cshtml", venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var venue = await _context.Venue
                .Include(v => v.Departments)
                .FirstOrDefaultAsync(v => v.Id == id && v.WorkspaceId == wsId.Value);

            if (venue == null) return NotFound();

            return PartialView("~/Views/Admin/Partials/Configuration/Venue/_DeleteVenue.cshtml", venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null || venue.WorkspaceId != wsId.Value) return NotFound();

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();
            // After deleting a venue, the venue no longer exists; redirect to dashboard without venue context
            return RedirectToAction("Dashboard", "Admin");
        }
    }
}