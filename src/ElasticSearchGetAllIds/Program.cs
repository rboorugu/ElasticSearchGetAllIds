using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using ElasticSearchGetAllIds.Response;
using ElasticSearchGetAllIds.Request;

namespace ElasticSearchGetAllIds
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            try
            {
                Execute().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("processed all the endpoints: press enter to close the window");
            Console.ReadLine();
        }

        public static async Task Execute()
        {
            //get all the appsettings 
            var settings = GetConfig();
            var esEndPointsToScan = settings
                .GetSection("elasticsearch:endpoints")
                .GetChildren()
                .Select(x => x.Value);
            //loop through all the end points found in configuration - dev and prod
            foreach (var endpoint in esEndPointsToScan)
            {
                //create a base payload to generate scroll id - once the scroll id is generated use the same scrollId until the ids are exhausted
                var basePayload = new RootObject
                {
                    _source = new List<string> {"UniversalId"},
                    size = Convert.ToInt32(settings["batchSize"]),
                    sort = "UniversalId",
                    query = new Query {match_all = new MatchAll()}
                };
                //setting below will keep the scroll live for how many ever seconds which are set here
                string keepScrollactiveFor = settings["keepScrollactiveFor"];
                var baseHttpContent = new StringContent(JsonConvert.SerializeObject(basePayload), Encoding.UTF8, "application/json");
                //use the string builder below to store all the universalids - which will later be saved into a textfile
                var allids = new StringBuilder();
                //counter to maintain count for showing the status on console
                int statusCounter = 0;
                using (var httpClient = new HttpClient())
                {
                    // Do the actual request and await the response with post
                    var baseHttpResponse = await httpClient.PostAsync(endpoint + "search-stack/all/_search?scroll=" + keepScrollactiveFor, baseHttpContent);
                    //if any errors will exit
                    if (baseHttpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine("Error Processing " + endpoint +  ": " + baseHttpResponse.ReasonPhrase);
                        continue;
                    }
                    // If the response contains content we want to read it!
                    var baseResponseContentString = await baseHttpResponse.Content.ReadAsStringAsync();
                    var baseResponse = JsonConvert.DeserializeObject<ScrollBase.RootObject>(baseResponseContentString);
                    if (baseResponse.hits.hits.Count > 0)
                    {
                        //add all the matches only if they are integers which will be universalids - there are some tagids also part if same index which are not ints and will be excluded in the statement below
                        foreach (var hit in baseResponse.hits.hits)
                        {
                            if (hit._id.All(Char.IsDigit))
                            {
                                allids.Append(hit._id + ",");
                            }
                        }
                        statusCounter += Convert.ToInt32(settings["batchSize"]);
                        Console.WriteLine("Processed " + endpoint + " count: " + statusCounter);
                        //hit the same url using scroll api until the matches het exhausted
                        var httpResponse = await httpClient.GetAsync(endpoint + "_search/scroll?&scroll="+ keepScrollactiveFor + "&scroll_id=" + baseResponse._scroll_id);
                        if (baseHttpResponse.StatusCode != HttpStatusCode.OK)
                            return;
                        var responseContentString = await httpResponse.Content.ReadAsStringAsync();
                        var response = JsonConvert.DeserializeObject<ScrollBase.RootObject>(responseContentString);
                        //loop will be run until all the matches are exhausted
                        while (response.hits.hits.Count > 0)
                        {
                            foreach (var hit in response.hits.hits)
                            {
                                if (hit._id.All(Char.IsDigit))
                                {
                                    allids.Append(hit._id + ",");
                                }
                            }
                            statusCounter += Convert.ToInt32(settings["batchSize"]);
                            Console.WriteLine("Processed " + endpoint + " count: " + statusCounter);
                            httpResponse = await httpClient.GetAsync(endpoint + "_search/scroll?&scroll=" + keepScrollactiveFor + "&scroll_id=" + baseResponse._scroll_id);
                            if (baseHttpResponse.StatusCode != HttpStatusCode.OK)
                                return;
                            responseContentString = await httpResponse.Content.ReadAsStringAsync();
                            response = JsonConvert.DeserializeObject<ScrollBase.RootObject>(responseContentString);
                        }
                    }
                    allids.Remove(allids.Length - 1, 1);
                    //save the ids to file
                    var file = settings["savePathFolder"] + endpoint.Replace("/", "").Replace(":", "") + ".txt";
                    using (StreamWriter sw = new StreamWriter(new FileStream(file, FileMode.Create)))
                    {
                        sw.WriteLine(allids.ToString().Remove(allids.ToString().Length - 1, 1));
                    }
                }
            }
        }

        public static IConfiguration GetConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json",
                optional: true,
                reloadOnChange: true);

            return builder.Build();
        }
    }

    public class Endpoints
    {
        public string baseEndPoint { get; set; }
        public string endPointWithIndex { get; set; }
    }
}
