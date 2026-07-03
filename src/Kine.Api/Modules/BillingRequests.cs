using System;

namespace Kine.Api.Modules;

/// <summary>
/// Request payloads for the Billing HTTP surface (Kine.Api layer).
/// Mutuelle is provided by the caller (copied from the patient record client-side)
/// because Billing has no direct access to the Patients module store.
/// </summary>
public sealed record CreateInvoiceRequest(Guid PatientId, string CodeInami, string? Mutuelle);
