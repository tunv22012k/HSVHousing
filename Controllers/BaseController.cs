using System;
using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Security;
using HSVHousing.Models;

namespace HSVHousing.Controllers
{
    public class BaseController : Controller
    {
        private HSVHousingDBEntities db = new HSVHousingDBEntities();
        protected override void OnAuthentication(AuthenticationContext filterContext)
        {
            base.OnAuthentication(filterContext);

            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
                if (user != null)
                {
                    var roleName = db.Roles.Find(user.RoleID)?.RoleName;
                    if (!string.IsNullOrEmpty(roleName))
                    {
                        var genericPrincipal = new GenericPrincipal(new GenericIdentity(user.Email), new[] { roleName });
                        filterContext.Principal = genericPrincipal;
                    }
                }
            }
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();

            ViewBag.GlobalCategories = categories;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}