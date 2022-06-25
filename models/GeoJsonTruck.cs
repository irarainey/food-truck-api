using Microsoft.Azure.Cosmos.Spatial;
using Newtonsoft.Json;

namespace food_truck_api.models
{
    public class GeoJsonTruck
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "Feature";

        [JsonProperty("properties")]
        public GeoJsonProperties Properties { get; set; }

        [JsonProperty("geometry")]
        public Point Geometry { get; set; }
    }
}
