namespace XperienceCommunity.CQRS.Core;

public interface IQuery<TResponse>
{

}

public interface IQuery<TResponse, TError>
{

}

/// <summary>
/// Indicates the query's result is cacheable by the query's value(s).
/// Any variations in the query's <see cref="CacheValueKey"/> result in
/// unique cache item entries
/// </summary>
public interface ICacheByValueQuery
{
    string CacheValueKey { get; }
}

public class NodeGuidQuery<TQueryResponse> : IQuery<TQueryResponse>, ICacheByValueQuery
{
    public NodeGuidQuery(Guid nodeGuid) => NodeGuid = nodeGuid;

    public virtual string CacheValueKey => NodeGuid.ToString();

    public Guid NodeGuid { get; }
}

public abstract class NodeAliasPathQuery<TQueryResponse> : IQuery<TQueryResponse>, ICacheByValueQuery
{
    public string NodeAliasPath { get; }

    public virtual string CacheValueKey => NodeAliasPath.ToLowerInvariant();

    public NodeAliasPathQuery(string nodeAliasPath) => NodeAliasPath = nodeAliasPath ?? "";
}
