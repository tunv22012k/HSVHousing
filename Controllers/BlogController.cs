using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HSVHousing.Models;
using PagedList;

namespace HSVHousing.Controllers
{
    public class BlogController : Controller
    {
        private HSVHousingDBEntities db = new HSVHousingDBEntities();

        // GET: /Blog/ or /Blog/Index
        public ActionResult Index(int page = 1)
        {
            var blogPosts = db.BlogPosts
                              .Where(b => b.Status == 1) // Chỉ lấy bài đã đăng
                              .OrderByDescending(b => b.CreatedAt)
                              .Include(b => b.User); // Lấy thông tin tác giả

            int pageSize = 6; // 6 bài viết mỗi trang
            var pagedBlogPosts = blogPosts.ToPagedList(page, pageSize);

            return View(pagedBlogPosts);
        }

        // GET: /Blog/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var blogPost = db.BlogPosts
                             .Include(b => b.User)
                             .FirstOrDefault(b => b.PostID == id);

            if (blogPost == null || blogPost.Status != 1)
            {
                return HttpNotFound();
            }

            return View(blogPost);
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