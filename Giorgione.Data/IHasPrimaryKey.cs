// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Data;

/// <summary>
/// An entity that has a primary key of a specific type.
/// </summary>
/// <typeparam name="T">The type that implements the interface.</typeparam>
/// <typeparam name="TKey">The type of the primary key. It must be a value type and implement <see cref="IEquatable{TKey}"/>.</typeparam>
public interface IHasPrimaryKey<out T, TKey> where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// The primary key of this entity
    /// </summary>
    public TKey Id { get; }

    /// <summary>
    /// Creates an instance of <typeparamref name="T"/> using the specified primary key.
    /// </summary>
    /// <param name="id">The primary key value to initialize the instance with.</param>
    /// <returns>A new instance of <typeparamref name="T"/>.</returns>
    abstract static T Create(TKey id);
}
