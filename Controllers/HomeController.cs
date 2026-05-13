using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using HSVHousing.Models;
using System.Collections.Generic;

namespace HSVHousing.Controllers
{
    public class HomeController : BaseController
    {
        private HSVHousingDBEntities db = new HSVHousingDBEntities();

        public ActionResult Index()
        {
            // Lấy 3 bài viết blog mới nhất đã đăng
            var newestBlogPosts = db.BlogPosts
                .Where(b => b.Status == 1)
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .ToList();

            // Lấy 6 tin đăng mới nhất đã được duyệt
            var newestListings = db.Listings
                .Where(l => l.Status == 1)
                .OrderByDescending(l => l.CreatedAt)
                .Take(6)
                .Include(l => l.Ward.District)
                .Include(l => l.ListingImages)
                .Include(l => l.Amenities)
                .ToList();

            // Lấy danh sách các loại nhà trọ
            var roomCategories = db.Categories.ToList();

            // Đếm số lượng tin đăng cho mỗi loại
            var categoryCounts = db.Listings
                .Where(l => l.Status == 1)
                .GroupBy(l => l.Category.CategoryName)
                .Select(g => new { CategoryName = g.Key, Count = g.Count() })
                .ToDictionary(x => x.CategoryName, x => x.Count);

            // Tạo ViewModel
            var viewModel = new HomeViewModel
            {
                RoomCategories = roomCategories,
                NewestListings = newestListings,
                NewestBlogPosts = newestBlogPosts,
                CategoryCounts = categoryCounts
            };

            // Truyền danh sách TẤT CẢ Quận/Huyện ra ViewBag cho lần tải đầu tiên
            ViewBag.Districts = new SelectList(db.Districts.OrderBy(c => c.DistrictName), "DistrictID", "DistrictName");

            return View(viewModel);
        }

        // --- ACTION ĐỂ LẤY PHƯỜNG/XÃ BẰNG AJAX ---
        [HttpGet]
        public JsonResult GetWardsByDistrict(int districtId)
        {
            var wards = db.Wards
                          .Where(w => w.DistrictID == districtId)
                          .OrderBy(w => w.WardName)
                          .Select(w => new { Value = w.WardID, Text = w.WardName })
                          .ToList();
            return Json(wards, JsonRequestBehavior.AllowGet);
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