using System;
using System.Collections.Generic;
using System.Text;

namespace CowinAppointmentAlerts
{
    public class User
    {
        public int Id { get; set; }

        public string name { get; set; }
        public string phonenumber { get; set; }

        public string pincode { get; set; }
    }

    public class usernotification
    {
        public int userid { get; set; }
        public string name { get; set; }
        public string phonenumber { get; set; }
        public string pincode { get; set; }
    }
}
