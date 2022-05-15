namespace XperienceCommunity.CQRS.Core.Tests;

public class QueryTests
{
    [Test]
    public void NodeGuidQuery_Will_Generate_A_CacheValueKey()
    {
        var guid = Guid.NewGuid();

        var sut = new TestNodeGuidQuery(guid);

        sut.CacheValueKey.Should().Be(guid.ToString());
    }

    [Test]
    public void NodeAliasPathQuery_Will_Generate_A_CacheValueKey()
    {
        string? path = "/path/to/node";

        var sut = new TestNodeAliasPathQuery(path);

        sut.CacheValueKey.Should().Be(path.ToLowerInvariant());
    }

    internal class TestNodeGuidQuery : NodeGuidQuery<string>
    {
        public TestNodeGuidQuery(Guid nodeGuid) : base(nodeGuid)
        {
        }
    }

    internal class TestNodeAliasPathQuery : NodeAliasPathQuery<string>
    {
        public TestNodeAliasPathQuery(string nodeAliasPath) : base(nodeAliasPath)
        {
        }
    }
}
