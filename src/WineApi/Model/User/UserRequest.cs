using System.Text.Json.Serialization;

namespace WineApi.Model.User
{
    public class UserRequest
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }
    }
}
