using System;

namespace Kine.Modules.Clinical.Domain;

/// <summary>
/// Prescription medicale de kinesitherapie (table prescriptions). Tenant-scoped.
/// F-A4 : condition du remboursement INAMI -- validite de 2 mois a compter de la
/// date de prescription, nombre maximum de seances prescrit par le medecin.
/// Les seances cliniques peuvent s'y rattacher (SeanceClinique.PrescriptionId);
/// le service refuse une seance au-dela du quota ou hors validite.
/// </summary>
public sealed record Prescription
{
    public required Guid Id { get; init; }
    public required string TenantId { get; init; }
    public required Guid PatientId { get; init; }

    /// <summary>Nom du medecin prescripteur (contenu obligatoire de la prescription).</summary>
    public required string PrescriberName { get; init; }

    /// <summary>Numero INAMI du prescripteur (optionnel au MVP).</summary>
    public string? PrescriberInami { get; init; }

    /// <summary>Date de la prescription.</summary>
    public required DateTime PrescribedAtUtc { get; init; }

    /// <summary>Fin de validite : prescription + 2 mois (regle INAMI).</summary>
    public required DateTime ValidUntilUtc { get; init; }

    /// <summary>Diagnostic / elements medicaux portes sur la prescription.</summary>
    public string? Diagnosis { get; init; }

    /// <summary>Nombre maximum de seances prescrites.</summary>
    public required int SessionsPrescribed { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
    public required string CreatedBy { get; init; }
}
