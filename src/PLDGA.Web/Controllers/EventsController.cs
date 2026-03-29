using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;

namespace PLDGA.Web.Controllers;

[Authorize]
public class EventsController : BaseController
{
    private readonly IEventService _eventService;
    private readonly IMemberService _memberService;
    private readonly ISiteSettingsService _settingsService;

    public EventsController(IEventService eventService, IMemberService memberService, ISiteSettingsService settingsService)
    {
        _eventService = eventService;
        _memberService = memberService;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index()
    {
        var events = await _eventService.GetAllEventsAsync();
        return View(events);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);
        if (evt == null) return NotFound();

        // Get registered member details
        var allMembers = await _memberService.GetAllMembersAsync();
        ViewData["AllMembers"] = allMembers.ToList();
        ViewData["CurrentMemberId"] = CurrentMemberId;
        return View(evt);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var settings = await _settingsService.GetSettingsAsync();
        var eventDate = DateTime.UtcNow.AddDays(30);
        var dto = new CreateEventDto
        {
            Date = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventDate.Hour, eventDate.Minute, 0),
            RegistrationDeadline = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventDate.Hour, eventDate.Minute - 1, 0),
            MaxParticipants = settings.DefaultMaxParticipants
        };
        return View(dto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEventDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError("", "Event name is required.");
            return View(model);
        }

        var seasonYear = model.Date.Year;
        await _eventService.CreateEventAsync(model, seasonYear);
        TempData["Success"] = "Event created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);
        if (evt == null) return NotFound();
        return View(evt);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventDto model)
    {
        await _eventService.UpdateEventAsync(model);
        TempData["Success"] = "Event updated.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Guid eventId)
    {
        if (CurrentMemberId == null) return RedirectToAction("Login", "Account");

        try
        {
            await _eventService.RegisterMemberAsync(eventId, CurrentMemberId.Value);
            TempData["Success"] = "Registered successfully!";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id = eventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unregister(Guid eventId)
    {
        if (CurrentMemberId == null) return RedirectToAction("Login", "Account");
        await _eventService.UnregisterMemberAsync(eventId, CurrentMemberId.Value);
        TempData["Success"] = "Unregistered from event.";
        return RedirectToAction(nameof(Details), new { id = eventId });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> RecordResults(Guid id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);
        if (evt == null) return NotFound();
        var members = await _memberService.GetAllMembersAsync();
        ViewData["AllMembers"] = members.ToList();
        return View(evt);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordResults(Guid id, List<RecordResultDto> results)
    {
        await _eventService.RecordResultsAsync(id, results);
        await _eventService.CompleteEventAsync(id);
        TempData["Success"] = "Results recorded and event completed.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _eventService.DeleteEventAsync(id);
        TempData["Success"] = "Event deleted.";
        return RedirectToAction(nameof(Index));
    }
}
