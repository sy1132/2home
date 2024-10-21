using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _2home.ViewModels
{
    public class pic
    {
        public string MotelName { get; set; }
        public string Location { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string is_available { get; set; }
        public string Link { get; set; }
        public List<string> ImgLinks { get; set; }
        public int? ID_user { get; set; }
        public int? Motel_ID { get; set; }
        public string LandlordName { get; set; }
        public string LandlordPhone { get; set; }

    }
}