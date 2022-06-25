using food_truck_api.models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Spatial;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace food_truck_api
{
    /// <summary>
    /// Mobile Food Facility Importer
    /// </summary>
    public class TruckDataImporter
    {
        /// <summary>
        /// Private instance of the HttpClient
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Private instance of the CosmosDB container
        /// </summary>
        private readonly Container _container;

        /// <summary>
        /// Private instance of the configuration manager
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor for Mobile Food Facility Importer class
        /// </summary>
        /// <param name="httpClientFactory">Http Client Factory instance</param>
        /// <param name="cosmosClient">CosmosDB client instance</param>
        /// <param name="config">Configuration manager instance</param>
        public TruckDataImporter(IHttpClientFactory httpClientFactory, CosmosClient cosmosClient, IConfiguration config)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient("truckdata");
            _container = cosmosClient.GetContainer(_config["databaseName"], _config["containerName"]);
        }

        /// <summary>
        /// Import Truck Data
        /// </summary>
        /// <param name="timer">TimerInfo object</param>
        /// <param name="log">Instance of logger</param>
        /// <returns>Async Task</returns>
        [FunctionName("ImportTruckData")]
        public async Task ImportTruckData([TimerTrigger("0 0 8 * * *")] TimerInfo timer, ILogger log)
        {
            // Attempt to get the JSON data from the public endpoint
            HttpResponseMessage response = await _httpClient.GetAsync(_httpClient.BaseAddress);

            // If that wasn't successful then log an error and drop out
            if (!response.IsSuccessStatusCode)
            {
                log.LogError($"Http request failed: {response.StatusCode} {response.ReasonPhrase}");
                return;
            }

            // If it was successful then read the contents of the response into a string
            string content = await response.Content.ReadAsStringAsync();

            // Deserialise the response contents into an array of Mobile Food Facility objects
            TruckData[] trucks = JsonConvert.DeserializeObject<TruckData[]>(content);

            // Iterate through each facility object in the array
            foreach (var truck in trucks)
            {
                // Check it has longitude and latitude information present and skip if it doesn't
                if (truck.Longitude != 0 && truck.Latitude != 0)
                {
                    // Create a GeoJson object with truck data and upsert it into the CosmosDB container
                    await _container.UpsertItemAsync(new GeoJsonTruck()
                    {
                        Id = truck.ObjectId,
                        Geometry = new Point(truck.Longitude, truck.Latitude),
                        Properties = new GeoJsonProperties()
                        {
                            Address = truck.Address,
                            Applicant = truck.Applicant,
                            Block = truck.Block,
                            Lot = truck.Lot,
                            FacilityType = truck.FacilityType,
                            Status = truck.Status,
                            FoodItems = truck.FoodItems,
                            LocationDescription = truck.LocationDescription,
                            Approved = truck.Approved,
                            ExpirationDate = truck.ExpirationDate
                        }
                    });

                    // Log successful upsert action
                    log.LogInformation($"Record {truck.ObjectId} / {truck.Applicant} / {truck.Address} upserted");
                }
            }

            // Log next scheduled run time
            log.LogInformation($"Next run scheduled for {timer.FormatNextOccurrences(1)}");
        }
    }
}
