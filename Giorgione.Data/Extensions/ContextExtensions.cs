// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Giorgione.Data.Extensions;

public static class ContextExtensions
{
    /// <summary>
    /// Adds the specified entity to the context if it is not being tracked locally,
    /// otherwise updates the existing entity in the local context.
    /// </summary>
    /// <typeparam name="T">The type of the entity to be added or updated. Must be a class.</typeparam>
    /// <typeparam name="TKey">The type of the key to find. Must be a struct.</typeparam>
    /// <param name="db">The <see cref="DbSet{T}"/> instance to which the entity belongs.</param>
    /// <param name="fallback">The default value to use if no entity is found in the database.</param>
    /// <param name="updateAction">The action to perform on the entity.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <remarks>
    /// This method checks if an entity with the provided key exists the database context.
    /// If the entity is not found, a new entity will be added to the context.
    /// If the entity is found, it will be updated in the context.
    /// </remarks>
    public static async Task UpsertAsync<T,TKey>(
        this DbSet<T> db,
        TKey fallback,
        Action<T> updateAction,
        CancellationToken ct = default)
        where T : class, IHasPrimaryKey<T,TKey>
        where TKey : struct, IEquatable<TKey>
    {
        var entity = await db.FirstOrDefaultAsync(x => x.Id.Equals(fallback), ct);

        EntityEntry<T> entry;

        if (entity == null)
        {
            entity = T.Create(fallback);
            updateAction(entity);
            entry = db.Add(entity);
        }
        else
        {
            updateAction(entity);
            entry = db.Update(entity);
        }

        await entry.Context.SaveChangesAsync(ct);
    }

    /// <inheritdoc cref="UpsertAsync{T,TKey}(Microsoft.EntityFrameworkCore.DbSet{T},TKey,System.Action{T},System.Threading.CancellationToken)"/>
    public static async Task UpsertAsync<T,TKey>(
        this DbSet<T> db,
        TKey fallback,
        Func<T, Task> updateAction,
        CancellationToken ct = default)
        where T : class, IHasPrimaryKey<T,TKey>
        where TKey : struct, IEquatable<TKey>
    {
        T? entity = await db.FirstOrDefaultAsync(x => x.Id.Equals(fallback), ct);

        EntityEntry<T> entry;

        if (entity == null)
        {
            entity = T.Create(fallback);
            await updateAction(entity);
            entry = db.Add(entity);
        }
        else
        {
            await updateAction(entity);
            entry = db.Update(entity);
        }

        await entry.Context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Adds the specified entity to the context if it is not being tracked locally,
    /// otherwise updates the existing entity in the local context.
    /// </summary>
    /// <typeparam name="T">The type of the entity to be added or updated. Must be a class.</typeparam>
    /// <typeparam name="TKey">The type of the key to find. Must be a struct.</typeparam>
    /// <typeparam name="TResult">The type of the result returned from the <paramref name="updateFunction"/>.</typeparam>
    /// <param name="db">The <see cref="DbSet{T}"/> instance to which the entity belongs.</param>
    /// <param name="fallback">The default value to use if no entity is found in the database.</param>
    /// <param name="updateFunction">The function to perform on the entity.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <remarks>
    /// This method checks if an entity with the provided key exists the database context.
    /// If the entity is not found, a new entity will be added to the context.
    /// If the entity is found, it will be updated in the context.
    /// </remarks>
    public static async Task<TResult> UpsertAsync<T,TKey,TResult>(
        this DbSet<T> db,
        TKey fallback,
        Func<T,TResult> updateFunction,
        CancellationToken ct = default)
        where T : class, IHasPrimaryKey<T, TKey>
        where TKey : struct, IEquatable<TKey>
    {
        var entity = await db.FirstOrDefaultAsync(x => x.Id.Equals(fallback), ct);

        TResult result;
        EntityEntry<T> entry;

        if (entity == null)
        {
            entity = T.Create(fallback);
            result = updateFunction(entity);
            entry = db.Add(entity);
        }
        else
        {
            result = updateFunction(entity);
            entry = db.Update(entity);
        }

        await entry.Context.SaveChangesAsync(ct);

        return result;
    }

    /// <inheritdoc cref="UpsertAsync{T,TKey,TResult}(Microsoft.EntityFrameworkCore.DbSet{T},TKey,System.Func{T,TResult},System.Threading.CancellationToken)"/>
    public static async Task<TResult> UpsertAsync<T,TKey,TResult>(
        this DbSet<T> db,
        TKey fallback,
        Func<T,Task<TResult>> updateFunction,
        CancellationToken ct = default)
        where T : class, IHasPrimaryKey<T,TKey>
        where TKey : struct, IEquatable<TKey>
    {
        var entity = await db.FirstOrDefaultAsync(x => x.Id.Equals(fallback), ct);

        TResult result;
        EntityEntry<T> entry;

        if (entity == null)
        {
            entity = T.Create(fallback);
            result = await updateFunction(entity);
            entry = db.Add(entity);
        }
        else
        {
            result = await updateFunction(entity);
            entry = db.Update(entity);
        }

        await entry.Context.SaveChangesAsync(ct);

        return result;
    }
}
