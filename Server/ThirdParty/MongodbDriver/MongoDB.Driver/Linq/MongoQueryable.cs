/* Copyright 2015-2016 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Extension for <see cref="IMongoQueryable" />.
    /// </summary>
    public static class MongoQueryable
    {
        /// <summary>
        /// Determines whether a sequence contains any elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence to check for being empty.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// true if the source sequence contains any elements; otherwise, false.
        /// </returns>
        public static Task<bool> AnyAsync<TSource>(this IMongoQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<bool>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, bool>(Queryable.Any, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence whose elements to test for a condition.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// true if any elements in the source sequence pass the test in the specified predicate; otherwise, false.
        /// </returns>
        public static Task<bool> AnyAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<bool>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, bool>(Queryable.Any, source, predicate),
                    source.Expression,
                    Expression.Quote(predicate)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="Decimal"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The average of the values in the sequence.</returns>
        public static Task<decimal> AverageAsync(this IMongoQueryable<decimal> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal>, decimal>(Queryable.Average, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="Nullable{Decimal}"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The average of the values in the sequence.</returns>
        public static Task<decimal?> AverageAsync(this IMongoQueryable<decimal?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal?>, decimal?>(Queryable.Average, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="Double"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The average of the values in the sequence.</returns>
        public static Task<double> AverageAsync(this IMongoQueryable<double> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double>, double>(Queryable.Average, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="Nullable{Double}"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The average of the values in the sequence.</returns>
        public static Task<double?> AverageAsync(this IMongoQueryable<double?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double?>, double?>(Queryable.Average, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="Single"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The average of the values in the sequence.</returns>
        public static Task<float> AverageAsync(this IMongoQueryable<float> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float>, float>(Queryable.Average, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="Nullable{Single}"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The average of the values in the sequence.</returns>
        public static Task<float?> AverageAsync(this IMongoQueryable<float?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float?>, float?>(Queryable.Average, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="Int32"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The average of the values in the sequence.</returns>
        public static Task<double> AverageAsync(this IMongoQueryable<int> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int>, double>(Queryable.Average, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="Nullable{Int32}"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The average of the values in the sequence.</returns>
        public static Task<double?> AverageAsync(this IMongoQueryable<int?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int?>, double?>(Queryable.Average, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="Int64"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The average of the values in the sequence.</returns>
        public static Task<double> AverageAsync(this IMongoQueryable<long> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long>, double>(Queryable.Average, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="Nullable{Int64}"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The average of the values in the sequence.</returns>
        public static Task<double?> AverageAsync(this IMongoQueryable<long?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long?>, double?>(Queryable.Average, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of the sequence of <see cref="Decimal" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The average of the projected values.
        /// </returns>
        public static Task<decimal> AverageAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal>>, decimal>(Queryable.Average, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of the sequence of <see cref="Nullable{Decimal}" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The average of the projected values.
        /// </returns>
        public static Task<decimal?> AverageAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal?>>, decimal?>(Queryable.Average, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of the sequence of <see cref="Double" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The average of the projected values.
        /// </returns>
        public static Task<double> AverageAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double>>, double>(Queryable.Average, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of the sequence of <see cref="Nullable{Double}" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The average of the projected values.
        /// </returns>
        public static Task<double?> AverageAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double?>>, double?>(Queryable.Average, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of the sequence of <see cref="Single" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The average of the projected values.
        /// </returns>
        public static Task<float> AverageAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float>>, float>(Queryable.Average, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of the sequence of <see cref="Nullable{Single}" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The average of the projected values.
        /// </returns>
        public static Task<float?> AverageAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float?>>, float?>(Queryable.Average, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of the sequence of <see cref="Int32" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The average of the projected values.
        /// </returns>
        public static Task<double> AverageAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int>>, double>(Queryable.Average, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of the sequence of <see cref="Nullable{Int32}" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The average of the projected values.
        /// </returns>
        public static Task<double?> AverageAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int?>>, double?>(Queryable.Average, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of the sequence of <see cref="Int64" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The average of the projected values.
        /// </returns>
        public static Task<double> AverageAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long>>, double>(Queryable.Average, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the average of the sequence of <see cref="Nullable{Int64}" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The average of the projected values.
        /// </returns>
        public static Task<double?> AverageAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long?>>, double?>(Queryable.Average, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Returns the number of elements in a sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">The <see cref="IMongoQueryable{TSource}" /> that contains the elements to be counted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of elements in the input sequence.
        /// </returns>
        public static Task<int> CountAsync<TSource>(this IMongoQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<int>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, int>(Queryable.Count, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Returns the number of elements in the specified sequence that satisfies a condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}" /> that contains the elements to be counted.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of elements in the sequence that satisfies the condition in the predicate function.
        /// </returns>
        public static Task<int> CountAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<int>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, int>(Queryable.Count, source, predicate),
                    source.Expression,
                    Expression.Quote(predicate)),
                cancellationToken);
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using the default equality comparer to compare values.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">The <see cref="IMongoQueryable{TSource}" /> to remove duplicates from.</param>
        /// <returns>
        /// An <see cref="IMongoQueryable{TSource}" /> that contains distinct elements from <paramref name="source" />.
        /// </returns>
        public static IMongoQueryable<TSource> Distinct<TSource>(this IMongoQueryable<TSource> source)
        {
            return (IMongoQueryable<TSource>)Queryable.Distinct(source);
        }

        /// <summary>
        /// Returns the first element of a sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">The <see cref="IMongoQueryable{TSource}" /> to return the first element of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The first element in <paramref name="source" />.
        /// </returns>
        public static Task<TSource> FirstAsync<TSource>(this IMongoQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, TSource>(Queryable.First, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Returns the first element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}" /> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The first element in <paramref name="source" /> that passes the test in <paramref name="predicate" />.
        /// </returns>
        public static Task<TSource> FirstAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, TSource>(Queryable.First, source, predicate),
                    source.Expression,
                    Expression.Quote(predicate)),
                cancellationToken);
        }

        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">The <see cref="IMongoQueryable{TSource}" /> to return the first element of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// default(<typeparamref name="TSource" />) if <paramref name="source" /> is empty; otherwise, the first element in <paramref name="source" />.
        /// </returns>
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IMongoQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, TSource>(Queryable.FirstOrDefault, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Returns the first element of a sequence that satisfies a specified condition or a default value if no such element is found.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}" /> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// default(<typeparamref name="TSource" />) if <paramref name="source" /> is empty or if no element passes the test specified by <paramref name="predicate" />; otherwise, the first element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.
        /// </returns>
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, TSource>(Queryable.FirstOrDefault, source, predicate),
                    source.Expression,
                    Expression.Quote(predicate)),
                cancellationToken);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in keySelector.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns>
        /// An <see cref="IMongoQueryable{T}" /> that has a type argument of <see cref="IGrouping{TKey, TSource}"/> 
        /// and where each <see cref="IGrouping{TKey, TSource}"/> object contains a sequence of objects 
        /// and a key.
        /// </returns>
        public static IMongoQueryable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IMongoQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return (IMongoQueryable<IGrouping<TKey, TSource>>)Queryable.GroupBy(source, keySelector);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
        /// and creates a result value from each group and its key.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in keySelector.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by resultSelector.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <returns>
        /// An <see cref="IMongoQueryable{T}" /> that has a type argument of TResult and where
        /// each element represents a projection over a group and its key.
        /// </returns>
        public static IMongoQueryable<TResult> GroupBy<TSource, TKey, TResult>(this IMongoQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TKey, IEnumerable<TSource>, TResult>> resultSelector)
        {
            return (IMongoQueryable<TResult>)Queryable.GroupBy(source, keySelector, resultSelector);
        }

        /// <summary>
        /// Correlates the elements of two sequences based on key equality and groups the results.
        /// </summary>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <returns>
        /// An <see cref="IMongoQueryable{TResult}" /> that contains elements of type <typeparamref name="TResult" /> obtained by performing a grouped join on two sequences.
        /// </returns>
        public static IMongoQueryable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IMongoQueryable<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector)
        {
            return (IMongoQueryable<TResult>)Queryable.GroupJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        /// <summary>
        /// Correlates the elements of two sequences based on key equality and groups the results.
        /// </summary>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The collection to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <returns>
        /// An <see cref="IMongoQueryable{TResult}" /> that contains elements of type <typeparamref name="TResult" /> obtained by performing a grouped join on two sequences.
        /// </returns>
        public static IMongoQueryable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IMongoQueryable<TOuter> outer, IMongoCollection<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector)
        {
            return GroupJoin(outer, inner.AsQueryable(), outerKeySelector, innerKeySelector, resultSelector);
        }

        /// <summary>
        /// Correlates the elements of two sequences based on matching keys.
        /// </summary>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable`1" /> that has elements of type <typeparamref name="TResult" /> obtained by performing an inner join on two sequences.
        /// </returns>
        public static IMongoQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(this IMongoQueryable<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            return (IMongoQueryable<TResult>)Queryable.Join(outer, inner.AsQueryable(), outerKeySelector, innerKeySelector, resultSelector);
        }

        /// <summary>
        /// Correlates the elements of two sequences based on matching keys.
        /// </summary>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable`1" /> that has elements of type <typeparamref name="TResult" /> obtained by performing an inner join on two sequences.
        /// </returns>
        public static IMongoQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(this IMongoQueryable<TOuter> outer, IMongoCollection<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            return Join(outer, inner.AsQueryable(), outerKeySelector, innerKeySelector, resultSelector);
        }

        /// <summary>
        /// Returns the number of elements in a sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">The <see cref="IMongoQueryable{TSource}" /> that contains the elements to be counted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of elements in the input sequence.
        /// </returns>
        public static Task<long> LongCountAsync<TSource>(this IMongoQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<long>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, long>(Queryable.LongCount, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Returns the number of elements in the specified sequence that satisfies a condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}" /> that contains the elements to be counted.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of elements in the sequence that satisfies the condition in the predicate function.
        /// </returns>
        public static Task<long> LongCountAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<long>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, long>(Queryable.LongCount, source, predicate),
                    source.Expression,
                    Expression.Quote(predicate)),
                cancellationToken);
        }

        /// <summary>
        /// Returns the maximum value in a generic <see cref="IMongoQueryable{TSource}" />.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The maximum value in the sequence.
        /// </returns>
        public static Task<TSource> MaxAsync<TSource>(this IMongoQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, TSource>(Queryable.Max, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Invokes a projection function on each element of a generic <see cref="IMongoQueryable{TSource}" /> and returns the maximum resulting value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the function represented by <paramref name="selector" />.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The maximum value in the sequence.
        /// </returns>
        public static Task<TResult> MaxAsync<TSource, TResult>(this IMongoQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<TResult>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, TResult>>, TResult>(Queryable.Max, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Returns the minimum value in a generic <see cref="IMongoQueryable{TSource}" />.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to determine the minimum of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The minimum value in the sequence.
        /// </returns>
        public static Task<TSource> MinAsync<TSource>(this IMongoQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, TSource>(Queryable.Min, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Invokes a projection function on each element of a generic <see cref="IMongoQueryable{TSource}" /> and returns the minimum resulting value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the function represented by <paramref name="selector" />.</typeparam>
        /// <param name="source">A sequence of values to determine the minimum of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The minimum value in the sequence.
        /// </returns>
        public static Task<TResult> MinAsync<TSource, TResult>(this IMongoQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<TResult>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, TResult>>, TResult>(Queryable.Min, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Filters the elements of an <see cref="IMongoQueryable" /> based on a specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to filter the elements of the sequence on.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable" /> whose elements to filter.</param>
        /// <returns>
        /// A collection that contains the elements from <paramref name="source" /> that have type <typeparamref name="TResult" />.
        /// </returns>
        public static IMongoQueryable<TResult> OfType<TResult>(this IMongoQueryable source)
        {
            return (IMongoQueryable<TResult>)Queryable.OfType<TResult>(source);
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by keySelector.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <returns>
        /// An <see cref="IOrderedMongoQueryable{TSource}"/> whose elements are sorted according to a key.
        /// </returns>
        public static IOrderedMongoQueryable<TSource> OrderBy<TSource, TKey>(this IMongoQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return (IOrderedMongoQueryable<TSource>)Queryable.OrderBy(source, keySelector);
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by keySelector.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <returns>
        /// An <see cref="IOrderedMongoQueryable{TSource}"/> whose elements are sorted in descending order according to a key.
        /// </returns>
        public static IOrderedMongoQueryable<TSource> OrderByDescending<TSource, TKey>(this IMongoQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return (IOrderedMongoQueryable<TSource>)Queryable.OrderByDescending(source, keySelector);
        }

        /// <summary>
        /// Returns a sample of the elements in the <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}" /> to return a sample of.</param>
        /// <param name="count">The number of elements in the sample.</param>
        /// <returns>
        /// A sample of the elements in the <paramref name="source"/>.
        /// </returns>
        public static IMongoQueryable<TSource> Sample<TSource>(this IMongoQueryable<TSource> source, long count)
        {
            return (IMongoQueryable<TSource>)source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(Sample, source, count),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Constant(count)));
        }

        /// <summary>
        /// Projects each element of a sequence into a new form by incorporating the
        /// element's index.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult"> The type of the value returned by the function represented by selector.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <returns>
        /// An <see cref="IMongoQueryable{TResult}"/> whose elements are the result of invoking a
        /// projection function on each element of source.
        /// </returns>
        public static IMongoQueryable<TResult> Select<TSource, TResult>(this IMongoQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            return (IMongoQueryable<TResult>)Queryable.Select(source, selector);
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="IEnumerable{TResult}" /> and combines the resulting sequences into one sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the sequence returned by the function represented by <paramref name="selector" />.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <returns>
        /// An <see cref="IMongoQueryable{TResult}" /> whose elements are the result of invoking a one-to-many projection function on each element of the input sequence.
        /// </returns>
        public static IMongoQueryable<TResult> SelectMany<TSource, TResult>(this IMongoQueryable<TSource> source, Expression<Func<TSource, IEnumerable<TResult>>> selector)
        {
            return (IMongoQueryable<TResult>)Queryable.SelectMany(source, selector);
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="IEnumerable{TCollection}" /> and 
        /// invokes a result selector function on each element therein. The resulting values from 
        /// each intermediate sequence are combined into a single, one-dimensional sequence and returned.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TCollection">The type of the intermediate elements collected by the function represented by <paramref name="collectionSelector" />.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="collectionSelector">A projection function to apply to each element of the input sequence.</param>
        /// <param name="resultSelector">A projection function to apply to each element of each intermediate sequence.</param>
        /// <returns>
        /// An <see cref="IMongoQueryable{TResult}" /> whose elements are the result of invoking the one-to-many projection function <paramref name="collectionSelector" /> on each element of <paramref name="source" /> and then mapping each of those sequence elements and their corresponding <paramref name="source" /> element to a result element.
        /// </returns>
        public static IMongoQueryable<TResult> SelectMany<TSource, TCollection, TResult>(this IMongoQueryable<TSource> source, Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector, Expression<Func<TSource, TCollection, TResult>> resultSelector)
        {
            return (IMongoQueryable<TResult>)Queryable.SelectMany(source, collectionSelector, resultSelector);
        }

        /// <summary>
        /// Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}" /> to return the single element of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The single element of the input sequence.
        /// </returns>
        public static Task<TSource> SingleAsync<TSource>(this IMongoQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, TSource>(Queryable.Single, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}" /> to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The single element of the input sequence that satisfies the condition in <paramref name="predicate" />.
        /// </returns>
        public static Task<TSource> SingleAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, TSource>(Queryable.Single, source, predicate),
                    source.Expression,
                    Expression.Quote(predicate)),
                cancellationToken);
        }

        /// <summary>
        /// Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}" /> to return the single element of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The single element of the input sequence, or default(<typeparamref name="TSource" />) if the sequence contains no elements.
        /// </returns>
        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IMongoQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, TSource>(Queryable.SingleOrDefault, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}" /> to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The single element of the input sequence that satisfies the condition in <paramref name="predicate" />, or default(<typeparamref name="TSource" />) if no such element is found.
        /// </returns>
        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, TSource>(Queryable.SingleOrDefault, source, predicate),
                    source.Expression,
                    Expression.Quote(predicate)),
                cancellationToken);
        }

        /// <summary>
        /// Bypasses a specified number of elements in a sequence and then returns the
        /// remaining elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}"/> to return elements from.</param>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <returns>
        /// An <see cref="IMongoQueryable{TSource}"/> that contains elements that occur after the
        /// specified index in the input sequence.
        /// </returns>
        public static IMongoQueryable<TSource> Skip<TSource>(this IMongoQueryable<TSource> source, int count)
        {
            return (IMongoQueryable<TSource>)Queryable.Skip(source, count);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double StandardDeviationPopulation(this IMongoQueryable<int> source)
        {
            return source.Provider.Execute<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<int>))));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double? StandardDeviationPopulation(this IMongoQueryable<int?> source)
        {
            return source.Provider.Execute<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<int?>))));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double StandardDeviationPopulation(this IMongoQueryable<long> source)
        {
            return source.Provider.Execute<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<long>))));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double? StandardDeviationPopulation(this IMongoQueryable<long?> source)
        {
            return source.Provider.Execute<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<long?>))));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static float StandardDeviationPopulation(this IMongoQueryable<float> source)
        {
            return source.Provider.Execute<float>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<float>))));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static float? StandardDeviationPopulation(this IMongoQueryable<float?> source)
        {
            return source.Provider.Execute<float?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<float?>))));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double StandardDeviationPopulation(this IMongoQueryable<double> source)
        {
            return source.Provider.Execute<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<double>))));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double? StandardDeviationPopulation(this IMongoQueryable<double?> source)
        {
            return source.Provider.Execute<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<double?>))));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static decimal StandardDeviationPopulation(this IMongoQueryable<decimal> source)
        {
            return source.Provider.Execute<decimal>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<decimal>))));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static decimal? StandardDeviationPopulation(this IMongoQueryable<decimal?> source)
        {
            return source.Provider.Execute<decimal?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<decimal?>))));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double StandardDeviationPopulation<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, int>> selector)
        {
            return source.Provider.Execute<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double? StandardDeviationPopulation<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, int?>> selector)
        {
            return source.Provider.Execute<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double StandardDeviationPopulation<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, long>> selector)
        {
            return source.Provider.Execute<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double? StandardDeviationPopulation<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, long?>> selector)
        {
            return source.Provider.Execute<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static float StandardDeviationPopulation<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, float>> selector)
        {
            return source.Provider.Execute<float>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static float? StandardDeviationPopulation<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, float?>> selector)
        {
            return source.Provider.Execute<float?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double StandardDeviationPopulation<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, double>> selector)
        {
            return source.Provider.Execute<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double? StandardDeviationPopulation<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, double?>> selector)
        {
            return source.Provider.Execute<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static decimal StandardDeviationPopulation<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, decimal>> selector)
        {
            return source.Provider.Execute<decimal>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static decimal? StandardDeviationPopulation<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector)
        {
            return source.Provider.Execute<decimal?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double> StandardDeviationPopulationAsync(this IMongoQueryable<int> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<int>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double?> StandardDeviationPopulationAsync(this IMongoQueryable<int?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<int?>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double> StandardDeviationPopulationAsync(this IMongoQueryable<long> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<long>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double?> StandardDeviationPopulationAsync(this IMongoQueryable<long?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<long?>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<float> StandardDeviationPopulationAsync(this IMongoQueryable<float> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<float>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<float?> StandardDeviationPopulationAsync(this IMongoQueryable<float?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<float?>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double> StandardDeviationPopulationAsync(this IMongoQueryable<double> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<double>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double?> StandardDeviationPopulationAsync(this IMongoQueryable<double?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<double?>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<decimal> StandardDeviationPopulationAsync(this IMongoQueryable<decimal> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<decimal>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<decimal?> StandardDeviationPopulationAsync(this IMongoQueryable<decimal?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<decimal?>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double> StandardDeviationPopulationAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double?> StandardDeviationPopulationAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double> StandardDeviationPopulationAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double?> StandardDeviationPopulationAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<float> StandardDeviationPopulationAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<float?> StandardDeviationPopulationAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double> StandardDeviationPopulationAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double?> StandardDeviationPopulationAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<decimal> StandardDeviationPopulationAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the population standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<decimal?> StandardDeviationPopulationAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationPopulation, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double StandardDeviationSample(this IMongoQueryable<int> source)
        {
            return source.Provider.Execute<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<int>))));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double? StandardDeviationSample(this IMongoQueryable<int?> source)
        {
            return source.Provider.Execute<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<int?>))));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double StandardDeviationSample(this IMongoQueryable<long> source)
        {
            return source.Provider.Execute<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<long>))));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double? StandardDeviationSample(this IMongoQueryable<long?> source)
        {
            return source.Provider.Execute<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<long?>))));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static float StandardDeviationSample(this IMongoQueryable<float> source)
        {
            return source.Provider.Execute<float>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<float>))));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static float? StandardDeviationSample(this IMongoQueryable<float?> source)
        {
            return source.Provider.Execute<float?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<float?>))));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double StandardDeviationSample(this IMongoQueryable<double> source)
        {
            return source.Provider.Execute<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<double>))));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double? StandardDeviationSample(this IMongoQueryable<double?> source)
        {
            return source.Provider.Execute<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<double?>))));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static decimal StandardDeviationSample(this IMongoQueryable<decimal> source)
        {
            return source.Provider.Execute<decimal>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<decimal>))));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static decimal? StandardDeviationSample(this IMongoQueryable<decimal?> source)
        {
            return source.Provider.Execute<decimal?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<decimal?>))));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double StandardDeviationSample<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, int>> selector)
        {
            return source.Provider.Execute<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double? StandardDeviationSample<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, int?>> selector)
        {
            return source.Provider.Execute<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double StandardDeviationSample<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, long>> selector)
        {
            return source.Provider.Execute<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double? StandardDeviationSample<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, long?>> selector)
        {
            return source.Provider.Execute<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static float StandardDeviationSample<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, float>> selector)
        {
            return source.Provider.Execute<float>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static float? StandardDeviationSample<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, float?>> selector)
        {
            return source.Provider.Execute<float?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double StandardDeviationSample<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, double>> selector)
        {
            return source.Provider.Execute<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static double? StandardDeviationSample<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, double?>> selector)
        {
            return source.Provider.Execute<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static decimal StandardDeviationSample<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, decimal>> selector)
        {
            return source.Provider.Execute<decimal>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static decimal? StandardDeviationSample<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector)
        {
            return source.Provider.Execute<decimal?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double> StandardDeviationSampleAsync(this IMongoQueryable<int> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<int>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double?> StandardDeviationSampleAsync(this IMongoQueryable<int?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<int?>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double> StandardDeviationSampleAsync(this IMongoQueryable<long> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<long>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double?> StandardDeviationSampleAsync(this IMongoQueryable<long?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<long?>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<float> StandardDeviationSampleAsync(this IMongoQueryable<float> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<float>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<float?> StandardDeviationSampleAsync(this IMongoQueryable<float?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<float?>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double> StandardDeviationSampleAsync(this IMongoQueryable<double> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<double>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double?> StandardDeviationSampleAsync(this IMongoQueryable<double?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<double?>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<decimal> StandardDeviationSampleAsync(this IMongoQueryable<decimal> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<decimal>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<decimal?> StandardDeviationSampleAsync(this IMongoQueryable<decimal?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<decimal?>))),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double> StandardDeviationSampleAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double?> StandardDeviationSampleAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double> StandardDeviationSampleAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double?> StandardDeviationSampleAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<float> StandardDeviationSampleAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<float?> StandardDeviationSampleAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double> StandardDeviationSampleAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<double?> StandardDeviationSampleAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<decimal> StandardDeviationSampleAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sample standard deviation of a sequence of values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to calculate the population standard deviation of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The population standard deviation of the sequence of values.
        /// </returns>
        public static Task<decimal?> StandardDeviationSampleAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo(StandardDeviationSample, source, selector),
                    Expression.Convert(source.Expression, typeof(IMongoQueryable<TSource>)),
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Decimal"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the sum of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        public static Task<decimal> SumAsync(this IMongoQueryable<decimal> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal>, decimal>(Queryable.Sum, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Nullable{Decimal}"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the sum of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        public static Task<decimal?> SumAsync(this IMongoQueryable<decimal?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal?>, decimal?>(Queryable.Sum, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Double"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the sum of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        public static Task<double> SumAsync(this IMongoQueryable<double> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double>, double>(Queryable.Sum, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Nullable{Double}"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the sum of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        public static Task<double?> SumAsync(this IMongoQueryable<double?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double?>, double?>(Queryable.Sum, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Single"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the sum of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        public static Task<float> SumAsync(this IMongoQueryable<float> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float>, float>(Queryable.Sum, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Nullable{Single}"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the sum of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        public static Task<float?> SumAsync(this IMongoQueryable<float?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float?>, float?>(Queryable.Sum, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Int32"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the sum of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        public static Task<int> SumAsync(this IMongoQueryable<int> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<int>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int>, int>(Queryable.Sum, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Nullable{Int32}"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the sum of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        public static Task<int?> SumAsync(this IMongoQueryable<int?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<int?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int?>, int?>(Queryable.Sum, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Int64"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the sum of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        public static Task<long> SumAsync(this IMongoQueryable<long> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<long>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long>, long>(Queryable.Sum, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Nullable{Int64}"/> values.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the sum of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        public static Task<long?> SumAsync(this IMongoQueryable<long?> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<long?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long?>, long?>(Queryable.Sum, source),
                    source.Expression),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="Decimal" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The sum of the projected values.
        /// </returns>
        public static Task<decimal> SumAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal>>, decimal>(Queryable.Sum, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="Nullable{Decimal}" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The sum of the projected values.
        /// </returns>
        public static Task<decimal?> SumAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal?>>, decimal?>(Queryable.Sum, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="Double" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The sum of the projected values.
        /// </returns>
        public static Task<double> SumAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double>>, double>(Queryable.Sum, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="Nullable{Double}" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The sum of the projected values.
        /// </returns>
        public static Task<double?> SumAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double?>>, double?>(Queryable.Sum, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="Single" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The sum of the projected values.
        /// </returns>
        public static Task<float> SumAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float>>, float>(Queryable.Sum, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="Nullable{Single}" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The sum of the projected values.
        /// </returns>
        public static Task<float?> SumAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float?>>, float?>(Queryable.Sum, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="Int32" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The sum of the projected values.
        /// </returns>
        public static Task<int> SumAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<int>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int>>, int>(Queryable.Sum, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="Nullable{Int32}" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The sum of the projected values.
        /// </returns>
        public static Task<int?> SumAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<int?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int?>>, int?>(Queryable.Sum, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="Int64" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The sum of the projected values.
        /// </returns>
        public static Task<long> SumAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<long>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long>>, long>(Queryable.Sum, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="Nullable{Int64}" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The sum of the projected values.
        /// </returns>
        public static Task<long?> SumAsync<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ((IMongoQueryProvider)source.Provider).ExecuteAsync<long?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long?>>, long?>(Queryable.Sum, source, selector),
                    source.Expression,
                    Expression.Quote(selector)),
                cancellationToken);
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the start of a sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>
        /// An <see cref="IMongoQueryable{TSource}"/> that contains the specified number of elements
        /// from the start of source.
        /// </returns>
        public static IMongoQueryable<TSource> Take<TSource>(this IMongoQueryable<TSource> source, int count)
        {
            return (IMongoQueryable<TSource>)Queryable.Take(source, count);
        }

        /// <summary>
        /// Performs a subsequent ordering of the elements in a sequence in ascending
        /// order according to a key.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by keySelector.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <returns>
        /// An <see cref="IOrderedMongoQueryable{TSource}"/> whose elements are sorted according to a key.
        /// </returns>
        public static IOrderedMongoQueryable<TSource> ThenBy<TSource, TKey>(this IOrderedMongoQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return (IOrderedMongoQueryable<TSource>)Queryable.ThenBy(source, keySelector);
        }

        /// <summary>
        /// Performs a subsequent ordering of the elements in a sequence in descending
        /// order according to a key.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by keySelector.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <returns>
        /// An <see cref="IOrderedMongoQueryable{TSource}"/> whose elements are sorted in descending order according to a key.
        /// </returns>
        public static IOrderedMongoQueryable<TSource> ThenByDescending<TSource, TKey>(this IOrderedMongoQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return (IOrderedMongoQueryable<TSource>)Queryable.ThenByDescending(source, keySelector);
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IMongoQueryable{TSource}"/> to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>
        /// An <see cref="IMongoQueryable{TSource}"/> that contains elements from the input sequence
        /// that satisfy the condition specified by predicate.
        /// </returns>
        public static IMongoQueryable<TSource> Where<TSource>(this IMongoQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return (IMongoQueryable<TSource>)Queryable.Where(source, predicate);
        }

        private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused)
        {
            return f.GetMethodInfo();
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2)
        {
            return f.GetMethodInfo();
        }
    }
}
