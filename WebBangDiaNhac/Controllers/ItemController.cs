using WebBangDiaNhac.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Data.Entity.Infrastructure;
using System.Reflection;
using System.Web.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;

namespace WebBangDiaNhac.Controllers
{
    public class ItemController : Controller
    {
        private BangDiaNhacEntities db = new BangDiaNhacEntities();

        private List<SanPham> ShowSP(int count)
        {
            return db.SanPhams.OrderByDescending(a => a.tenSanPham).Take(count).ToList();
        }

        // Hàm bỏ dấu tiếng Việt
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Normalize(NormalizationForm.FormD);
            Regex regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
            return regex.Replace(text, "").Replace('đ', 'd').Replace('Đ', 'D');
        }

        // GET: Item
        public ActionResult Index(string sortOrder = "newest", string searchString = "", int page = 1, int pageSize = 12)
        {
            var allowedSortOrders = new[] { "newest", "price_asc", "price_desc", "name_asc", "name_desc" };
            if (!allowedSortOrders.Contains(sortOrder)) sortOrder = "newest";

            // B1: Lấy toàn bộ dữ liệu từ DB trước
            // B1: Lấy toàn bộ sản phẩm
            var allProducts = db.SanPhams.ToList(); // DỮ LIỆU đã vào bộ nhớ → LINQ to Objects

            // B2: Tìm kiếm (bỏ dấu tiếng Việt)
            if (!string.IsNullOrEmpty(searchString))
            {
                string keyword = RemoveDiacritics(searchString.ToLower());

                allProducts = allProducts
                    .Where(sp => !string.IsNullOrEmpty(sp.tenSanPham) &&
                                 RemoveDiacritics(sp.tenSanPham.ToLower()).Contains(keyword))
                    .ToList(); // lọc xong → lưu lại
            }

            // B3: Sắp xếp
            switch (sortOrder)
            {
                case "price_asc":
                    allProducts = allProducts.OrderBy(sp => sp.donGia).ToList();
                    break;
                case "price_desc":
                    allProducts = allProducts.OrderByDescending(sp => sp.donGia).ToList();
                    break;
                case "name_asc":
                    allProducts = allProducts.OrderBy(sp => sp.tenSanPham).ToList();
                    break;
                case "name_desc":
                    allProducts = allProducts.OrderByDescending(sp => sp.tenSanPham).ToList();
                    break;
                default: // newest
                    allProducts = allProducts.OrderByDescending(sp => sp.idSanPham).ToList();
                    break;
            }

            // B4: Phân trang
            int totalItems = allProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pageItems = allProducts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // B5: Truyền dữ liệu sang View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SearchString = searchString;

            return View(pageItems);

        }


            [OutputCache(Duration = 120, VaryByParam = "id")] // Tăng thời gian cache để phục vụ tốt hơn
        public ActionResult Chitiet(string id)
        {
            if (!int.TryParse(id, out int idSanPham))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Dùng FirstOrDefault + AsNoTracking để giảm chi phí truy vấn EF
            var ctsp = db.SanPhams
                         .AsNoTracking() // không theo dõi entity => nhanh hơn
                         .FirstOrDefault(s => s.idSanPham == idSanPham);

            if (ctsp == null)
            {
                return HttpNotFound();
            }

            return View(ctsp);
        }

        [OutputCache(Duration = 120, VaryByParam = "none")]
        public ActionResult DanhMuc()
        {
            var dm = from d in db.SanPhams
                     select d;
            return PartialView(dm);
        }
        [OutputCache(Duration = 60, VaryByParam = "id")]
        public ActionResult SXDanhMuc(string id)
        {
            int idSanPham = int.Parse(id);
            var sx = from d in db.SanPhams
                      where d.idSanPham == idSanPham
                      select d;
            return PartialView(sx);
        }

        


        public ActionResult FlashSale()
        {
            var allAlbums = db.SanPhams.ToList(); // hoặc lấy theo điều kiện tùy bạn
            var random = new Random();
            var randomAlbums = allAlbums.OrderBy(x => random.Next()).Take(4).ToList();

            return View(randomAlbums);
        }


    }
}