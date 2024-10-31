using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _2home.ViewModels
{
    public class TogetherDto
    {
        public int? ID_user { get; set; }
        public int? Motel_ID { get; set; }
        public int? room_ID { get; set; }
        public decimal? price { get; set; }
        public string location { get; set; }
        public string roomdetails { get; set; }
        public string requestdetails { get; set; }
    }
}