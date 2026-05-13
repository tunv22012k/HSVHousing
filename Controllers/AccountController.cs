using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using HSVHousing.Models;
using System.IO;

namespace HSVHousing.Controllers
{
    public class AccountController : BaseController
    {
        private HSVHousingDBEntities db = new HSVHousingDBEntities();

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            ViewBag.ReturnUrl = returnUrl;
            var authViewModel = new AuthViewModel
            {
                Login = new LoginViewModel(),
                Register = new RegisterViewModel()
            };
            return View(authViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AuthViewModel model, string returnUrl)
        {
            if (!ModelState.IsValidField("Login.Email") || !ModelState.IsValidField("Login.Password"))
            {
                model.Register = new RegisterViewModel();
                return View(model);
            }

            var user = db.Users.FirstOrDefault(u => u.Email == model.Login.Email);

            if (user != null && BCrypt.Net.BCrypt.Verify(model.Login.Password, user.PasswordHash))
            {
                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                }
                else
                {
                    Session.Clear();
                    FormsAuthentication.SetAuthCookie(user.Email, false);
                    Session["UserID"] = user.UserID;
                    Session["FullName"] = user.FullName;
                    Session["RoleID"] = user.RoleID;

                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Manage");
                }
            }
            else
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            }

            model.Register = new RegisterViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(AuthViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(u => u.Email == model.Register.Email))
                {
                    ModelState.AddModelError("", "Email này đã được sử dụng.");
                }
                else
                {
                    string avatarPath = null;
                    if (model.Register.AvatarFile != null && model.Register.AvatarFile.ContentLength > 0)
                    {
                        string relativePath = "~/Uploads/Avatars/";
                        string physicalPath = Server.MapPath(relativePath);

                        if (!Directory.Exists(physicalPath))
                        {
                            Directory.CreateDirectory(physicalPath);
                        }

                        string fileName = Path.GetFileName(model.Register.AvatarFile.FileName);
                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
                        string savedFilePath = Path.Combine(physicalPath, uniqueFileName);

                        model.Register.AvatarFile.SaveAs(savedFilePath);
                        avatarPath = relativePath + uniqueFileName;
                    }

                    var user = new User
                    {
                        FullName = model.Register.FullName,
                        Email = model.Register.Email,
                        PhoneNumber = model.Register.PhoneNumber,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Register.Password),
                        RoleID = model.Register.RoleID,
                        AvatarPath = avatarPath,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
            }

            ViewBag.ActiveTab = "Register";
            model.Login = new LoginViewModel();
            return View("Login", model);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            Session.Abandon();

            Session.Clear();

            Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));

            return RedirectToAction("Index", "Home");
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