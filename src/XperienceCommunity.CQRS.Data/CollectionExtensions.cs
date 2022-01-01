using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        public static async Task<Maybe<TSource>> TryFirst<TSource>(this Task<IEnumerable<TSource>> source)
        {
            var result = await source;

            return result.TryFirst();
        }

        public static async Task<Maybe<TReturn>> TryFirst<TSource, TReturn>(this Task<IEnumerable<TSource>> source, Func<TSource, TReturn> projection)
        {
            var result = await source;

            return result.Select(projection).TryFirst();
        }
    }
}