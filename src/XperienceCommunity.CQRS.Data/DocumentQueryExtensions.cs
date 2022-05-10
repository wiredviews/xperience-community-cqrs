using CMS.DocumentEngine;

namespace CMS.DocumentEngine;

public static class DocumentQueryExtensions
{
    public static async Task<Maybe<TDocument>> TryFirst<TDocument>(this DocumentQuery<TDocument> query, CancellationToken token)
        where TDocument : TreeNode, new()
    {
        var result = await query.GetEnumerableTypedResultAsync(cancellationToken: token);

        return result is null
            ? Maybe<TDocument>.None
            : result.TryFirst();
    }
}

public static class MultiDocumentQueryExtensions
{
    public static async Task<Maybe<TreeNode>> TryFirst(this MultiDocumentQuery query, CancellationToken token)
    {
        var result = await query.GetEnumerableTypedResultAsync(cancellationToken: token);

        return result is null
            ? Maybe<TreeNode>.None
            : result.TryFirst();
    }
}
