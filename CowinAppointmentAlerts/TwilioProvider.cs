﻿using System;
using System.Collections.Generic;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace CowinAppointmentAlerts
{
    public class TwilioProvider
    {
        private static readonly string sId = Environment.GetEnvironmentVariable("TwilioSid");
        private static readonly string authToken = Environment.GetEnvironmentVariable("TwilioAuthToken");

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
