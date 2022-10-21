using CMS.Base;

namespace XperienceCommunity.CQRS.Data.Tests;

public class CacheDependencyKeysBuilderTests
{
    [Test, AutoDomainData]
    public void CustomKeyWillAddAKeyIfItsIsValid(ISiteService siteService)
    {
        var sut = new CacheDependencyKeysBuilder(siteService);

        _ = sut.CustomKey("key");

        var keys = sut.GetKeys();

        _ = keys.Should().BeEquivalentTo(new[] { "key" });

        keys.Clear();

        _ = sut.CustomKey("  ");

        keys = sut.GetKeys();

        _ = keys.Should().BeEquivalentTo(Array.Empty<string>());
    }
}
