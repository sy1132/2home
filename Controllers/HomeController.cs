using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace _2home.Controllers
{
    public class HomeController : Controller
    {
        

        public ActionResult Index()

        {


            ViewBag.message = "Chào mừng bạn đến với ASP.NET MVC 5";

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

    }
}