using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TestableHttpClient
{
    public class ApiClient : IApiClient
    {
        private const string BASE_PATH = "https://graph.facebook.com/";

        private readonly HttpClient _httpClient;

        public ApiClient()
            : this(new HttpClient())
        {
        }

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            if (httpClient == null) throw new ArgumentNullException("httpClient");
        }

        public async Task<object> GetAsync(string path, object parameters = null)
        {
            var url = new StringBuilder(BASE_PATH);

            if (path == null)
            {
                path = string.Empty;
            }

            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            url.Append(path);
            url.Append("?");

            var dict = parameters as IDictionary<string, object>;

            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    if (kvp.Value != null)
                    {
                        url.AppendFormat("{0}={1}&", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value.ToString()));
                    }
                }
            }

            url.Length--;

            var request = new HttpRequestMessage(HttpMethod.Get, url.ToString());

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            return SimpleJson.DeserializeObject(responseString);
        }
    }
}