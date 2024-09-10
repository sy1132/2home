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

        public string Location { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string is_available { get; set; }
    }
}