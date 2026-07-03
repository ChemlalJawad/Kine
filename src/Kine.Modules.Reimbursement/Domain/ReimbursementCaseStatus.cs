namespace Kine.Modules.Reimbursement.Domain;

/// <summary>
/// Etats du dossier remboursement, conformes a SPEC/14-reimbursement-rules.md.
/// Les transitions autorisees sont validees par ReimbursementService (state machine).
/// </summary>
public enum ReimbursementCaseStatus
{
    /// <summary>Cree, non soumis.</summary>
    Draft = 0,

    /// <summary>Soumis a l'INAMI (mock eFact tant que Q-B03 est ouvert).</summary>
    Submitted = 1,

    /// <summary>En cours de traitement INAMI.</summary>
    Pending = 2,

    /// <summary>Approuve, remboursement futur.</summary>
    Approved = 3,

    /// <summary>Rejete par l'INAMI.</summary>
    Rejected = 4,

    /// <summary>Correction INAMI requise.</summary>
    CorrectionRequired = 5,

    /// <summary>Corrige, pret pour resoumission.</summary>
    Corrected = 6,

    /// <summary>Remboursement finalise (ou rejet acte).</summary>
    Completed = 7,

    /// <summary>Cloture, tracabilite maintenue.</summary>
    Archived = 8
}
