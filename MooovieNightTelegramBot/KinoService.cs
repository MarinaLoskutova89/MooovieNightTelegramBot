using Microsoft.Extensions.Configuration;
using MooovieNightTelegramBot.Model;
using System.Text.Json;

namespace MooovieNightTelegramBot
{
    public class KinoService
    {
        private readonly IConfiguration _configuration;

        public KinoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Movie> GetRandomMovie(string requestUriPart)
        {
            HttpClient client = GetHttpClient();
            string uri = $"{client.BaseAddress}/movie/random{requestUriPart}";
            HttpRequestMessage response = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
                Headers =
                {
                    { "accept", "application/json" },
                    { "X-API-KEY", _configuration["ApiKinopoisk:Token"] },
                },
            };

            using (HttpResponseMessage? request = await client.SendAsync(response))
            {
                request.EnsureSuccessStatusCode();
                string? body = await request.Content.ReadAsStringAsync();
                Movie? movieInfo = JsonSerializer.Deserialize<Movie>(body);

                return movieInfo;
            }
        }

        private HttpClient GetHttpClient()
        {
            HttpClient client = new();
            string? uri = _configuration["ApiKinopoisk:Host"];
            TimeSpan timeout = TimeSpan.FromMinutes(Convert.ToDouble(_configuration["TimeoutMin"]));

            client.BaseAddress = new Uri(uri);
            client.Timeout = timeout;

            return client;
        }
    }
}
