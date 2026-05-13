using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HSVHousing.Models;
using PagedList;

namespace HSVHousing.Controllers
{
    public class RoommateController : BaseController
    {
        private HSVHousingDBEntities db = new HSVHousingDBEntities();

        // GET: /Roommate/
        public ActionResult Index(int page = 1)
        {
            var roommatePosts = db.RoommatePosts
                                  .Where(p => p.Status == 1)
                                  .OrderByDescending(p => p.CreatedAt)
                                  .Include(p => p.User)
                                  .Include(p => p.RoommatePostComments.Select(c => c.User));

            int pageSize = 5;
            var pagedPosts = roommatePosts.ToPagedList(page, pageSize);

            if (Session["UserID"] != null)
            {
                ViewBag.CurrentUserAvatar = db.Users.Find(Convert.ToInt32(Session["UserID"]))?.AvatarPath;
            }

            return View(pagedPosts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult AddComment(int postId, string content, int? parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được để trống.";
                return RedirectToAction("Index");
            }

            var newComment = new RoommatePostComment
            {
                PostID = postId,
                UserID = Convert.ToInt32(Session["UserID"]),
                Content = content,
                ParentCommentID = parentCommentId,
                CreatedAt = DateTime.Now
            };

            db.RoommatePostComments.Add(newComment);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: /Roommate/Create
        [Authorize]
        public ActionResult Create()
        {
            if (Convert.ToInt32(Session["RoleID"]) != 3) // Giả sử RoleID=3 là Student
            {
                TempData["ErrorMessage"] = "Chỉ sinh viên mới có thể đăng tin tìm bạn ở ghép.";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Roommate/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "Title,Content,GenderPreference,DesiredArea,ContactInfo")] RoommatePost roommatePost)
        {
            if (Convert.ToInt32(Session["RoleID"]) != 3) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                roommatePost.UserID = Convert.ToInt32(Session["UserID"]);
                roommatePost.Status = 0; // Chờ duyệt
                roommatePost.CreatedAt = DateTime.Now;

                db.RoommatePosts.Add(roommatePost);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đăng tin thành công! Tin của bạn đang chờ quản trị viên phê duyệt.";
                return RedirectToAction("Index");
            }

            return View(roommatePost);
        }

        // --- BỔ SUNG ACTION EDIT ---

        // GET: /Roommate/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            RoommatePost post = db.RoommatePosts.Find(id);
            if (post == null) return HttpNotFound();

            // Kiểm tra quyền sở hữu
            var currentUserId = Convert.ToInt32(Session["UserID"]);
            if (post.UserID != currentUserId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Bạn không có quyền sửa tin này.");
            }

            return View(post);
        }

        // POST: /Roommate/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "PostID,Title,Content,GenderPreference,DesiredArea,ContactInfo")] RoommatePost post)
        {
            var postToUpdate = db.RoommatePosts.Find(post.PostID);
            if (postToUpdate == null) return HttpNotFound();

            var currentUserId = Convert.ToInt32(Session["UserID"]);
            if (postToUpdate.UserID != currentUserId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (ModelState.IsValid)
            {
                postToUpdate.Title = post.Title;
                postToUpdate.Content = post.Content;
                postToUpdate.GenderPreference = post.GenderPreference;
                postToUpdate.DesiredArea = post.DesiredArea;
                postToUpdate.ContactInfo = post.ContactInfo;
                postToUpdate.Status = 0; // Gửi duyệt lại
                db.Entry(postToUpdate).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật tin thành công! Tin đã được gửi để duyệt lại.";
                return RedirectToAction("Index");
            }
            return View(post);
        }


        // --- BỔ SUNG ACTION DELETE ---

        // GET: /Roommate/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            RoommatePost post = db.RoommatePosts.Find(id);
            if (post == null) return HttpNotFound();

            var currentUserId = Convert.ToInt32(Session["UserID"]);
            if (post.UserID != currentUserId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return View(post);
        }

        // POST: /Roommate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            RoommatePost post = db.RoommatePosts.Find(id);
            var currentUserId = Convert.ToInt32(Session["UserID"]);
            if (post.UserID != currentUserId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            db.RoommatePosts.Remove(post);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Đã xóa tin thành công.";
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