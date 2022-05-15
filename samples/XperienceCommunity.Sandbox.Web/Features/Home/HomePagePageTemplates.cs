using CMS.DocumentEngine.Types.Sandbox;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using XperienceCommunity.Sandbox.Web.Features.Home;

[assembly: RegisterPageTemplate(
    "Sandbox.HomePage_Default",
    "Home Page (Default)",
    typeof(HomePageTemplateProperties),
    "~/Features/Home/Sandbox.HomePage_Default.cshtml")]

namespace XperienceCommunity.Sandbox.Web.Features.Home;

public class HomePagePageTemplates : PageTypePageTemplateFilter
{
    public override string PageTypeClassName => HomePage.CLASS_NAME;
}

public class HomePageTemplateProperties : IPageTemplateProperties
{

}
