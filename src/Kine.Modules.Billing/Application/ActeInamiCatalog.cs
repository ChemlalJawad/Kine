using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Billing.Domain;

namespace Kine.Modules.Billing.Application;

/// <summary>
/// Catalogue statique des actes INAMI facturables (MVP).
/// Codes/montants demo issus du mockup Q-INE Redesign; a remplacer par la
/// nomenclature officielle une fois les tarifs valides (SPEC/14, Q-B03).
/// </summary>
public static class ActeInamiCatalog
{
    public static IReadOnlyList<ActeInami> All { get; } = new ActeInami[]
    {
        new() { Code = "560011", Label = "Seance kine courante — cabinet", Amount = 26.14m },
        new() { Code = "558014", Label = "Seance kine — pathologie lourde", Amount = 23.45m },
        new() { Code = "558310", Label = "Reeducation post-operatoire", Amount = 21.80m },
        new() { Code = "558891", Label = "Kinesitherapie respiratoire", Amount = 19.60m },
        new() { Code = "639192", Label = "Bilan kinesitherapique", Amount = 32.00m }
    };

    public static ActeInami? Find(string code) =>
        All.FirstOrDefault(acte => acte.Code == code);
}
