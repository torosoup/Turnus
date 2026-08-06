using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize]
    public class WorkspacesController : Controller
    {
        private readonly TurnusContext _context;

        public WorkspacesController(TurnusContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActive(int id, string? returnUrl = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Forbid();

            var member = await _context.WorkspaceMember.FindAsync(id, userId);
            if (member == null) return Forbid();

            var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = true, SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax };
            if (Request.IsHttps) cookieOptions.Secure = true;
            Response.Cookies.Append("workspace", id.ToString(), cookieOptions);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Schedule");
        }
    }
}
