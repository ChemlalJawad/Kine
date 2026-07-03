using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Kine.Modules.Billing.Application;
using Kine.Modules.Billing.Domain;
using Kine.Modules.Clinical.Application;
using Kine.Modules.Patients.Application;
using Kine.Modules.Patients.Domain;
using Kine.Modules.Scheduling.Application;
using Kine.Modules.Scheduling.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kine.Api.Modules;

/// <summary>
/// Reporting v1 (roadmap S11): vues agregees read-only (activite, CA,
/// remboursements) + export CSV. Acces reserve AdminCabinet via RbacMiddleware.
/// MVP: l'agregation est composee ici, dans la couche API (qui reference deja
/// tous les modules), plutot que dans Kine.Modules.Reporting -- le module reste
/// un marqueur tant qu'aucune persistence dediee (vues materialisees) n'existe.
/// </summary>
public static class ReportingEndpoints
{
    private const string TenantItemKey = "TenantId";

    private sealed record MonthlyRow(
        string Month,
        int Appointments,
        int Completed,
        int Cancelled,
        int NoShows,
        int Seances,
        decimal InvoicedAmount,
        decimal ReimbursedAmount,
        int PendingInvoices,
        int RejectedInvoices);

    public static void MapReportingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reporting");

        group.MapGet("/summary", (
            PatientService patients,
            SchedulingService scheduling,
            BillingService billing,
            ClinicalService clinical,
            HttpContext context) =>
        {
            var tenantId = Tenant(context);
            var allPatients = patients.ListPatients(tenantId);
            var rows = BuildMonthlyRows(tenantId, allPatients, scheduling, billing, clinical);

            return Results.Ok(new
            {
                patientsActive = allPatients.Count(p => p.Status == PatientStatus.Active),
                patientsArchived = allPatients.Count(p => p.Status == PatientStatus.Archived),
                months = rows
            });
        });

        group.MapGet("/export.csv", (
            PatientService patients,
            SchedulingService scheduling,
            BillingService billing,
            ClinicalService clinical,
            HttpContext context) =>
        {
            var tenantId = Tenant(context);
            var rows = BuildMonthlyRows(tenantId, patients.ListPatients(tenantId), scheduling, billing, clinical);

            var csv = new StringBuilder();
            csv.AppendLine("mois;rdv;termines;annules;no_shows;seances;montant_facture;montant_rembourse;factures_en_attente;factures_rejetees");
            foreach (var row in rows)
            {
                csv.AppendLine(string.Join(';',
                    row.Month,
                    row.Appointments,
                    row.Completed,
                    row.Cancelled,
                    row.NoShows,
                    row.Seances,
                    row.InvoicedAmount.ToString("0.00", CultureInfo.InvariantCulture),
                    row.ReimbursedAmount.ToString("0.00", CultureInfo.InvariantCulture),
                    row.PendingInvoices,
                    row.RejectedInvoices));
            }

            return Results.File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "q-ine-reporting.csv");
        });
    }

    private static IReadOnlyList<MonthlyRow> BuildMonthlyRows(
        string tenantId,
        IReadOnlyList<Patient> allPatients,
        SchedulingService scheduling,
        BillingService billing,
        ClinicalService clinical)
    {
        var appointments = scheduling.ListAppointments(tenantId);
        var invoices = billing.ListInvoices(tenantId);
        var seances = allPatients
            .SelectMany(patient => clinical.ListSeances(tenantId, patient.Id))
            .ToList();

        static string MonthKey(DateTime utc) => utc.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        var monthKeys = appointments.Select(a => MonthKey(a.StartAtUtc))
            .Concat(invoices.Select(i => MonthKey(i.CreatedAtUtc)))
            .Concat(seances.Select(s => MonthKey(s.DateSeanceUtc)))
            .Distinct()
            .OrderByDescending(key => key)
            .ToList();

        return monthKeys.Select(month =>
        {
            var monthAppointments = appointments.Where(a => MonthKey(a.StartAtUtc) == month).ToList();
            var monthInvoices = invoices.Where(i => MonthKey(i.CreatedAtUtc) == month).ToList();
            var monthSeances = seances.Count(s => MonthKey(s.DateSeanceUtc) == month);

            return new MonthlyRow(
                month,
                monthAppointments.Count,
                monthAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                monthAppointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                monthAppointments.Count(a => a.Status == AppointmentStatus.NoShow),
                monthSeances,
                monthInvoices.Sum(i => i.Amount),
                monthInvoices.Where(i => i.Status == InvoiceStatus.Reimbursed).Sum(i => i.Amount),
                monthInvoices.Count(i => i.Status == InvoiceStatus.Pending),
                monthInvoices.Count(i => i.Status == InvoiceStatus.Rejected));
        }).ToList();
    }

    private static string Tenant(HttpContext context) => (string)context.Items[TenantItemKey]!;
}
