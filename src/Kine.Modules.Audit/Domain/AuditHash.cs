using System;
using System.Security.Cryptography;
using System.Text;

namespace Kine.Modules.Audit.Domain;

/// <summary>
/// Canonical hashing rules for the audit hash chain.
/// Shared by event production and chain verification so both sides
/// compute the exact same digest.
/// </summary>
public static class AuditHash
{
    public const string GenesisHash = "GENESIS";

    public static string HashPayload(string? payload)
    {
        return Hash(payload ?? string.Empty);
    }

    public static string ComputeEventHash(
        Guid id,
        string tenantId,
        string actorId,
        string action,
        string entity,
        string entityId,
        DateTime tsUtc,
        string payloadHash,
        string prevHash)
    {
        var canonical = string.Join(
            '|',
            id,
            tenantId,
            actorId,
            action,
            entity,
            entityId,
            tsUtc.Ticks,
            payloadHash,
            prevHash);

        return Hash(canonical);
    }

    private static string Hash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
