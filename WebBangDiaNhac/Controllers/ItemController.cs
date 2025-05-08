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
        public ActionResult Index(string searchString, int page = 1, int pageSize = 12)
        {
            var query = db.SanPhams.ToList(); 

            if (!string.IsNullOrEmpty(searchString))
            {
                var keyword = RemoveDiacritics(searchString.ToLower());
                query = query.Where(sp =>
                    !string.IsNullOrEmpty(sp.tenSanPham) &&
                    RemoveDiacritics(sp.tenSanPham.ToLower()).Contains(keyword)
                ).ToList();
            }


            var totalRecords = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.SearchString = searchString;

            var SPList = query
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            return View(SPList);
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
    }
}