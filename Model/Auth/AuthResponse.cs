using System.Text.Json.Serialization;

public class AuthResponse {
    [JsonPropertyName("token")]
    public required string Token {get; set;}
    
    [JsonPropertyName("expires")]
    public required DateTime Expires {get; set;}
}