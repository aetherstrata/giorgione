// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Api.AnimeWorld;

public abstract record EpisodeNumber;

public sealed record IntEpNumber(int Number) : EpisodeNumber;

public sealed record DoubleEpNumber(double Number) : EpisodeNumber;

public sealed record NumberNotAvailable : EpisodeNumber;
