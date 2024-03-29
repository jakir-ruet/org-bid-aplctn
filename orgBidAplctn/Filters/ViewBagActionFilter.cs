using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Linq;

namespace orgBidAplctn.Filters
{
    public class ViewBagActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string userName = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userFullName")?.Value;
            string usrAccType = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "accessType")?.Value;
            string profilePic = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "profilePic")?.Value;
            ((Controller)context.Controller).ViewBag.UserFullName = userName;
            ((Controller)context.Controller).ViewBag.UsrAccType = usrAccType;
            ((Controller)context.Controller).ViewBag.UserProfileImg = profilePic;
            base.OnActionExecuting(context);
        }
    }
}
