using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Azure;
using System.Net.Http;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;


using System.Collections.Generic;
using System.Text.Json;
using System.Net;

namespace KEKO
{
    /*
     * Example implementation of BIM APIs
     * Currently demonstrated operations:
     *  - GET bim/areas/{buildingId}
     */
    public static class BIMAreasAPI
    {

        private static readonly HttpClient httpClient = new HttpClient();
        private static string adtServiceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static DigitalTwinsClient client;
        static ILogger log;

        static BIMAreasAPI()
        {
            //Authenticate with Digital Twins
            var credentials = new DefaultAzureCredential();

            //Setup Digital Twin client
            client = new DigitalTwinsClient(new Uri(adtServiceUrl), credentials, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(httpClient) });
        }

        [FunctionName("GetBIMAreasAPI")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get",  Route = "bim/areas/{buildingId}")] HttpRequest req,
            string buildingId, ILogger log)
        {
            
            log.LogInformation("GetBIMAreasAPI function processing a request.");

            // Set log 
            SetupLogging(log);

            //Basic query format
            string queryFormat = "SELECT Level FROM DIGITALTWINS Level JOIN Building RELATED Level.isPartOf where Building.$dtId = '{0}' AND IS_OF_MODEL(Level, 'dtmi:digitaltwins:rec_3_3:core:Level;1')  AND IS_OF_MODEL(Building, 'dtmi:digitaltwins:rec_3_3:core:Building;1')";
            
            //Populate query
            string query = String.Format(queryFormat, buildingId);

            log.LogInformation($"Submitting query: {query}...");

            List<BasicDigitalTwin> resultList = await Query(query);

            // Sending not found if response empty
            if (resultList == null || resultList.Count == 0)
            {
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent(JsonSerializer.Serialize(
                        new ErrorMessage()
                        {
                            statusCode = 404,
                            message = "Not found"
                        }
                        ))
                };

            }

            //Response list of areas
            List<Area> areas = new List<Area>();

            if (resultList != null)
            {
                foreach (BasicDigitalTwin item in resultList)
                {
                    log.LogInformation(JsonSerializer.Serialize(item));

                    //Get level raw text
                    string levelRawText = ((JsonElement)item.Contents["Level"]).GetRawText();

                    //Parse raw text into level object
                    var level = JsonSerializer.Deserialize<BasicDigitalTwin>(levelRawText);

                    //Fill area data
                    Area area = new Area()
                    {
                        name = ((JsonElement)level.Contents["name"]).GetString(),
                        area_id = level.Id,
                        building_id = buildingId,
                        area_type = level.Metadata.ModelId
                    };

                    //Add area to response list of areas
                    areas.Add(area);
                }
                    
            }
            log.LogInformation("Successfully responding to query");

            //Sending response with areas
            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(areas))
            };
        }
        private static async Task<List<BasicDigitalTwin>> Query(string query)
        {
            try
            {
                AsyncPageable<BasicDigitalTwin> queryResult = client.QueryAsync<BasicDigitalTwin>(query);
                var resultlist = new List<BasicDigitalTwin>();

                await foreach (BasicDigitalTwin item in queryResult) 
                {
                    resultlist.Add(item);
                }
                
                return resultlist;
            }
            catch (RequestFailedException e)
            {
                log.LogInformation($"RequestFailedException error {e.Status}: {e.Message}");
                return null;
            }
            catch (Exception ex)
            {
                log.LogInformation($"Exception error: {ex}");
                return null;
            }
        }


        private static void SetupLogging(ILogger logger)
        {
            log = logger;
        }

        /*
         * Error message object
         */
        public class ErrorMessage
        {
            public int statusCode { get; set; }

            public string message { get; set; }

        }
    }
}
