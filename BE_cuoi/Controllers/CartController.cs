using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using BE_cuoi.Models;

namespace BE_cuoi.Controllers
{
    public class CartController : Controller
    {
        private KINGgameStoreDBEntities db = new KINGgameStoreDBEntities();

        // GET: Cart (Xem giỏ hàng)
        public ActionResult Index()
        {
            // Kiểm tra đăng nhập (Lấy UserID từ Session)
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            var cartItems = db.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserID == userId)
                .ToList();

            return View(cartItems);
        }

        // POST: Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            // Kiểm tra đăng nhập
            if (Session["UserID"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để mua game!";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // Kiểm tra xem sản phẩm có tồn tại trong cơ sở dữ liệu không
            var product = db.Products.Find(productId);
            if (product == null)
            {
                TempData["Error"] = "Tựa game không tồn tại!";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng của user này chưa
            var existingItem = db.CartItems.FirstOrDefault(c => c.UserID == userId && c.ProductID == productId);

            if (existingItem != null)
            {
                // Nếu có rồi thì tăng số lượng lên
                existingItem.Quantity += quantity;
            }
            else
            {
                // Nếu chưa có thì tạo mới CartItem
                CartItem newItem = new CartItem()
                {
                    UserID = userId,
                    ProductID = productId,
                    Quantity = quantity > 0 ? quantity : 1,
                    AddedAt = DateTime.Now
                };
                db.CartItems.Add(newItem);
            }

            db.SaveChanges();
            TempData["Success"] = "Đã thêm tựa game vào giỏ hàng thành công!";
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public ActionResult RemoveFromCart(int id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            var cartItem = db.CartItems.FirstOrDefault(c => c.CartItemID == id && c.UserID == userId);

            if (cartItem != null)
            {
                db.CartItems.Remove(cartItem);
                db.SaveChanges();
                TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}