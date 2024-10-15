// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq.Expressions;

namespace Giorgione.Data.Extensions;

public static class ExpressionExtensions
{
    /// <summary>
    /// Negate a predicate expression
    /// </summary>
    /// <param name="expr">The expression to negate</param>
    /// <typeparam name="T">The type of the object to compare</typeparam>
    /// <returns>A new expression that negates the result of the original boolean expression.</returns>
    /// <example>
    /// For an expression like <c>x => x.IsActive</c>, this method will return an expression
    /// equivalent to <c>x => !x.IsActive</c>.
    /// </example>
    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expr)
    {
        return Expression.Lambda<Func<T,bool>>(Expression.Not(expr.Body), expr.Parameters);
    }
}
