using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MatchFinder.Domain.Entities;
using MatchFinder.Infrastructure.Services;

namespace MatchFinder.Web.Controllers;

[Authorize]
public class MatchesController : Controller
{
    private readonly IMatchService _matchService;
    private readonly UserManager<ApplicationUser> _userManager;

    public MatchesController(IMatchService matchService, UserManager<ApplicationUser> userManager)
    {
        _matchService = matchService;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? sport, string? venue)
    {
        var matches = await _matchService.GetActiveMatchesAsync(sport, venue);
        ViewData["CurrentSport"] = sport;
        ViewData["CurrentVenue"] = venue;
        return View(matches);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id)
    {
        var match = await _matchService.GetMatchByIdAsync(id);
        if (match == null) return NotFound();
        return View(match);
    }

    [Authorize(Roles = "Host,Admin")]
    public IActionResult Create() => View();

    [HttpPost]
    [Authorize(Roles = "Host,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Match match)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        match.HostId = user.Id;
        match.CurrentPlayersCount = 1;

        var result = await _matchService.CreateMatchAsync(match);
        if (result.Succeeded)
        {
            await _matchService.JoinMatchAsync(match.Id, user.Id);
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        return View(match);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var result = await _matchService.JoinMatchAsync(id, user.Id);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Successfully joined the match!" : result.ErrorMessage;

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var result = await _matchService.LeaveMatchAsync(id, user.Id);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "You have left the match." : result.ErrorMessage;

        return RedirectToAction(nameof(Details), new { id });
    }
}