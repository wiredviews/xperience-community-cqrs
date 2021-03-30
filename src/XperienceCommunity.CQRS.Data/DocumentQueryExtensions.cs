using System.Threading;
using System.Threading.Tasks;
using CMS.DocumentEngine;
using CSharpFunctionalExtensions;

namespace XperienceCommunity.CQRS.Data
{
    public static class DocumentQueryExtensions
    {
        public static async Task<Maybe<TDocument>> FirstOrNoneAsync<TDocument>(this DocumentQuery<TDocument> query, CancellationToken token)
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
        public static async Task<Maybe<TreeNode>> FirstOrNoneAsync(this MultiDocumentQuery query, CancellationToken token)
        {
            var result = await query.GetEnumerableTypedResultAsync(cancellationToken: token);

            return result is null
                ? Maybe<TreeNode>.None
                : result.TryFirst();
        }
    }
}