using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace APPHospitalCore_Alberto.Filters
{
    public class AutorizacionUsuariosAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string[] rolesPermitidos;
        public AutorizacionUsuariosAttribute(params string[] roles)
        {
            this.rolesPermitidos = roles;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            string controller = context.RouteData.Values["controller"].ToString();
            string action = context.RouteData.Values["action"].ToString();
            if (user == null)
            {
                ITempDataProvider provider = (ITempDataProvider)context.HttpContext.RequestServices.GetService(typeof(ITempDataProvider));
                var TempData = provider.LoadTempData(context.HttpContext);

                TempData["CONTROLLER"] = controller;
                TempData["ACTION"] = action;
                provider.SaveTempData(context.HttpContext, TempData);
                context.Result = GetRoute("Login", "Manage");
            }
            else
            {
                int contador = 0;
                foreach (var role in rolesPermitidos)
                {
                    if (role == user.FindFirst(ClaimTypes.Role).Value)
                    {
                        contador++;
                    };
                }
                if (contador == 0)
                {
                    context.Result = GetRoute("SinPermisos", "Manage");
                }
            }
        }
        public RedirectToRouteResult GetRoute(string action, string controller)
        {
            RouteValueDictionary route = new RouteValueDictionary(new { controller = controller, action = action });
            return new RedirectToRouteResult(route);
        }
    }
}
