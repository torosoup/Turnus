using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    public class ShiftDefinitionsController : Controller
    {
        private readonly TurnusContext _context;

        public ShiftDefinitionsController(TurnusContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create(int departmentId)
        {
            return PartialView("~/Views/Admin/Partials/ShiftDefinition/_CreateShiftDefinition.cshtml",
                new ShiftDefinition { DepartmentId = departmentId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ShiftDefinition model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView(
                    "~/Views/Admin/Partials/ShiftDefinition/_CreateShiftDefinition.cshtml",
                    model);
            }

            _context.ShiftDefinition.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "Admin");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var sd = await _context.ShiftDefinition.FindAsync(id);
            return PartialView("~/Views/Admin/Partials/ShiftDefinition/_EditShiftDefinition.cshtml", sd);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ShiftDefinition model)
        {
            if (!ModelState.IsValid)
                return PartialView("~/Views/Admin/Partials/ShiftDefinition/_EditShiftDefinition.cshtml", model);

            _context.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "Admin");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var sd = await _context.ShiftDefinition.FindAsync(id);
            return PartialView("~/Views/Admin/Partials/ShiftDefinition/_DeleteShiftDefinition.cshtml", sd);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(ShiftDefinition model)
        {
            var sd = await _context.ShiftDefinition.FindAsync(model.Id);
            _context.ShiftDefinition.Remove(sd);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "Admin");
        }
    }
}
