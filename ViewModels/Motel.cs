namespace _2home.ViewModels
{
    public class Motel
    {
        public int ID_user { get; set; }
        public int Motel_ID { get; set; }
        public int room_ID { get; set; }
        public string fullname { get; set; }
        public string Room_Status { get; set; }
        public string MotelName { get; set; }
        public string Location { get; set; }
        public decimal? Price { get; set; }
        public decimal? Debt { get; set; }

        public string is_available { get; set; } 
        public string ImgLink { get; set; }
        public string Details { get; set; }

    }
}
