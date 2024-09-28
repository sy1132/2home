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
using System.Security.Principal;
using Microsoft.Ajax.Utilities;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Web.Security;
using System.IO;

namespace _2home.Controllers
{
    public class HomeController : Controller
    {
        
        DataClasses1DataContext db= new DataClasses1DataContext();

        public ActionResult Index(int? size, int? page, string searchString)
        {
            ViewBag.Keyword = searchString;
            
            var query = from img in db.imgs
                        join motel in db.Motels on img.ID_user equals motel.ID_user
                        where motel.is_available == "Còn trống"
                        group img by new
                        {
                            motel.Name_motel,
                            motel.location,
                            motel.price,
                            motel.is_available,
                            motel.ID_user
                        } into grouped
                select new index_Viewmodel
                {
                    ImgLink = grouped.Select(g => g.Link).FirstOrDefault(),
                    MotelName = grouped.Key.Name_motel,
                    Location = grouped.Key.location,
                    Price = grouped.Key.price,
                    is_available = grouped.Key.is_available,
                    ID_user = grouped.Key.ID_user
                };
            if (!String.IsNullOrEmpty(searchString))
                query = query.Where(b => b.MotelName.Contains(searchString));
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
            var motelDetails = (from img in db.imgs
                                join motel in db.Motels on img.ID_user equals motel.ID_user
                                join vid in db.Videos on motel.ID_user equals vid.ID_user
                                where motel.ID_user == ID_user.Value
                                select new pic
                                {

                                    MotelName = motel.Name_motel,
                                    Location = motel.location,
                                    Price = motel.price,
                                    is_available = motel.is_available,
                                    ID_user = motel.ID_user,
                                    Link = vid.Link,
                                    ImgLinks = (from img in db.imgs
                                                where img.ID_user == motel.ID_user
                                                select img.Link).ToList()
                                }).FirstOrDefault();

            return View(motelDetails);
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
                    (u.Email == account || u.username.ToString() == account) && u.password == password);

                if (user != null)
                {
                    Session["User"] = new _2home.ViewModels.User
                    {
                        username = user.username,
                        Email = user.Email,
                        password = user.password,
                        fullname= user.fullname,
                        userrole=user.userrole,
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
            return View();
        }

        [HttpPost]
        public ActionResult DK(string username, string fullname, string email, string password, string phone, string gender)
        {
            using (var db = new DataClasses1DataContext())
            {
                var existingUser = db.users.FirstOrDefault(u => u.username == username || u.Email == email);

                if (existingUser != null)
                {
                    ViewBag.Error = "Tên đăng nhập hoặc email đã tồn tại.";
                    return View();
                }

                var newUser = new user
                {
                    username = username,
                    fullname = fullname,
                    Email = email,
                    password = password,
                    PhoneNumber = phone,
                    gender = gender,
                    userrole = "user"
                };

                db.users.InsertOnSubmit(newUser);
                db.SubmitChanges();

                return RedirectToAction("Login", "Home");
            }
        }
        [HttpGet]
        public ActionResult DKthue()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DKthue(string roomName, int? Rooms, string LocationName, string city, string ward, string district, decimal Price, IEnumerable<HttpPostedFileBase> Images, HttpPostedFileBase Video)
        {
            
                using (var db = new DataClasses1DataContext())
                {
                    

                    var motel = new _2home.Models.Motel
                    {
                        Name_motel = roomName,
                        location = $"{LocationName} ({city}, {ward}, {district})",
                        price = Price,
                        is_available = "Đang chờ duyệt"
                    };
                    db.Motels.InsertOnSubmit(motel);
                    db.SubmitChanges(); 

                    if (Images != null && Images.Any())
                    {
                        foreach (var image in Images)
                        {
                            if (image != null && image.ContentLength > 0)
                            {
                                var imagePath = Path.Combine(Server.MapPath("~/asset/images"), Path.GetFileName(image.FileName));
                                image.SaveAs(imagePath);

                                var motelImage = new _2home.Models.img
                                {
                                    Link = $"/asset/images/{Path.GetFileName(image.FileName)}",
                                    createdAt = DateTime.Now,
                                    updatedAt = DateTime.Now
                                };

                                db.imgs.InsertOnSubmit(motelImage);
                            }
                        }
                    }

                    if (Video != null && Video.ContentLength > 0)
                    {
                        var videoPath = Path.Combine(Server.MapPath("/asset/videos/"), Path.GetFileName(Video.FileName));
                        Video.SaveAs(videoPath);

                        var motelVideo = new _2home.Models.Video
                        {
                            Link = $"/asset/videos/{Path.GetFileName(Video.FileName)}",
                            createdAt = DateTime.Now,
                            updatedAt = DateTime.Now
                        };

                        db.Videos.InsertOnSubmit(motelVideo);
                    }

                    db.SubmitChanges(); 

                    return RedirectToAction("index");
                }
            
          
        }



        public ActionResult Manager_DK(int? size, int? page, string fullname, string motelID, string userrole)

        {
            var query = db.users.Select(u => new User
            {
                ID_user = u.ID_user,
                username = u.username,
                fullname = u.fullname,
                password=u.password,
                MotelID = u.MotelID,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                userrole=u.userrole,
                gender = u.gender
            });
            if (!String.IsNullOrEmpty(fullname))
            {
                query = query.Where(u => u.fullname.Contains(fullname));
            }

            if (!String.IsNullOrEmpty(motelID))
            {
                query = query.Where(u => u.MotelID.ToString() == motelID);
            }

            if (!String.IsNullOrEmpty(userrole))
            {
                query = query.Where(u => u.userrole == userrole);
            }

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
        [HttpPost]
        public ActionResult UpdateUserRole(int ID_user, string userrole)
        {
            
                using (var context = new DataClasses1DataContext())
                {
                    var role = context.users.FirstOrDefault(m => m.ID_user == ID_user);
                    if (role != null)
                    {
                        role.userrole = userrole;
                        context.SubmitChanges();
                    }
                }
                return RedirectToAction("Manager_DK");
        }

        public ActionResult rental_management(int? size, int? page, string MotelName, string Location, decimal? Price1, decimal? Price2, string is_available)

        {
            ViewBag.message = "Quản lý đăng ký cho thuê";
            var query = from img in db.imgs
                        join motel in db.Motels on img.ID_user equals motel.ID_user
                        group img by new
                        {
                            motel.Name_motel,
                            motel.location,
                            motel.price,
                            motel.is_available,
                            motel.ID_user
                        } into grouped
                        select new index_Viewmodel
                        {
                            ImgLink = grouped.Select(g => g.Link).FirstOrDefault(),
                            MotelName = grouped.Key.Name_motel,
                            Location = grouped.Key.location,
                            Price = grouped.Key.price,
                            is_available = grouped.Key.is_available,
                            ID_user = grouped.Key.ID_user
                        };
            if (!string.IsNullOrEmpty(MotelName))
            {
                query = query.Where(m => m.MotelName.Contains(MotelName));
            }

            if (!string.IsNullOrEmpty(Location))
            {
                query = query.Where(m => m.Location.Contains(Location));
            }
            if (Price1.HasValue)
            {
                query = query.Where(m => m.Price >= Price1.Value);
            }
            if (Price2.HasValue)
            {
                query = query.Where(m => m.Price <= Price2.Value);
            }
            if (!String.IsNullOrEmpty(is_available))
            {
                query = query.Where(u => u.is_available == is_available);
            }
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
        [HttpPost]
        public ActionResult UpdateAvailability(int ID_user, string is_available)
        {

            using (var context =new DataClasses1DataContext()) 
  {
                var motel = context.Motels.FirstOrDefault(m => m.ID_user == ID_user);
                if (motel != null)
                {
                  motel.is_available = is_available;
                    context.SubmitChanges();
                }
           }
            return RedirectToAction("rental_management");
        }

        public ActionResult togher()

        {


            ViewBag.message = "Ghép phòng";

            return View();

        }

    }
}