using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HSVHousing.Models;

namespace HSVHousing.Controllers
{
    [Authorize]
    public class ManageController : BaseController
    {
        private HSVHousingDBEntities db = new HSVHousingDBEntities();

        // GET: /Manage/
        public ActionResult Index()
        {
            if (Session["RoleID"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }

            var roleId = Convert.ToInt32(Session["RoleID"]);

            if (roleId == 2) 
            {
                return RedirectToAction("MyListings");
            }
            else if (roleId == 1) 
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("EditProfile");
        }

        // GET: /Manage/EditProfile
        // Hiển thị form cho người dùng chỉnh sửa thông tin cá nhân.
        public ActionResult EditProfile()
        {
            var currentUserId = Convert.ToInt32(Session["UserID"]);
            var user = db.Users.Find(currentUserId);
            if (user == null) return HttpNotFound();

            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber
            };
            return View(model);
        }

        // POST: /Manage/EditProfile
        // Xử lý việc cập nhật thông tin cá nhân.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = Convert.ToInt32(Session["UserID"]);
                var userToUpdate = db.Users.Find(currentUserId);
                if (userToUpdate != null)
                {
                    userToUpdate.FullName = model.FullName;
                    userToUpdate.PhoneNumber = model.PhoneNumber;
                    db.Entry(userToUpdate).State = EntityState.Modified;
                    db.SaveChanges();

                    // Cập nhật lại Session để header hiển thị tên mới ngay lập tức
                    Session["FullName"] = userToUpdate.FullName;

                    TempData["SuccessMessage"] = "Hồ sơ của bạn đã được cập nhật thành công!";
                    return RedirectToAction("Index");
                }
            }
            // Nếu có lỗi validation, hiển thị lại form
            return View(model);
        }

        // =============================================================
        // CÁC CHỨC NĂNG DÀNH RIÊNG CHO CHỦ TRỌ (LANDLORD)
        // =============================================================

        // GET: /Manage/MyListings
        // Hiển thị danh sách các tin đăng của chính chủ trọ đang đăng nhập.
        public ActionResult MyListings()
        {
            if (Convert.ToInt32(Session["RoleID"]) != 2) return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Chỉ Chủ trọ mới có quyền truy cập chức năng này.");

            var currentUserId = Convert.ToInt32(Session["UserID"]);
            var myListings = db.Listings
                               .Where(l => l.UserID == currentUserId)
                               .OrderByDescending(l => l.CreatedAt)
                               .Include(l => l.Category)
                               .ToList();
            return View(myListings);
        }

        // GET: /Manage/CreateListing
        // Hiển thị form để đăng tin mới.
        public ActionResult CreateListing()
        {
            if (Convert.ToInt32(Session["RoleID"]) != 2) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var viewModel = new ListingFormViewModel
            {
                CategoryList = new SelectList(db.Categories, "CategoryID", "CategoryName"),
                WardList = new SelectList(db.Wards, "WardID", "WardName")
            };
            return View(viewModel);
        }

        // POST: /Manage/CreateListing
        // Xử lý việc lưu tin đăng mới vào CSDL.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateListing(ListingFormViewModel viewModel)
        {
            if (Convert.ToInt32(Session["RoleID"]) != 2) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                var newListing = new Listing
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Price = viewModel.Price,
                    Area = (float)viewModel.Area, // Ép kiểu từ double (ViewModel) về float (Model)
                    AddressStreet = viewModel.AddressStreet,
                    WardID = viewModel.WardID,
                    CategoryID = viewModel.CategoryID,
                    UserID = Convert.ToInt32(Session["UserID"]),
                    Status = 0, // 0 = Chờ duyệt
                    CreatedAt = DateTime.Now,
                    ViewCount = 0
                };

                db.Listings.Add(newListing);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đăng tin thành công! Tin của bạn đang chờ phê duyệt.";
                return RedirectToAction("MyListings");
            }

            // Nếu có lỗi, nạp lại dropdown và trả về view để người dùng sửa
            viewModel.CategoryList = new SelectList(db.Categories, "CategoryID", "CategoryName", viewModel.CategoryID);
            viewModel.WardList = new SelectList(db.Wards, "WardID", "WardName", viewModel.WardID);
            return View(viewModel);
        }

        // GET: /Manage/EditListing/5
        // Hiển thị form để sửa một tin đăng đã có.
        public ActionResult EditListing(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Listing listing = db.Listings.Find(id);
            if (listing == null) return HttpNotFound();

            var currentUserId = Convert.ToInt32(Session["UserID"]);
            if (listing.UserID != currentUserId) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var viewModel = new ListingFormViewModel
            {
                ListingID = listing.ListingID,
                Title = listing.Title,
                Description = listing.Description,
                Price = listing.Price,
                Area = (double)listing.Area, // Ép kiểu từ float (Model) sang double (ViewModel)
                AddressStreet = listing.AddressStreet,
                WardID = listing.WardID,
                CategoryID = listing.CategoryID,
                CategoryList = new SelectList(db.Categories, "CategoryID", "CategoryName", listing.CategoryID),
                WardList = new SelectList(db.Wards, "WardID", "WardName", listing.WardID)
            };

            return View(viewModel);
        }

        // POST: /Manage/EditListing/5
        // Xử lý việc cập nhật tin đăng.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditListing(ListingFormViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var listingToUpdate = db.Listings.Find(viewModel.ListingID);
                if (listingToUpdate == null) return HttpNotFound();

                var currentUserId = Convert.ToInt32(Session["UserID"]);
                if (listingToUpdate.UserID != currentUserId) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

                listingToUpdate.Title = viewModel.Title;
                listingToUpdate.Description = viewModel.Description;
                listingToUpdate.Price = viewModel.Price;
                listingToUpdate.Area = (float)viewModel.Area;
                listingToUpdate.AddressStreet = viewModel.AddressStreet;
                listingToUpdate.WardID = viewModel.WardID;
                listingToUpdate.CategoryID = viewModel.CategoryID;
                listingToUpdate.Status = 0; // Gửi duyệt lại sau khi sửa
                listingToUpdate.LastModified = DateTime.Now;

                db.Entry(listingToUpdate).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật thành công! Tin đã được gửi để duyệt lại.";
                return RedirectToAction("MyListings");
            }

            viewModel.CategoryList = new SelectList(db.Categories, "CategoryID", "CategoryName", viewModel.CategoryID);
            viewModel.WardList = new SelectList(db.Wards, "WardID", "WardName", viewModel.WardID);
            return View(viewModel);
        }

        // GET: /Manage/DeleteListing/5
        // Hiển thị trang xác nhận xóa.
        public ActionResult DeleteListing(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Listing listing = db.Listings.Find(id);
            if (listing == null) return HttpNotFound();

            var currentUserId = Convert.ToInt32(Session["UserID"]);
            if (listing.UserID != currentUserId) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(listing);
        }

        // POST: /Manage/DeleteListing/5
        // Thực hiện hành động xóa.
        [HttpPost, ActionName("DeleteListing")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Listing listing = db.Listings.Find(id);

            var currentUserId = Convert.ToInt32(Session["UserID"]);
            if (listing.UserID != currentUserId) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            db.Listings.Remove(listing);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Đã xóa tin đăng thành công!";
            return RedirectToAction("MyListings");
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

