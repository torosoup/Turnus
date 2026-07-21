using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Roles = "Manager")]
    public class VenuesController : Controller
    {
        private readonly TurnusContext _context;

        public VenuesController(TurnusContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Venue
                .Include(v => v.Departments)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue
                .Include(v => v.Departments)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venue == null) return NotFound();

            return View(venue);
        }

        public IActionResult Create()
        {
            return PartialView("~/Views/Admin/Partials/Configuration/Venue/_CreateVenue.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Venue venue)
        {
            if (ModelState.IsValid)
            {
                venue.CreatedAt = DateTime.UtcNow;
                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            return PartialView("~/Views/Admin/Partials/Configuration/Venue/_EditVenue.cshtml", venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedAt")] Venue venue)
        {
            if (id != venue.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Venue.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Dashboard", "Admin");
            }
            return PartialView("~/Views/Admin/Partials/Configuration/Venue/_EditVenue.cshtml", venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue
                .Include(v => v.Departments)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venue == null) return NotFound();

            return PartialView("~/Views/Admin/Partials/Configuration/Venue/_DeleteVenue.cshtml", venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue != null) _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard", "Admin");
        }
    }
}