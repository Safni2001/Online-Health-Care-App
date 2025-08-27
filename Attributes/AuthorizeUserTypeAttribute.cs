using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HealthCareApp.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeUserTypeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedUserTypes;

        public AuthorizeUserTypeAttribute(params string[] allowedUserTypes)
        {
            _allowedUserTypes = allowedUserTypes;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userType = context.HttpContext.Session.GetString("UserType");
            var userId = context.HttpContext.Session.GetString("UserID");

            // Check if user is authenticated
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Check if user type is allowed
            if (string.IsNullOrEmpty(userType) || !_allowedUserTypes.Contains(userType))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
