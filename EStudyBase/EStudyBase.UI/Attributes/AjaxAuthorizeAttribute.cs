using System.Web.Mvc;
using WebMatrix.WebData;

namespace EStudyBase.UI.Attributes
{
    public class AjaxAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext context)
        {
            if (context.HttpContext.Request.IsAjaxRequest())
            {
                var urlHelper = new UrlHelper(context.RequestContext);
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new JsonResult
                {
                    Data = new
                    {
                        Error = "NotAuthorized",
                        LogOnUrl = urlHelper.Action("Login", "Account")
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                //...
                // If User is authenticated and has no access
                if (WebSecurity.IsAuthenticated)
                {
                    context.RequestContext.HttpContext.Response.Redirect("/Account/UnAuthorized");
                }
                base.HandleUnauthorizedRequest(context);
            }
        }
    }
}