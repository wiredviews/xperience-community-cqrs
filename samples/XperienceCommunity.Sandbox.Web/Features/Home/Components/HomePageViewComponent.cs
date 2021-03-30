using System.Threading.Tasks;
using CMS.DocumentEngine;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using XperienceCommunity.CQRS.Core;
using XperienceCommunity.Sandbox.Core.Features.Home;

namespace XperienceCommunity.Sandbox.Web.Features.Home.Components
{
    public class HomePageViewComponent : ViewComponent
    {
        private readonly IQueryDispatcher dispatcher;

        public HomePageViewComponent(IQueryDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public Task<ViewViewComponentResult> InvokeAsync(TreeNode node) =>
            dispatcher.Dispatch(new HomePageQuery(), HttpContext.RequestAborted)
                .Map(r => new HomePageViewModel(r))
                .Finally(r => View("_HomePage", r));
    }

    public class HomePageViewModel
    {
        public HomePageViewModel(HomePageQueryResponse resp)
        {
            Title = resp.Title;
            BodyHTML = resp.BodyHTML.Map(b => new HtmlString(b));
            ImagePath = resp.Image.Map(i => i.ImagePath);
        }

        public string Title { get; }
        public Maybe<HtmlString> BodyHTML { get; }
        public Maybe<string> ImagePath { get; }
    }
}