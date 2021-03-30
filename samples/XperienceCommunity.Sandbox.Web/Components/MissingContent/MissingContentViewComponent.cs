using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace XperienceCommunity.Sandbox.Web.Components.MissingContent
{
    public class MissingContentViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Result failure)
        {
            return View("_MissingContent", failure.Error);
        }
    }
}