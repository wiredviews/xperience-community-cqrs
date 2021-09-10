using System;
using System.Linq;
using CMS.DocumentEngine;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Options;
using XperienceCommunity.CQRS.Data;
using XperienceCommunity.PageBuilderModeTagHelper;

namespace XperienceCommunity.CQRS.Web
{
    /// <summary>
    /// Settings for Razor (CacheTagHelper) cache configuration
    /// </summary>
    public class RazorCacheConfiguration
    {
        /// <summary>
        /// Defaults to 1 minute
        /// </summary>
        /// <returns></returns>
        public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(1);
    }

    /// <summary>
    /// Exposes helpful properties and methods from other classes when interacting and initializing with Razor caching (CacheTagHelper)
    /// </summary>
    public class RazorCacheService
    {
        private readonly IPageBuilderContext context;
        private readonly ICacheDependenciesScope scope;
        private readonly RazorCacheConfiguration config;
        private readonly IPageDataContextRetriever retriever;

        public RazorCacheService(IPageBuilderContext context, ICacheDependenciesScope scope, IOptions<RazorCacheConfiguration> config, IPageDataContextRetriever retriever)
        {
            this.retriever = retriever;
            this.scope = scope;
            this.context = context;
            this.config = config.Value;
        }

        /// <summary>
        /// Expiration length for cached content
        /// </summary>
        public TimeSpan Expires => config.CacheExpiration;

        /// <summary>
        /// True if caching is currently enabled based on the <see cref="IPageBuilderContext" />
        /// </summary>
        public bool IsEnabled => context.IsLiveMode;

        /// <summary>
        /// Starts a cache dependencies scope
        /// </summary>
        /// <returns></returns>
        public HtmlString BeginScope()
        {
            scope.Begin();

            return HtmlString.Empty;
        }

        /// <summary>
        /// Ends a cache dependencies scope and returns the collected dependency keys
        /// </summary>
        /// <returns></returns>
        public string[] EndScope() =>
            scope.End().ToArray();

        /// <summary>
        /// The default 'vary by' for cached content which varies by the current Document
        /// </summary>
        /// <returns></returns>
        public string VaryBy() =>
            retriever.TryRetrieve<TreeNode>(out var data)
                ? data.Page.DocumentGUID.ToString()
                : "";
    }

}