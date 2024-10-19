 using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _2home.ViewModels
{
    public class index_Viewmodel
    {
        public string ImgLink { get; set; }
        public string MotelName { get; set; }
        public int? Motel_ID { get; set; }


        public string Location { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string is_available { get; set; }
        public int? ID_user { get; set; }
        public int? video_ID { get; set; }
        public string Link { get; set; }
        public string link_location { get; set; }
        public string Details { get; set; }
        public int? rooms { get; set; }
    }
}
