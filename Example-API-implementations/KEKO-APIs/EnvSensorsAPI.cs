using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Azure;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;


namespace KEKO
{
    /*
    * Example implementation of BIM APIs
    * Currently demonstrated operations:
    *  - GET environment/sensors/{buildingId}
    */
    public static class EnvSensorsAPI
    {

        private static readonly HttpClient httpClient = new HttpClient();
        private static string adtServiceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static DigitalTwinsClient client;
        static ILogger log;

        static EnvSensorsAPI()
        {
            //Authenticate with Digital Twins
            var credentials = new DefaultAzureCredential();

            //Setup Digital Twin client
            client = new DigitalTwinsClient(new Uri(adtServiceUrl), credentials, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(httpClient) });
        }

        [FunctionName("GetEnvSensorsAPI")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "environment/sensors/{buildingId}")] HttpRequest req,
            string buildingId, ILogger log)
        {

            log.LogInformation("GetEnvSensorsAPI function processing a request.");

            // Set log 
            SetupLogging(log);

            //Basic query format
            string queryFormat = "SELECT Room,Sensor, Level FROM DIGITALTWINS Room JOIN Level RELATED Room.isPartOf JOIN Sensor RELATED Room.hasCapability JOIN Building RELATED Level.isPartOf where Building.$dtId = '{0}' AND IS_OF_MODEL(Room, 'dtmi:digitaltwins:rec_3_3:core:Room;1') AND IS_OF_MODEL(Sensor, 'dtmi:digitaltwins:rec_3_3:core:Sensor;1') AND IS_OF_MODEL(Level, 'dtmi:digitaltwins:rec_3_3:core:Level;1') AND IS_OF_MODEL(Building, 'dtmi:digitaltwins:rec_3_3:core:Building;1')";

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
            List<Sensor> sensors = new List<Sensor>();

            if (resultList != null)
            {
                foreach (BasicDigitalTwin item in resultList)
                {
                    log.LogInformation(JsonSerializer.Serialize(item));

                    //Get level raw text
                    string levelRawText = ((JsonElement)item.Contents["Level"]).GetRawText();

                    //Parse raw text into level object
                    var levelTwin = JsonSerializer.Deserialize<BasicDigitalTwin>(levelRawText);

                    //Get room raw text
                    string roomRawText = ((JsonElement)item.Contents["Room"]).GetRawText();

                    //Parse raw text into room object
                    var roomTwin = JsonSerializer.Deserialize<BasicDigitalTwin>(roomRawText);

                    //Get sensor raw text
                    string sensorRawText = ((JsonElement)item.Contents["Sensor"]).GetRawText();

                    //Parse raw text into sensor object
                    var sensorTwin = JsonSerializer.Deserialize<BasicDigitalTwin>(sensorRawText);

                    //Fill area data
                    Sensor sensor = new Sensor()
                    {
                        building_id = buildingId,
                        area_id = levelTwin.Id,
                        resource_id = sensorTwin.Id,
                        type = sensorTwin.Metadata.ModelId

                    };

                    //Add area to response list of areas
                    sensors.Add(sensor);
                }

            }
            log.LogInformation("Successfully responding to query");

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(sensors))
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

        public class ErrorMessage
        {
            public int statusCode { get; set; }

            public string message { get; set; }

        }
    }
}
