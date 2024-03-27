// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;

namespace Giorgione.Config;

internal sealed class DbServerConfig
{
    [JsonPropertyName("address")]
    public string Address { get; init; } = "localhost";

    [JsonPropertyName("port")]
    public int Port { get; init; } = 5432;

    [JsonPropertyName("database")]
    public required string Database { get; init; }

    [JsonPropertyName("username")]
    public required string Username { get; init; }

    [JsonPropertyName("password")]
    public required string Password { get; init; }

    public string GetConnectionString()
    {
        return $"Server={Address};Port={Port};Database={Database};User Id={Username};Password={Password};";
    }
}
