using Kine.Modules.Clinical.Domain;

namespace Kine.Modules.Clinical.Application;

/// <summary>
/// Prescription enrichie de son utilisation reelle : seances imputees, seances
/// restantes et etat d'expiration -- ce que l'UI affiche comme alertes F-A4.
/// </summary>
public sealed record PrescriptionUsage(
    Prescription Prescription,
    int SeancesUsed,
    int SeancesRemaining,
    bool IsExpired);
