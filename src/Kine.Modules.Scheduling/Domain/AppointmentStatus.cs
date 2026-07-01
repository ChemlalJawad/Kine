namespace Kine.Modules.Scheduling.Domain;

/// <summary>
/// Explicit lifecycle status for an appointment (SPEC/02: "rdv, annulation, no-show").
/// </summary>
public enum AppointmentStatus
{
    Scheduled = 0,
    Cancelled = 1,
    NoShow = 2,
    Completed = 3
}
