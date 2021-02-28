using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;

namespace XperienceCommunity.CQRS.Core
{
    public interface IPagedQuery
    {
        /// <summary>
        /// 0-based page number/index
        /// </summary>
        int PageIndex { get; }
        /// <summary>
        /// Number of items per result page
        /// </summary>
        int PageSize { get; }
    }

    public abstract class PagedQueryResponse<TItem>
    {
        /// <summary>
        /// Total count of items across all pages
        /// </summary>
        public int TotalItemCount { get; }
        /// <summary>
        /// Current page number, 0-based
        /// </summary>
        public int CurrentPageIndex { get; }

        public IEnumerable<TItem> Items { get; }

        /// <summary>
        /// Total number of pages calculated from the <see cref="IPagedQuery.PageSize"/> and number of <see cref="Items"/>
        /// </summary>
        public int TotalPageCount { get; }

        public PagedQueryResponse(
            IPagedQuery query,
            int totalItemCount,
            IEnumerable<TItem> items)
        {
            Guard.Against.Null(query, nameof(query));
            Guard.Against.Null(items, nameof(items));
            Guard.Against.Negative(totalItemCount, nameof(totalItemCount));

            TotalItemCount = totalItemCount;
            CurrentPageIndex = query.PageIndex;
            TotalPageCount = (int)Math.Ceiling((decimal)TotalItemCount / query.PageSize);
            Items = items;
        }
    }

    public abstract class PagedQuery<TResponse, UItem> :
        IPagedQuery,
        ICacheByValueQuery,
        IQuery<TResponse> where TResponse : PagedQueryResponse<UItem>
    {
        /// <summary>
        /// 0-based page number/index
        /// </summary>
        public int PageIndex { get; }
        /// <summary>
        /// Number of items per result page
        /// </summary>
        public int PageSize { get; }

        public virtual string CacheValueKey => $"page:{PageIndex}:{PageSize}";

        public PagedQuery(int pageIndex, int pageSize = 10)
        {
            Guard.Against.Negative(pageIndex, nameof(pageIndex));
            Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

            PageIndex = pageIndex;
            PageSize = pageSize;
        }
    }

    public interface IPagedOrderableQuery : IPagedQuery
    {
        /// <summary>
        /// Query result order by field name(s). Defaults to ""
        /// </summary>
        string OrderBy { get; }
        /// <summary>
        /// Direction of query order; only used if the OrderBy field is specified. Defaults to true
        /// </summary>
        bool OrderByDesc { get; }
    }

    public abstract class PagedOrderableQuery<TResponse, UItem> :
        PagedQuery<TResponse, UItem>, 
        IPagedOrderableQuery where TResponse : PagedQueryResponse<UItem>

    {
        protected PagedOrderableQuery(int pageIndex, int pageSize = 10, string? orderBy = null, bool orderByDesc = true) : base(pageIndex, pageSize)
        {
            OrderBy = orderBy ?? "";
            OrderByDesc = orderByDesc;
        }

        /// <summary>
        /// Query result order by field name(s). Defaults to ""
        /// </summary>
        public string OrderBy { get; }
        /// <summary>
        /// Direction of query order; only used if the OrderBy field is specified. Defaults to true
        /// </summary>
        public bool OrderByDesc { get; }

        public override string CacheValueKey => $"{base.CacheValueKey}:{OrderBy}:{OrderByDesc}";
    }
}