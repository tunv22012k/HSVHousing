using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HSVHousing.Models;
using PagedList;

namespace HSVHousing.Controllers
{
    public class ListingController : BaseController
    {
        private HSVHousingDBEntities db = new HSVHousingDBEntities();

        // GET: /Listing/ or /Listing/Index
        public ActionResult Index(int? districtId, string priceRange, string areaRange, int? categoryId, int page = 1)
        {
            // Bắt đầu với một truy vấn cơ sở, chưa thực thi
            var listingsQuery = db.Listings.AsQueryable();

            // Chỉ lấy các tin đã được duyệt (Status = 1)
            listingsQuery = listingsQuery.Where(l => l.Status == 1)
                                         .Include(l => l.Ward.District)
                                         .Include(l => l.ListingImages)
                                         .Include(l => l.Amenities);

            // --- Áp dụng các bộ lọc ---
            if (districtId.HasValue && districtId > 0)
            {
                listingsQuery = listingsQuery.Where(l => l.Ward.DistrictID == districtId.Value);
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                listingsQuery = listingsQuery.Where(l => l.CategoryID == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "1-2": listingsQuery = listingsQuery.Where(l => l.Price < 2000000); break;
                    case "2-3": listingsQuery = listingsQuery.Where(l => l.Price >= 2000000 && l.Price <= 3000000); break;
                    case "3-5": listingsQuery = listingsQuery.Where(l => l.Price >= 3000000 && l.Price <= 5000000); break;
                    case "5+": listingsQuery = listingsQuery.Where(l => l.Price > 5000000); break;
                }
            }

            if (!string.IsNullOrEmpty(areaRange))
            {
                switch (areaRange)
                {
                    case "15-20": listingsQuery = listingsQuery.Where(l => l.Area >= 15 && l.Area <= 20); break;
                    case "20-25": listingsQuery = listingsQuery.Where(l => l.Area >= 20 && l.Area <= 25); break;
                }
            }

            // Sắp xếp theo ngày đăng mới nhất
            listingsQuery = listingsQuery.OrderByDescending(l => l.CreatedAt);

            // Phân trang
            int pageSize = 9; // 9 tin đăng mỗi trang
            var pagedListings = listingsQuery.ToPagedList(page, pageSize);

            // Gửi danh sách quận/huyện ra View để hiển thị trong bộ lọc
            ViewBag.Districts = new SelectList(db.Districts.OrderBy(d => d.DistrictName), "DistrictID", "DistrictName", districtId);

            // Giữ lại giá trị lọc của người dùng để hiển thị lại trên View
            ViewBag.SelectedDistrict = districtId;
            ViewBag.SelectedPrice = priceRange;
            ViewBag.SelectedArea = areaRange;

            return View(pagedListings);
        }

        // GET: /Listing/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var listing = db.Listings.Include(l => l.User).Include(l => l.Ward.District).Include(l => l.ListingImages).Include(l => l.Amenities).Include(l => l.Reviews.Select(r => r.User)).FirstOrDefault(l => l.ListingID == id);
            if (listing == null || listing.Status != 1)
            {
                return HttpNotFound();
            }

            // Kiểm tra xem người dùng hiện tại đã đánh giá tin này chưa
            if (Session["UserID"] != null)
            {
                var currentUserId = Convert.ToInt32(Session["UserID"]);
                ViewBag.HasUserReviewed = db.Reviews.Any(r => r.ListingID == id && r.UserID == currentUserId);
            }
            else
            {
                ViewBag.HasUserReviewed = false;
            }

            // Tăng lượt xem
            listing.ViewCount++;
            db.Entry(listing).State = EntityState.Modified;
            db.SaveChanges();

            // Tạo một model cho form đánh giá để truyền cho Partial View
            var reviewViewModel = new ReviewViewModel { ListingID = id.Value };
            ViewBag.ReviewViewModel = reviewViewModel;

            return View(listing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult AddReview(ReviewViewModel reviewModel)
        {
            // Nếu model hợp lệ, lưu vào CSDL
            if (ModelState.IsValid)
            {
                var currentUserId = Convert.ToInt32(Session["UserID"]);

                // Kiểm tra lại để chắc chắn người dùng chưa đánh giá
                if (db.Reviews.Any(r => r.ListingID == reviewModel.ListingID && r.UserID == currentUserId))
                {
                    TempData["ReviewError"] = "Bạn đã đánh giá phòng trọ này rồi.";
                    return RedirectToAction("Details", new { id = reviewModel.ListingID });
                }

                var newReview = new Review
                {
                    ListingID = reviewModel.ListingID,
                    UserID = currentUserId,
                    Rating = reviewModel.Rating,
                    Comment = reviewModel.Comment,
                    CreatedAt = DateTime.Now
                };

                db.Reviews.Add(newReview);
                db.SaveChanges();

                TempData["ReviewSuccess"] = "Cảm ơn bạn đã gửi đánh giá!";
                return RedirectToAction("Details", new { id = reviewModel.ListingID });
            }

            // --- XỬ LÝ KHI MODELSTATE KHÔNG HỢP LỆ ---
            // Nếu form không hợp lệ, tải lại toàn bộ dữ liệu của trang Details và hiển thị lại trang
            TempData["ReviewError"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";

            // Tải lại dữ liệu của tin đăng chính
            var listing = db.Listings
                .Include(l => l.User)
                .Include(l => l.Ward.District)
                .Include(l => l.ListingImages)
                .Include(l => l.Amenities)
                .Include(l => l.Reviews.Select(r => r.User))
                .FirstOrDefault(l => l.ListingID == reviewModel.ListingID);

            if (listing == null)
            {
                return HttpNotFound();
            }

            // Gửi lại ReviewViewModel với các lỗi validation
            ViewBag.ReviewViewModel = reviewModel;
            ViewBag.HasUserReviewed = false; // Vẫn cho phép hiển thị form

            // Trả về View "Details" với model là "listing", không phải "reviewModel"
            return View("Details", listing);
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