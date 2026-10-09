using DesignAuthority.Application.Abstractions;
using DesignAuthority.Application.Features;
using DesignAuthority.Web.ViewModels;
using DesignAuthority.Web.ViewModels.Decisions;
using Microsoft.AspNetCore.Mvc;

namespace DesignAuthority.Web.Controllers;

public sealed class DecisionsController(WorkTrackingService service) : Controller
{
    public async Task<IActionResult> Index(
        string? search, int? technologyId, int? statusId, string sortBy = "RegistrationDate",
        bool descending = true, int page = 1, CancellationToken cancellationToken = default)
    {
        var options = await service.GetFormOptionsAsync(WorkItemKind.Decision, null, cancellationToken);
        var result = await service.ListAsync(WorkItemKind.Decision,
            new WorkItemQuery(search, technologyId, statusId, sortBy, descending, page, 25), cancellationToken);
        return View(new WorkItemListViewModel(WorkItemKind.Decision, result, search, technologyId,
            statusId, sortBy, descending, options.Technologies, options.Statuses));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var options = await service.GetFormOptionsAsync(WorkItemKind.Decision, null, cancellationToken);
        return View(new DecisionViewModel
        {
            TramiteId = options.DefaultTramiteId ?? 0,
            StatusId = options.Statuses.FirstOrDefault(option => option.Name == "Abierto")?.Id ?? 0,
            Options = options
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DecisionViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await service.CreateDecisionAsync(model.ToInput(), cancellationToken);
            AddErrors(result.Errors);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
        }
        model.Options = await service.GetFormOptionsAsync(WorkItemKind.Decision, model.TramiteId, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var input = await service.GetDecisionAsync(id, cancellationToken);
        if (input is null) return NotFound();
        ViewData["DecisionId"] = id;
        return View(DecisionViewModel.FromInput(input,
            await service.GetFormOptionsAsync(WorkItemKind.Decision, input.TramiteId, cancellationToken)));
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        var input = await service.GetDecisionAsync(id, cancellationToken);
        if (input is null) return NotFound();
        return View("~/Views/Shared/WorkItemDetails.cshtml", WorkItemDetailsViewModel.Create(
            "Decisión de Arquitectura", id, "Decisions",
            [
                ("Trámite", input.TramiteId), ("Subetapa", input.SubstageId),
                ("Tecnología", input.TechnologyId), ("Equipo responsable", input.ResponsibleTeamTechnologyId),
                ("Descripción del problema", input.ProblemDescription), ("Enfoque de solución", input.SolutionApproach),
                ("Decisión", input.DecisionText), ("Fecha de registro", input.RegistrationDate),
                ("Estatus", input.StatusId)
            ]));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, DecisionViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await service.UpdateDecisionAsync(id, model.ToInput(), cancellationToken);
            AddErrors(result.Errors);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
        }
        ViewData["DecisionId"] = id;
        model.Options = await service.GetFormOptionsAsync(WorkItemKind.Decision, model.TramiteId, cancellationToken);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await service.DeleteDecisionAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private void AddErrors(IReadOnlyDictionary<string, string[]> errors)
    {
        foreach (var (field, messages) in errors)
            foreach (var message in messages)
                ModelState.AddModelError(field, message);
    }
}
