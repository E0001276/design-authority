using DesignAuthority.Application.Abstractions;
using DesignAuthority.Application.Features;
using DesignAuthority.Web.ViewModels;
using DesignAuthority.Web.ViewModels.Findings;
using Microsoft.AspNetCore.Mvc;

namespace DesignAuthority.Web.Controllers;

public sealed class FindingsController(WorkTrackingService service) : Controller
{
    public async Task<IActionResult> Index(
        string? search,
        int? technologyId,
        int? statusId,
        string sortBy = "Id",
        bool descending = true,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var options = await service.GetFormOptionsAsync(WorkItemKind.Finding, null, cancellationToken);
        var result = await service.ListAsync(WorkItemKind.Finding,
            new WorkItemQuery(search, technologyId, statusId, sortBy, descending, page, 25), cancellationToken);
        return View(new WorkItemListViewModel(WorkItemKind.Finding, result, search, technologyId, statusId,
            sortBy, descending, options.Technologies, options.Statuses));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var options = await service.GetFormOptionsAsync(WorkItemKind.Finding, null, cancellationToken);
        return View(new FindingViewModel
        {
            TramiteId = options.DefaultTramiteId ?? 0,
            StatusId = options.Statuses.FirstOrDefault(option => option.Name == "Abierto")?.Id ?? 0,
            Options = options
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FindingViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await service.CreateFindingAsync(model.ToInput(), cancellationToken);
            AddErrors(result.Errors);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
        }
        model.Options = await service.GetFormOptionsAsync(WorkItemKind.Finding, model.TramiteId, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var input = await service.GetFindingAsync(id, cancellationToken);
        if (input is null) return NotFound();
        var options = await service.GetFormOptionsAsync(WorkItemKind.Finding, input.TramiteId, cancellationToken);
        ViewData["FindingId"] = id;
        return View(FindingViewModel.FromInput(input, options));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var input = await service.GetFindingAsync(id, cancellationToken);
        if (input is null) return NotFound();
        return View("~/Views/Shared/WorkItemDetails.cshtml", WorkItemDetailsViewModel.Create(
            "Hallazgo", id.ToString(), "Findings",
            [
                ("Fecha de registro", input.RegistrationDate), ("Trámite", input.TramiteId),
                ("Subetapa", input.SubstageId), ("Tecnología", input.TechnologyId),
                ("Categoría", input.CategoryId), ("Descripción", input.Description),
                ("Persona responsable", input.ResponsiblePersonId), ("Estatus", input.StatusId),
                ("Criticidad", input.CriticalityId), ("Prioridad", input.PriorityId),
                ("Fase de registro", input.RegistrationPhaseId), ("Fase de atención", input.AttentionPhaseId),
                ("Fecha compromiso", input.CommitmentDate), ("Fecha de solución", input.SolutionDate)
            ], nameof(FollowUps)));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FindingViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await service.UpdateFindingAsync(id, model.ToInput(), cancellationToken);
            AddErrors(result.Errors);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
        }
        model.Options = await service.GetFormOptionsAsync(WorkItemKind.Finding, model.TramiteId, cancellationToken);
        ViewData["FindingId"] = id;
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await service.DeleteFindingAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> FollowUps(int id, CancellationToken cancellationToken)
    {
        ViewData["FindingId"] = id;
        ViewData["FollowUps"] = await service.GetFindingFollowUpsAsync(id, cancellationToken);
        var options = await service.GetFormOptionsAsync(WorkItemKind.Finding, null, cancellationToken);
        return View(new FollowUpViewModel { Statuses = options.Statuses });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> FollowUps(int id, FollowUpViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await service.AddFindingFollowUpAsync(id,
                new FollowUpInput(model.Date, model.Observation, model.OldStatusId, model.NewStatusId,
                    model.OldCommitmentDate, model.NewCommitmentDate), cancellationToken);
            AddErrors(result.Errors);
            if (result.Succeeded) return RedirectToAction(nameof(FollowUps), new { id });
        }
        ViewData["FindingId"] = id;
        ViewData["FollowUps"] = await service.GetFindingFollowUpsAsync(id, cancellationToken);
        model.Statuses = (await service.GetFormOptionsAsync(WorkItemKind.Finding, null, cancellationToken)).Statuses;
        return View(model);
    }

    private void AddErrors(IReadOnlyDictionary<string, string[]> errors)
    {
        foreach (var (field, messages) in errors)
            foreach (var message in messages)
                ModelState.AddModelError(field, message);
    }
}
