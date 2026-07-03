using System;
using System.Collections.Generic;
using Kine.Modules.Reimbursement.Application;
using Kine.Modules.Reimbursement.Domain;
using Kine.Modules.Reimbursement.Infrastructure;
using Xunit;

namespace Kine.UnitTests;

public class ReimbursementServiceTests
{
    private const string TenantId = "tenant-001";
    private const string Actor = "staff-1";

    private static ReimbursementService CreateService() => new(new InMemoryReimbursementCaseStore());

    private static ReimbursementCase CreateDraft(ReimbursementService service) =>
        service.CreateCase(TenantId, new[] { Guid.NewGuid() }, Actor);

    [Fact]
    public void Create_case_starts_in_draft_without_submission_ref()
    {
        var service = CreateService();

        var reimbursementCase = CreateDraft(service);

        Assert.Equal(ReimbursementCaseStatus.Draft, reimbursementCase.Status);
        Assert.Null(reimbursementCase.SubmissionRef);
        Assert.Single(reimbursementCase.InvoiceIds);
    }

    [Fact]
    public void Create_case_rejects_empty_invoice_list()
    {
        var service = CreateService();

        Assert.Throws<ArgumentException>(() => service.CreateCase(TenantId, Array.Empty<Guid>(), Actor));
        Assert.Throws<ArgumentException>(() => service.CreateCase(TenantId, new[] { Guid.Empty }, Actor));
    }

    [Fact]
    public void Create_case_deduplicates_invoice_ids()
    {
        var service = CreateService();
        var invoiceId = Guid.NewGuid();

        var reimbursementCase = service.CreateCase(TenantId, new[] { invoiceId, invoiceId }, Actor);

        Assert.Single(reimbursementCase.InvoiceIds);
    }

    [Fact]
    public void Submit_generates_mock_efact_reference()
    {
        var service = CreateService();
        var reimbursementCase = CreateDraft(service);

        var submitted = service.Transition(TenantId, reimbursementCase.Id, ReimbursementCaseStatus.Submitted);

        Assert.Equal(ReimbursementCaseStatus.Submitted, submitted.Status);
        Assert.NotNull(submitted.SubmissionRef);
        Assert.StartsWith("EFACT-", submitted.SubmissionRef);
    }

    [Fact]
    public void Full_happy_path_reaches_archived()
    {
        var service = CreateService();
        var reimbursementCase = CreateDraft(service);

        service.Transition(TenantId, reimbursementCase.Id, ReimbursementCaseStatus.Submitted);
        service.Transition(TenantId, reimbursementCase.Id, ReimbursementCaseStatus.Pending);
        service.Transition(TenantId, reimbursementCase.Id, ReimbursementCaseStatus.Approved);
        service.Transition(TenantId, reimbursementCase.Id, ReimbursementCaseStatus.Completed);
        var archived = service.Transition(TenantId, reimbursementCase.Id, ReimbursementCaseStatus.Archived);

        Assert.Equal(ReimbursementCaseStatus.Archived, archived.Status);
    }

    [Fact]
    public void Correction_loop_allows_resubmission_and_keeps_reference()
    {
        var service = CreateService();
        var reimbursementCase = CreateDraft(service);

        var submitted = service.Transition(TenantId, reimbursementCase.Id, ReimbursementCaseStatus.Submitted);
        var firstRef = submitted.SubmissionRef;

        service.Transition(TenantId, reimbursementCase.Id, ReimbursementCaseStatus.Pending);
        service.Transition(TenantId, reimbursementCase.Id, ReimbursementCaseStatus.CorrectionRequired);
        service.Transition(TenantId, reimbursementCase.Id, ReimbursementCaseStatus.Corrected);
        var resubmitted = service.Transition(TenantId, reimbursementCase.Id, ReimbursementCaseStatus.Submitted);

        Assert.Equal(ReimbursementCaseStatus.Submitted, resubmitted.Status);
        Assert.Equal(firstRef, resubmitted.SubmissionRef);
    }

    [Theory]
    [InlineData(ReimbursementCaseStatus.Pending)]
    [InlineData(ReimbursementCaseStatus.Approved)]
    [InlineData(ReimbursementCaseStatus.Completed)]
    [InlineData(ReimbursementCaseStatus.Archived)]
    public void Invalid_transitions_from_draft_are_rejected(ReimbursementCaseStatus target)
    {
        var service = CreateService();
        var reimbursementCase = CreateDraft(service);

        Assert.Throws<InvalidOperationException>(() =>
            service.Transition(TenantId, reimbursementCase.Id, target));
    }

    [Fact]
    public void Archived_is_terminal()
    {
        Assert.False(ReimbursementService.IsTransitionAllowed(ReimbursementCaseStatus.Archived, ReimbursementCaseStatus.Draft));
        Assert.False(ReimbursementService.IsTransitionAllowed(ReimbursementCaseStatus.Archived, ReimbursementCaseStatus.Submitted));
    }

    [Fact]
    public void Transition_on_unknown_case_throws_not_found()
    {
        var service = CreateService();

        Assert.Throws<KeyNotFoundException>(() =>
            service.Transition(TenantId, Guid.NewGuid(), ReimbursementCaseStatus.Submitted));
    }

    [Fact]
    public void Cases_are_isolated_between_tenants()
    {
        var service = CreateService();
        var reimbursementCase = CreateDraft(service);

        Assert.Null(service.GetCase("tenant-other", reimbursementCase.Id));
        Assert.Empty(service.ListCases("tenant-other"));
        Assert.Throws<KeyNotFoundException>(() =>
            service.Transition("tenant-other", reimbursementCase.Id, ReimbursementCaseStatus.Submitted));
    }
}
