
using System;
using System.Threading.Tasks;
using System.Threading;
using RestSharp;
using WebAPITime.HelperTools;

public class Service
{
    //https://us1.locationiq.com/v1/matrix/driving/13.388860,52.517037;13.397634,52.529407;13.428555,52.523219?annotations=distance,duration&key=ece7de2808b2c0
    private const string BaseUrl = "https://us1.locationiq.com/";
    private const string apiKey = "ece7de2808b2c0";
    private const string annotations = "duration,distance";
    private const bool steps = false;

    public async Task<IRestResponse> GetAsyncDistance(string origin, string destination)
    {
        try
        {
            //var client = new RestClient(BaseUrl);
            //var request = new RestRequest(String.Format("v1/directions/driving/{0};{1}", origin, destination))
            //    //.AddParameter("units", "meter", ParameterType.QueryString)
            //    //.AddParameter("sources", origin, ParameterType.QueryString)
            //    //.AddParameter("destinations", destination, ParameterType.QueryString)
            //    .AddParameter("key", apiKey, ParameterType.QueryString)
            //    .AddParameter("steps", steps.ToString().ToLower(), ParameterType.QueryString);
            ////.AddParameter("annotations", annotations, ParameterType.QueryString);


            //var cancellationTokenSource = new CancellationTokenSource();
            //var response = await client.ExecuteAsync(request, cancellationTokenSource.Token);

            //===============================
            string urlRequest = "https://us1.locationiq.com/v1/matrix/driving/" + origin + ";" + destination + "?annotations=distance,duration&key=" + apiKey;
            var client = new RestClient();
            var request = new RestRequest(urlRequest);
            var cancellationTokenSource = new CancellationTokenSource();
            var response = await client.ExecuteAsync(request, cancellationTokenSource.Token);

            return response;

        }
        catch (Exception ex)
        {
            Logger.LogEvent(ConfigurationManager.AppSettings["mProjName"], String.Format("Service GetAsyncDistance(): {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            return null;
        }

    }

}


