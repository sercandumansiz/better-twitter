using System.Text.Json.Serialization;

namespace BetterTwitter
{
    public class Token
    {
        [JsonPropertyName("token_type")]
        public string Type { get; set; }

        [JsonPropertyName("access_token")]
        public string Value { get; set; }
    }
}