using System;
using System.Collections.Generic;
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
        static readonly Uri baseURL = new Uri("https://en.wikipedia.org/api/rest_v1/page/summary/");

        public static async Task<string> GetResponse(string query)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = baseURL;
            HttpResponseMessage response = await client.GetAsync(query);
            if(response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }
    }
}