using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using _2home.Models;
using System.Data.Linq;
using _2home.ViewModels;
using System.Drawing;
using System.Web.UI;
using System.Linq.Dynamic;
using PagedList;
using System.Drawing.Printing;
using System.Web.Helpers;

namespace _2home.Controllers
{
    public class HomeController : Controller
    {
        
        DataClasses1DataContext db= new DataClasses1DataContext();

        public ActionResult Index(int? size, int? page)
        {
            var query = from img in db.imgs
                        join motel in db.Motels on img.ID_user equals motel.ID_user
                        select new index_Viewmodel
                        {
                            ImgLink = img.Link,
                            MotelName = motel.Name_motel,
                            Location = motel.location,
                            Price = motel.price,
                            is_available = motel.is_available,
                            ID_user=motel.ID_user,
                        };
            ViewBag.Page = page;
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "10", Value = "10" });
            items.Add(new SelectListItem { Text = "20", Value = "20" });
            items.Add(new SelectListItem { Text = "25", Value = "25" });
            items.Add(new SelectListItem { Text = "50", Value = "50" });
            items.Add(new SelectListItem { Text = "100", Value = "100" });
            items.Add(new SelectListItem { Text = "200", Value = "200" });
            foreach (var item in items)
            {
                if (item.Value == size.ToString()) item.Selected = true;
            }
            ViewBag.size = items;
            ViewBag.currentSize = size; 

            page = page ?? 1;
            int pageSize = (size ?? 10);

            int pageNumber = (page ?? 1);
            var model = query.ToList();
            return View(model.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Details(int? ID_user)
        {
            var query = from img in db.imgs
                        join motel in db.Motels on img.ID_user equals motel.ID_user
                        join vid in db.Videos on motel.ID_user equals vid.ID_user
                        where motel.ID_user == ID_user.Value
                        select new index_Viewmodel
                        {
                            ImgLink = img.Link,
                            MotelName = motel.Name_motel,
                            Location = motel.location,
                            Price = motel.price,
                            is_available = motel.is_available,
                            ID_user = motel.ID_user,
                            Link = vid.Link,

                        };

            var viewModel = query.FirstOrDefault();
            return View(viewModel);
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
            return View();
        }
        [HttpPost]
        public ActionResult Login(string account, string password)
        {

            using (var db = new DataClasses1DataContext())
            {
              
                var user = db.users.FirstOrDefault(u =>
                    (u.Email == account || u.ID_user.ToString() == account) && u.password == password);

                if (user != null)
                {
                    Session["User"] = new _2home.ViewModels.User
                    {
                        ID_user = user.ID_user,
                        Email = user.Email,
                        password = user.password,
                        fullname= user.fullname,
                    };
                    ViewBag.Message = "Đăng nhập thành công!";
                    return RedirectToAction("Index", "Home");
                }else
                    {
                        ViewBag.Message = "Tên tài khoản hoặc mật khẩu không đúng!";
                        return View();
                    }
                
            }
        }
        public ActionResult Logout()
        {
            Session["User"] = null;
            TempData["Message"] = "Bạn đã đăng xuất!";
            return RedirectToAction("Index", "Home");
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