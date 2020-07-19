using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BetterTwitter
{
    public class PaginatedUser
    {
        [JsonPropertyName("users")]
        public List<User> Users { get; set; }

        [JsonPropertyName("next_cursor_str")]
        public string NextCursor { get; set; }
    }
    public class User
    {
        [JsonPropertyName("status")]
        public Status Status { get; set; }

        [JsonPropertyName("screen_name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class Status
    {
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
    }
}