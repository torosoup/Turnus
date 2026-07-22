using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Roles = "Manager")]
    public class DepartmentsController : Controller
    {
        private readonly TurnusContext _context;

        public DepartmentsController(TurnusContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Department
                .Include(d => d.Venue)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Department
                .Include(d => d.Venue)
                .Include(d => d.ShiftDefinitions)
                .Include(d => d.VenueStaffingRequirements)
                    .ThenInclude(r => r.Role)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null) return NotFound();

            return View(department);
        }

        public IActionResult Create(int? venueId)
        {
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", venueId);
            return PartialView("~/Views/Admin/Partials/Configuration/Department/_CreateDepartment.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VenueId,Name")] Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard", "Admin", new { venueId = department.VenueId, departmentId = department.Id });
            }
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", department.VenueId);
            return PartialView("~/Views/Admin/Partials/Configuration/Department/_CreateDepartment.cshtml", department);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Department.FindAsync(id);
            if (department == null) return NotFound();

            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", department.VenueId);
            return PartialView("~/Views/Admin/Partials/Configuration/Department/_EditDepartment.cshtml", department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VenueId,Name")] Department department)
        {
            if (id != department.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Department.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Dashboard", "Admin");
            }
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", department.VenueId);
            return PartialView("~/Views/Admin/Partials/Configuration/Department/_EditDepartment.cshtml", department);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Department
                .Include(d => d.Venue)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null) return NotFound();

            return PartialView("~/Views/Admin/Partials/Configuration/Department/_DeleteDepartment.cshtml", department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Department.FindAsync(id);
            if (department != null) _context.Department.Remove(department);
            await _context.SaveChangesAsync();
            // After deletion, preserve venue context but department no longer exists
            return RedirectToAction("Dashboard", "Admin", new { venueId = department?.VenueId });
        }
    }
}