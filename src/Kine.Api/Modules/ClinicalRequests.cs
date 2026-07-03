using System;

namespace Kine.Api.Modules;

/// <summary>
/// Request payloads for the Clinical HTTP surface (Kine.Api layer).
/// </summary>
public sealed record CreateSeanceRequest(DateTime DateSeanceUtc, string? Note = null, Guid? AppointmentId = null);
