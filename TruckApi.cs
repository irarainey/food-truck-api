using food_truck_api.models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Spatial;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace food_truck_api
{
    public class TruckApi
    {
        /// <summary>
        /// Private instance of the CosmosDB container
        /// </summary>
        private readonly Container _container;

        /// <summary>
        /// Private instance of the configuration manager
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor for Food Truck Api class
        /// </summary>
        /// <param name="cosmosClient">CosmosDB client instance</param>
        /// <param name="config">Configuration manager instance</param>
        public TruckApi(CosmosClient cosmosClient, IConfiguration config)
        {
            _config = config;
            _container = cosmosClient.GetContainer(_config["databaseName"], _config["containerName"]);
        }

        /// <summary>
        /// Get all food trucks within a specified range of the defined starting point
        /// </summary>
        /// <param name="req">Http request object</param>
        /// <param name="log">Instance of logger</param>
        /// <returns>GeoJSON result of food trucks in the specified area</returns>
        [FunctionName("GetTrucks")]
        public async IAsyncEnumerable<GeoJsonTruck> GetTrucks([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            // Attempt to get longitude and latitude from querystring but if either or both not supplied then default to 555 California Street, San Francisco
            if (!double.TryParse(req.Query["lon"], out double lon) || !double.TryParse(req.Query["lat"], out double lat))
            {
                lon = double.Parse(_config["defaultLongitude"]);
                lat = double.Parse(_config["defaultLatitude"]);
            }

            // Attempt to get maximum range from querystring but if not supplied then default to 800m (1/2 mile)
            if (!int.TryParse(req.Query["range"], out int range))
            {
                range = int.Parse(_config["defaultRange"]);
            }

            // Query CosmosDB with geospatial data to find trucks that are:
            // 1. Within the range specified at the location given
            // 2. Have a permit status of APPROVED
            // 3. Where their permit has not expired
            var trucks = _container.GetItemLinqQueryable<GeoJsonTruck>()
                                .Where(t => t.Geometry.Distance(new Point(lon, lat)) <= range
                                            && t.Properties.Status == "APPROVED"
                                            && t.Properties.ExpirationDate > DateTime.Today)
                                .ToFeedIterator();

            // Iterate through the results and return the trucks
            while (trucks.HasMoreResults)
            {
                foreach (var truck in await trucks.ReadNextAsync())
                {
                    yield return truck;
                }
            }
        }
    }
}
