using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatchFinder.Domain.Entities;

namespace MatchFinder.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // GET: /Admin/Users
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        return View(users);
    }

    // POST: /Admin/ApproveHost/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveHost(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        user.IsApprovedHost = true;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["Success"] = $"Host {user.FullName} has been approved!";
        }
        else
        {
            TempData["Error"] = "Failed to approve host.";
        }

        return RedirectToAction(nameof(Index));
    }
}