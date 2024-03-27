// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;

namespace Giorgione.Config;

public sealed class BotConfig
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }

    [JsonPropertyName("guild_id")]
    public required ulong GuildId { get; init; }

    [JsonPropertyName("starboard_id")]
    public required ulong StarboardId { get; init; }

    [JsonPropertyName("db_server")]
    public required DbServerConfig DbServer { get; init; }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(BotConfig))]
internal sealed partial class ConfigJsonContext : JsonSerializerContext;
