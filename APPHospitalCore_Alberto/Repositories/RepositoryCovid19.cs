using APPHospitalCore_Alberto.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace APPHospitalCore_Alberto.Repositories
{
    public class RepositoryCovid19 : IRepositoryCovid19
    {
        string url;
        public RepositoryCovid19()
        {
            this.url = "https://api.covid19api.com";
        }

        private async Task<T> CallAPI<T>(string request)
        {
            RestClient client = new RestClient(url);
            client.Timeout = -1;
            RestRequest restRequest = new RestRequest(request, Method.GET);
            IRestResponse response = await client.ExecuteAsync(restRequest);
            if (response.IsSuccessful)
            {
                T datos = JsonConvert.DeserializeObject<T>(response.Content);
                return (T)Convert.ChangeType(datos, typeof(T));
            }
            else return default(T);
        }


        #region Covid19

        public async Task<List<Country>> GetCountriesAsync()
        {
            string request = "/countries";
            List<Country> countries = await CallAPI<List<Country>>(request);
            if (countries != null)
            {
                countries = countries.OrderBy(c => c.Name).ToList();
            }
            return countries;
        }

        public async Task<List<Country>> GetByCountryAllStatusAsync(string slug, DateTime startDate, DateTime endDate)
        {
            if (startDate.ToShortDateString() == DateTime.Now.ToShortDateString())
            {
                startDate = startDate.AddDays(-1);
            }

            string startDateFormat = startDate.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
            string endDateFormat = endDate.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
            string request = "/country/" + slug + "?from=" + startDateFormat + "Z&to=" + endDateFormat + "Z";
            List<Country> countries = await CallAPI<List<Country>>(request);
            countries = countries
                .OrderBy(c => c.Date).ThenBy(c => c.Province)
                .Select(c => new Country
                {
                    CountryCod = c.CountryCod,
                    Name = c.Name,
                    Slug = c.Slug,
                    City = c.City,
                    CityCode = c.CityCode,
                    Date = c.Date,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    Province = c.Province,
                    Confirmed = c.Confirmed,
                    Deaths = c.Deaths,
                    Recovered = c.Recovered,
                    Active = c.Confirmed - c.Deaths - c.Recovered
                }).ToList();
            return countries;
        }
        #endregion
    }
}
