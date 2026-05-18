using System.Text.Json;
using System.Text.Json.Serialization;

public static class Json
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };

    public static readonly JsonSerializerOptions IndentedOptions = new(Options)
    {
        WriteIndented = true
    };
}
