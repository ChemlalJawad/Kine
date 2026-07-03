namespace Kine.Api.Security;

/// <summary>
/// Roles cabinet (P0-006, Q-B06 ferme) : AdminCabinet, Kine, Assistant, Billing.
/// </summary>
public static class RoleNames
{
    public const string AdminCabinet = "AdminCabinet";
    public const string Kine = "Kine";
    public const string Assistant = "Assistant";
    public const string Billing = "Billing";

    public static readonly string[] All = { AdminCabinet, Kine, Assistant, Billing };
}
