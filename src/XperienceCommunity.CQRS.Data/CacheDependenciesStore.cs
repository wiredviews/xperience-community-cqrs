using System.Collections.Concurrent;

namespace XperienceCommunity.CQRS.Data;

public interface ICacheDependenciesStore
{
    void Store(string[] keys);
}

public interface ICacheDependenciesScope
{
    void Begin();
    IEnumerable<string> End();
}

public class CacheDependenciesStore : ICacheDependenciesStore, ICacheDependenciesScope
{
    private readonly ConcurrentStack<HashSet<string>> keyScopes = new();

    public void Begin() => keyScopes.Push(new HashSet<string>(StringComparer.OrdinalIgnoreCase));

    public void Store(string[] keys)
    {
        if (!keyScopes.TryPeek(out var currentScope))
        {
            return;
        }

        foreach (string key in keys)
        {
            currentScope.Add(key);
        }
    }

    public IEnumerable<string> End()
    {
        if (!keyScopes.TryPop(out var currentScope))
        {
            return Enumerable.Empty<string>();
        }

        if (!keyScopes.TryPeek(out var parentScope))
        {
            return currentScope.AsEnumerable();
        }

        foreach (string? key in currentScope)
        {
            parentScope.Add(key);
        }

        return currentScope.AsEnumerable();
    }
}
