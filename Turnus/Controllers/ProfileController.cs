using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly TurnusContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(TurnusContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var today = DateTime.Today;

            var shiftAssignments = await _context.ShiftAssignment
                .Include(a => a.ScheduledShift)
                    .ThenInclude(s => s.ScheduledDay)
                        .ThenInclude(d => d.Venue)
                .Include(a => a.ScheduledShift)
                    .ThenInclude(s => s.ShiftDefinition)
                .Include(a => a.Role)
                .Where(a => a.EmployeeId == user.Id && a.ScheduledShift.ScheduledDay.Date >= today)
                .OrderBy(a => a.ScheduledShift.ScheduledDay.Date)
                .ToListAsync();

            var dayAssignments = await _context.DayAssignment
                .Include(a => a.ScheduledDay)
                    .ThenInclude(d => d.Venue)
                .Include(a => a.Role)
                .Where(a => a.EmployeeId == user.Id && a.ScheduledDay.Date >= today)
                .OrderBy(a => a.ScheduledDay.Date)
                .ToListAsync();

            ViewBag.ShiftAssignments = shiftAssignments;
            ViewBag.DayAssignments = dayAssignments;

            return View(user);
        }

        // GET: /Profile/EditInfo
        public async Task<IActionResult> EditInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new EditInfoViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            return View(model);
        }

        // POST: /Profile/EditInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInfo(EditInfoViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(EditInfo));
        }

        // GET: /Profile/Security
        public async Task<IActionResult> Security()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            ViewBag.TwoFactorEnabled = user.TwoFactorEnabled;
            ViewBag.EmailConfirmed = user.EmailConfirmed;
            ViewBag.PhoneNumberConfirmed = user.PhoneNumberConfirmed;

            return View();
        }

        // Nested view models — not separate model files
        public class EditInfoViewModel
        {
            [StringLength(100)]
            [Display(Name = "Full name")]
            public string FullName { get; set; } = string.Empty;

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; } = string.Empty;
        }

        public class ChangePasswordViewModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            public string ConfirmNewPassword { get; set; } = string.Empty;
        }
    }
}