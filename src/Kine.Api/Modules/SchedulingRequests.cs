using System;

namespace Kine.Api.Modules;

/// <summary>
/// Request payloads for the Scheduling HTTP surface (Kine.Api layer).
/// </summary>
public sealed record CreateSlotRequest(string PractitionerId, DateTime StartAtUtc, DateTime EndAtUtc);
public sealed record BookAppointmentRequest(Guid SlotId, Guid PatientId);
public sealed record CreatePractitionerRequest(string FirstName, string LastName, string? InamiNumber);
