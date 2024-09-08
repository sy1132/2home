using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using _2home.Models;



namespace _2home.Controllers
{
    public class HomeController : Controller
    {
        
            DataClasses1DataContext db= new DataClasses1DataContext();
        

        public ActionResult Index()

        {
            var id_img= from tt in db.imgs select tt;

            return View(id_img);

        }

        public ActionResult Selectlocation()
        {

            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "Tất cả", Value = "0", Selected = true });

            items.Add(new SelectListItem { Text = "Bình chuẩn", Value = "1" });

            items.Add(new SelectListItem { Text = "Thành phố mới", Value = "2" });

            items.Add(new SelectListItem { Text = "Thủ dầu một", Value = "3" });

            ViewBag.location = items;

            return View();

        }
        public ActionResult Login()

        {


            ViewBag.message = "trang đăng nhập";

            return View();

        }
        public ActionResult DK()

        {


            ViewBag.message = "trang đăng ký";

            return View();

        }
        public ActionResult DKthue()

        {


            ViewBag.message = "trang đăng ký cho thuê";

            return View();

        }
        public ActionResult Manager_DK()

        {


            ViewBag.message = "Quản lý đăng ký cho thuê";

            return View();

        }
        public ActionResult togher()

        {


            ViewBag.message = "Ghép phòng";

            return View();

        }

    }
}