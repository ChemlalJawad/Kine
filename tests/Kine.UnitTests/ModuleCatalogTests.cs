using Kine.Api.Modules;
using Xunit;

namespace Kine.UnitTests;

public class ModuleCatalogTests
{
    [Fact]
    public void ModuleCatalog_contains_all_backend_modules()
    {
        Assert.Equal(8, ModuleCatalog.All.Count);
        Assert.Contains(ModuleCatalog.All, module => module.Name == "Identity");
    }
}
