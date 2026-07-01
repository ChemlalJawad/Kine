using System.Linq;
using Kine.Modules.Identity.Domain;
using Xunit;

namespace Kine.UnitTests;

public class RbacMatrixTests
{
    [Theory]
    [InlineData(Role.AdminCabinet, Permission.PatientsRead, true)]
    [InlineData(Role.AdminCabinet, Permission.ClinicalWrite, true)]
    [InlineData(Role.AdminCabinet, Permission.BillingWrite, true)]
    [InlineData(Role.AdminCabinet, Permission.ReimbursementWrite, true)]
    [InlineData(Role.AdminCabinet, Permission.AuditRead, true)]
    [InlineData(Role.AdminCabinet, Permission.IdentityManage, true)]
    public void AdminCabinet_has_full_access(Role role, Permission permission, bool expected)
    {
        Assert.Equal(expected, RbacMatrix.HasPermission(role, permission));
    }

    [Theory]
    [InlineData(Permission.PatientsRead, true)]
    [InlineData(Permission.PatientsWrite, true)]
    [InlineData(Permission.ClinicalRead, true)]
    [InlineData(Permission.ClinicalWrite, true)]
    [InlineData(Permission.SchedulingRead, true)]
    [InlineData(Permission.SchedulingWrite, true)]
    [InlineData(Permission.BillingWrite, false)]
    [InlineData(Permission.ReimbursementWrite, false)]
    [InlineData(Permission.AuditRead, false)]
    [InlineData(Permission.IdentityManage, false)]
    public void Kine_role_is_scoped_to_clinical_and_scheduling(Permission permission, bool expected)
    {
        Assert.Equal(expected, RbacMatrix.HasPermission(Role.Kine, permission));
    }

    [Theory]
    [InlineData(Permission.PatientsRead, true)]
    [InlineData(Permission.PatientsWrite, true)]
    [InlineData(Permission.SchedulingRead, true)]
    [InlineData(Permission.SchedulingWrite, true)]
    [InlineData(Permission.BillingRead, true)]
    [InlineData(Permission.BillingWrite, false)]
    [InlineData(Permission.ClinicalWrite, false)]
    [InlineData(Permission.ReimbursementRead, false)]
    [InlineData(Permission.IdentityManage, false)]
    public void Assistant_role_cannot_write_clinical_or_billing(Permission permission, bool expected)
    {
        Assert.Equal(expected, RbacMatrix.HasPermission(Role.Assistant, permission));
    }

    [Theory]
    [InlineData(Permission.PatientsRead, true)]
    [InlineData(Permission.BillingRead, true)]
    [InlineData(Permission.BillingWrite, true)]
    [InlineData(Permission.ReimbursementRead, true)]
    [InlineData(Permission.ReimbursementWrite, true)]
    [InlineData(Permission.PatientsWrite, false)]
    [InlineData(Permission.ClinicalRead, false)]
    [InlineData(Permission.SchedulingWrite, false)]
    [InlineData(Permission.AuditRead, false)]
    public void Billing_role_is_scoped_to_billing_and_reimbursement(Permission permission, bool expected)
    {
        Assert.Equal(expected, RbacMatrix.HasPermission(Role.Billing, permission));
    }

    [Fact]
    public void Every_role_has_at_least_one_permission_and_no_role_has_all_permissions_except_admin()
    {
        var allRoles = new[] { Role.AdminCabinet, Role.Kine, Role.Assistant, Role.Billing };
        var allPermissions = System.Enum.GetValues<Permission>();

        foreach (var role in allRoles)
        {
            var granted = RbacMatrix.PermissionsFor(role);
            Assert.NotEmpty(granted);

            if (role != Role.AdminCabinet)
            {
                Assert.True(granted.Count < allPermissions.Length);
            }
        }

        Assert.Equal(allPermissions.Length, RbacMatrix.PermissionsFor(Role.AdminCabinet).Count);
    }

    [Fact]
    public void Only_AdminCabinet_can_manage_identity_or_read_audit()
    {
        var nonAdminRoles = new[] { Role.Kine, Role.Assistant, Role.Billing };

        Assert.All(nonAdminRoles, role =>
        {
            Assert.False(RbacMatrix.HasPermission(role, Permission.IdentityManage));
            Assert.False(RbacMatrix.HasPermission(role, Permission.AuditRead));
        });

        Assert.True(RbacMatrix.HasPermission(Role.AdminCabinet, Permission.IdentityManage));
        Assert.True(RbacMatrix.HasPermission(Role.AdminCabinet, Permission.AuditRead));
    }
}
