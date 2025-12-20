using System.Text.Json;

public static class Json
{
    public static readonly JsonSerializerOptions Options =
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
}
