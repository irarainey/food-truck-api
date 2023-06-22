using Newtonsoft.Json;
using System;

namespace food_truck_api.models
{
    public class TruckData
    {
        [JsonProperty("objectid")]
        public string ObjectId { get; set; }

        [JsonProperty("applicant")]
        public string Applicant { get; set; }

        [JsonProperty("facilitytype")]
        public string FacilityType { get; set; }

        [JsonProperty("locationdescription")]
        public string LocationDescription { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("block")]
        public string Block { get; set; }

        [JsonProperty("lot")]
        public string Lot { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("fooditems")]
        public string FoodItems { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("approved")]
        public DateTime? Approved { get; set; }

        [JsonProperty("expirationdate")]
        public DateTime? ExpirationDate { get; set; }
    }
}
