// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Api.AnimeWorld;

public abstract record EpisodeCount;

public sealed record CountNotAvailable : EpisodeCount;

public sealed record CountAvailable(int Count) : EpisodeCount;
