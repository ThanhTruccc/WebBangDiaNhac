using WebBangDiaNhac.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace BangDiaNhac.Controllers
{
    public class UserController : Controller
    {
        KhachHang _kh;
        BangDiaNhacEntities db = new BangDiaNhacEntities();

        // GET: User
        public ActionResult Index()
        {
            return View();
        }
        //dang ky
        public ActionResult DangKy()
        {
            _kh = (KhachHang)Session["user"];
            if (_kh != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(KhachHang khh)
        {
            if (Session["user"] != null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(khh);
            }

            KhachHang newUser = new KhachHang
            {
                hoTen = khh.hoTen,
                soDienThoai = khh.soDienThoai,
                diaChi = khh.diaChi,
                email = khh.email,
                matKhau = khh.matKhau,
                idPhanQuyen = 1
            };

            try
            {
                db.KhachHangs.Add(newUser);
                db.SaveChanges();
                Session["user"] = newUser; // Đăng nhập tự động sau khi đăng ký thành công
                return RedirectToAction("Index", "Home");
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }

            return View(khh);
        }


        //dang nhap
        public ActionResult DangNhap()
        {
            _kh = (KhachHang)Session["user"];
            if (_kh != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(Login lg)
        {
            _kh = (KhachHang)Session["user"];

            if (_kh != null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                _kh = db.KhachHangs.SingleOrDefault(n => n.email == lg.email && n.matKhau == lg.matKhau);

                if (_kh != null)
                {
                    Session.Add("user", _kh);
                    if (_kh.idPhanQuyen == 2)
                    {
                        return RedirectToAction("Index", "SanPhams", new { area = "Admin" });
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                ViewBag.Fail = "Đăng nhập thất bại";
                return View("DangNhap");
            }
            return View(lg);

        }
        public ActionResult DangXuat()
        {
            Session["user"] = null;
            return RedirectToAction("Index", "Home");

        }
    }
}