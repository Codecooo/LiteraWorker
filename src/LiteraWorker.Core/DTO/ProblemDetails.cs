namespace LiteraWorker.Core.DTO;

/// <summary>
/// A class for problem details for error that is returned by the API
/// </summary>
public class ProblemDetails
{
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    [System.Text.Json.Serialization.JsonPropertyName("detail")]
    [System.Text.Json.Serialization.JsonPropertyOrder(-2)]
    public string? Detail { get; set; } = "Unknown Error on Server";

    [System.Text.Json.Serialization.JsonExtensionData]
    public IDictionary<string, object?> Extensions { get; set; }

    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    [System.Text.Json.Serialization.JsonPropertyName("instance")]
    [System.Text.Json.Serialization.JsonPropertyOrder(-1)]
    public string? Instance { get; set; }

    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    [System.Text.Json.Serialization.JsonPropertyName("status")]
    [System.Text.Json.Serialization.JsonPropertyOrder(-3)]
    public int? Status { get; set; } = 500;

    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    [System.Text.Json.Serialization.JsonPropertyName("title")]
    [System.Text.Json.Serialization.JsonPropertyOrder(-4)]
    public string? Title { get; set; } = "Unknown Error";

    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    [System.Text.Json.Serialization.JsonPropertyName("type")]
    [System.Text.Json.Serialization.JsonPropertyOrder(-5)]
    public string? Type { get; set; }
}