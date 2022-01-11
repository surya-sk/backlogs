using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace backlog.Utils
{
    public static class RestClient
    {
        static readonly HttpClient client = new HttpClient();

        public static async Task<string> GetFilmResponse(string query)
        {
            string key = Keys.IMDB_KEY;
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
            string key = Keys.IMDB_KEY;
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
            string key = Keys.IMDB_KEY;
            Uri imdbURL = new Uri($"https://imdb-api.com/en/API/SearchSeries/{key}/{query}");
            HttpResponseMessage response = await client.GetAsync(imdbURL);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        public static async Task<string> GetSeriesDataResponse(string query)
        {
            string key = Keys.IMDB_KEY;
            Uri imdbURL = new Uri($"https://imdb-api.com/en/API/Title/{key}/{query}");
            HttpResponseMessage response = await client.GetAsync(imdbURL);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        public static async Task<string> GetGameResponse(string query)
        {
            string clientID = Keys.TWITCH_CLIENT_ID;
            string accessToken = Keys.TWITCH_ACCESS_TOKEN;
            Uri igdbURL = new Uri($"https://api.igdb.com/v4/games");
            var cl = new HttpClient();
            cl.BaseAddress = igdbURL;
            cl.DefaultRequestHeaders.Add("Client-ID", clientID);
            cl.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            cl.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await cl.PostAsync(igdbURL, new StringContent($"search \"{query}\";", Encoding.UTF8, "application/json"));
            if(response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        public static async Task<string> GetGameResult(string id)
        {
            string clientID = Keys.TWITCH_CLIENT_ID;
            string accessToken = Keys.TWITCH_ACCESS_TOKEN;
            Uri igdbURL = new Uri($"https://api.igdb.com/v4/games");
            var cl = new HttpClient();
            cl.BaseAddress = igdbURL;
            cl.DefaultRequestHeaders.Add("Client-ID", clientID);
            cl.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            cl.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await cl.PostAsync(igdbURL, new StringContent($"fields name,storyline,release_dates,cover,involved_companies; where id = {id};", Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        public static async Task<int> GetCompanyID(string id)
        {
            string clientID = Keys.TWITCH_CLIENT_ID;
            string accessToken = Keys.TWITCH_ACCESS_TOKEN;
            Uri igdbURL = new Uri($"https://api.igdb.com/v4/involved_companies");
            var cl = new HttpClient();
            cl.BaseAddress = igdbURL;
            cl.DefaultRequestHeaders.Add("Client-ID", clientID);
            cl.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            cl.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await cl.PostAsync(igdbURL, new StringContent($"fields company; where id = {id};", Encoding.UTF8, "application/json"));
            string result;
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
                var companies = JsonConvert.DeserializeObject<InvovledGameCompanies[]>(result);
                return companies[0].company;
            }
            return 0;
        }

        public static async Task<string> GetGameCompanyResponse(string id)
        {
            string clientID = Keys.TWITCH_CLIENT_ID;
            string accessToken = Keys.TWITCH_ACCESS_TOKEN;
            Uri igdbURL = new Uri($"https://api.igdb.com/v4/companies");
            var cl = new HttpClient();
            cl.BaseAddress = igdbURL;
            cl.DefaultRequestHeaders.Add("Client-ID", clientID);
            cl.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            cl.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await cl.PostAsync(igdbURL, new StringContent($"fields name; where id = {id};", Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        public static async Task<string> GetGameCover(string id)
        {
            string clientID = Keys.TWITCH_CLIENT_ID;
            string accessToken = Keys.TWITCH_ACCESS_TOKEN;
            Uri igdbURL = new Uri($"https://api.igdb.com/v4/covers");
            var cl = new HttpClient();
            cl.BaseAddress = igdbURL;
            cl.DefaultRequestHeaders.Add("Client-ID", clientID);
            cl.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            cl.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await cl.PostAsync(igdbURL, new StringContent($"fields url; where id = {id};", Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        public static async Task<string> GetGameReleaseResponse(string id)
        {
            string clientID = Keys.TWITCH_CLIENT_ID;
            string accessToken = Keys.TWITCH_ACCESS_TOKEN;
            Uri igdbURL = new Uri($"https://api.igdb.com/v4/release_dates");
            var cl = new HttpClient();
            cl.BaseAddress = igdbURL;
            cl.DefaultRequestHeaders.Add("Client-ID", clientID);
            cl.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            cl.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await cl.PostAsync(igdbURL, new StringContent($"fields date; where id = {id};", Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        public static async Task<string> GetBookResponse(string id)
        {
            Uri queryURL = new Uri($"https://www.googleapis.com/books/v1/volumes?q={id}&maxResults=30");
            HttpResponseMessage response = await client.GetAsync(queryURL);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        public static async Task<string> GetMusicResponse(string query)
        {
            string apiKey = Keys.LASTFM_KEY;
            string[] queryArr = query.Split('-');
            string artist = queryArr[0];
            string album = queryArr[1];
            Uri queryURL = new Uri($"http://ws.audioscrobbler.com/2.0/?method=album.getinfo&api_key={apiKey}&format=json&artist={artist}&album={album}");
            HttpResponseMessage response = await client.GetAsync(queryURL);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }
    }
}