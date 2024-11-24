using System.Text.Json.Serialization;

namespace MooovieNightTelegramBot.Model
{
    public class Movie
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("alternativeName")]
        public string AlternativeName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        public virtual List<User> Users { get; set; } = new List<User>();
    }
}
