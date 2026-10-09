using DesignAuthority.Application.Abstractions;
using DesignAuthority.Application.Features;
using DesignAuthority.Web.ViewModels;
using DesignAuthority.Web.ViewModels.Meetings;
using Microsoft.AspNetCore.Mvc;

namespace DesignAuthority.Web.Controllers;

public sealed class MeetingsController(WorkTrackingService service, IFileStorage fileStorage) : Controller
{
    public async Task<IActionResult> Index(
        string? search, string sortBy = "Date",
        bool descending = true, int page = 1, CancellationToken cancellationToken = default)
    {
        var options = await service.GetFormOptionsAsync(WorkItemKind.Meeting, null, cancellationToken);
        var result = await service.ListAsync(WorkItemKind.Meeting,
            new WorkItemQuery(search, null, null, sortBy, descending, page, 25), cancellationToken);
        return View(new WorkItemListViewModel(WorkItemKind.Meeting, result, search, null,
            null, sortBy, descending, options.Technologies, options.Statuses));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var options = await service.GetFormOptionsAsync(WorkItemKind.Meeting, null, cancellationToken);
        return View(new MeetingViewModel
        {
            TramiteId = options.DefaultTramiteId ?? 0,
            StatusId = options.Statuses.FirstOrDefault(option => option.Name == "Realizada")?.Id ?? 0,
            Options = options
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MeetingViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await service.CreateMeetingAsync(model.ToInput(), cancellationToken);
            AddErrors(result.Errors);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
        }
        model.Options = await service.GetFormOptionsAsync(WorkItemKind.Meeting, model.TramiteId, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var input = await service.GetMeetingAsync(id, cancellationToken);
        if (input is null) return NotFound();
        ViewData["MeetingId"] = id;
        ViewData["CurrentMinute"] = await service.GetMeetingMinuteAsync(id, cancellationToken);
        return View(MeetingViewModel.FromInput(input,
            await service.GetFormOptionsAsync(WorkItemKind.Meeting, input.TramiteId, cancellationToken)));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var input = await service.GetMeetingAsync(id, cancellationToken);
        if (input is null) return NotFound();
        var minute = await service.GetMeetingMinuteAsync(id, cancellationToken);
        return View("~/Views/Shared/WorkItemDetails.cshtml", WorkItemDetailsViewModel.Create(
            "Reunión", id.ToString(), "Meetings",
            [
                ("Trámite", input.TramiteId), ("Tecnología", input.TechnologyId),
                ("Fecha", input.Date), ("Hora", input.Time), ("Estatus", input.StatusId),
                ("Asunto", input.Subject), ("Resumen de Acuerdos", input.AgreementsSummary),
                ("Resumen de compromisos", input.CommitmentsSummary), ("Comentarios", input.Comments),
                ("Asistentes", string.Join(", ", input.Participants.Where(person => person.Attended).Select(person => person.PersonId))),
                ("Ausentes", string.Join(", ", input.Participants.Where(person => !person.Attended).Select(person => person.PersonId)))
            ], currentMinuteFileName: minute?.OriginalFileName));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MeetingViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await service.UpdateMeetingAsync(id, model.ToInput(), cancellationToken);
            AddErrors(result.Errors);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
        }
        ViewData["MeetingId"] = id;
        ViewData["CurrentMinute"] = await service.GetMeetingMinuteAsync(id, cancellationToken);
        model.Options = await service.GetFormOptionsAsync(WorkItemKind.Meeting, model.TramiteId, cancellationToken);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await service.DeleteMeetingAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReplaceMinute(int id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            TempData["Error"] = "Seleccione una Minuta .docx.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        await using var stream = file.OpenReadStream();
        var result = await service.ReplaceMeetingMinuteAsync(
            id, file.FileName, stream, file.Length, fileStorage, cancellationToken);
        if (!result.Succeeded)
            TempData["Error"] = string.Join(" ", result.Errors.Values.SelectMany(messages => messages));
        else
            TempData["Message"] = "La Minuta vigente se reemplazó correctamente.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadMinute(int id, CancellationToken cancellationToken)
    {
        if (await service.GetMeetingAsync(id, cancellationToken) is null) return NotFound();
        var minute = await service.GetMeetingMinuteAsync(id, cancellationToken);
        if (minute is null) return NotFound();
        var stream = await fileStorage.OpenReadAsync(minute.RelativePath, cancellationToken);
        return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            minute.OriginalFileName);
    }

    private void AddErrors(IReadOnlyDictionary<string, string[]> errors)
    {
        foreach (var (field, messages) in errors)
            foreach (var message in messages)
                ModelState.AddModelError(field, message);
    }
}
