using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace _2home.ViewModels
{
    public class MotelViewModel
    {
        public int Motel_ID { get; set; }
        public int ID_user { get; set; }
        public string Name_motel { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public string Is_available { get; set; }
        public string Details { get; set; }
        public int Rooms { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal Deposit { get; set; }
        public decimal Debt { get; set; }
    }
}
