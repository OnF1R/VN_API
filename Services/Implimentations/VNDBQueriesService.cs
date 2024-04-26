using Newtonsoft.Json;
using System.Text;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Services.Implimentations
{
    public class VNDBQueriesService : IVNDBQueriesService
    {
        private const string API_VN_URL = "https://api.vndb.org/kana/vn";
        private const string API_TAG_URL = "https://api.vndb.org/kana/tag";

        public async Task<VNDBQueryResult<VNDBResult>> GetRating(string id)
        {
            string query = 
                "{\"filters\": [\"id\", \"=\", \"" + id + "\"], " +
                "\"fields\": \"rating, votecount, length_minutes\"}";
            string data = "";
            using (var client = new HttpClient())
            {
                try
                {
                    HttpContent content = new StringContent(query, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(API_VN_URL, content);

                    if (response.IsSuccessStatusCode)
                    {
                        data = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }

            return JsonConvert.DeserializeObject<VNDBQueryResult<VNDBResult>>(data);
        }

        public async Task<VNDBQueryResult<VNDBResult>> GetRating(List<string> ids)
        {
            string firstHalfQuery = "{\"filters\": [\"or\", ";

            for (int i = 0; i < ids.Count; i++)
            {
                if (i != ids.Count - 1)
                {
                    firstHalfQuery += "[\"id\", \"=\", \"" + ids[i] + "\"], ";
                }
                else
                {
                    firstHalfQuery += "[\"id\", \"=\", \"" + ids[i] + "\"]";
                }
            }

            firstHalfQuery += "], ";

            string secondHaflQuery = "\"fields\": \"rating, votecount, length_minutes\"}";
            string query = firstHalfQuery + secondHaflQuery;
            string data = "";
            using (var client = new HttpClient())
            {
                try
                {
                    HttpContent content = new StringContent(query, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(API_VN_URL, content);

                    if (response.IsSuccessStatusCode)
                    {
                        data = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }

            return JsonConvert.DeserializeObject<VNDBQueryResult<VNDBResult>>(data);
        }

        public async Task<VNDBQueryResult<VNDBTag>> GetTags(string initialId, bool isMore = true)
        {
            string query;

            if (isMore)
            {
                query =
                    "{\"filters\": [\"id\", \">\", \"" + initialId + "\"], " +
                    "\"fields\": \"id, name, category, applicable, searchable\", " +
                    "\"results\": \"100\"}";
            }
            else
            {
                query =
                    "{\"fields\": \"id, name, category, applicable, searchable\", " +
                    "\"results\": \"100\"}";
            }

            string data = "";
            using (var client = new HttpClient())
            {
                try
                {
                    HttpContent content = new StringContent(query, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(API_TAG_URL, content);

                    if (response.IsSuccessStatusCode)
                    {
                        data = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }

            return JsonConvert.DeserializeObject<VNDBQueryResult<VNDBTag>>(data);
        }

        public async Task<VNDBQueryResult<VNDBResult>> GetVisualNovelTags(string id)
        {
            string query = 
                "{\"filters\": [\"id\", \"=\", \"" + id + "\"], " +
                "\"fields\": \"id, name, tags.id, tags.name, tags.category, tags.spoiler, tags.applicable\"}";
            string data = "";
            using (var client = new HttpClient())
            {
                try
                {
                    HttpContent content = new StringContent(query, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(API_VN_URL, content);

                    if (response.IsSuccessStatusCode)
                    {
                        data = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }

            return JsonConvert.DeserializeObject<VNDBQueryResult<VNDBResult>>(data);
        }

        public async Task<VNDBQueryResult<VNDBResult>> SearchOnVNDB(string search, string sort = "searchrank")
        {
            string query = "{\"filters\": [\"search\", \"=\", \"" + search + "\"], \"fields\": \"title, olang, released, rating, votecount, length, developers.name\", \"sort\": \"" + sort + "\"}";
            string data = "";
            using (var client = new HttpClient())
            {
                try
                {
                    HttpContent content = new StringContent(query, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(API_VN_URL, content);

                    if (response.IsSuccessStatusCode)
                    {
                        data = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }

            return JsonConvert.DeserializeObject<VNDBQueryResult<VNDBResult>>(data);
        }
    }
}
