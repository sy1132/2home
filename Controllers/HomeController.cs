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

        DataClasses1DataContext db = new DataClasses1DataContext();

        public ActionResult Index(int? size, int? page, string searchString, string City, string Ward, string District)
        {
            ViewBag.Keyword = searchString;

            var query = from img in db.imgs
                        join motel in db.Motels on img.Motel_ID equals motel.Motel_ID
                        where motel.is_available == "Còn trống" 
                        group img by new
                        {
                            motel.Name_motel,
                            motel.location,
                            motel.price,
                            motel.is_available,
                            motel.Motel_ID,
                            motel.Details
                        } into grouped
                        select new index_Viewmodel
                        {
                            ImgLink = grouped.Select(g => g.Link).FirstOrDefault(),
                            MotelName = grouped.Key.Name_motel,
                            Location = grouped.Key.location,
                            Price = grouped.Key.price,
                            is_available = grouped.Key.is_available,
                            ID_user = grouped.Key.Motel_ID,
                            Details = grouped.Key.Details,
                            Motel_ID= grouped.Key.Motel_ID,
                        };
            if (!String.IsNullOrEmpty(searchString))
            {
                var keywords = searchString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(k => k.Trim())
                                           .ToList();

                foreach (var keyword in keywords)
                {
                    query = query.Where(b => b.MotelName.Contains(keyword)
                                          || b.Details.Contains(keyword));
                }
            }
            if (!String.IsNullOrEmpty(City))
                query = query.Where(b => b.Location.Contains(City));

            if (!String.IsNullOrEmpty(Ward))
                query = query.Where(b => b.Location.Contains(Ward));

            if (!String.IsNullOrEmpty(District))
                query = query.Where(b => b.Location.Contains(District));
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

        public ActionResult Details(int? Motel_ID)
        {
            var motelDetails = (from img in db.imgs
                                join motel in db.Motels on img.Motel_ID equals motel.Motel_ID
                                join vid in db.Videos on motel.Motel_ID equals vid.Motel_ID
                                where motel.Motel_ID == Motel_ID.Value
                                select new pic
                                {

                                    MotelName = motel.Name_motel,
                                    Location = motel.location,
                                    Price = motel.price,
                                    is_available = motel.is_available,
                                    ID_user = motel.ID_user,
                                    Link = vid.Link,
                                    ImgLinks = (from img in db.imgs
                                                where img.Motel_ID == motel.Motel_ID
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
                        ID_user = user.ID_user,
                        username = user.username,
                        Email = user.Email,
                        password = user.password,
                        fullname = user.fullname,
                        userrole = user.userrole,
                        blance = user.blance,
                    };
                    ViewBag.Message = "Đăng nhập thành công!";
                    return RedirectToAction("Index", "Home");
                } else
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
                    userrole = "user",
                    blance = 0
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
        public ActionResult DKthue(string roomName, string Details, int? Rooms, string LocationName, string city, string ward, string district, decimal Price, IEnumerable<HttpPostedFileBase> Images, HttpPostedFileBase Videos)
        {
            using (var db = new DataClasses1DataContext())
            {
                var user = Session["User"] as _2home.ViewModels.User;
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                var motel = new _2home.Models.Motel
                {
                    Name_motel = roomName,
                    location = $"{LocationName} ({city}, {ward}, {district})",
                    price = Price,
                    is_available = "Đang chờ duyệt",
                    ID_user = user.ID_user,
                    Details = Details,
                    rooms = Rooms
                };

                db.Motels.InsertOnSubmit(motel);
                db.SubmitChanges();

                int motelId = motel.Motel_ID;

                if (Images != null && Images.Any())
                {
                    foreach (var image in Images)
                    {
                        if (image != null && image.ContentLength > 0)
                        {
                            var imagePath = Path.Combine(Server.MapPath("/asset/images"), Path.GetFileName(image.FileName));
                            image.SaveAs(imagePath);

                            var motelImage = new _2home.Models.img
                            {
                                Link = $"/asset/images/{Path.GetFileName(image.FileName)}",
                                createdAt = DateTime.Now,
                                updatedAt = DateTime.Now,
                                Motel_ID = motelId, 
                            };

                            db.imgs.InsertOnSubmit(motelImage);
                        }
                    }
                }

                if (Videos != null && Videos.ContentLength > 0)
                {
                    var videoPath = Path.Combine(Server.MapPath("/asset/videos/"), Path.GetFileName(Videos.FileName));
                    Videos.SaveAs(videoPath);

                    var motelVideo = new _2home.Models.Video
                    {
                        Link = $"/asset/videos/{Path.GetFileName(Videos.FileName)}",  // Lưu đường dẫn video
                        createdAt = DateTime.Now,
                        updatedAt = DateTime.Now,
                        Motel_ID = motelId,  
                    };

                    db.Videos.InsertOnSubmit(motelVideo);
                }

                db.SubmitChanges();
                return RedirectToAction("index");
            }
        }

        public ActionResult Kiemduyet(int? size, int? page)
        {
            var query = from img in db.imgs
                        join motel in db.Motels on img.Motel_ID equals motel.Motel_ID
                        where motel.is_available == "Đang chờ duyệt"
                        group img by new
                        {
                            motel.Name_motel,
                            motel.location,
                            motel.price,
                            motel.is_available,
                            motel.Motel_ID, // Đảm bảo trường này được chọn
                            motel.rooms
                        } into grouped
                        select new index_Viewmodel
                        {
                            ImgLink = grouped.Select(g => g.Link).FirstOrDefault(),
                            MotelName = grouped.Key.Name_motel,
                            Location = grouped.Key.location,
                            Price = grouped.Key.price,
                            is_available = grouped.Key.is_available,
                            Motel_ID = grouped.Key.Motel_ID, // Đảm bảo sử dụng trường này
                            rooms = grouped.Key.rooms
                        };


            ViewBag.Page = page;
            ViewBag.size = new SelectList(new List<SelectListItem>
        {
            new SelectListItem { Text = "10", Value = "10" },
            new SelectListItem { Text = "20", Value = "20" },
            new SelectListItem { Text = "25", Value = "25" },
            new SelectListItem { Text = "50", Value = "50" },
            new SelectListItem { Text = "100", Value = "100" },
            new SelectListItem { Text = "200", Value = "200" }
        }, "Value", "Text", size);

            page = page ?? 1;
            int pageSize = size ?? 10;
            int pageNumber = page ?? 1;

            var model = query.ToList();
            return View(model.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult Accept(int? Motel_ID)
        {
            if (Motel_ID.HasValue)
            {
                var motel = db.Motels.FirstOrDefault(m => m.Motel_ID == Motel_ID.Value);
                if (motel != null)
                {
                    motel.is_available = "Còn trống";
                    motel.Debt = 1200000;
                    motel.CreatedDate = DateTime.Now;
                    db.SubmitChanges();

                    var senderID = Session["User"] as _2home.ViewModels.User;
                    if (senderID == null)
                    {
                        return RedirectToAction("Login");
                    }

                    var recipientUser = db.users.FirstOrDefault(u => u.ID_user == motel.ID_user);
                    var recipientID = recipientUser?.ID_user;

                    if (recipientID.HasValue)
                    {
                        for (int i = 0; i < motel.rooms; i++)
                        {
                            Room newRoom = new Room
                            {
                                Motel_ID = motel.Motel_ID,
                                Date_of_Issue = DateTime.Now,
                                Electricity_Meter = 0.00m,
                                Water_Meter = 0.00m,
                                Electricity_Bill = 0.00m,
                                Water_Bill = 0.00m,
                                Room_Status = "Trống",
                                Room_Rent = motel.price,
                                Additional_Charges = 0,
                                Total_Amount_Due = 0,
                            };
                            db.Rooms.InsertOnSubmit(newRoom);
                        }

                        Mail newMail = new Mail
                        {
                            Sender = senderID.ID_user,
                            Recipient = recipientID.Value,
                            Content = "Đơn đăng ký nhà trọ của bạn đã được chấp nhận.",
                            SendDate = DateTime.Now,
                            ID_user = (int)motel.ID_user
                        };

                        db.Mails.InsertOnSubmit(newMail);
                        db.SubmitChanges();
                    }
                }
            }
            
                return RedirectToAction("Index");
            
        }

        [HttpPost]
        public ActionResult Reject(int? Motel_ID)
        {
            if (Motel_ID.HasValue)
            {
                var motel = db.Motels.FirstOrDefault(m => m.Motel_ID == Motel_ID.Value); 
                if (motel != null)
                {
                    motel.is_available = "Bị từ chối";
                    db.SubmitChanges();

                    var senderID = Session["User"] as _2home.ViewModels.User;
                    if (senderID == null)
                    {
                        return RedirectToAction("Login");
                    }

                    var recipientUser = db.users.FirstOrDefault(u => u.ID_user == motel.ID_user);
                    var recipientID = recipientUser?.ID_user;

                    if (recipientID.HasValue)
                    {
                        Mail newMail = new Mail
                        {
                            Sender = senderID.ID_user,
                            Recipient = recipientID.Value,
                            Content = "Đơn đăng ký nhà trọ của bạn đã bị từ chối.",
                            SendDate = DateTime.Now,
                            ID_user = (int)motel.ID_user
                        };

                        db.Mails.InsertOnSubmit(newMail);
                        db.SubmitChanges();
                    }
                }
            }
            return RedirectToAction("Kiemduyet");
        }


        public ActionResult Manager_DK(int? size, int? page, string fullname, string motelID, string userrole)

        {
            var query = db.users.Select(u => new User
            {
                ID_user = u.ID_user,
                username = u.username,
                fullname = u.fullname,
                password = u.password,
                MotelID = u.MotelID,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                userrole = u.userrole,
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
        public ActionResult mail(int? size, int? page)
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            var user_save = id_save.ID_user;
            var query = from m in db.Mails
                        join u in db.users on m.ID_user equals u.ID_user
                        where m.Recipient == user_save
                        select new
                        {
                            user1 = (from u1 in db.users
                                     where u1.ID_user == m.Sender
                                     select u1.fullname).FirstOrDefault(),
                            user2 = (from u1 in db.users
                                     where u1.ID_user == m.Recipient
                                     select u1.fullname).FirstOrDefault(),

                            SendDate = m.SendDate,
                            Content = m.Content
                        };


            var mailList = query.ToList().Select(m => new _2home.ViewModels.viewmail
            {
                user1 = m.user1,
                user2 = m.user2,

                SendDate = m.SendDate,
                Content = m.Content
            });

            List<SelectListItem> items = new List<SelectListItem>
    {
        new SelectListItem { Text = "10", Value = "10" },
        new SelectListItem { Text = "20", Value = "20" },
        new SelectListItem { Text = "25", Value = "25" },
        new SelectListItem { Text = "50", Value = "50" },
        new SelectListItem { Text = "100", Value = "100" },
        new SelectListItem { Text = "200", Value = "200" }
    };

            if (size.HasValue)
            {
                foreach (var item in items)
                {
                    if (item.Value == size.Value.ToString()) item.Selected = true;
                }
            }

            ViewBag.size = items;
            ViewBag.currentSize = size;

            page = page ?? 1;
            int pageSize = size ?? 10;
            int pageNumber = page.Value;

            var model = mailList.ToPagedList(pageNumber, pageSize);
            return View(model);
        }

        public ActionResult rental_management(int? size, int? page, string MotelName, string Location, decimal? Price1, decimal? Price2, string is_available)

        {
            ViewBag.message = "Quản lý đăng ký cho thuê";
            var query = from img in db.imgs
                        join motel in db.Motels on img.Motel_ID equals motel.Motel_ID
                        group img by new
                        {
                            motel.Name_motel,
                            motel.location,
                            motel.price,
                            motel.is_available,
                            motel.Motel_ID
                        } into grouped
                        select new index_Viewmodel
                        {
                            ImgLink = grouped.Select(g => g.Link).FirstOrDefault(),
                            MotelName = grouped.Key.Name_motel,
                            Location = grouped.Key.location,
                            Price = grouped.Key.price,
                            is_available = grouped.Key.is_available,
                            Motel_ID = grouped.Key.Motel_ID
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
        public ActionResult UpdateAvailability(int Motel_ID, string is_available)
        {

            using (var context = new DataClasses1DataContext())
            {
                var motel = context.Motels.FirstOrDefault(m => m.Motel_ID == Motel_ID);
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

        public ActionResult QL_room(int? size, int? page, string MotelName, string Location, decimal? Price1, decimal? Price2, string is_available)

        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            var user_save = id_save.ID_user;
            var query = from r in db.Rooms
                        join m in db.Motels on r.Motel_ID equals m.Motel_ID
                        join u in db.users on m.ID_user equals u.ID_user
                        where u.ID_user == user_save
                        select new room
                        {
                            Price = m.price,
                            Room_ID = r.room_ID,
                            Motel_ID = r.Motel_ID,
                            ID_User = r.ID_user,
                            fullname = (from user in db.users
                                        where u.ID_user == r.ID_user
                                        select u.fullname).FirstOrDefault(),
                            Date_of_Issue = r.Date_of_Issue,
                            Electricity_Meter = r.Electricity_Meter,
                            Water_Meter = r.Water_Meter,
                            Previous_Water_Meter=r.Previous_Water_Meter,
                            Previous_Electricity_Usage=r.Previous_Electricity_Usage,
                            Water_Unit_Price = r.Water_Unit_Price,
                            Electricity_Unit_Price=r.Electricity_Unit_Price,
                            Electricity_Bill = r.Electricity_Bill,
                            Water_Bill = r.Water_Bill,
                            Room_Status = r.Room_Status,
                            Room_Rent = r.Room_Rent,
                            Additional_Charges = r.Additional_Charges,
                            Total_Amount_Due = r.Total_Amount_Due
                        };
            var firstRecord = query.FirstOrDefault();
            if (firstRecord != null)
            {
                ViewBag.MotelName = firstRecord.Electricity_Unit_Price;
                ViewBag.Location = firstRecord.Water_Unit_Price;
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

        public ActionResult rent_check(int? size, int? page, string MotelName, string Location, decimal? Price1, decimal? Price2, string is_available)

        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            var user_save = id_save.ID_user;
            var query = from r in db.Rooms
                        join m in db.Motels on r.Motel_ID equals m.Motel_ID
                        join u in db.users on m.ID_user equals u.ID_user
                        where u.ID_user == user_save
                        select new room
                        {
                            Price = m.price,
                            Room_ID = r.room_ID,
                            Motel_ID = r.Motel_ID,
                            ID_User = r.ID_user,
                            fullname = (from user in db.users
                                        where u.ID_user == r.ID_user
                                        select u.fullname).FirstOrDefault(),
                            Date_of_Issue = r.Date_of_Issue,
                            Electricity_Meter = r.Electricity_Meter,
                            Water_Meter = r.Water_Meter,

                            Electricity_Bill = r.Electricity_Bill,
                            Water_Bill = r.Water_Bill,
                            Room_Status = r.Room_Status,
                            Room_Rent = r.Room_Rent,
                            Additional_Charges = r.Additional_Charges,
                            Total_Amount_Due = r.Total_Amount_Due
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
        public ActionResult Profile_user(int ID_user)
        {
            using (var db = new DataClasses1DataContext())
            {
                var user = db.users.FirstOrDefault(u => u.ID_user == ID_user);
                if (user != null)
                {
                    ViewBag.Fullname = user.fullname;
                    ViewBag.Email = user.Email;
                    ViewBag.PhoneNumber = user.PhoneNumber;
                    ViewBag.Gender = user.gender;

                    var rooms = db.Motels.Where(m => m.ID_user == ID_user).ToList();
                    ViewBag.Rooms = rooms;
                }
            }
            return View();
        }
        [HttpGet]
        public ActionResult JoinRental()
        {
            return View();
        }

        [HttpPost]
        public ActionResult JoinRental(string searchString)
        {
            ViewBag.HasSubmitted = true;

            if (!string.IsNullOrEmpty(searchString))
            {
                var parts = searchString.Split('r');
                if (parts.Length == 2)
                {
                    var motelId = parts[0];
                    var roomId = parts[1];
                    int motelIdInt = int.Parse(roomId);
                    int roomIdInt = int.Parse(roomId);
                    var result = (from r in db.Rooms
                                  where r.Motel_ID == motelIdInt && r.room_ID == roomIdInt
                                  select r).ToList();

                    if (result != null && result.Any())
                    {
                        ViewBag.SearchResult = result;
                    }
                    else
                    {
                        ViewBag.SearchResult = null;
                    }
                }
                else
                {
                    ViewBag.SearchResult = null; // Không tìm thấy kết quả nếu định dạng sai
                }
            }
            else
            {
                ViewBag.SearchResult = null;
            }

            return View();
        }



        public ActionResult TopUp(int ID_user, int ID_room, decimal amount)
        {
            var user = db.users.FirstOrDefault(u => u.ID_user == ID_user);
            if (user != null)
            {
                string bank = "Vietcombank";
                string qrUrl = $"https://qr.sepay.vn/img?acc=1026787663&bank={bank}&amount={amount}&des={ID_user}";
                ViewBag.QrUrl = qrUrl;
                ViewBag.blance = user.blance;
                ViewBag.UserName = user.fullname; 
                return View();
            }
            return View();

        }
        public ActionResult ForgotPassword()
        {
           
            return View();

        }
        [HttpPost]
        public ActionResult ForgotPassword(string identifier)
        {

            var user = (from u in db.users
                        where identifier == u.Email || identifier == u.username
                        select u).FirstOrDefault();

            if (user != null)
            {
                ViewBag.Username = user.username;
                ViewBag.Fullname = user.fullname;
                ViewBag.Password = user.password;
            }
            else
            {
                ViewBag.ErrorMessage = "Không tìm thấy tài khoản với thông tin đã cung cấp.";
            }

            return View();

        }
    }
}