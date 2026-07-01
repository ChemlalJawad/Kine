using System;
using Kine.Modules.Patients.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kine.Api.Modules;

/// <summary>
/// Request payloads for the Patients HTTP surface (Kine.Api layer).
/// </summary>
public sealed record CreatePatientRequest(string FirstName, string LastName, DateOnly? DateOfBirth);
public sealed record UpdatePatientRequest(string FirstName, string LastName, DateOnly? DateOfBirth);
public sealed record CreatePatientContactRequest(PatientContactType Type, string Value, bool IsPrimary);
public sealed record UpdatePatientContactRequest(string Value, bool IsPrimary);
public sealed record CreatePatientConsentRequest(ConsentType Type, bool Granted);
