using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebAPITime.HelperTools;
using WebAPITime.Models;

namespace WebAPITime.Services
{
    public class PushNotificationService
    {
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        public async Task<bool> NewRoutesNotification(List<PushNotification> tokens, string mode)
        {
            int successCount = 0;
            string title = "";
            string body = "";

            try
            {                             
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (mode == "adhoc_route")
                    {
                        title = "New Assigned Ad-Hoc Route!";
                        body = String.Format("You've been assigned to a new ad-hoc route on {0}!", tokens[i].TimeWindowStart.ToString("dd-MMM-yyyy"));
                    }
                    else
                    {
                        title = "New Assigned Routes!";
                        body = String.Format("You've been assigned to new routes on {0}!", tokens[i].TimeWindowStart.ToString("dd-MMM-yyyy"));
                    }

                    string urlRequest = "https://fcm.googleapis.com/fcm/send";
                    var client = new RestClient();
                    var request = new RestRequest(urlRequest, Method.POST);
                    request.AddHeader("Content-type", "application/json");
                    request.AddHeader("Authorization", "key=AAAA236oo0M:APA91bGycBy6mA_8w86jDNvGuYCvsj-vl7JkMSO6MyrwnTp9Z_R1FAnuG9TJWncCMutlMRPSa4i2IyD4lgPpbdF2fhYBphMR97f_7wmWdBcr5sq1t_VmNSOCLDkc4zDc_f3p3pG8pY6Q");
                    request.AddJsonBody(
                        new
                        {
                            to = tokens[i].Token,
                            notification = new
                            {
                                title = title,
                                body = body,
                                //icon = "https://app.track-asia.com/tracksgwebapi/images/stay-home-icon.png"
                            },
                            priority = "high",
                            time_to_live = 0,
                            delay_while_idle = false,
                            android = new
                            {
                                priority = "high"
                            },
                            webpush = new
                            {
                                headers = new
                                {
                                    Urgency = "high"
                                }
                            }
                        }
                    );
                    var cancellationTokenSource = new CancellationTokenSource();
                    var response = await client.ExecuteAsync(request, cancellationTokenSource.Token);
                    dynamic responseObj = JObject.Parse(response.Content);

                    if (responseObj.success == 1)
                    {
                        successCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("PushNotificationService NewRoutesNotification() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }
                       
            return successCount == tokens.Count;
        }
    }
}
