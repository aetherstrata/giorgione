// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

namespace Giorgione.Data.Extensions;

public static class ContextExpressions
{
    /// <summary>
    /// Adds the specified entity to the context if it is not being tracked locally,
    /// otherwise updates the existing entity in the local context.
    /// </summary>
    /// <typeparam name="T">The type of the entity to be added or updated. Must be a class.</typeparam>
    /// <param name="db">The <see cref="AppDbContext"/> instance to which the entity belongs.</param>
    /// <param name="predicate">The function to test the key</param>
    /// <param name="fallback">The default value to use if no entity is found in the database.</param>
    /// <param name="updateAction">The action to perform on the entity.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <remarks>
    /// This method checks if an entity with the provided key exists the database context.
    /// If the entity is not found, a new entity will be added to the context.
    /// If the entity is found, it will be updated in the context.
    /// </remarks>
    public static async Task UpsertAsync<T>(
        this AppDbContext db,
        Expression<Func<T, bool>> predicate,
        T fallback,
        Action<T> updateAction,
        CancellationToken ct = default) where T : class
    {
        var entity = await db.Set<T>().FirstOrDefaultAsync(predicate, ct);

        if (entity == null)
        {
            updateAction(fallback);
            db.Add(fallback);
        }
        else
        {
            updateAction(entity);
            db.Update(entity);
        }

        await db.SaveChangesAsync(ct);
    }

    /// <inheritdoc cref="UpsertAsync{T}(Giorgione.Data.AppDbContext,System.Linq.Expressions.Expression{System.Func{T,bool}},T,System.Action{T},System.Threading.CancellationToken)"/>
    public static async Task UpsertAsync<T>(
        this AppDbContext db,
        Expression<Func<T, bool>> predicate,
        T fallback,
        Func<T, Task> updateAction,
        CancellationToken ct = default) where T : class
    {
        var entity = await db.Set<T>().FirstOrDefaultAsync(predicate, ct);

        if (entity == null)
        {
            await updateAction(fallback);
            db.Add(fallback);
        }
        else
        {
            await updateAction(entity);
            db.Update(entity);
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Adds the specified entity to the context if it is not being tracked locally,
    /// otherwise updates the existing entity in the local context.
    /// </summary>
    /// <typeparam name="T">The type of the entity to be added or updated. Must be a class.</typeparam>
    /// <typeparam name="TResult">The type of the result returned from the <paramref name="updateFunction"/>.</typeparam>
    /// <param name="db">The <see cref="AppDbContext"/> instance to which the entity belongs.</param>
    /// <param name="predicate">The function to test the key</param>
    /// <param name="fallback">The default value to use if no entity is found in the database.</param>
    /// <param name="updateFunction">The function to perform on the entity.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <remarks>
    /// This method checks if an entity with the provided key exists the database context.
    /// If the entity is not found, a new entity will be added to the context.
    /// If the entity is found, it will be updated in the context.
    /// </remarks>
    public static async Task<TResult> UpsertAsync<T,TResult>(
        this AppDbContext db,
        Expression<Func<T, bool>> predicate,
        T fallback,
        Func<T,TResult> updateFunction,
        CancellationToken ct = default) where T : class
    {
        var entity = await db.Set<T>().FirstOrDefaultAsync(predicate, ct);

        TResult result;

        if (entity == null)
        {
            result = updateFunction(fallback);
            db.Add(fallback);
        }
        else
        {
            result = updateFunction(entity);
            db.Update(entity);
        }

        await db.SaveChangesAsync(ct);

        return result;
    }

    /// <inheritdoc cref="UpsertAsync{T,TResult}(Giorgione.Data.AppDbContext,System.Linq.Expressions.Expression{System.Func{T,bool}},T,System.Func{T,TResult},System.Threading.CancellationToken)"/>
    public static async Task<TResult> UpsertAsync<T,TResult>(
        this AppDbContext db,
        Expression<Func<T, bool>> predicate,
        T fallback,
        Func<T,Task<TResult>> updateFunction,
        CancellationToken ct = default) where T : class
    {
        var entity = await db.Set<T>().FirstOrDefaultAsync(predicate, ct);

        TResult result;

        if (entity == null)
        {
            result = await updateFunction(fallback);
            db.Add(fallback);
        }
        else
        {
            result = await updateFunction(entity);
            db.Update(entity);
        }

        await db.SaveChangesAsync(ct);

        return result;
    }
}
