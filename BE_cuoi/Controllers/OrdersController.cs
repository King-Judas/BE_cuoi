using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using BE_cuoi.Models;

namespace BE_cuoi.Controllers
{
    public class OrdersController : Controller
    {
        private KINGgameStoreDBEntities db = new KINGgameStoreDBEntities();

        // [GET] Orders/Checkout (Hiển thị trang thanh toán)
        public ActionResult Checkout()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = Convert.ToInt32(Session["UserID"]);

            // Lấy danh sách sản phẩm trong giỏ hàng của user
            var cartItems = db.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserID == userId)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            return View(cartItems);
        }

        // [POST] Xử lý lưu đơn hàng vào bảng Order và OrderDetail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessCheckout(string shippingAddress, string paymentMethod, string shippingMethod)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = Convert.ToInt32(Session["UserID"]);

            var cartItems = db.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserID == userId)
                .ToList();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // Tính tổng tiền đơn hàng
            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);

            // 1. Tạo bản ghi Order mới
            Order newOrder = new Order()
            {
                UserID = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                PaymentMethod = string.IsNullOrEmpty(paymentMethod) ? "Thanh toán tiền mặt" : paymentMethod,
                PaymentStatus = "Chưa thanh toán",
                ShippingMethod = string.IsNullOrEmpty(shippingMethod) ? "Digital Key / Giao tiêu chuẩn" : shippingMethod,
                ShippingAddress = string.IsNullOrEmpty(shippingAddress) ? "Nhận qua Email tài khoản" : shippingAddress,
                OrderStatus = "Chờ thanh toán"
            };

            db.Orders.Add(newOrder);
            db.SaveChanges(); // Lưu để lấy OrderID tự sinh

            // 2. Tạo các bản ghi OrderDetail tương ứng cho từng game trong giỏ hàng
            foreach (var cartItem in cartItems)
            {
                OrderDetail detail = new OrderDetail()
                {
                    OrderID = newOrder.OrderID,
                    ProductID = cartItem.ProductID,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product.Price,
                    SubTotal = cartItem.Quantity * cartItem.Product.Price
                };
                db.OrderDetails.Add(detail);

                // Trừ số lượng tồn kho (StockQuantity) của sản phẩm game
                if (cartItem.Product.StockQuantity >= cartItem.Quantity)
                {
                    cartItem.Product.StockQuantity -= cartItem.Quantity;
                }
                else
                {
                    cartItem.Product.StockQuantity = 0;
                }
            }

            // 3. Xóa sạch giỏ hàng của user sau khi đặt hàng thành công
            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đặt hàng thành công! Mã đơn hàng của bạn là #" + newOrder.OrderID;

            // Điều hướng sang trang Lịch sử mua hàng (MyOrder)
            return RedirectToAction("MyOrder", new { statusTab = "Chờ thanh toán" });
        }

        // [GET] Orders/MyOrder (Xem lịch sử đơn hàng của user)
        public ActionResult MyOrder(string statusTab)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = Convert.ToInt32(Session["UserID"]);

            var query = db.Orders
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .Where(o => o.UserID == userId);

            // Lọc theo tab trạng thái nếu có
            if (!string.IsNullOrEmpty(statusTab))
            {
                query = query.Where(o => o.OrderStatus == statusTab);
            }

            var orders = query.OrderByDescending(o => o.OrderDate).ToList();
            ViewBag.CurrentTab = statusTab;

            return View(orders);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}