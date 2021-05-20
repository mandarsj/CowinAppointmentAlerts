﻿using System;
using System.Collections.Generic;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace CowinAppointmentAlerts
{
    public class TwilioProvider
    {
        private static readonly string sId = "AC488dbc541efe70ab117fa8e780e4303d";
        private static readonly string authToken = Environment.GetEnvironmentVariable("TwilioAuthToken"); //"605e098005ce742a6d30dae145fc1946";

        public static void SendAppointmentSMS(string centers, string reciever, string vaccineNames, string pincode, string minAge,string doseNumber)
        {
            var fromNumber = new Twilio.Types.PhoneNumber("+15122706530");
            TwilioClient.Init(sId, authToken);

            MessageResource.Create(
                  body: $"For {minAge} {vaccineNames} {doseNumber}  available at {pincode} at {centers}  .Book at https://www.cowin.gov.in/home",
                  from: fromNumber,
                  to: new Twilio.Types.PhoneNumber(reciever)
               );
        }

    }
}
