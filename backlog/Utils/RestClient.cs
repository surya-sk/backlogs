using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace backlog.Utils
{
    public static class RestClient
    {
        static readonly HttpClient client = new HttpClient();

        public static async Task<string> GetFilmResponse(string query)
        {
            string key = "k_ek9sn0t8";
            Uri imdbURL = new Uri($"https://imdb-api.com/en/API/SearchMovie/{key}/{query}"); 
            HttpResponseMessage response = await client.GetAsync(imdbURL);
            if(response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        public static async Task<string> GetFilmDataResponse(string query)
        {
            string key = "k_ek9sn0t8";
            Uri imdbURL = new Uri($"https://imdb-api.com/en/API/Title/{key}/{query}");
            HttpResponseMessage response = await client.GetAsync(imdbURL);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        public static async Task<string> GetSeriesResponse(string query)
        {
            string key = "k_ek9sn0t8";
            Uri imdbURL = new Uri($"https://imdb-api.com/en/API/SearchSeries/{key}/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = imdbURL;
            HttpResponseMessage response = await client.GetAsync(query);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }
    }
}