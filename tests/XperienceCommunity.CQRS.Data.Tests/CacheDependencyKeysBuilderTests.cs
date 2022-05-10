namespace XperienceCommunity.CQRS.Data.Tests;

public class CacheDependencyKeysBuilderTests
{
    [Test, AutoDomainData]
    public void CustomKey_Will_Add_A_Key_If_Its_Is_Valid(ISiteContext context)
    {
        var sut = new CacheDependencyKeysBuilder(context);

        sut.CustomKey("key");

        var keys = sut.GetKeys();

        keys.Should().BeEquivalentTo(new[] { "key" });

        keys.Clear();

        sut.CustomKey("  ");

        keys = sut.GetKeys();

        keys.Should().BeEquivalentTo(new string[] { });
    }
}
