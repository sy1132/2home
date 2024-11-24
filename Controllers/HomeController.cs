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
using System.Diagnostics.Contracts;
using System.Web.Util;

namespace _2home.Controllers
{
    public class HomeController : Controller
    {

        DataClasses1DataContext db = new DataClasses1DataContext();

        public ActionResult Index(int? size, int? page, string searchString, string City, string Ward, string District, int? minPrice, int? maxPrice)
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
                            Motel_ID = grouped.Key.Motel_ID,
                        };

            var vipMotels = (from m in db.Motels
                             join p in db.imgs on m.Motel_ID equals p.Motel_ID
                             where m.is_available == "Còn trống" && m.VIP == 1
                             group p by m.Motel_ID into motelGroup
                             orderby Guid.NewGuid()
                             select new index_Viewmodel
                             {
                                 Motel_ID = motelGroup.Key,
                                 MotelName = motelGroup.First().Motel.Name_motel,  
                                 Location = motelGroup.First().Motel.location,    
                                 ImgLink = motelGroup.Select(img => img.Link).FirstOrDefault(),
                                 price = motelGroup.First().Motel.price            
                             }).Take(4).ToList();

            var sharedMotels = (from t in db.togethers
                                join motel in db.Motels on t.Motel_ID equals motel.Motel_ID
                                join img in db.imgs on motel.Motel_ID equals img.Motel_ID
                                where motel.is_available == "Còn trống" && motel.VIP == 1
                                select new { t, motel, img } 
                    )
                    .GroupBy(x => x.motel.Motel_ID) 
                    .Select(group => new index_Viewmodel
                    {
                        ID_user = group.First().t.ID_user, 
                        Motel_ID = group.Key, 
                        room_ID = group.First().t.room_ID, 
                        price = group.First().t.price, 
                        location = group.First().t.location,
                        roomdetails = group.First().t.roomdetails, 
                        requestdetails = group.First().t.requestdetails,
                        MotelName = group.First().motel.Name_motel, 
                        ImgLink = group.Select(g => g.img.Link).FirstOrDefault() 
                    }).Take(4).ToList();

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
            if (minPrice.HasValue)
                query = query.Where(b => b.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(b => b.Price <= maxPrice.Value);
            ViewBag.Page = page;
            List<SelectListItem> items = new List<SelectListItem>
    {
        new SelectListItem { Text = "10", Value = "10" },
        new SelectListItem { Text = "20", Value = "20" },
        new SelectListItem { Text = "25", Value = "25" },
        new SelectListItem { Text = "50", Value = "50" },
        new SelectListItem { Text = "100", Value = "100" },
        new SelectListItem { Text = "200", Value = "200" }
    };
            foreach (var item in items)
            {
                if (item.Value == size.ToString()) item.Selected = true;
            }
            ViewBag.size = items;
            ViewBag.currentSize = size;

            page = page ?? 1;
            int pageSize = (size ?? 10);
            int pageNumber = (page ?? 1);

            var model = new index_Viewmodel
            {
                Motels = query.ToPagedList(pageNumber, pageSize),
                VipMotels = vipMotels,
                SharedMotels = sharedMotels
            };

            return View(model);
        }

        public ActionResult TogetherView(string City, string Ward, string District, int? size, int? page)
        {
            var togetherQuery = from t in db.togethers
                                join motel in db.Motels on t.Motel_ID equals motel.Motel_ID
                                where t.is_available == "Còn trống"
                                      && t.creation_date >= DateTime.Now.AddDays(-30)
                                select new index_Viewmodel
                                {
                                    ID_user = t.ID_user,
                                    Motel_ID = t.Motel_ID,
                                    room_ID = t.room_ID,
                                    price = t.price,
                                    location = t.location,
                                    roomdetails = t.roomdetails,
                                    requestdetails = t.requestdetails,
                                    MotelName = motel.Name_motel,
                                    is_available=t.is_available,
                                    ImgLink = (from i in db.imgs
                                               where i.Motel_ID == motel.Motel_ID
                                               select i.Link).FirstOrDefault()
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
            var model = togetherQuery.ToList();
            return View(model.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Details(int? Motel_ID)
        {
            var motelDetails = (from img in db.imgs
                                join motel in db.Motels on img.Motel_ID equals motel.Motel_ID
                                join vid in db.Videos on motel.Motel_ID equals vid.Motel_ID
                                join user in db.users on motel.ID_user equals user.ID_user
                                where motel.Motel_ID == Motel_ID.Value
                                select new pic
                                {
                                    phonenumber = user.PhoneNumber,
                                    fullname = user.fullname,
                                    MotelName = motel.Name_motel,
                                    Location = motel.location,
                                    Price = motel.price,
                                    is_available = motel.is_available,
                                    ID_user = motel.ID_user,
                                    Link = vid.Link,
                                    Details = motel.Details,
                                    ImgLinks = (from img in db.imgs
                                                where img.Motel_ID == motel.Motel_ID
                                                select img.Link).ToList()

                                }).FirstOrDefault();
            var randomMotels = (from m in db.Motels
                                join p in db.imgs on m.Motel_ID equals p.Motel_ID
                                where m.Motel_ID != Motel_ID && m.VIP == 1 && m.is_available=="Còn trống"
                                orderby Guid.NewGuid() 
                                select new pic
                                {
                                    MotelName = m.Name_motel,
                                    Location = m.location,
                                    Link = p.Link,
                                    Linkd = Url.Action("Details", new { Motel_ID = m.Motel_ID }) 
                                }).Take(4).ToList();

                motelDetails.RelatedMotels = randomMotels;
            

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
                        PhoneNumber = user.PhoneNumber,
                        gender = user.gender,
                        MotelID = user.MotelID,
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
        private bool IsRentPriceValid(int rentalDuration, int roomType)
        {
            var user = Session["User"] as _2home.ViewModels.User;
            
            int kt;
            if (roomType == 0)
            {
                kt = rentalDuration * 500000;
            }else
            {
                kt = rentalDuration * 700000;

            }
            if (kt < user.blance)
            {
                return true;
            }
            else { return false; }
        }
        [HttpPost]
        public ActionResult DKthue(int rentalDuration,int roomType, string roomName, string Details,decimal? Deposit, string LocationName, string city, string ward, string district, decimal Price, IEnumerable<HttpPostedFileBase> Images, HttpPostedFileBase Videos)
        {
            using (var db = new DataClasses1DataContext())
            {
                var user = Session["User"] as _2home.ViewModels.User;
                if (user == null)
                {
                    return RedirectToAction("Login");
                }
                bool isValid = IsRentPriceValid(rentalDuration, roomType);
                if (!isValid)
                {
                    TempData["RentPriceError"] = "Tiền trong tài khoản không đủ để đăng ký";
                    return RedirectToAction("DKthue"); 
                }

                var motel = new _2home.Models.Motel
                {
                    Name_motel = roomName,
                    location = $"{LocationName} ({city}, {ward}, {district})",
                    price = Price,
                    is_available = "Đang chờ duyệt",
                    ID_user = user.ID_user,
                    Details = Details,
                    rooms = 1,
                    CreatedDate= DateTime.Now,
                    months= rentalDuration,
                    VIP=roomType
                };

                db.Motels.InsertOnSubmit(motel);
                db.SubmitChanges();
                string transactionId = Guid.NewGuid().ToString();

                var paymentHistory = new PaymentHistory
                {
                    UserID = user.ID_user,
                    Amount = pay(rentalDuration, roomType),
                    PaymentDate = DateTime.Now,
                    TransactionID = transactionId,
                    Status = "Hoàn thành"
                };

                db.PaymentHistories.InsertOnSubmit(paymentHistory);
                db.SubmitChanges();
                var u = (User)Session["User"];

                if (u != null)
                {
                    u.blance -= pay(rentalDuration, roomType);

                    db.SubmitChanges();

                    Session["User"] = u;
                }
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
                        Link = $"/asset/videos/{Path.GetFileName(Videos.FileName)}",
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
        private int pay(int rentalDuration, int roomType)
        {
            var user = Session["User"] as _2home.ViewModels.User;

            int kt;
            if (roomType == 0)
            {
                kt = rentalDuration * 500000;
            }
            else
            {
                kt = rentalDuration * 700000;

            }
            return kt;
        }
        public ActionResult KiemduyetInkeeper(int? size, int? page)
        {
            var query = from motel in db.Motels
                        join together in db.togethers on motel.Motel_ID equals together.Motel_ID
                        join user in db.users on motel.ID_user equals user.ID_user
                        select new index_Viewmodel
                        {
                            MotelName = motel.Name_motel,
                            Location = motel.location,
                            Price = motel.price,
                            Motel_ID = motel.Motel_ID,
                            together_ID = together.together_ID,
                            rooms = motel.rooms,
                            Room_ID = together.room_ID,
                            requestdetails = together.requestdetails,
                            roomdetails = together.roomdetails,
                            fullname = user.fullname,
                            is_available=together.is_available
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
                            motel.Motel_ID,
                            motel.rooms
                        } into grouped
                        select new index_Viewmodel
                        {
                            ImgLink = grouped.Select(g => g.Link).FirstOrDefault(),
                            MotelName = grouped.Key.Name_motel,
                            Location = grouped.Key.location,
                            Price = grouped.Key.price,
                            is_available = grouped.Key.is_available,
                            Motel_ID = grouped.Key.Motel_ID,
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
                        recipientUser.userrole = "Innkeeper";
                        recipientUser.MotelID = motel.Motel_ID;
                        db.SubmitChanges();
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

            return RedirectToAction("Kiemduyet");

        }

        [HttpPost]
        public ActionResult Reject(int? Motel_ID,string Reason)
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
                            Content = $"Đơn đăng ký ở chung của bạn bị từ chối. Lý do: {Reason}",
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
        public ActionResult AddFunds(int ID_user, int amount)
        {
            var user = db.users.SingleOrDefault(u => u.ID_user == ID_user && u.userrole == "Innkeeper");
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại hoặc không phải là Innkeeper.";
                return RedirectToAction("Manager_DK");
            }

            user.blance += amount; 

            db.SubmitChanges();

            TempData["Success"] = "Nạp tiền thành công!";
            return RedirectToAction("Manager_DK");
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
                        orderby m.SendDate descending
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

        [HttpPost]
        public ActionResult AcceptRoom(string roomId)
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            if (!string.IsNullOrEmpty(roomId))
            {
                int parsedRoomId;
                if (int.TryParse(roomId, out parsedRoomId))
                {
                    var room = db.Rooms.FirstOrDefault(r => r.room_ID == parsedRoomId);
                    if (room != null)
                    {
                        var mail = db.Mails.FirstOrDefault(m => m.Recipient == id_save.ID_user && m.Content.Contains($"mã phòng: {roomId}"));
                        if (mail != null)
                        {
                            var senderUser = db.users.FirstOrDefault(u => u.ID_user == mail.Sender);
                            if (senderUser != null)
                            {
                                senderUser.MotelID = parsedRoomId;
                            }
                            var motel = db.Motels.FirstOrDefault(m => m.Motel_ID == room.Motel_ID);
                            
                            room.ID_user = senderUser.ID_user;
                            room.Room_Status = "Đã thuê";
                            room.Previous_Electricity_Usage = 0;
                            room.Electricity_Unit_Price = 0;
                            room.Water_Unit_Price = 0;
                            room.Previous_Water_Meter = 0;
                            db.SubmitChanges();
                        }
                    }
                }
            }

            return RedirectToAction("mail");
        }
        [HttpPost]
        public ActionResult RejectRoom(string roomId)
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            // Xử lý logic từ chối ở đây
            if (!string.IsNullOrEmpty(roomId))
            {
                int parsedRoomId;
                if (int.TryParse(roomId, out parsedRoomId))
                {
                    var mail = db.Mails.FirstOrDefault(m => m.Recipient == id_save.ID_user && m.Content.Contains($"mã phòng: {roomId}"));
                    if (mail != null)
                    {
                        // Cập nhật thông tin hoặc trạng thái mail nếu cần
                        db.SubmitChanges();
                    }
                }
            }

            return RedirectToAction("mail"); // Hoặc bạn có thể chỉ cần trả về một phần view nếu cần
        }

        public ActionResult viewContractinkeeper()
        {
            
            return View();
        }
       
       
        public ActionResult rental_management(int? size, int? page, string MotelName, string Location, decimal? Price1, decimal? Price2, string is_available)

        {
            ViewBag.message = "Quản lý đăng ký cho thuê";
            var query = from img in db.imgs
                        join motel in db.Motels on img.Motel_ID equals motel.Motel_ID
                        where motel.is_available == "Còn trống" || motel.is_available == "Hết phòng" || motel.is_available == "Đang chờ duyệt"
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
        [HttpPost]
        public ActionResult UpdateAvailability1(int Motel_ID, string is_available)
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
            return RedirectToAction("motel_manager");
        }
        public ActionResult togher()
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            var q = from u in db.users
                    join m in db.Motels on u.MotelID equals m.Motel_ID
                    where u.ID_user == id_save.ID_user
                    select new _2home.ViewModels.index_Viewmodel
                    {
                        Motel_ID=m.Motel_ID,
                        MotelName = m.Name_motel,
                        Location = m.location,
                        fullname = u.fullname,
                        priceroom = (int?)m.price,
                    };

            var model = q.FirstOrDefault();

            return View(model);
        }

        [HttpPost]
        public ActionResult togher(int? userId, int? Motel_ID, int? age, decimal? price, string location, int? imgId, int? videoId, string roomDetails, string requestDetails)
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }
            using (var db = new DataClasses1DataContext())
            {
                var together = new together
                {
                    ID_user = id_save.ID_user,
                    Motel_ID = id_save.MotelID,
                    room_ID = (from r in db.Rooms where r.room_ID == id_save.ID_user select r.room_ID).FirstOrDefault(),
                    price = price,
                    location = location,
                    roomdetails = roomDetails,
                    requestdetails = requestDetails,
                    creation_date = DateTime.Now,
                    is_available="Còn trống"
                };

                db.togethers.InsertOnSubmit(together);
                db.SubmitChanges();
            }
            TempData["SuccessMessage"] = "Đăng ký thành công!";
            return RedirectToAction("togher", "Home");
        }
        public ActionResult motel_manager(int? size, int? page)
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            var user_save = id_save.ID_user;
            var query = from m in db.Motels
                        where m.ID_user == user_save && m.is_available=="Còn trống" || m.ID_user == user_save && m.is_available == "Hết phòng"
                        select new
                        {
                            Motel_ID = m.Motel_ID,
                            ID_user = m.ID_user,
                            Name_motel = m.Name_motel,
                            location = m.location,
                            price = m.price,
                            is_available = m.is_available,
                            Details = m.Details,
                            rooms = m.rooms,
                            CreatedDate = m.CreatedDate,
                        };

            var model = query.AsEnumerable().Select(m => new _2home.Models.Motel
            {
                Motel_ID = m.Motel_ID,
                ID_user = m.ID_user,
                Name_motel = m.Name_motel,
                location = m.location,
                price = m.price,
                is_available = m.is_available,
                Details = m.Details,
                rooms = m.rooms,
                CreatedDate = m.CreatedDate,
            }).ToList();

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

            return View(model.ToPagedList(pageNumber, pageSize));
        }
        [HttpGet]
        public ActionResult pay(int Motel_ID)
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            var motel = db.Motels.FirstOrDefault(m => m.Motel_ID == Motel_ID);
            if (motel == null)
            {
                return HttpNotFound();
            }

            var model = new _2home.ViewModels.Motel
            {
                MotelName = motel.Name_motel,
                Motel_ID = motel.Motel_ID
            };

            return View(model);
        }


        [HttpPost]
        public ActionResult pay(int Motel_ID, decimal amount)
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            var userId = id_save.ID_user;
            var user = db.users.FirstOrDefault(u => u.ID_user == userId);
            var motel = db.Motels.FirstOrDefault(m => m.Motel_ID == Motel_ID);

            if (user == null || motel == null)
            {
                return HttpNotFound();
            }


            if (user.blance < amount)
            {
                ModelState.AddModelError("", "Số tiền không đủ để đóng.");
                ViewBag.Motel_ID = Motel_ID;
                return View();
            }


            user.blance -= (int)amount;



            db.SubmitChanges();

            return RedirectToAction("motel_manager");
        }
        public ActionResult QL_room(int? Motel_ID, int? size, int? page)
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            var user_save = id_save.ID_user;

            var currentRoom = db.Rooms.FirstOrDefault(r => r.Motel_ID == Motel_ID);
            if (currentRoom != null)
            {
                ViewBag.MotelName = currentRoom.Electricity_Unit_Price;
                ViewBag.Location = currentRoom.Water_Unit_Price;
            }
            else
            {
                ViewBag.MotelName = 0;
                ViewBag.Location = 0;
            }
            var query = from r in db.Rooms
                        join m in db.Motels on r.Motel_ID equals m.Motel_ID
                        where m.ID_user == user_save && (Motel_ID == null || r.Motel_ID == Motel_ID) && r.ID_user!=null
                        select new room
                        {
                            Room_ID = r.room_ID,
                            ID_User = r.ID_user,
                            Motel_ID = r.Motel_ID,
                            fullname = db.users.Where(u => u.ID_user == r.ID_user).Select(u => u.fullname).FirstOrDefault() ?? "N/A",
                            Date_of_Issue = db.RoomBills.Where(rb1 => rb1.Room_ID == r.room_ID).Max(rb1 => (DateTime?)rb1.Date_of_Issue) ?? DateTime.Today,
                            Electricity_Bill = db.RoomBills.Where(rb1 => rb1.Room_ID == r.room_ID).Select(rb1 => (decimal?)rb1.Electricity_Bill).FirstOrDefault() ?? 0m, // giá trị mặc định cho decimal
                            Water_Bill = db.RoomBills.Where(rb1 => rb1.Room_ID == r.room_ID).Select(rb1 => (decimal?)rb1.Water_Bill).FirstOrDefault() ?? 0m, // giá trị mặc định cho decimal
                            Additional_Charges = db.RoomBills.Where(rb1 => rb1.Room_ID == r.room_ID).Select(rb1 => (decimal?)rb1.Additional_Charges).FirstOrDefault() ?? 0m, // giá trị mặc định cho decimal
                            Price = m.price ?? 0m, 
                            moneypay = (int)(r.money_paid ?? 0m), 
                            Room_Status = db.RoomBills
                                .Where(rb1 => rb1.Room_ID == r.room_ID)
                                .OrderByDescending(rb1 => rb1.Date_of_Issue)
                                .Select(rb1 => rb1.Room_Status)
                                .FirstOrDefault() ?? "Giá trị mặc định",
                            Total_Amount_Due = db.RoomBills.Where(rb1 => rb1.Room_ID == r.room_ID).Sum(rb1 => (decimal?)rb1.Total_Amount_Due) ?? 0m, // giá trị mặc định cho decimal
                            Latest_Total_Amount_Due = db.RoomBills.Where(rb1 => rb1.Room_ID == r.room_ID).OrderByDescending(rb1 => rb1.Date_of_Issue).Select(rb1 => (decimal?)rb1.Total_Amount_Due).FirstOrDefault() ?? 0m // giá trị mặc định cho decimal
                        };


            page = page ?? 1;
            int pageSize = size ?? 10;
            var model = query.ToPagedList(page.Value, pageSize);

            return View(model);
        }
        
    public ActionResult updatebill(int? Motel_ID, int? size, int? page)
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            var user_save = id_save.ID_user;

            var currentRoom = db.Rooms.FirstOrDefault(r => r.Motel_ID == Motel_ID);
            if (currentRoom != null)
            {
                ViewBag.MotelName = currentRoom.Electricity_Unit_Price;
                ViewBag.Location = currentRoom.Water_Unit_Price;
            }
            else
            {
                ViewBag.MotelName = 0;
                ViewBag.Location = 0;
            }

            var query = from r in db.Rooms
                        join m in db.Motels on r.Motel_ID equals m.Motel_ID
                        where m.ID_user == user_save && (Motel_ID == null || r.Motel_ID == Motel_ID)
                        select new room
                        {
                            Room_ID = r.room_ID,
                            Motel_ID = r.Motel_ID,
                            fullname = (from u in db.users where u.ID_user == r.ID_user select u.fullname).FirstOrDefault(),
                            Date_of_Issue = r.Date_of_Issue,
                            Electricity_Bill = r.Electricity_Bill,
                            Water_Bill = r.Water_Bill,
                            Electricity_Meter = r.Electricity_Meter,
                            Water_Meter = r.Water_Meter,
                            Additional_Charges = r.Additional_Charges,
                            Price = m.price,
                            Room_Status = r.Room_Status,
                            Previous_Electricity_Usage = r.Previous_Electricity_Usage,
                            Previous_Water_Meter = r.Previous_Water_Meter,
                        };

            page = page ?? 1;
            int pageSize = size ?? 10;
            var model = query.ToPagedList(page.Value, pageSize);

            return View(model);
        }
        [HttpPost]
        public ActionResult Create(int? Motel_ID)
        {
            if (Motel_ID.HasValue)
            {
                    Room rooms = new Room
                    {
                        Motel_ID= Motel_ID.Value,
                        Room_Status="Trống"
                    };

                    db.Rooms.InsertOnSubmit(rooms);
                    db.SubmitChanges();
                
            }


            return RedirectToAction("updatebill", new { Motel_ID = Motel_ID });

        }
        [HttpPost]
        public ActionResult Delete(int? Room_ID, int? Motel_ID)
        {
            if (Room_ID.HasValue)
            {
                var roomToDelete = db.Rooms.FirstOrDefault(r => r.room_ID == Room_ID.Value);
                if (roomToDelete != null)
                {
                    var relatedRoomBills = db.RoomBills.Where(rb => rb.Room_ID == Room_ID.Value);
                    db.RoomBills.DeleteAllOnSubmit(relatedRoomBills);

                    var relatedTogetherRecords = db.togethers.Where(t => t.room_ID == Room_ID.Value);
                    db.togethers.DeleteAllOnSubmit(relatedTogetherRecords);

                    int? userId = roomToDelete.ID_user;
                    if (userId.HasValue)
                    {
                        var user = db.users.FirstOrDefault(u => u.ID_user == userId.Value);
                        if (user != null)
                        {
                            user.MotelID = null; 
                        }
                    }

                    db.Rooms.DeleteOnSubmit(roomToDelete);
                    db.SubmitChanges(); 
                }
            }

            return RedirectToAction("updatebill", new { Motel_ID = Motel_ID });
        }


        [HttpPost]
        public ActionResult Refresh(int? Room_ID, int? Motel_ID)
        {
            if (Room_ID.HasValue)
            {
                var roomToRefresh = db.Rooms.FirstOrDefault(r => r.room_ID == Room_ID.Value);
                if (roomToRefresh != null)
                {
                    int? userId = roomToRefresh.ID_user;
                    if (userId.HasValue)
                    {
                        var user = db.users.FirstOrDefault(u => u.ID_user == userId.Value);
                        if (user != null)
                        {
                            user.MotelID = null;
                        }
                    }
                    roomToRefresh.ID_user = null;

                    roomToRefresh.Electricity_Meter = 0;
                    roomToRefresh.Water_Meter = 0;
                    roomToRefresh.Electricity_Bill = 0;
                    roomToRefresh.Water_Bill = 0;
                    roomToRefresh.Previous_Electricity_Usage = 0;
                    roomToRefresh.Previous_Water_Meter = 0;
                    roomToRefresh.Additional_Charges = 0;
                    roomToRefresh.Room_Status = "Còn trống";

                    db.SubmitChanges();
                }
            }

            return RedirectToAction("updatebill", new { Motel_ID = Motel_ID });
        }

        [HttpPost]
        public ActionResult UpdateElectricityAndWater(int? Motel_ID, decimal Electricity_Usage, decimal Water_Usage)
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            bool updateSuccessful = false;

            var roomsToUpdate = db.Rooms.Where(r => r.Motel_ID == Motel_ID).ToList();
            if (roomsToUpdate.Any())
            {
                foreach (var room in roomsToUpdate)
                {
                    room.Electricity_Unit_Price = Electricity_Usage;
                    room.Water_Unit_Price = Water_Usage;
                }

                try
                {
                    db.SubmitChanges();
                    updateSuccessful = true;
                }
                catch (Exception ex)
                {
                    TempData["UpdateMessage"] = "Đã xảy ra lỗi trong quá trình cập nhật: " + ex.Message;
                }
            }

            if (updateSuccessful)
            {
                TempData["UpdateMessage"] = "Cập nhật đơn giá điện và nước thành công.";
            }
            else
            {
                TempData["UpdateMessage"] = "Không tìm thấy phòng để cập nhật.";
            }

            return RedirectToAction("updatebill", new { Motel_ID = Motel_ID });
        }

        [HttpPost]
        public ActionResult updatebill(int Motel_ID, decimal Electricity_Usage, decimal Water_Usage, int Room_ID,
    decimal Previous_Electricity_Usage, decimal? Electricity_Meter, decimal Previous_Water_Meter,
    decimal? Water_Meter, decimal Additional_Charges, decimal Price)
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }

            bool updateSuccessful = false;

            var roomToUpdate = db.Rooms.FirstOrDefault(r => r.Motel_ID == Motel_ID && r.room_ID == Room_ID);
            if (roomToUpdate != null)
            {
                if (Electricity_Meter.HasValue && Water_Meter.HasValue)
                {
                    roomToUpdate.Previous_Electricity_Usage = Electricity_Meter.Value; 
                    roomToUpdate.Previous_Water_Meter = Water_Meter.Value; 
                                        db.SubmitChanges();

  
                    var roomBill = new _2home.Models.RoomBill
                    {
                        Room_ID = roomToUpdate.room_ID,
                        Previous_Electricity_Usage = Previous_Electricity_Usage,
                        Electricity_Meter = (decimal)Electricity_Meter,
                        Previous_Water_Meter = Previous_Water_Meter,
                        Water_Meter = (decimal)Water_Meter,
                        Additional_Charges = Additional_Charges,
                        Price = Price,
                        Electricity_Bill = (Electricity_Meter.Value - Previous_Electricity_Usage) * Electricity_Usage,
                        Water_Bill = (Water_Meter.Value - Previous_Water_Meter) * Water_Usage,
                        Total_Amount_Due = ((Electricity_Meter.Value - Previous_Electricity_Usage) * Electricity_Usage) +
                                           ((Water_Meter.Value - Previous_Water_Meter) * Water_Usage) + Additional_Charges,
                        Room_Status = roomToUpdate.Room_Status,
                        Date_of_Issue = DateTime.Now
                    };

                    db.RoomBills.InsertOnSubmit(roomBill);
                    updateSuccessful = true;
                }
            }

            if (updateSuccessful)
            {
                try
                {
                    db.SubmitChanges();
                    TempData["UpdateMessage"] = "Cập nhật đơn giá điện và nước thành công.";
                }
                catch (Exception ex)
                {
                    TempData["UpdateMessage"] = "Đã xảy ra lỗi trong quá trình cập nhật: " + ex.Message;
                }
            }
            else
            {
                TempData["UpdateMessage"] = "Không tìm thấy phòng để cập nhật hoặc các giá trị meter không hợp lệ.";
            }

            return RedirectToAction("updatebill", new { Motel_ID = Motel_ID });
        }

        private decimal CalculateElectricityBill(_2home.Models.Room room)
        {
            if (room.Electricity_Meter != null && room.Previous_Electricity_Usage != null && room.Electricity_Unit_Price != null)
            {
                decimal electricityUsage = (decimal)(room.Electricity_Meter - room.Previous_Electricity_Usage);

                if (electricityUsage < 0) return 0;

                return electricityUsage * (decimal)room.Electricity_Unit_Price;
            }

            return 0;
        }

        private decimal CalculateWaterBill(_2home.Models.Room room)
        {
            if (room.Water_Meter != null && room.Previous_Water_Meter != null && room.Water_Unit_Price != null)
            {
                decimal waterUsage = (decimal)(room.Water_Meter - room.Previous_Water_Meter);

                if (waterUsage < 0) return 0;

                return waterUsage * (decimal)room.Water_Unit_Price;
            }

            return 0;
        }

        public ActionResult rent_check(int? size, int? page, string MotelName, string Location, decimal? Price1, decimal? Price2, string is_available)
        {
            var id_save = Session["User"] as _2home.ViewModels.User;
            if (id_save == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.save = id_save.userrole;
            ViewBag.ID_User = id_save.ID_user;

            var user_save = id_save.ID_user;

            var query = from rb in db.RoomBills
                        join r in db.Rooms on rb.Room_ID equals r.room_ID
                        where r.ID_user == user_save
                        orderby rb.Date_of_Issue descending
                        select rb;

            var totalElectricity = query.Sum(rb => rb.Electricity_Bill);
            var totalWater = query.Sum(rb => rb.Water_Bill);
            var totalAdditionalCharges = query.Sum(rb => rb.Additional_Charges);
            var totalRent = query.Sum(rb => rb.Price);
            var totalAmountDue = query.Sum(rb => rb.Total_Amount_Due);

            var totalPaid = db.Rooms
                .Where(r => r.ID_user == user_save)
                .Sum(r => (decimal?)r.money_paid) ?? 0;
            

            ViewBag.TotalElectricity = totalElectricity;
            ViewBag.TotalWater = totalWater;
            ViewBag.TotalAdditionalCharges = totalAdditionalCharges;
            ViewBag.TotalAmountDue = totalAmountDue;
            ViewBag.TotalRent = query.Sum(rb => rb.Price);
            ViewBag.TotalPaid = totalPaid;

            ViewBag.TotalDebt = totalAmountDue- totalPaid;
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
        [HttpGet]
        public ActionResult rent_checkin(int? size, int? page,int ID_User)
        {
            

            var query = from rb in db.RoomBills
                        join r in db.Rooms on rb.Room_ID equals r.room_ID
                        where r.ID_user == ID_User
                        orderby rb.Date_of_Issue descending
                        select rb;

            var totalElectricity = query.Sum(rb => rb.Electricity_Bill);
            var totalWater = query.Sum(rb => rb.Water_Bill);
            var totalAdditionalCharges = query.Sum(rb => rb.Additional_Charges);
            var totalRent = query.Sum(rb => rb.Price);
            var totalAmountDue = query.Sum(rb => rb.Total_Amount_Due);

            var totalPaid = db.Rooms
                .Where(r => r.ID_user == ID_User)
                .Sum(r => (decimal?)r.money_paid) ?? 0;

            ViewBag.ID_User = ID_User;
            ViewBag.TotalElectricity = totalElectricity;
            ViewBag.TotalWater = totalWater;
            ViewBag.TotalAdditionalCharges = totalAdditionalCharges;
            ViewBag.TotalAmountDue = totalAmountDue;
            ViewBag.TotalRent = query.Sum(rb => rb.Price);
            ViewBag.TotalPaid = totalPaid;

            ViewBag.TotalDebt = totalAmountDue - totalPaid;
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

        public ActionResult detailsbill(int id)
        {
            var bill = db.RoomBills.SingleOrDefault(b => b.Bill_ID == id);
            if (bill == null)
            {
                return HttpNotFound();
            }
            return View(bill);
        }
        [HttpPost]
        public ActionResult UpdatePayment(decimal paymentAmount, int ID_User)
        {
            using (var context = new DataClasses1DataContext())
            {
                var room = context.Rooms.SingleOrDefault(r => r.ID_user == ID_User);

                if (room != null)   
                {
                    room.money_paid += paymentAmount;

                    context.SubmitChanges();
                }
            }

            return RedirectToAction("rent_checkin", new { ID_User = ID_User });
        }

        public ActionResult Profile_user(int ID_user)
        {
            using (var db = new DataClasses1DataContext())
            {
                var user = Session["User"] as _2home.ViewModels.User;

                if (user == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                ViewBag.Fullname = user.fullname;
                ViewBag.Email = user.Email;
                ViewBag.PhoneNumber = user.PhoneNumber;
                ViewBag.Gender = user.gender;
                var motels = from m in db.Motels
                             where m.ID_user == user.ID_user
                             select new _2home.ViewModels.Motel
                             {
                                 MotelName = m.Name_motel,
                                 Location = m.location,
                                 Price = m.price,
                                 Motel_ID = m.Motel_ID
                             };

                var rooms = from r in db.Rooms
                            join m in db.Motels on r.Motel_ID equals m.Motel_ID
                            where m.ID_user == user.ID_user
                            select new _2home.ViewModels.Motel
                            {
                                room_ID = r.room_ID,
                                Room_Status = r.Room_Status,
                                fullname = (from u in db.users where u.ID_user == r.ID_user select u.fullname).FirstOrDefault()
                            };

                var motelList = motels.ToList();
                var roomList = rooms.ToList();

                var model = Tuple.Create(motelList, roomList);
                return View(model);
            }
        }


        [HttpGet]
        public ActionResult JoinRental()
        {
            return View();
        }

        [HttpPost]
        public ActionResult JoinRental(string searchString, int? size, int? page)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập mã phòng trọ.";
                ViewBag.SearchResult = null;
                return View();
            }
            var parts = searchString.Split('r');
            if (parts.Length == 2 && int.TryParse(parts[0], out int motelId) && int.TryParse(parts[1], out int roomId))
            {
                var result = (from r in db.Rooms
                              join m in db.Motels on r.Motel_ID equals m.Motel_ID
                              where r.Motel_ID == motelId && r.room_ID == roomId
                              select new index_Viewmodel
                              {
                                  Motel_ID = r.Motel_ID,
                                  MotelName = m.Name_motel,
                                  Location = m.location,
                                  Price = m.price,
                                  room_ID = r.room_ID,
                              });

                if (result.Any())
                {
                    ViewBag.SearchResult = result.ToList();
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

                    return View(result.ToPagedList(pageNumber, pageSize));
                }
                else
                {
                    ViewBag.ErrorMessage = "Không tìm thấy kết quả phù hợp.";
                    ViewBag.SearchResult = null;
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Định dạng mã không đúng. Vui lòng nhập theo mẫu MotelIDrRoomID.";
                ViewBag.SearchResult = null;
            }



            return View();
        }

        [HttpGet]
        public ActionResult Join(int? Motel_ID, int? Room_ID, decimal? Deposit)
        {
            if (Motel_ID.HasValue && Room_ID.HasValue)
            {
                var motel = db.Motels.FirstOrDefault(m => m.Motel_ID == Motel_ID.Value);
                if (motel != null)
                {

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
                            Content = "Xin vào phòng trọ với mã phòng: " + Room_ID.Value,
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


        public ActionResult TopUp()
        {
            var userlogin = Session["User"] as _2home.ViewModels.User;
            if (userlogin == null)
            {
                return RedirectToAction("Login");
            }

           
                string bank = "Vietcombank";
                string qrUrl = $"https://qr.sepay.vn/img?acc=1026787663&bank={bank}&des={userlogin.ID_user}";
                ViewBag.QrUrl = qrUrl;
                ViewBag.Blance = userlogin.blance;
                ViewBag.UserName = userlogin.fullname;
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
        [HttpGet]
        public ActionResult ConfirmPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ConfirmPassword(string password)
        {
            var user = Session["User"] as _2home.ViewModels.User;
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (user.password == password)
            {
                return RedirectToAction("Settings");
            }
            else
            {
                ViewBag.ErrorMessage = "Mật khẩu không chính xác.";
                return View();
            }
        }
        public ActionResult Settings()
        {
            var user = Session["User"] as _2home.ViewModels.User;
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpPost]
        public ActionResult UpdateSettings(_2home.ViewModels.User model, string password, string password1, string password2)
        {
            var user = Session["User"] as _2home.ViewModels.User;
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            bool isPasswordChangeRequested = !string.IsNullOrEmpty(password) ||
                                             !string.IsNullOrEmpty(password1) ||
                                             !string.IsNullOrEmpty(password2);

            if (isPasswordChangeRequested)
            {
                if (password1 != password2)
                {
                    ViewBag.PasswordError = "Mật khẩu mới và mật khẩu xác nhận không khớp.";
                    return View("Settings", model);
                }

                if (password != user.password)
                {
                    ViewBag.PasswordError = "Mật khẩu cũ không đúng.";
                    return View("Settings", model);
                }

                user.password = password1;
            }

            using (var db = new DataClasses1DataContext())
            {
                var userInDb = db.users.SingleOrDefault(u => u.ID_user == user.ID_user);
                if (userInDb != null)
                {
                    bool isUpdated = false;

                    if (model.fullname != userInDb.fullname)
                    {
                        userInDb.fullname = model.fullname;
                        user.fullname = model.fullname;
                        isUpdated = true;
                    }

                    if (model.Email != userInDb.Email)
                    {
                        userInDb.Email = model.Email;
                        user.Email = model.Email;
                        isUpdated = true;
                    }

                    if (model.PhoneNumber != userInDb.PhoneNumber)
                    {
                        userInDb.PhoneNumber = model.PhoneNumber;
                        user.PhoneNumber = model.PhoneNumber;
                        isUpdated = true;
                    }

                    if (model.gender != userInDb.gender)
                    {
                        userInDb.gender = model.gender;
                        user.gender = model.gender;
                        isUpdated = true;
                    }

                    if (isPasswordChangeRequested && !string.IsNullOrEmpty(password1))
                    {
                        userInDb.password = password1;
                        user.password = password1;
                        isUpdated = true;
                    }

                    db.SubmitChanges();

                    if (isUpdated)
                    {
                        ViewBag.Message = "Cập nhật thông tin thành công!";
                    }
                    else
                    {
                        ViewBag.Message = "Không có thay đổi nào.";
                    }
                }
                else
                {
                    ViewBag.Message = "Không tìm thấy người dùng.";
                }
            }

            return View("Settings", model);
        }

        public ActionResult loaded()
        {
            var user = Session["User"] as _2home.ViewModels.User;
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            var dbUser = db.users.FirstOrDefault(u => u.ID_user == user.ID_user);
            var model = new _2home.ViewModels.User
            {
                fullname = dbUser.fullname,
                Email = dbUser.Email,
                PhoneNumber = dbUser.PhoneNumber,
            };
            return View(model);
        }
        [HttpPost]
        public ActionResult loaded(int amount)
        {
            var user = Session["User"] as _2home.ViewModels.User;
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (amount <= 0)
            {
                ModelState.AddModelError("", "Số tiền phải lớn hơn 0");
                return View(user);
            }

            using (var db = new DataClasses1DataContext())
            {
                var currentUser = db.users.SingleOrDefault(u => u.ID_user == user.ID_user);
                if (currentUser != null)
                {
                    string transactionId = Guid.NewGuid().ToString();

                    var paymentHistory = new PaymentHistory
                    {
                        UserID = user.ID_user,
                        Amount = amount,
                        PaymentDate = DateTime.Now,
                        TransactionID = transactionId,
                        Status = "Hoàn thành" 
                    };

                    db.PaymentHistories.InsertOnSubmit(paymentHistory);
                    db.SubmitChanges();
                    currentUser.blance += amount;
                    db.SubmitChanges();
                    ViewBag.TransactionID = transactionId;
                    user.blance = currentUser.blance;
                    Session["User"] = user;
                }
            }
            ViewBag.Blance = user.blance;
            @ViewBag.UserName = user.fullname;
            TempData["Message"] = "Nạp tiền thành công!";
            return View("TopUp", user.ID_user);
        }


public ActionResult history(int? page)
    {
        var user = Session["User"] as _2home.ViewModels.User;
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        int pageSize = 10;
        int pageNumber = (page ?? 1);

        var histories = from ph in db.PaymentHistories
                        join u in db.users on ph.UserID equals u.ID_user
                        where ph.UserID == user.ID_user
                        select new _2home.ViewModels.pay
                        {
                            PaymentID = ph.PaymentID,
                            UserName = u.fullname, 
                            Amount = ph.Amount,
                            PaymentDate = ph.PaymentDate,
                            TransactionID = ph.TransactionID,
                            Status = ph.Status
                        };

        return View(histories.ToPagedList(pageNumber, pageSize));
    }
        public ActionResult AboutUs()
        {
            
            return View();
        }
        public ActionResult AllHistory(int? page)
        {
            var histories = from ph in db.PaymentHistories
                            join u in db.users on ph.UserID equals u.ID_user
                            select new _2home.ViewModels.pay
                            {
                                PaymentID = ph.PaymentID,
                                UserName = u.fullname, 
                                Amount = ph.Amount,
                                PaymentDate = ph.PaymentDate,
                                TransactionID = ph.TransactionID,
                                Status = ph.Status
                            };

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var monthlyRevenue = histories
                .Where(h => h.PaymentDate.HasValue && h.PaymentDate.Value.Month == currentMonth && h.PaymentDate.Value.Year == currentYear)
                .Sum(h => h.Amount);
            var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var previousMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;
            var previousMonthRevenue = histories
                .Where(h => h.PaymentDate.HasValue && h.PaymentDate.Value.Month == previousMonth && h.PaymentDate.Value.Year == previousMonthYear)
                .Sum(h => h.Amount);

            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.PreviousMonthRevenue = previousMonthRevenue;


            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(histories.ToPagedList(pageNumber, pageSize));
        }
    }
}