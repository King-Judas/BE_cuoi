using System.Linq;
using System.Web.Mvc;
using BE_cuoi.Models; // Sử dụng đúng namespace của project

namespace BE_cuoi.Controllers
{
    public class HomeController : Controller
    {
        // Khởi tạo DbContext trùng khớp với file KINGStore.Context.cs của bạn
        private KINGgameStoreDBEntities db = new KINGgameStoreDBEntities();

        // GET: Home/Index (Trang chủ danh sách game)
        public ActionResult Index()
        {
            var games = db.Products.OrderByDescending(p => p.ProductID).Take(12).ToList();
            return View(games);
        }

        // GET: Home/Details/{id} (Trang chi tiết game)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            // Tìm sản phẩm theo ProductID trong database
            var game = db.Products.Find(id);
            if (game == null)
            {
                return HttpNotFound();
            }

            return View(game);
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