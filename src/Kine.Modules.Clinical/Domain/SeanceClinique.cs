using System;

namespace Kine.Modules.Clinical.Domain;

/// <summary>
/// Seance clinique realisee (table seances_cliniques). Tenant-scoped.
/// Resout Q-B20 : "+ Ajouter une seance" cree desormais un enregistrement reel
/// et tracable au lieu d'incrementer un entier sans trace; le compteur
/// Patient.SessionsDone devient derive (nombre de seances) cote UI.
/// Lien optionnel vers le rendez-vous (module Scheduling) par id uniquement,
/// sans reference projet croisee (isolation des modules).
/// </summary>
public sealed record SeanceClinique
{
    public required Guid Id { get; init; }
    public required string TenantId { get; init; }
    public required Guid PatientId { get; init; }

    /// <summary>Rendez-vous d'origine si la seance decoule d'un rdv agenda (id opaque, module Scheduling).</summary>
    public Guid? AppointmentId { get; init; }

    /// <summary>Prescription medicale a laquelle la seance est imputee (F-A4, optionnel au MVP).</summary>
    public Guid? PrescriptionId { get; init; }

    /// <summary>Date/heure de realisation de la seance.</summary>
    public required DateTime DateSeanceUtc { get; init; }

    /// <summary>Note clinique libre (MVP: texte simple, pas de structure SOAP).</summary>
    public string? Note { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
    public required string CreatedBy { get; init; }
}
