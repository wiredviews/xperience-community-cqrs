using CMS.DocumentEngine.Types.Sandbox;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using XperienceCommunity.CQRS.Core;
using XperienceCommunity.Sandbox.Core.Features.Home;

namespace XperienceCommunity.Sandbox.Web.Features.Home.Components;

public class HomePageViewComponent : ViewComponent
{
    private readonly IQueryDispatcher dispatcher;

    public HomePageViewComponent(IQueryDispatcher dispatcher) => this.dispatcher = dispatcher;

    public Task<IViewComponentResult> InvokeAsync(HomePage page) =>
        dispatcher.Dispatch(new HomePageQuery(), HttpContext.RequestAborted)
            .ViewWithFallbackOnFailure(this, "_HomePage", r => new HomePageViewModel(r));
}

public class HomePageViewModel
{
    public HomePageViewModel(HomePageQueryData data)
    {
        Title = data.Title;
        BodyHTML = data.BodyHTML.Map(b => new HtmlString(b));
        ImagePath = data.Image.Map(i => i.ImagePath);
    }

    public string Title { get; }
    public Maybe<HtmlString> BodyHTML { get; }
    public Maybe<string> ImagePath { get; }
}
