using DesignAuthority.Application.Abstractions;
using DesignAuthority.Application.Features;
using DesignAuthority.Web.ViewModels;
using DesignAuthority.Web.ViewModels.Agreements;
using Microsoft.AspNetCore.Mvc;

namespace DesignAuthority.Web.Controllers;

public sealed class AgreementsController(WorkTrackingService service) : Controller
{
    public async Task<IActionResult> Index(
        string? search, int? technologyId, int? statusId, string sortBy = "Id",
        bool descending = true, int page = 1, CancellationToken cancellationToken = default)
    {
        var options = await service.GetFormOptionsAsync(WorkItemKind.Agreement, null, cancellationToken);
        var result = await service.ListAsync(WorkItemKind.Agreement,
            new WorkItemQuery(search, technologyId, statusId, sortBy, descending, page, 25), cancellationToken);
        return View(new WorkItemListViewModel(WorkItemKind.Agreement, result, search, technologyId,
            statusId, sortBy, descending, options.Technologies, options.Statuses));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var options = await service.GetFormOptionsAsync(WorkItemKind.Agreement, null, cancellationToken);
        return View(new AgreementViewModel
        {
            TramiteId = options.DefaultTramiteId ?? 0,
            StatusId = options.Statuses.FirstOrDefault(option => option.Name == "Abierto")?.Id ?? 0,
            Options = options
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AgreementViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await service.CreateAgreementAsync(model.ToInput(), cancellationToken);
            AddErrors(result.Errors);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
        }
        model.Options = await service.GetFormOptionsAsync(WorkItemKind.Agreement, model.TramiteId, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var input = await service.GetAgreementAsync(id, cancellationToken);
        if (input is null) return NotFound();
        ViewData["AgreementId"] = id;
        return View(AgreementViewModel.FromInput(input,
            await service.GetFormOptionsAsync(WorkItemKind.Agreement, input.TramiteId, cancellationToken)));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var input = await service.GetAgreementAsync(id, cancellationToken);
        if (input is null) return NotFound();
        return View("~/Views/Shared/WorkItemDetails.cshtml", WorkItemDetailsViewModel.Create(
            "Acuerdo", id.ToString(), "Agreements",
            [
                ("Fecha de registro", input.RegistrationDate), ("Trámite", input.TramiteId),
                ("Tecnología", input.TechnologyId), ("Descripción", input.Description),
                ("Persona responsable", input.ResponsiblePersonId), ("Estatus", input.StatusId),
                ("Fecha compromiso", input.CommitmentDate), ("Fecha de solución", input.SolutionDate)
            ], nameof(FollowUps)));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AgreementViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await service.UpdateAgreementAsync(id, model.ToInput(), cancellationToken);
            AddErrors(result.Errors);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
        }
        ViewData["AgreementId"] = id;
        model.Options = await service.GetFormOptionsAsync(WorkItemKind.Agreement, model.TramiteId, cancellationToken);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await service.DeleteAgreementAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> FollowUps(int id, CancellationToken cancellationToken)
    {
        ViewData["AgreementId"] = id;
        ViewData["FollowUps"] = await service.GetAgreementFollowUpsAsync(id, cancellationToken);
        var options = await service.GetFormOptionsAsync(WorkItemKind.Agreement, null, cancellationToken);
        return View(new FollowUpViewModel { Statuses = options.Statuses });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> FollowUps(int id, FollowUpViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await service.AddAgreementFollowUpAsync(id,
                new FollowUpInput(model.Date, model.Observation, model.OldStatusId, model.NewStatusId,
                    model.OldCommitmentDate, model.NewCommitmentDate), cancellationToken);
            AddErrors(result.Errors);
            if (result.Succeeded) return RedirectToAction(nameof(FollowUps), new { id });
        }
        ViewData["AgreementId"] = id;
        ViewData["FollowUps"] = await service.GetAgreementFollowUpsAsync(id, cancellationToken);
        model.Statuses = (await service.GetFormOptionsAsync(WorkItemKind.Agreement, null, cancellationToken)).Statuses;
        return View(model);
    }

    private void AddErrors(IReadOnlyDictionary<string, string[]> errors)
    {
        foreach (var (field, messages) in errors)
            foreach (var message in messages)
                ModelState.AddModelError(field, message);
    }
}
