using System;
using System.Collections.Generic;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace CowinAppointmentAlerts
{
    public class TwilioProvider
    {
        private static readonly string sId =  Environment.GetEnvironmentVariable("twilioSid");
        private static readonly string authToken =Environment.GetEnvironmentVariable("TwilioAuthToken");
        private static readonly string phoneNumber = Environment.GetEnvironmentVariable("twilioPhoneNumber");


        public static void SendAppointmentSMS(string centers, string reciever, string vaccineNames, string pincode, string minAge,string doseNumber)
        {
            var fromNumber = new Twilio.Types.PhoneNumber(phoneNumber);
            TwilioClient.Init(sId, authToken);

            MessageResource.Create(
                  body: $"For {minAge} {vaccineNames} {doseNumber}  available at {pincode} .Book at https://www.cowin.gov.in/home",
                  from: fromNumber,
                  to: new Twilio.Types.PhoneNumber(reciever)
               );
        }

    }
}
