namespace Kine.Modules.Billing.Domain;

/// <summary>
/// Entree du catalogue d'actes INAMI (nomenclature kinesitherapie).
/// Montants demo en attendant la validation des tarifs officiels
/// (SPEC/14-reimbursement-rules.md, gate "validation INAMI rules").
/// </summary>
public sealed record ActeInami
{
    public required string Code { get; init; }
    public required string Label { get; init; }

    /// <summary>Montant facturable en EUR (placeholder MVP, a confirmer INAMI).</summary>
    public required decimal Amount { get; init; }
}
