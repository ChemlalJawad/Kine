namespace Kine.Modules.Patients.Domain;

/// <summary>
/// Explicit lifecycle status for a patient record (SPEC/02: "creation, mise a jour, statut, historique").
/// </summary>
public enum PatientStatus
{
    Active = 0,
    Archived = 1
}
