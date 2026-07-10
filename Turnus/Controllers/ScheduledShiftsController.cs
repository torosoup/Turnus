using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    public class ScheduledShiftsController : Controller
    {
        private readonly TurnusContext _context;

        public ScheduledShiftsController(TurnusContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create(int venueId, DateTime date)
        {
            var shiftDefs = await _context.ShiftDefinition.ToListAsync();
            ViewBag.ShiftDefinitions = shiftDefs;

            return PartialView("~/Views/Admin/Partials/ScheduledShift/_CreateScheduledShift.cshtml",
                new ScheduledShift { VenueId = venueId, Date = date });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ScheduledShift model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ShiftDefinitions = await _context.ShiftDefinition.ToListAsync();
                return PartialView("~/Views/Admin/Partials/ScheduledShift/_CreateScheduledShift.cshtml", model);
            }

            _context.ScheduledShift.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "Admin", new { venueId = model.VenueId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _context.ScheduledShift
                .Include(s => s.ShiftDefinition)
                .FirstOrDefaultAsync(s => s.Id == id);

            return PartialView("~/Views/Admin/Partials/ScheduledShift/_DeleteScheduledShift.cshtml", shift);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(ScheduledShift model)
        {
            var shift = await _context.ScheduledShift.FindAsync(model.Id);
            _context.ScheduledShift.Remove(shift);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "Admin", new { venueId = shift.VenueId });
        }
    }
}
