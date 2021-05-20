using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CowinAppointmentAlerts
{
    public class alertDetails
    {

        public static readonly Dictionary<string, List<string>> alertSpecs = new Dictionary<string, List<string>>() {
            {"400051", new List<string>() { "+919820278758" } },
            { "400607", new List<string>() { "+919820287993" } },
            {"400610", new List<string>() { "+919769800713" }},
            {"400601",new List<string>(){"+919892076927" } },
            {"411057",new List<string>(){"+919819187981"} },
            {"416416",new List<string>(){"+919930845506"} },
            {"400028",new List<string>(){"+919920943022"} }
        };


    }
    public class allsessions
    {
        public List<session> sessions { get; set; }
    }
    public class session
    {
        public int center_id { get; set; }
        public string name { get; set; }
        public string address { get; set; }

        public string state_name { get; set; }

        public string district_name { get; set; }

        public string block_name { get; set; }

        public int pincode { get; set; }

        public string from { get; set; }

        public string to { get; set; }

        public int lat { get; set; }

        [JsonPropertyName("long")]
        public int longitude { get; set; }

        public string fee_type { get; set; }
        public string session_id { get; set; }
        public string date { get; set; }

        public int available_capacity { get; set; }

        public int available_capacity_dose1 { get; set; }

        public int available_capacity_dose2 { get; set; }

        public string fee { get; set; }
        public int min_age_limit { get; set; }
        public string vaccine { get; set; }
        public string[] slots { get; set; }

    }
}
