using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APPHospitalCore_Alberto.Models
{
    public class Country
    {
        [JsonProperty("ISO2")]
        public string CountryCod { get; set; }
        [JsonProperty("Country")]
        public string Name { get; set; }
        [JsonProperty("Slug")]
        public string Slug { get; set; }
        [JsonProperty("Province")]
        public string Province { get; set; }
        [JsonProperty("CityCode")]
        public string CityCode { get; set; }
        [JsonProperty("City")]
        public string City { get; set; }
        [JsonProperty("Lat")]
        public string Latitude { get; set; }
        [JsonProperty("Lon")]
        public string Longitude { get; set; }
        [JsonProperty("Confirmed")]
        public int Confirmed { get; set; }
        [JsonProperty("Deaths")]
        public int Deaths { get; set; }
        [JsonProperty("Recovered")]
        public int Recovered { get; set; }
        [JsonProperty("Active")]
        public int Active { get; set; }
        [JsonProperty("Date")]
        public DateTime Date { get; set; }
    }
}
