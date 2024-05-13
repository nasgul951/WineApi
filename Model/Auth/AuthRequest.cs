
using System.Text.Json.Serialization;

public class AuthRequest {
    [JsonPropertyName("username")]
    public required string UserName {get; set;}
 
    [JsonPropertyName("password")]
    public required string Password {get; set;}
}