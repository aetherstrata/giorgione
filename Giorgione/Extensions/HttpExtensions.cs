// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Extensions;

public static class HttpExtensions
{
    public static bool IsImage(this HttpClient http, string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = http.Send(request);

        return response.IsSuccessStatusCode && (response.Content.Headers.ContentType?.MediaType?.StartsWith("image/") ?? false);
    }
}
