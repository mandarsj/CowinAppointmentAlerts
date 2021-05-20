using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Collections.Generic;

namespace CowinAppointmentAlerts
{
    public class CowinAppointmentTrigger
    {

        private static readonly string connectionString = Environment.GetEnvironmentVariable("DbConnectionString");

        //private static readonly string connectionString = "Data Source = LIN22004804\\SQLEXPRESS; Persist Security Info = False; Integrated Security = true; Initial Catalog = CowinNotifDb"; //Environment.GetEnvironmentVariable("DbConnectionString");
        private static readonly HttpClient httpClient = new HttpClient();
        [FunctionName("CowinAppointmentFn")]
        public async Task Run([TimerTrigger("0 */10 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            await Run(log);
        }

        private static async Task<Dictionary<int,List<session>>> GetSessionsForPinCodes(List<User> users)
        {
            Log($"Query sessions for all unique pincodes begin {DateTime.UtcNow}");
            Dictionary<int, List<session>> pincodeSessionsMapping = new Dictionary<int, List<session>>();
            if (users.Count > 0)
            {
                var pincodes = string.Join(",", users.Select(p => p.pincode)).Split(",").Distinct().ToList();

                foreach (var pin in pincodes)
                {
                    Log($"API Query for {pincodes.Count}");
                    int pintosend = 0;
                    if (pin.Length.Equals(6) && int.TryParse(pin, out pintosend))
                    {
                        Log($"Query session for {pintosend.ToString()}");
                        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36");

                        var appointmentTomorrow = await httpClient.GetStringAsync($"https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/findByPin?pincode={pintosend.ToString()}&date={DateTime.Now.AddDays(1).ToString("dd-MM-yyyy")}");
                        var sessions = JsonSerializer.Deserialize<allsessions>(appointmentTomorrow);

                        if (sessions.sessions.Count > 0)
                        {
                            Log($"Sessions found {pintosend}");
                            pincodeSessionsMapping.Add(pintosend, sessions.sessions);
                        }


                    }
                }
            }

            Log($"Query sessions for all unique pincodes complete {DateTime.UtcNow}");
            return pincodeSessionsMapping;
        }

        private static async Task Run(ILogger logger)
        {


            var users = await GetUsers();
            Dictionary<int, List<session>> pincodeSessions =await GetSessionsForPinCodes(users);

            foreach (var user in users)
            {

                Log($"cowin_alert api call starts {user.name}.");




                var pincodes = user.pincode.Split(',');

                foreach (var pin in pincodes)
                {
                    int pintosend = 0;
                    if (pin.Length.Equals(6) && int.TryParse(pin, out pintosend))
                    {
                        var sessions = pincodeSessions[pintosend];

                        Log("cowin_alert api call ends.");

                        //sessions.sessions.AddRange(sessionsToday.sessions);

                        if (sessions.Count > 0)
                        {

                            var validsessions = sessions.Where(p => p.available_capacity_dose1 > 0 || p.available_capacity_dose2 > 0);

                            //send sms.

                            var centernames = validsessions
                                .Where(p => (p.min_age_limit == 18 || p.min_age_limit == 45))
                                .Select(p => p.name).Distinct().ToList();

                            var dose1Available = validsessions.Where(p => p.available_capacity_dose1 > 0).Distinct().ToList();
                            var dose2Available = validsessions.Where(p => p.available_capacity_dose2 > 0).Distinct().ToList();

                            string whichDose = string.Empty;

                            if (dose1Available.Count > 0)
                                whichDose += "Dose 1";

                            if (dose2Available.Count > 0)
                            {
                                if (!string.IsNullOrWhiteSpace(whichDose))
                                {
                                    whichDose += ", Dose 2";
                                }
                                else
                                    whichDose += " Dose 2";
                            }


                            if (centernames.Count > 0)
                            {
                                if (await GetNotificationData(new usernotification() { userid = user.Id, pincode = pintosend.ToString() }) == 0)
                                {
                                    var centernamesJoin = string.Join(",", centernames);
                                    Log($"cowin_alert Appointments found for pin code {pintosend.ToString()} at {centernamesJoin}");

                                    var vaccineNames = string.Join(",", validsessions.Select(p => p.vaccine).Distinct());

                                    var agegroup = validsessions.Select(p => p.min_age_limit).Distinct();
                                    var ages = string.Join(",", agegroup.Select(p => p.ToString() + "+"));



                                    TwilioProvider.SendAppointmentSMS(centernamesJoin, user.phonenumber, vaccineNames, pintosend.ToString(), ages, whichDose);
                                    Log($"cowin_alert Notification sent for pin code {pintosend}");

                                    await SaveNotification(new usernotification() { userid = user.Id, pincode = pintosend.ToString() });
                                }
                            }

                        }
                        else Log("cowin_alert No Appointment found");
                    }
                }
            }
        }

        private static void Log(string message)
        {

            TelemetryClient client = new TelemetryClient();
            client.InstrumentationKey = "235be26c-962c-4076-8b2f-f78daff2b9ea";

            client.TrackTrace(message);
        }

        private static async Task<List<User>> GetUsers()
        {
            var connection = new SqlConnection(connectionString);

            string query = @"select Id,name,phonenumber,pincode from Users where isactive=1";

            var users = await connection.QueryAsync<User>(query);

            return users.ToList();
        }

        private static async Task SaveNotification(usernotification usernotification)
        {

            //string connectionstring = "Data Source=LIN22004804\\SQLEXPRESS;Persist Security Info=False;Integrated Security=true;Initial Catalog=CowinNotifDb";
            var connection = new SqlConnection(connectionString);

            string query = @"Insert into NotificationHistory(UserId,SentDate,pincode)values(@userId,Convert(date,GetDate()),@pincode)";

            await connection.ExecuteAsync(query, usernotification);

        }

        private static async Task<int> GetNotificationData(usernotification usernotification)
        {
            var connection = new SqlConnection(connectionString);
            string query = @"select count('x') from NotificationHistory where UserId=@userid and pincode=@pincode";

            int results = (int)await connection.ExecuteScalarAsync(query, usernotification);

            return results;
        }
    }
}
