using DesignAuthority.Application.Abstractions;

namespace DesignAuthority.Application.Features;

public sealed class WorkTrackingService(
    IWorkTrackingRepository workItems,
    IReferenceDataRepository references,
    IConfiguredUserContext userContext)
{
    private const int MaximumPageSize = 100;

    public Task<WorkItemFormOptions> GetFormOptionsAsync(WorkItemKind kind, int? tramiteId, CancellationToken cancellationToken) =>
        references.GetWorkItemFormOptionsAsync(kind, tramiteId, cancellationToken);

    public Task<PagedResult<WorkItemListRow>> ListAsync(
        WorkItemKind kind,
        WorkItemQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Clamp(query.Page, 1, 100_000);
        var size = Math.Clamp(query.PageSize, 1, MaximumPageSize);
        if (kind == WorkItemKind.Meeting && (query.TechnologyId is not null || query.StatusId is not null))
            throw new ArgumentException("Meeting lists support subject search and ordering, not these filters.", nameof(query));
        return workItems.ListAsync(kind, query with { Page = page, PageSize = size }, cancellationToken);
    }

    public Task<FindingInput?> GetFindingAsync(int id, CancellationToken cancellationToken) =>
        workItems.GetFindingAsync(id, cancellationToken);

    public Task<AgreementInput?> GetAgreementAsync(int id, CancellationToken cancellationToken) =>
        workItems.GetAgreementAsync(id, cancellationToken);

    public Task<DecisionInput?> GetDecisionAsync(string id, CancellationToken cancellationToken) =>
        workItems.GetDecisionAsync(id, cancellationToken);

    public Task<MeetingInput?> GetMeetingAsync(int id, CancellationToken cancellationToken) =>
        workItems.GetMeetingAsync(id, cancellationToken);

    public Task<MeetingMinuteInput?> GetMeetingMinuteAsync(int id, CancellationToken cancellationToken) =>
        workItems.GetMeetingMinuteAsync(id, cancellationToken);

    public Task<IReadOnlyList<FollowUpRow>> GetFindingFollowUpsAsync(int id, CancellationToken cancellationToken) =>
        workItems.GetFindingFollowUpsAsync(id, cancellationToken);

    public Task<IReadOnlyList<FollowUpRow>> GetAgreementFollowUpsAsync(int id, CancellationToken cancellationToken) =>
        workItems.GetAgreementFollowUpsAsync(id, cancellationToken);

    public async Task<ServiceResult<int>> CreateFindingAsync(FindingInput input, CancellationToken cancellationToken)
    {
        var error = await ValidateCommonAsync(input.TramiteId, input.TechnologyId, input.ResponsiblePersonId,
            input.StatusId, ReferenceKind.FindingStatus, cancellationToken);
        if (error is not null) return error;
        if (input.SubstageId <= 0 || !await references.IsActiveSubstageForTramiteAsync(input.SubstageId, input.TramiteId, cancellationToken))
            return ServiceResult<int>.Failure(nameof(input.SubstageId), "Seleccione una Subetapa activa del Trámite.");
        if (input.CategoryId <= 0 || !await references.IsReferenceValueAsync(ReferenceKind.FindingCategory, input.CategoryId, cancellationToken))
            return ServiceResult<int>.Failure(nameof(input.CategoryId), "Seleccione una categoría válida.");
        if (!await references.IsReferenceValueAsync(ReferenceKind.Criticality, input.CriticalityId, cancellationToken))
            return ServiceResult<int>.Failure(nameof(input.CriticalityId), "Seleccione una criticidad válida.");
        if (!await references.IsReferenceValueAsync(ReferenceKind.Priority, input.PriorityId, cancellationToken))
            return ServiceResult<int>.Failure(nameof(input.PriorityId), "Seleccione una prioridad válida.");
        if (!await references.IsReferenceValueAsync(ReferenceKind.RegistrationPhase, input.RegistrationPhaseId, cancellationToken))
            return ServiceResult<int>.Failure(nameof(input.RegistrationPhaseId), "Seleccione una fase de registro válida.");
        if (!await references.IsReferenceValueAsync(ReferenceKind.AttentionPhase, input.AttentionPhaseId, cancellationToken))
            return ServiceResult<int>.Failure(nameof(input.AttentionPhaseId), "Seleccione una fase de atención válida.");
        if (!Required(input.Description) || input.RegistrationDate == default || input.CommitmentDate == default)
            return ServiceResult<int>.Failure(nameof(input.Description), "Complete todos los campos obligatorios.");

        return ServiceResult<int>.Success(await workItems.CreateFindingAsync(input, userContext.UserId, cancellationToken));
    }

    public async Task<ServiceResult<bool>> UpdateFindingAsync(int id, FindingInput input, CancellationToken cancellationToken)
    {
        var error = await ValidateCommonAsync(input.TramiteId, input.TechnologyId, input.ResponsiblePersonId,
            input.StatusId, ReferenceKind.FindingStatus, cancellationToken);
        if (error is not null) return ConvertFailure<bool>(error);
        if (input.SubstageId <= 0 || !await references.IsActiveSubstageForTramiteAsync(input.SubstageId, input.TramiteId, cancellationToken))
            return ServiceResult<bool>.Failure(nameof(input.SubstageId), "Seleccione una Subetapa activa del Trámite.");
        if (input.CategoryId <= 0 || !await references.IsReferenceValueAsync(ReferenceKind.FindingCategory, input.CategoryId, cancellationToken))
            return ServiceResult<bool>.Failure(nameof(input.CategoryId), "Seleccione una categoría válida.");
        if (!await references.IsReferenceValueAsync(ReferenceKind.Criticality, input.CriticalityId, cancellationToken))
            return ServiceResult<bool>.Failure(nameof(input.CriticalityId), "Seleccione una criticidad válida.");
        if (!await references.IsReferenceValueAsync(ReferenceKind.Priority, input.PriorityId, cancellationToken))
            return ServiceResult<bool>.Failure(nameof(input.PriorityId), "Seleccione una prioridad válida.");
        if (!await references.IsReferenceValueAsync(ReferenceKind.RegistrationPhase, input.RegistrationPhaseId, cancellationToken))
            return ServiceResult<bool>.Failure(nameof(input.RegistrationPhaseId), "Seleccione una fase de registro válida.");
        if (!await references.IsReferenceValueAsync(ReferenceKind.AttentionPhase, input.AttentionPhaseId, cancellationToken))
            return ServiceResult<bool>.Failure(nameof(input.AttentionPhaseId), "Seleccione una fase de atención válida.");
        if (!Required(input.Description) || input.RegistrationDate == default || input.CommitmentDate == default)
            return ServiceResult<bool>.Failure(nameof(input.Description), "Complete todos los campos obligatorios.");
        await workItems.UpdateFindingAsync(id, input, userContext.UserId, cancellationToken);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<int>> CreateAgreementAsync(AgreementInput input, CancellationToken cancellationToken)
    {
        var error = await ValidateCommonAsync(input.TramiteId, input.TechnologyId, input.ResponsiblePersonId,
            input.StatusId, ReferenceKind.AgreementStatus, cancellationToken);
        if (error is not null) return error;
        if (!Required(input.Description) || input.RegistrationDate == default || input.CommitmentDate == default)
            return ServiceResult<int>.Failure(nameof(input.Description), "Complete todos los campos obligatorios.");
        return ServiceResult<int>.Success(await workItems.CreateAgreementAsync(input, userContext.UserId, cancellationToken));
    }

    public async Task<ServiceResult<bool>> UpdateAgreementAsync(int id, AgreementInput input, CancellationToken cancellationToken)
    {
        var error = await ValidateCommonAsync(input.TramiteId, input.TechnologyId, input.ResponsiblePersonId,
            input.StatusId, ReferenceKind.AgreementStatus, cancellationToken);
        if (error is not null) return ConvertFailure<bool>(error);
        if (!Required(input.Description) || input.RegistrationDate == default || input.CommitmentDate == default)
            return ServiceResult<bool>.Failure(nameof(input.Description), "Complete todos los campos obligatorios.");
        await workItems.UpdateAgreementAsync(id, input, userContext.UserId, cancellationToken);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<string>> CreateDecisionAsync(DecisionInput input, CancellationToken cancellationToken)
    {
        var error = await ValidateTramiteTechnologyAsync(input.TramiteId, input.TechnologyId, cancellationToken);
        if (error is not null) return ConvertFailure<string>(error);
        if (input.SubstageId <= 0 || !await references.IsSubstageForTramiteAsync(input.SubstageId, input.TramiteId, cancellationToken))
            return ServiceResult<string>.Failure(nameof(input.SubstageId), "Seleccione una Subetapa del mismo Trámite.");
        if (!await references.IsTeamAsync(input.ResponsibleTeamTechnologyId, cancellationToken))
            return ServiceResult<string>.Failure(nameof(input.ResponsibleTeamTechnologyId), "Seleccione un Equipo responsable.");
        if (!await references.IsReferenceValueAsync(ReferenceKind.DecisionStatus, input.StatusId, cancellationToken))
            return ServiceResult<string>.Failure(nameof(input.StatusId), "Seleccione un estatus válido.");
        if (!Required(input.ProblemDescription) || !Required(input.SolutionApproach) || !Required(input.DecisionText)
            || input.RegistrationDate == default)
            return ServiceResult<string>.Failure(nameof(input.ProblemDescription), "Complete todos los campos obligatorios.");
        return ServiceResult<string>.Success(await workItems.CreateDecisionAsync(input, userContext.UserId, cancellationToken));
    }

    public async Task<ServiceResult<bool>> UpdateDecisionAsync(string id, DecisionInput input, CancellationToken cancellationToken)
    {
        var error = await ValidateTramiteTechnologyAsync(input.TramiteId, input.TechnologyId, cancellationToken);
        if (error is not null) return ConvertFailure<bool>(error);
        if (!await references.IsSubstageForTramiteAsync(input.SubstageId, input.TramiteId, cancellationToken))
            return ServiceResult<bool>.Failure(nameof(input.SubstageId), "Seleccione una Subetapa del mismo Trámite.");
        if (!await references.IsTeamAsync(input.ResponsibleTeamTechnologyId, cancellationToken))
            return ServiceResult<bool>.Failure(nameof(input.ResponsibleTeamTechnologyId), "Seleccione un Equipo responsable.");
        if (!await references.IsReferenceValueAsync(ReferenceKind.DecisionStatus, input.StatusId, cancellationToken))
            return ServiceResult<bool>.Failure(nameof(input.StatusId), "Seleccione un estatus válido.");
        if (!Required(input.ProblemDescription) || !Required(input.SolutionApproach) || !Required(input.DecisionText)
            || input.RegistrationDate == default)
            return ServiceResult<bool>.Failure(nameof(input.ProblemDescription), "Complete todos los campos obligatorios.");
        await workItems.UpdateDecisionAsync(id, input, userContext.UserId, cancellationToken);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<int>> CreateMeetingAsync(MeetingInput input, CancellationToken cancellationToken)
    {
        var error = await ValidateMeetingAsync(input, cancellationToken);
        if (error is not null) return error;
        return ServiceResult<int>.Success(await workItems.CreateMeetingAsync(input, userContext.UserId, cancellationToken));
    }

    public async Task<ServiceResult<bool>> UpdateMeetingAsync(int id, MeetingInput input, CancellationToken cancellationToken)
    {
        var error = await ValidateMeetingAsync(input, cancellationToken);
        if (error is not null) return ConvertFailure<bool>(error);
        await workItems.UpdateMeetingAsync(id, input, userContext.UserId, cancellationToken);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> ReplaceMeetingMinuteAsync(
        int meetingId,
        string originalFileName,
        Stream file,
        long length,
        IFileStorage storage,
        CancellationToken cancellationToken)
    {
        if (length < 0 || length > 5 * 1024 * 1024)
            return ServiceResult<bool>.Failure("Minute", "La Minuta no puede exceder 5 MB.");
        if (string.IsNullOrWhiteSpace(originalFileName)
            || !string.Equals(Path.GetExtension(originalFileName), ".docx", StringComparison.OrdinalIgnoreCase))
            return ServiceResult<bool>.Failure("Minute", "La Minuta debe ser un archivo .docx.");
        if (await workItems.GetMeetingAsync(meetingId, cancellationToken) is null)
            return ServiceResult<bool>.Failure("MeetingId", "No se encontró la Reunión.");

        StoredFile? staged = null;
        try
        {
            staged = await storage.StageAsync(originalFileName, file, length, cancellationToken);
            await storage.PromoteAsync(staged, cancellationToken);
            await workItems.ReplaceMeetingMinuteAsync(
                meetingId,
                new MeetingMinuteInput(staged.OriginalFileName, staged.StorageFileName, staged.RelativePath),
                userContext.UserId,
                cancellationToken);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception operationError)
        {
            if (staged is not null)
            {
                try
                {
                    await storage.DeleteAsync(staged.RelativePath, CancellationToken.None);
                }
                catch (Exception cleanupError)
                {
                    throw new AggregateException("Minuta operation failed and its staged file could not be cleaned up.",
                        operationError, cleanupError);
                }
            }
            throw;
        }
    }

    private async Task<ServiceResult<int>?> ValidateMeetingAsync(MeetingInput input, CancellationToken cancellationToken)
    {
        var commonError = await ValidateTramiteTechnologyAsync(input.TramiteId, input.TechnologyId, cancellationToken);
        if (commonError is not null) return commonError;
        if (!await references.IsReferenceValueAsync(ReferenceKind.MeetingStatus, input.StatusId, cancellationToken))
            return ServiceResult<int>.Failure(nameof(input.StatusId), "Seleccione un estatus válido.");
        if (!Required(input.Subject) || !Required(input.AgreementsSummary) || !Required(input.CommitmentsSummary)
            || input.Date == default || input.Participants is null || !input.Participants.Any(person => person.Attended))
            return ServiceResult<int>.Failure(nameof(input.Participants), "Complete los campos obligatorios e incluya al menos un asistente.");
        var ids = input.Participants.Select(person => person.PersonId).ToArray();
        if (ids.Any(id => id <= 0) || ids.Distinct().Count() != ids.Length)
            return ServiceResult<int>.Failure(nameof(input.Participants), "No repita personas entre asistentes y ausentes.");
        foreach (var personId in ids)
        {
            if (!await references.IsPersonAsync(personId, cancellationToken))
                return ServiceResult<int>.Failure(nameof(input.Participants), "Seleccione personas válidas.");
        }
        return null;
    }

    public Task DeleteFindingAsync(int id, CancellationToken cancellationToken) =>
        workItems.SoftDeleteFindingAsync(id, userContext.UserId, cancellationToken);

    public Task DeleteAgreementAsync(int id, CancellationToken cancellationToken) =>
        workItems.SoftDeleteAgreementAsync(id, userContext.UserId, cancellationToken);

    public Task DeleteDecisionAsync(string id, CancellationToken cancellationToken) =>
        workItems.SoftDeleteDecisionAsync(id, userContext.UserId, cancellationToken);

    public Task DeleteMeetingAsync(int id, CancellationToken cancellationToken) =>
        workItems.SoftDeleteMeetingAsync(id, userContext.UserId, cancellationToken);

    public async Task<ServiceResult<bool>> AddFindingFollowUpAsync(
        int findingId,
        FollowUpInput input,
        CancellationToken cancellationToken)
    {
        var error = await ValidateFollowUpAsync(input, ReferenceKind.FindingStatus, cancellationToken);
        if (error is not null) return ConvertFailure<bool>(error);
        await workItems.AddFindingFollowUpAsync(findingId, input, userContext.UserId, cancellationToken);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> AddAgreementFollowUpAsync(
        int agreementId,
        FollowUpInput input,
        CancellationToken cancellationToken)
    {
        var error = await ValidateFollowUpAsync(input, ReferenceKind.AgreementStatus, cancellationToken);
        if (error is not null) return ConvertFailure<bool>(error);
        await workItems.AddAgreementFollowUpAsync(agreementId, input, userContext.UserId, cancellationToken);
        return ServiceResult<bool>.Success(true);
    }

    private async Task<ServiceResult<int>?> ValidateCommonAsync(
        int tramiteId,
        int technologyId,
        int personId,
        int statusId,
        ReferenceKind statusKind,
        CancellationToken cancellationToken)
    {
        var referenceError = await ValidateTramiteTechnologyAsync(tramiteId, technologyId, cancellationToken);
        if (referenceError is not null) return referenceError;
        if (!await references.IsPersonAsync(personId, cancellationToken))
            return ServiceResult<int>.Failure(nameof(personId), "Seleccione una Persona responsable válida.");
        if (!await references.IsReferenceValueAsync(statusKind, statusId, cancellationToken))
            return ServiceResult<int>.Failure(nameof(statusId), "Seleccione un estatus válido.");
        return null;
    }

    private async Task<ServiceResult<int>?> ValidateFollowUpAsync(
        FollowUpInput input,
        ReferenceKind statusKind,
        CancellationToken cancellationToken)
    {
        if (input.Date == default || !Required(input.Observation))
            return ServiceResult<int>.Failure(nameof(input.Observation), "La fecha y la observación son obligatorias.");
        if (input.OldStatusId is int oldStatus
            && !await references.IsReferenceValueAsync(statusKind, oldStatus, cancellationToken))
            return ServiceResult<int>.Failure(nameof(input.OldStatusId), "El estatus anterior no es válido.");
        if (input.NewStatusId is int newStatus
            && !await references.IsReferenceValueAsync(statusKind, newStatus, cancellationToken))
            return ServiceResult<int>.Failure(nameof(input.NewStatusId), "El estatus nuevo no es válido.");
        if (input.OldStatusId.HasValue != input.NewStatusId.HasValue)
            return ServiceResult<int>.Failure(nameof(input.NewStatusId), "Capture ambos estatus o ninguno.");
        if (input.OldCommitmentDate.HasValue != input.NewCommitmentDate.HasValue)
            return ServiceResult<int>.Failure(nameof(input.NewCommitmentDate), "Capture ambas fechas compromiso o ninguna.");
        return null;
    }

    private async Task<ServiceResult<int>?> ValidateTramiteTechnologyAsync(
        int tramiteId,
        int technologyId,
        CancellationToken cancellationToken)
    {
        if (tramiteId <= 0 || !await references.IsActiveTramiteAsync(tramiteId, cancellationToken))
            return ServiceResult<int>.Failure(nameof(tramiteId), "Seleccione un Trámite activo.");
        if (!await references.IsTechnologyAsync(technologyId, cancellationToken))
            return ServiceResult<int>.Failure(nameof(technologyId), "Seleccione una Tecnología válida.");
        return null;
    }

    private static bool Required(string? value) => !string.IsNullOrWhiteSpace(value);

    private static ServiceResult<TTarget> ConvertFailure<TTarget, TSource>(ServiceResult<TSource> source) =>
        new(default, source.Errors);

    private static ServiceResult<TTarget> ConvertFailure<TTarget>(ServiceResult<int> source) =>
        new(default, source.Errors);
}
