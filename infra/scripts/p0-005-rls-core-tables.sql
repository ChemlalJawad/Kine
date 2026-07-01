-- P0-005: activate Row-Level Security on Q-INE core tables.

ALTER TABLE IF EXISTS public.identity_users ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.identity_users FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS identity_users_tenant_isolation ON public.identity_users;
CREATE POLICY identity_users_tenant_isolation ON public.identity_users
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.identity_roles ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.identity_roles FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS identity_roles_tenant_isolation ON public.identity_roles;
CREATE POLICY identity_roles_tenant_isolation ON public.identity_roles
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.identity_user_roles ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.identity_user_roles FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS identity_user_roles_tenant_isolation ON public.identity_user_roles;
CREATE POLICY identity_user_roles_tenant_isolation ON public.identity_user_roles
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.patients ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.patients FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS patients_tenant_isolation ON public.patients;
CREATE POLICY patients_tenant_isolation ON public.patients
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.patient_contacts ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.patient_contacts FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS patient_contacts_tenant_isolation ON public.patient_contacts;
CREATE POLICY patient_contacts_tenant_isolation ON public.patient_contacts
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.patient_consents ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.patient_consents FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS patient_consents_tenant_isolation ON public.patient_consents;
CREATE POLICY patient_consents_tenant_isolation ON public.patient_consents
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.clinical_records ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.clinical_records FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS clinical_records_tenant_isolation ON public.clinical_records;
CREATE POLICY clinical_records_tenant_isolation ON public.clinical_records
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.clinical_sessions ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.clinical_sessions FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS clinical_sessions_tenant_isolation ON public.clinical_sessions;
CREATE POLICY clinical_sessions_tenant_isolation ON public.clinical_sessions
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.clinical_documents ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.clinical_documents FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS clinical_documents_tenant_isolation ON public.clinical_documents;
CREATE POLICY clinical_documents_tenant_isolation ON public.clinical_documents
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.appointments ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.appointments FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS appointments_tenant_isolation ON public.appointments;
CREATE POLICY appointments_tenant_isolation ON public.appointments
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.practitioner_slots ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.practitioner_slots FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS practitioner_slots_tenant_isolation ON public.practitioner_slots;
CREATE POLICY practitioner_slots_tenant_isolation ON public.practitioner_slots
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.invoices ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.invoices FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS invoices_tenant_isolation ON public.invoices;
CREATE POLICY invoices_tenant_isolation ON public.invoices
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.invoice_lines ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.invoice_lines FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS invoice_lines_tenant_isolation ON public.invoice_lines;
CREATE POLICY invoice_lines_tenant_isolation ON public.invoice_lines
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.payments ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.payments FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS payments_tenant_isolation ON public.payments;
CREATE POLICY payments_tenant_isolation ON public.payments
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.reimbursement_cases ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.reimbursement_cases FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS reimbursement_cases_tenant_isolation ON public.reimbursement_cases;
CREATE POLICY reimbursement_cases_tenant_isolation ON public.reimbursement_cases
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.reimbursement_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.reimbursement_events FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS reimbursement_events_tenant_isolation ON public.reimbursement_events;
CREATE POLICY reimbursement_events_tenant_isolation ON public.reimbursement_events
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE IF EXISTS public.audit_logs_append_only ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS public.audit_logs_append_only FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS audit_logs_append_only_tenant_isolation ON public.audit_logs_append_only;
CREATE POLICY audit_logs_append_only_tenant_isolation ON public.audit_logs_append_only
    USING (tenant_id::text = current_setting('app.tenant_id', true))
    WITH CHECK (tenant_id::text = current_setting('app.tenant_id', true));
