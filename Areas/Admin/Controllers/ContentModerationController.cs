using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HSVHousing.Models;

namespace HSVHousing.Areas.Admin.Controllers
{
    [Authorize]
    public class ContentModerationController : Controller
    {
        private HSVHousingDBEntities db = new HSVHousingDBEntities();

        // GET: Admin/ContentModeration
        public ActionResult Index(int? status)
        {
            if (Session["RoleID"] == null || Convert.ToInt32(Session["RoleID"]) != 1)
            {
                return RedirectToAction("Logout", "Account", new { area = "" });
            }

            // Default to Pending (0) if no status specified
            int filterStatus = status ?? 0;
            ViewBag.CurrentStatus = filterStatus;

            var listings = db.Listings.Include(l => l.Category).Include(l => l.User).Include(l => l.Ward)
                                      .Where(l => l.Status == filterStatus)
                                      .OrderByDescending(l => l.CreatedAt).ToList();

            return View(listings);
        }

        // GET: Admin/ContentModeration/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["RoleID"] == null || Convert.ToInt32(Session["RoleID"]) != 1) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Listing listing = db.Listings.Include(l => l.Category).Include(l => l.User).Include(l => l.Ward)
                                         .Include(l => l.ListingImages).Include(l => l.Amenities)
                                         .SingleOrDefault(l => l.ListingID == id);
            
            if (listing == null) return HttpNotFound();

            return View(listing);
        }

        // POST: Admin/ContentModeration/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            if (Session["RoleID"] == null || Convert.ToInt32(Session["RoleID"]) != 1) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var listing = db.Listings.Find(id);
            if (listing != null)
            {
                listing.Status = 1; // Approved
                db.Entry(listing).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã duyệt bài đăng thành công.";
            }

            return RedirectToAction("Index");
        }

        // POST: Admin/ContentModeration/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(int id)
        {
            if (Session["RoleID"] == null || Convert.ToInt32(Session["RoleID"]) != 1) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var listing = db.Listings.Find(id);
            if (listing != null)
            {
                listing.Status = 2; // Rejected/Hidden
                db.Entry(listing).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã từ chối bài đăng.";
            }

            return RedirectToAction("Index");
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