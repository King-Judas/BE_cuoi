using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using BE_cuoi.Models;

namespace BE_cuoi.Controllers
{
    public class AdminController : Controller
    {
        private KINGgameStoreDBEntities db = new KINGgameStoreDBEntities();

        // Kiểm tra quyền Admin cơ bản qua Session
        private bool CheckAdmin()
        {
            if (Session["UserID"] == null) return false;
            return true;
        }

        // ================= 1. QUẢN LÝ SẢN PHẨM (GAME) =================

        // GET: Admin/Products (Danh sách game)
        public ActionResult Products()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            var products = db.Products.OrderByDescending(p => p.ProductID).ToList();
            return View(products);
        }

        // [GET] Admin/CreateProduct
        public ActionResult CreateProduct()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        // [POST] Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProduct(Product product)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Thêm tựa game mới thành công!";
                return RedirectToAction("Products");
            }
            return View(product);
        }

        // [GET] Admin/EditProduct/{id}
        public ActionResult EditProduct(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // [POST] Admin/EditProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(Product product)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật thông tin game thành công!";
                return RedirectToAction("Products");
            }
            return View(product);
        }

        // Xóa game
        public ActionResult DeleteProduct(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            var product = db.Products.Find(id);
            if (product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["Success"] = "Đã xóa tựa game khỏi hệ thống!";
            }
            return RedirectToAction("Products");
        }

        // ================= 2. QUẢN LÝ ĐƠN HÀNG =================

        // GET: Admin/Orders (Danh sách đơn hàng)
        public ActionResult Orders()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            var orders = db.Orders.Include(o => o.User).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        // [GET] Admin/OrderDetails/{id} (Chi tiết đơn hàng)
        public ActionResult OrderDetails(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // Cập nhật trạng thái đơn hàng
        [HttpPost]
        public ActionResult UpdateOrderStatus(int orderId, string orderStatus, string paymentStatus)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            var order = db.Orders.Find(orderId);
            if (order != null)
            {
                order.OrderStatus = orderStatus;
                order.PaymentStatus = paymentStatus;
                db.SaveChanges();
                TempData["Success"] = $"Đã cập nhật trạng thái đơn hàng #{orderId} thành công!";
            }
            return RedirectToAction("Orders");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}