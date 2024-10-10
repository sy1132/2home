using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _2home.ViewModels
{
    public class room
    {
        public int? Room_ID { get; set; }

        public int? Motel_ID { get; set; }

        public int? ID_User { get; set; }

        public DateTime? Date_of_Issue { get; set; }

        public decimal? Electricity_Meter { get; set; }

        public decimal? Water_Meter { get; set; }

        public decimal? Previous_Water_Meter { get; set; }

        public decimal? Previous_Electricity_Usage { get; set; }
        public decimal? Water_Unit_Price { get; set; }
        public decimal? Electricity_Unit_Price { get; set; }

        public decimal? Electricity_Bill { get; set; }

        public decimal? Water_Bill { get; set; }

        public string Room_Status { get; set; }

        public decimal? Room_Rent { get; set; }

        public decimal? Additional_Charges { get; set; }


        public decimal? Total_Amount_Due { get; set; }
        public string fullname { get; set; }

        public virtual Motel Motel { get; set; }
        public virtual User User { get; set; }
        public Nullable<decimal> Price { get; set; }
    }
}