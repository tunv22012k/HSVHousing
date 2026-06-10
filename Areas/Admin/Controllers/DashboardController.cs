using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HSVHousing.Models;

namespace HSVHousing.Areas.Admin.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private HSVHousingDBEntities db = new HSVHousingDBEntities();

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            if (Session["RoleID"] == null || Convert.ToInt32(Session["RoleID"]) != 1)
            {
                return RedirectToAction("Logout", "Account", new { area = "" });
            }

            ViewBag.TotalUsers = db.Users.Count(u => u.RoleID == 3);
            ViewBag.TotalLandlords = db.Users.Count(u => u.RoleID == 2);
            ViewBag.ActiveListings = db.Listings.Count(l => l.Status == 1);
            ViewBag.PendingListings = db.Listings.Count(l => l.Status == 0);
            ViewBag.TotalViews = db.Listings.Sum(l => (int?)l.ViewCount) ?? 0;

            return View();
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