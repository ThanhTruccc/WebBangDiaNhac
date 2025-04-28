using WebBangDiaNhac.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace WebBangDiaNhac.Controllers
{
    public class GioHangController : Controller
    {
        BangDiaNhacEntities db = new BangDiaNhacEntities();
        // GET: GioHang
        public List<GioHang> LayGioHang()
        {
            List<GioHang> lstGioHang = Session["GIOHANG"] as List<GioHang>;
            if (lstGioHang == null)
            {
                //Nếu giỏ hàng chưa tồn tại thì mình tiến hành khởi tao list giỏ hàng (sessionGioHang)
                lstGioHang = new List<GioHang>();
                Session["GIOHANG"] = lstGioHang;
            }
            return lstGioHang;
        }
        // GET: GioHang
       
        public ActionResult ThemGioHang(int idSanPham, string strURL)
        {
            SanPham m = db.SanPhams.SingleOrDefault(r => r.idSanPham == idSanPham);
            if (m == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            List<GioHang> lstGioHang = LayGioHang();
            //Kiểm tra sp này đã tồn tại trong session[giohang] chưa
            GioHang sanpham = lstGioHang.Find(n => n.idSanPham == idSanPham);
            if (sanpham == null)
            {
                sanpham = new GioHang(idSanPham);
                //Add sản phẩm mới thêm vào list
                lstGioHang.Add(sanpham);
                return Redirect(strURL);
            }
            else
            {
                sanpham.soLuong++;
                return Redirect(strURL);
            }
        }
        public ActionResult CapNhatGioHang(int Id, FormCollection f)
        {
            //Kiểm tra masp
            SanPham sp = db.SanPhams.SingleOrDefault(n => n.idSanPham == Id);
            //Nếu get sai masp thì sẽ trả về trang lỗi 404
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            //Lấy giỏ hàng ra từ session
            List<GioHang> lstGioHang = LayGioHang();
            //Kiểm tra sp có tồn tại trong session["GioHang"]
            GioHang sanpham = lstGioHang.SingleOrDefault(n => n.idSanPham == Id);
            //Nếu mà tồn tại thì chúng ta cho sửa số lượng
            if (sanpham != null)
            {
                sanpham.soLuong = (short?)int.Parse(f["txtSoLuong"].ToString());

            }
            return RedirectToAction("GioHang");
        }
        public ActionResult XoaGioHang(int Id)
        {
            //Kiểm tra masp
            SanPham sp = db.SanPhams.SingleOrDefault(n => n.idSanPham == Id);
            //Nếu get sai masp thì sẽ trả về trang lỗi 404
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            //Lấy giỏ hàng ra từ session
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sanpham = lstGioHang.SingleOrDefault(n => n.idSanPham == Id);
            //Nếu mà tồn tại thì chúng ta cho sửa số lượng
            if (sanpham != null)
            {
                lstGioHang.RemoveAll(n => n.idSanPham == Id);

            }
            if (lstGioHang.Count == 0)
            {
                return RedirectToAction("GioHang", "GioHang");
            }
            return RedirectToAction("GioHang");
        }
        public ActionResult GioHang()
        {

            //if (Session["GIOHANG"] == null)
            //{
            //    return RedirectToAction("Index", "DatHang");
            //}
            List<GioHang> lstGioHang = LayGioHang();
            return View(lstGioHang);
        }
        public ActionResult DonHang()
        {
            KhachHang nd = (KhachHang)Session["user"];
            if ((KhachHang)Session["user"] == null)
            {
                return RedirectToAction("DangNhap", "User");
            }
            List<GioHang> lstGioHang = LayGioHang();
            return View(lstGioHang);
        }
        private int TongSoLuong()
        {
            int iTongSoLuong = 0;
            List<GioHang> lstGioHang = Session["GIOHANG"] as List<GioHang>;
            if (lstGioHang != null)
            {
                iTongSoLuong = (int)lstGioHang.Sum(n => n.soLuong);
            }
            return iTongSoLuong;
        }
        //Tính tổng thành tiền
        private double TongTien()
        {
            double dTongTien = 0;
            List<GioHang> lstGioHang = Session["GIOHANG"] as List<GioHang>;
            if (lstGioHang != null)
            {
                dTongTien = (double)lstGioHang.Sum(n => n.thanhTien);
            }
            return dTongTien;
        }
        //tạo partial giỏ hàng

        public ActionResult GioHangPartial()
        {
            try
            {
                ViewBag.TongSoLuong = TongSoLuong();
                ViewBag.TongTien = TongTien();
                return PartialView();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi tại GioHangPartial", ex);
            }
        }

        //Xây dựng 1 view cho người dùng chỉnh sửa giỏ hàng

        public ActionResult SuaGioHang()
        {
            if (Session["GIOHANG"] == null)
            {
                return RedirectToAction("Index", "DatHang");
            }
            List<GioHang> lstGioHang = LayGioHang();
            return View(lstGioHang);

        }


        [HttpPost]
        public ActionResult DatHang(string tennguoinhan, string diachi, string phone, string payment, DateTime ngaygiao)
        {
            // Get current user's information from session
            KhachHang nd = (KhachHang)Session["user"];
            if (nd == null)
            {
                // If the user is not logged in, redirect to the login page
                return RedirectToAction("DangNhap", "User");
            }
            List<GioHang> lstGioHang = Session["GIOHANG"] as List<GioHang>;
            if (lstGioHang == null || !lstGioHang.Any())
            {
                ModelState.AddModelError("", "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi đặt hàng.");
                return RedirectToAction("GioHang");
            }
            decimal totalValue = lstGioHang.Sum(item => item.donGia* item.soLuong).GetValueOrDefault();
            // Create an order object
            DonHang order = new DonHang
            {
                ngayDatHang = DateTime.Now,
                idKhachHang = nd.idKhachHang,  
                tenNguoiNhan = tennguoinhan,
                diaChiGiaoHang = diachi,
                soDienThoaiNguoiNhan = phone,
                trangThaiDon = "2",
                triGiaDon = totalValue,
                hinhThucThanhToan = payment == "cod" ? 2 : 3,
            };


            // Add order to the database
            db.DonHangs.Add(order);
            db.SaveChanges(); 

            foreach (var item in lstGioHang)
            {
                // Ensure valid item exists in the database
                SanPham sp = db.SanPhams.SingleOrDefault(m => m.idSanPham == item.idSanPham);
               

                if (sp == null)
                {
                    ModelState.AddModelError("", $"Id {item.idSanPham} khong tồn tại..");
                    return View(lstGioHang); // Return with error message if product is invalid
                }

                
                
                // Create and add order details
                ChiTietDonHang__ orderDetail = new ChiTietDonHang__
                {
                    idDonHang = order.idDonHang,  // Link order with MaDon
                    idSanPham = item.idSanPham,
                    soLuong = item.soLuong,
                    donGia = item.donGia
                };

                db.ChiTietDonHang__.Add(orderDetail);
            }

            // Save order details
            db.SaveChanges();

            // Clear the cart (session)
            Session["GIOHANG"] = null;

            // Redirect to the order confirmation page
            return RedirectToAction("XacNhanDatHang", "Giohang");
        }




        // Xac Nhan Dat Hang (Order Confirmation)
        public ActionResult XacNhanDatHang()
        {
            ViewBag.Message = "Đặt hàng thành công!";
            return View();
        }

        public ActionResult FailureView()
        {
            return View();
        }
        public ActionResult SuccessView()
        {
            return View();
        }
    }
}