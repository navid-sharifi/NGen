using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace NGen
{
    public class Gate : ActionFilterAttribute
    {
        private string roles;
        private string token;
        private string failUrl;


        public Gate(string roles = "", string token = "", string failUrl = "/login")
        {
            this.roles = roles;
            this.token = token;
            this.failUrl = failUrl;

        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var gateService = context.HttpContext.RequestServices.GetService<GateService>();


            //if (gateService == null) return;

            //if (!string.IsNullOrEmpty(token))
            //{
            //    var tk = gateService.ReadToken();
            //    if (!string.IsNullOrEmpty(tk) && token == tk)
            //    {
            //        await base.OnActionExecutionAsync(context, next);
            //        return;
            //    }
            //    else
            //    {
            //        context.Result = new OkObjectResult("token isn't valid");
            //        return;
            //    }
            //}

            var token = context.HttpContext.Request.Headers?["ngate"].ToString();
            var user = await gateService.GetUserByToken(token);
            if (user is null)
            {
                context.Result = new BadRequestObjectResult(new { redirect = failUrl });
                return;
            }

            await base.OnActionExecutionAsync(context, next);

            //if (string.IsNullOrEmpty(roles) ? await gateService.IsUserAvailable() : await gateService.IsUserAvailable(roles.Split(',')))
            //{
            //    await base.OnActionExecutionAsync(context, next);
            //    return;
            //}

            //context.Result = new OkObjectResult(new ApiResult() { Type = ApiResultType.Redirect, Url = failUrl });

            //context.Result = new RedirectToRouteResult(
            //      new RouteValueDictionary {{ "Controller", "Account" },
            //              { "Action", "Login" } });

            //await base.OnActionExecutionAsync(context, next);
        }


    }
}
