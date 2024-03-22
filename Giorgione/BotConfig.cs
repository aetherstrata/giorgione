using System.Text.Json.Serialization;

namespace Giorgione;

public sealed class BotConfig
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }

    [JsonPropertyName("guild_id")]
    public required ulong GuildId { get; init; }

    [JsonPropertyName("starboard_id")]
    public required ulong StarboardId { get; init; }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(BotConfig))]
public sealed partial class JsonContext : JsonSerializerContext;
