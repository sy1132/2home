using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _2home.ViewModels
{
    public class pay
    {
        
            public int PaymentID { get; set; }
            public string UserName { get; set; }
            public decimal? Amount { get; set; }
            public DateTime? PaymentDate { get; set; }
            public string TransactionID { get; set; }
            public string Status { get; set; }
        
    }
}