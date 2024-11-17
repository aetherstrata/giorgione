// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Data.Models;

public sealed record SeenEpisode(int Id) : IHasPrimaryKey<SeenEpisode, int>
{
    /// <inheritdoc />
    public static SeenEpisode Create(int id) => new(id);
}
