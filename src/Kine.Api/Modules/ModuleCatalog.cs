using System.Collections.Generic;
using Kine.Modules.Audit;
using Kine.Modules.Billing;
using Kine.Modules.Clinical;
using Kine.Modules.Identity;
using Kine.Modules.Patients;
using Kine.Modules.Reimbursement;
using Kine.Modules.Reporting;
using Kine.Modules.Scheduling;
using Kine.SharedKernel;

namespace Kine.Api.Modules;

public static class ModuleCatalog
{
    public static IReadOnlyCollection<IModule> All { get; } = new IModule[]
    {
        new IdentityModule(),
        new PatientsModule(),
        new ClinicalModule(),
        new SchedulingModule(),
        new BillingModule(),
        new ReimbursementModule(),
        new ReportingModule(),
        new AuditModule()
    };
}
