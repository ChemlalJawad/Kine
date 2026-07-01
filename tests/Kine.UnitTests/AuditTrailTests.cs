using System.Linq;
using Kine.Modules.Audit.Application;
using Kine.Modules.Audit.Domain;
using Kine.Modules.Audit.Infrastructure;
using Xunit;

namespace Kine.UnitTests;

public class AuditTrailTests
{
    [Fact]
    public void Record_chains_events_via_prev_hash_and_event_hash()
    {
        var store = new InMemoryAuditLogStore();
        var service = new AuditTrailService(store);

        var first = service.Record("tenant-a", "user-1", "create", "patient", "p-1", "payload-1");
        var second = service.Record("tenant-a", "user-1", "update", "patient", "p-1", "payload-2");

        Assert.Equal(AuditHash.GenesisHash, first.PrevHash);
        Assert.Equal(first.EventHash, second.PrevHash);
        Assert.NotEqual(first.EventHash, second.EventHash);
    }

    [Fact]
    public void Record_isolates_chain_per_tenant()
    {
        var store = new InMemoryAuditLogStore();
        var service = new AuditTrailService(store);

        service.Record("tenant-a", "user-1", "create", "patient", "p-1", "payload");
        service.Record("tenant-b", "user-2", "create", "patient", "p-2", "payload");

        var chainA = store.GetChain("tenant-a");
        var chainB = store.GetChain("tenant-b");

        Assert.Single(chainA);
        Assert.Single(chainB);
        Assert.All(chainA, e => Assert.Equal("tenant-a", e.TenantId));
        Assert.All(chainB, e => Assert.Equal("tenant-b", e.TenantId));
    }

    [Fact]
    public void AuditChainVerifier_accepts_untouched_chain()
    {
        var store = new InMemoryAuditLogStore();
        var service = new AuditTrailService(store);

        service.Record("tenant-a", "user-1", "create", "patient", "p-1", "payload-1");
        service.Record("tenant-a", "user-1", "update", "patient", "p-1", "payload-2");
        service.Record("tenant-a", "user-1", "view", "patient", "p-1", "payload-3");

        var chain = store.GetChain("tenant-a");

        Assert.True(AuditChainVerifier.IsValid(chain));
    }

    [Fact]
    public void AuditChainVerifier_detects_tampering_of_a_middle_event()
    {
        var store = new InMemoryAuditLogStore();
        var service = new AuditTrailService(store);

        service.Record("tenant-a", "user-1", "create", "patient", "p-1", "payload-1");
        service.Record("tenant-a", "user-1", "update", "patient", "p-1", "payload-2");
        service.Record("tenant-a", "user-1", "view", "patient", "p-1", "payload-3");

        var chain = store.GetChain("tenant-a").ToList();
        var tampered = chain[1] with { Action = "delete" };
        chain[1] = tampered;

        Assert.False(AuditChainVerifier.IsValid(chain));
    }

    [Fact]
    public void AuditChainVerifier_detects_reordered_events()
    {
        var store = new InMemoryAuditLogStore();
        var service = new AuditTrailService(store);

        service.Record("tenant-a", "user-1", "create", "patient", "p-1", "payload-1");
        service.Record("tenant-a", "user-1", "update", "patient", "p-1", "payload-2");

        var chain = store.GetChain("tenant-a").ToList();
        var reordered = new[] { chain[1], chain[0] };

        Assert.False(AuditChainVerifier.IsValid(reordered));
    }

    [Fact]
    public void GetChain_returns_a_snapshot_not_the_backing_store()
    {
        var store = new InMemoryAuditLogStore();
        var service = new AuditTrailService(store);

        service.Record("tenant-a", "user-1", "create", "patient", "p-1", "payload-1");

        var snapshot = store.GetChain("tenant-a").ToList();
        snapshot.Clear();

        Assert.Single(store.GetChain("tenant-a"));
    }
}
