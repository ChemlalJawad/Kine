using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Kine.UnitTests;

public class RlsPolicyScriptTests
{
    private static readonly string ScriptPath =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "infra", "scripts", "p0-005-rls-core-tables.sql"));

    private static readonly IReadOnlyList<string> CoreTables = new[]
    {
        "identity_users",
        "identity_roles",
        "identity_user_roles",
        "patients",
        "patient_contacts",
        "patient_consents",
        "clinical_records",
        "clinical_sessions",
        "clinical_documents",
        "appointments",
        "practitioner_slots",
        "invoices",
        "invoice_lines",
        "payments",
        "reimbursement_cases",
        "reimbursement_events",
        "audit_logs_append_only"
    };

    [Fact]
    public void Rls_script_covers_all_core_tables()
    {
        var script = File.ReadAllText(ScriptPath);

        foreach (var table in CoreTables)
        {
            Assert.Contains($"ALTER TABLE IF EXISTS public.{table} ENABLE ROW LEVEL SECURITY;", script);
            Assert.Contains($"ALTER TABLE IF EXISTS public.{table} FORCE ROW LEVEL SECURITY;", script);
            Assert.Contains($"DROP POLICY IF EXISTS {table}_tenant_isolation ON public.{table};", script);
            Assert.Contains($"CREATE POLICY {table}_tenant_isolation ON public.{table}", script);
        }
    }

    [Fact]
    public void Rls_script_uses_tenant_setting_for_read_and_write_paths()
    {
        var script = File.ReadAllText(ScriptPath);

        Assert.Contains("USING (tenant_id::text = current_setting('app.tenant_id', true))", script);
        Assert.Contains("WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true))", script);
    }
}
