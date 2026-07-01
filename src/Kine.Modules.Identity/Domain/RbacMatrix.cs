using System.Collections.Generic;

namespace Kine.Modules.Identity.Domain;

/// <summary>
/// Matrice de permissions minimales par role cabinet (RBAC par tenant).
/// AdminCabinet: acces complet, incluant gestion identite et audit.
/// Kine: patients et dossier clinique et agenda.
/// Assistant: patients et agenda, lecture facturation.
/// Billing: facturation et remboursement, lecture patients.
/// </summary>
public static class RbacMatrix
{
    private static readonly IReadOnlyDictionary<Role, IReadOnlySet<Permission>> Matrix =
        new Dictionary<Role, IReadOnlySet<Permission>>
        {
            [Role.AdminCabinet] = new HashSet<Permission>
            {
                Permission.PatientsRead,
                Permission.PatientsWrite,
                Permission.ClinicalRead,
                Permission.ClinicalWrite,
                Permission.SchedulingRead,
                Permission.SchedulingWrite,
                Permission.BillingRead,
                Permission.BillingWrite,
                Permission.ReimbursementRead,
                Permission.ReimbursementWrite,
                Permission.AuditRead,
                Permission.IdentityManage
            },
            [Role.Kine] = new HashSet<Permission>
            {
                Permission.PatientsRead,
                Permission.PatientsWrite,
                Permission.ClinicalRead,
                Permission.ClinicalWrite,
                Permission.SchedulingRead,
                Permission.SchedulingWrite
            },
            [Role.Assistant] = new HashSet<Permission>
            {
                Permission.PatientsRead,
                Permission.PatientsWrite,
                Permission.SchedulingRead,
                Permission.SchedulingWrite,
                Permission.BillingRead
            },
            [Role.Billing] = new HashSet<Permission>
            {
                Permission.PatientsRead,
                Permission.BillingRead,
                Permission.BillingWrite,
                Permission.ReimbursementRead,
                Permission.ReimbursementWrite
            }
        };

    public static IReadOnlySet<Permission> PermissionsFor(Role role) => Matrix[role];

    public static bool HasPermission(Role role, Permission permission) => Matrix[role].Contains(permission);
}
