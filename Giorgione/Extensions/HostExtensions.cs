// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Giorgione.Extensions;

public static class HostExtensions
{
    public static T GetConfig<T>(this HostApplicationBuilder builder, string key)
    {
        return builder.Configuration.GetSection(key).Get<T>()
               ?? throw new InvalidOperationException($"Could not read the {key} configuration section");
    }
}
