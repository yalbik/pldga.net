using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;

namespace PLDGA.Web.Controllers;

[Authorize]
public class PollsController : BaseController
{
    private readonly IPollService _pollService;

    public PollsController(IPollService pollService)
    {
        _pollService = pollService;
    }

    public async Task<IActionResult> Index()
    {
        var polls = await _pollService.GetAllPollsAsync(CurrentMemberId);
        return View(polls);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var poll = await _pollService.GetPollByIdAsync(id, CurrentMemberId);
        if (poll == null) return NotFound();
        return View(poll);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreatePollDto { Answers = new List<string> { "", "" } });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePollDto model)
    {
        if (CurrentMemberId == null) return RedirectToAction("Login", "Account");

        model.Answers = model.Answers.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

        if (string.IsNullOrWhiteSpace(model.Question) || model.Answers.Count < 2)
        {
            ModelState.AddModelError("", "Question and at least 2 answers are required.");
            return View(model);
        }

        if (model.Answers.Count > 10)
        {
            ModelState.AddModelError("", "Maximum 10 answers allowed.");
            return View(model);
        }

        await _pollService.CreatePollAsync(model, CurrentMemberId.Value);
        TempData["Success"] = "Poll created!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Vote(Guid pollId, int answerId)
    {
        if (CurrentMemberId == null) return RedirectToAction("Login", "Account");

        try
        {
            await _pollService.VoteAsync(pollId, CurrentMemberId.Value, answerId);
            TempData["Success"] = "Vote recorded!";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id = pollId });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(Guid id)
    {
        await _pollService.ClosePollAsync(id);
        TempData["Success"] = "Poll closed.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _pollService.DeletePollAsync(id);
        TempData["Success"] = "Poll deleted.";
        return RedirectToAction(nameof(Index));
    }
}
