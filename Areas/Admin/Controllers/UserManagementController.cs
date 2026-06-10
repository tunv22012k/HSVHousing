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
    public class UserManagementController : Controller
    {
        private HSVHousingDBEntities db = new HSVHousingDBEntities();

        // GET: Admin/UserManagement
        public ActionResult Index()
        {
            if (Session["RoleID"] == null || Convert.ToInt32(Session["RoleID"]) != 1)
            {
                return RedirectToAction("Logout", "Account", new { area = "" });
            }

            var users = db.Users.Include(u => u.Role).OrderByDescending(u => u.CreatedAt).ToList();
            return View(users);
        }

        // POST: Admin/UserManagement/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            if (Session["RoleID"] == null || Convert.ToInt32(Session["RoleID"]) != 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            // Prevent admin from locking themselves
            if (id == Convert.ToInt32(Session["UserID"]))
            {
                TempData["ErrorMessage"] = "Bạn không thể khóa tài khoản của chính mình.";
                return RedirectToAction("Index");
            }

            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.IsActive = !user.IsActive;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            TempData["SuccessMessage"] = user.IsActive ? $"Đã mở khóa tài khoản {user.Email}." : $"Đã khóa tài khoản {user.Email}.";
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