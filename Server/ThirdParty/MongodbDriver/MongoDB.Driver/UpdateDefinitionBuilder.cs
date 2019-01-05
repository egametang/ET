/* Copyright 2010-present MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Extension methods for UpdateDefinition.
    /// </summary>
    public static class UpdateDefinitionExtensions
    {
        /// <summary>
        /// Combines an existing update with an add to set operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> AddToSet<TDocument, TItem>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument> field, TItem value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.AddToSet<TItem>(field, value));
        }

        /// <summary>
        /// Combines an existing update with an add to set operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> AddToSet<TDocument, TItem>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.AddToSet<TItem>(field, value));
        }

        /// <summary>
        /// Combines an existing update with an add to set operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> AddToSetEach<TDocument, TItem>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument> field, IEnumerable<TItem> values)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.AddToSetEach<TItem>(field, values));
        }

        /// <summary>
        /// Combines an existing update with an add to set operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> AddToSetEach<TDocument, TItem>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, IEnumerable<TItem> values)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.AddToSetEach<TItem>(field, values));
        }

        /// <summary>
        /// Combines an existing update with a bitwise and operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> BitwiseAnd<TDocument, TField>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument, TField> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.BitwiseAnd(field, value));
        }

        /// <summary>
        /// Combines an existing update with a bitwise and operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> BitwiseAnd<TDocument, TField>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, TField>> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.BitwiseAnd(field, value));
        }

        /// <summary>
        /// Combines an existing update with a bitwise or operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> BitwiseOr<TDocument, TField>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument, TField> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.BitwiseOr(field, value));
        }

        /// <summary>
        /// Combines an existing update with a bitwise or operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> BitwiseOr<TDocument, TField>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, TField>> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.BitwiseOr(field, value));
        }

        /// <summary>
        /// Combines an existing update with a bitwise xor operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> BitwiseXor<TDocument, TField>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument, TField> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.BitwiseXor(field, value));
        }

        /// <summary>
        /// Combines an existing update with a bitwise xor operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> BitwiseXor<TDocument, TField>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, TField>> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.BitwiseXor(field, value));
        }

        /// <summary>
        /// Combines an existing update with a current date operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="type">The type.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> CurrentDate<TDocument>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument> field, UpdateDefinitionCurrentDateType? type = null)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.CurrentDate(field, type));
        }

        /// <summary>
        /// Combines an existing update with a current date operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="type">The type.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> CurrentDate<TDocument>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, object>> field, UpdateDefinitionCurrentDateType? type = null)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.CurrentDate(field, type));
        }

        /// <summary>
        /// Combines an existing update with an increment operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Inc<TDocument, TField>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument, TField> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Inc(field, value));
        }

        /// <summary>
        /// Combines an existing update with an increment operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Inc<TDocument, TField>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, TField>> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Inc(field, value));
        }

        /// <summary>
        /// Combines an existing update with a max operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Max<TDocument, TField>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument, TField> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Max(field, value));
        }

        /// <summary>
        /// Combines an existing update with a max operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Max<TDocument, TField>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, TField>> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Max(field, value));
        }

        /// <summary>
        /// Combines an existing update with a min operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Min<TDocument, TField>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument, TField> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Min(field, value));
        }

        /// <summary>
        /// Combines an existing update with a min operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Min<TDocument, TField>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, TField>> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Min(field, value));
        }

        /// <summary>
        /// Combines an existing update with a multiply operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Mul<TDocument, TField>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument, TField> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Mul(field, value));
        }

        /// <summary>
        /// Combines an existing update with a multiply operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Mul<TDocument, TField>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, TField>> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Mul(field, value));
        }

        /// <summary>
        /// Combines an existing update with a pop operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> PopFirst<TDocument>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.PopFirst(field));
        }

        /// <summary>
        /// Combines an existing update with a pop operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> PopFirst<TDocument>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.PopFirst(field));
        }

        /// <summary>
        /// Combines an existing update with a pop operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> PopLast<TDocument>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.PopLast(field));
        }

        /// <summary>
        /// Combines an existing update with a pop operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> PopLast<TDocument>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.PopLast(field));
        }

        /// <summary>
        /// Combines an existing update with a pull operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Pull<TDocument, TItem>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument> field, TItem value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Pull(field, value));
        }

        /// <summary>
        /// Combines an existing update with a pull operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Pull<TDocument, TItem>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Pull(field, value));
        }

        /// <summary>
        /// Combines an existing update with a pull operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> PullAll<TDocument, TItem>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument> field, IEnumerable<TItem> values)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.PullAll(field, values));
        }

        /// <summary>
        /// Combines an existing update with a pull operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> PullAll<TDocument, TItem>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, IEnumerable<TItem> values)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.PullAll(field, values));
        }

        /// <summary>
        /// Combines an existing update with a pull operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> PullFilter<TDocument, TItem>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument> field, FilterDefinition<TItem> filter)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.PullFilter(field, filter));
        }

        /// <summary>
        /// Combines an existing update with a pull operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> PullFilter<TDocument, TItem>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, FilterDefinition<TItem> filter)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.PullFilter(field, filter));
        }

        /// <summary>
        /// Combines an existing update with a pull operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> PullFilter<TDocument, TItem>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, Expression<Func<TItem, bool>> filter)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.PullFilter(field, filter));
        }

        /// <summary>
        /// Combines an existing update with a push operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Push<TDocument, TItem>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument> field, TItem value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Push(field, value));
        }

        /// <summary>
        /// Combines an existing update with a push operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Push<TDocument, TItem>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Push(field, value));
        }

        /// <summary>
        /// Combines an existing update with a push operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <param name="slice">The slice.</param>
        /// <param name="position">The position.</param>
        /// <param name="sort">The sort.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> PushEach<TDocument, TItem>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument> field, IEnumerable<TItem> values, int? slice = null, int? position = null, SortDefinition<TItem> sort = null)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.PushEach(field, values, slice, position, sort));
        }

        /// <summary>
        /// Combines an existing update with a push operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <param name="slice">The slice.</param>
        /// <param name="position">The position.</param>
        /// <param name="sort">The sort.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> PushEach<TDocument, TItem>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, IEnumerable<TItem> values, int? slice = null, int? position = null, SortDefinition<TItem> sort = null)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.PushEach(field, values, slice, position, sort));
        }

        /// <summary>
        /// Combines an existing update with a field renaming operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="newName">The new name.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Rename<TDocument>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument> field, string newName)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Rename(field, newName));
        }

        /// <summary>
        /// Combines an existing update with a field renaming operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="newName">The new name.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Rename<TDocument>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, object>> field, string newName)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Rename(field, newName));
        }

        /// <summary>
        /// Combines an existing update with a set operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Set<TDocument, TField>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument, TField> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Set(field, value));
        }

        /// <summary>
        /// Combines an existing update with a set operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Set<TDocument, TField>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, TField>> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Set(field, value));
        }

        /// <summary>
        /// Combines an existing update with a set on insert operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> SetOnInsert<TDocument, TField>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument, TField> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.SetOnInsert(field, value));
        }

        /// <summary>
        /// Combines an existing update with a set on insert operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> SetOnInsert<TDocument, TField>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, TField>> field, TField value)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.SetOnInsert(field, value));
        }

        /// <summary>
        /// Combines an existing update with an unset operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Unset<TDocument>(this UpdateDefinition<TDocument> update, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Unset(field));
        }

        /// <summary>
        /// Combines an existing update with an unset operator.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="update">The update.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined update.
        /// </returns>
        public static UpdateDefinition<TDocument> Unset<TDocument>(this UpdateDefinition<TDocument> update, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.Update;
            return builder.Combine(update, builder.Unset(field));
        }
    }

    /// <summary>
    /// The type to use for a $currentDate operator.
    /// </summary>
    public enum UpdateDefinitionCurrentDateType
    {
        /// <summary>
        /// A date.
        /// </summary>
        Date,
        /// <summary>
        /// A timestamp.
        /// </summary>
        Timestamp
    }

    /// <summary>
    /// A builder for an <see cref="UpdateDefinition{TDocument}"/>.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class UpdateDefinitionBuilder<TDocument>
    {
        /// <summary>
        /// Creates an add to set operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>An add to set operator.</returns>
        public UpdateDefinition<TDocument> AddToSet<TItem>(FieldDefinition<TDocument> field, TItem value)
        {
            return new AddToSetUpdateDefinition<TDocument, TItem>(
                field,
                new[] { value });
        }

        /// <summary>
        /// Creates an add to set operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>An add to set operator.</returns>
        public UpdateDefinition<TDocument> AddToSet<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            return AddToSet<TItem>(new ExpressionFieldDefinition<TDocument>(field), value);
        }

        /// <summary>
        /// Creates an add to set operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <returns>An add to set operator.</returns>
        public UpdateDefinition<TDocument> AddToSetEach<TItem>(FieldDefinition<TDocument> field, IEnumerable<TItem> values)
        {
            return new AddToSetUpdateDefinition<TDocument, TItem>(field, values);
        }

        /// <summary>
        /// Creates an add to set operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <returns>An add to set operator.</returns>
        public UpdateDefinition<TDocument> AddToSetEach<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, IEnumerable<TItem> values)
        {
            return AddToSetEach(new ExpressionFieldDefinition<TDocument>(field), values);
        }

        /// <summary>
        /// Creates a bitwise and operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A bitwise and operator.</returns>
        public UpdateDefinition<TDocument> BitwiseAnd<TField>(FieldDefinition<TDocument, TField> field, TField value)
        {
            return new BitwiseOperatorUpdateDefinition<TDocument, TField>("and", field, value);
        }

        /// <summary>
        /// Creates a bitwise and operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A bitwise and operator.</returns>
        public UpdateDefinition<TDocument> BitwiseAnd<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return BitwiseAnd(new ExpressionFieldDefinition<TDocument, TField>(field), value);
        }

        /// <summary>
        /// Creates a bitwise or operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A bitwise or operator.</returns>
        public UpdateDefinition<TDocument> BitwiseOr<TField>(FieldDefinition<TDocument, TField> field, TField value)
        {
            return new BitwiseOperatorUpdateDefinition<TDocument, TField>("or", field, value);
        }

        /// <summary>
        /// Creates a bitwise or operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A bitwise or operator.</returns>
        public UpdateDefinition<TDocument> BitwiseOr<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return BitwiseOr(new ExpressionFieldDefinition<TDocument, TField>(field), value);
        }

        /// <summary>
        /// Creates a bitwise xor operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A bitwise xor operator.</returns>
        public UpdateDefinition<TDocument> BitwiseXor<TField>(FieldDefinition<TDocument, TField> field, TField value)
        {
            return new BitwiseOperatorUpdateDefinition<TDocument, TField>("xor", field, value);
        }

        /// <summary>
        /// Creates a bitwise xor operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A bitwise xor operator.</returns>
        public UpdateDefinition<TDocument> BitwiseXor<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return BitwiseXor(new ExpressionFieldDefinition<TDocument, TField>(field), value);
        }

        /// <summary>
        /// Creates a combined update.
        /// </summary>
        /// <param name="updates">The updates.</param>
        /// <returns>A combined update.</returns>
        public UpdateDefinition<TDocument> Combine(params UpdateDefinition<TDocument>[] updates)
        {
            return Combine((IEnumerable<UpdateDefinition<TDocument>>)updates);
        }

        /// <summary>
        /// Creates a combined update.
        /// </summary>
        /// <param name="updates">The updates.</param>
        /// <returns>A combined update.</returns>
        public UpdateDefinition<TDocument> Combine(IEnumerable<UpdateDefinition<TDocument>> updates)
        {
            return new CombinedUpdateDefinition<TDocument>(updates);
        }

        /// <summary>
        /// Creates a current date operator.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="type">The type.</param>
        /// <returns>A current date operator.</returns>
        public UpdateDefinition<TDocument> CurrentDate(FieldDefinition<TDocument> field, UpdateDefinitionCurrentDateType? type = null)
        {
            BsonValue value;
            if (type.HasValue)
            {
                switch (type.Value)
                {
                    case UpdateDefinitionCurrentDateType.Date:
                        value = new BsonDocument("$type", "date");
                        break;
                    case UpdateDefinitionCurrentDateType.Timestamp:
                        value = new BsonDocument("$type", "timestamp");
                        break;
                    default:
                        throw new InvalidOperationException("Unknown value for " + typeof(UpdateDefinitionCurrentDateType));
                }
            }
            else
            {
                value = true;
            }

            return new OperatorUpdateDefinition<TDocument>("$currentDate", field, value);
        }

        /// <summary>
        /// Creates a current date operator.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="type">The type.</param>
        /// <returns>A current date operator.</returns>
        public UpdateDefinition<TDocument> CurrentDate(Expression<Func<TDocument, object>> field, UpdateDefinitionCurrentDateType? type = null)
        {
            return CurrentDate(new ExpressionFieldDefinition<TDocument>(field), type);
        }

        /// <summary>
        /// Creates an increment operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>An increment operator.</returns>
        public UpdateDefinition<TDocument> Inc<TField>(FieldDefinition<TDocument, TField> field, TField value)
        {
            return new OperatorUpdateDefinition<TDocument, TField>("$inc", field, value);
        }

        /// <summary>
        /// Creates an increment operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>An increment operator.</returns>
        public UpdateDefinition<TDocument> Inc<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return Inc(new ExpressionFieldDefinition<TDocument, TField>(field), value);
        }

        /// <summary>
        /// Creates a max operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A max operator.</returns>
        public UpdateDefinition<TDocument> Max<TField>(FieldDefinition<TDocument, TField> field, TField value)
        {
            return new OperatorUpdateDefinition<TDocument, TField>("$max", field, value);
        }

        /// <summary>
        /// Creates a max operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A max operator.</returns>
        public UpdateDefinition<TDocument> Max<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return Max(new ExpressionFieldDefinition<TDocument, TField>(field), value);
        }

        /// <summary>
        /// Creates a min operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A min operator.</returns>
        public UpdateDefinition<TDocument> Min<TField>(FieldDefinition<TDocument, TField> field, TField value)
        {
            return new OperatorUpdateDefinition<TDocument, TField>("$min", field, value);
        }

        /// <summary>
        /// Creates a min operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A min operator.</returns>
        public UpdateDefinition<TDocument> Min<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return Min(new ExpressionFieldDefinition<TDocument, TField>(field), value);
        }

        /// <summary>
        /// Creates a multiply operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A multiply operator.</returns>
        public UpdateDefinition<TDocument> Mul<TField>(FieldDefinition<TDocument, TField> field, TField value)
        {
            return new OperatorUpdateDefinition<TDocument, TField>("$mul", field, value);
        }

        /// <summary>
        /// Creates a multiply operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A multiply operator.</returns>
        public UpdateDefinition<TDocument> Mul<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return Mul(new ExpressionFieldDefinition<TDocument, TField>(field), value);
        }

        /// <summary>
        /// Creates a pop operator.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A pop operator.</returns>
        public UpdateDefinition<TDocument> PopFirst(FieldDefinition<TDocument> field)
        {
            return new OperatorUpdateDefinition<TDocument>("$pop", field, -1);
        }

        /// <summary>
        /// Creates a pop first operator.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A pop first operator.</returns>
        public UpdateDefinition<TDocument> PopFirst(Expression<Func<TDocument, object>> field)
        {
            return PopFirst(new ExpressionFieldDefinition<TDocument>(field));
        }

        /// <summary>
        /// Creates a pop operator.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A pop operator.</returns>
        public UpdateDefinition<TDocument> PopLast(FieldDefinition<TDocument> field)
        {
            return new OperatorUpdateDefinition<TDocument>("$pop", field, 1);
        }

        /// <summary>
        /// Creates a pop first operator.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A pop first operator.</returns>
        public UpdateDefinition<TDocument> PopLast(Expression<Func<TDocument, object>> field)
        {
            return PopLast(new ExpressionFieldDefinition<TDocument>(field));
        }

        /// <summary>
        /// Creates a pull operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A pull operator.</returns>
        public UpdateDefinition<TDocument> Pull<TItem>(FieldDefinition<TDocument> field, TItem value)
        {
            return new PullUpdateDefinition<TDocument, TItem>(field, new[] { value });
        }

        /// <summary>
        /// Creates a pull operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A pull operator.</returns>
        public UpdateDefinition<TDocument> Pull<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            return Pull<TItem>(new ExpressionFieldDefinition<TDocument>(field), value);
        }

        /// <summary>
        /// Creates a pull operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <returns>A pull operator.</returns>
        public UpdateDefinition<TDocument> PullAll<TItem>(FieldDefinition<TDocument> field, IEnumerable<TItem> values)
        {
            return new PullUpdateDefinition<TDocument, TItem>(field, values);
        }

        /// <summary>
        /// Creates a pull operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <returns>A pull operator.</returns>
        public UpdateDefinition<TDocument> PullAll<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, IEnumerable<TItem> values)
        {
            return PullAll(new ExpressionFieldDefinition<TDocument>(field), values);
        }

        /// <summary>
        /// Creates a pull operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>A pull operator.</returns>
        public UpdateDefinition<TDocument> PullFilter<TItem>(FieldDefinition<TDocument> field, FilterDefinition<TItem> filter)
        {
            return new PullUpdateDefinition<TDocument, TItem>(field, filter);
        }

        /// <summary>
        /// Creates a pull operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>A pull operator.</returns>
        public UpdateDefinition<TDocument> PullFilter<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, FilterDefinition<TItem> filter)
        {
            return PullFilter(new ExpressionFieldDefinition<TDocument>(field), filter);
        }

        /// <summary>
        /// Creates a pull operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>A pull operator.</returns>
        public UpdateDefinition<TDocument> PullFilter<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, Expression<Func<TItem, bool>> filter)
        {
            return PullFilter(new ExpressionFieldDefinition<TDocument>(field), new ExpressionFilterDefinition<TItem>(filter));
        }

        /// <summary>
        /// Creates a push operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A push operator.</returns>
        public UpdateDefinition<TDocument> Push<TItem>(FieldDefinition<TDocument> field, TItem value)
        {
            return new PushUpdateDefinition<TDocument, TItem>(field, new[] { value });
        }

        /// <summary>
        /// Creates a push operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A push operator.</returns>
        public UpdateDefinition<TDocument> Push<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            return Push(new ExpressionFieldDefinition<TDocument>(field), value);
        }

        /// <summary>
        /// Creates a push operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <param name="slice">The slice.</param>
        /// <param name="position">The position.</param>
        /// <param name="sort">The sort.</param>
        /// <returns>A push operator.</returns>
        public UpdateDefinition<TDocument> PushEach<TItem>(FieldDefinition<TDocument> field, IEnumerable<TItem> values, int? slice = null, int? position = null, SortDefinition<TItem> sort = null)
        {
            return new PushUpdateDefinition<TDocument, TItem>(field, values, slice, position, sort);
        }

        /// <summary>
        /// Creates a push operator.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <param name="slice">The slice.</param>
        /// <param name="position">The position.</param>
        /// <param name="sort">The sort.</param>
        /// <returns>A push operator.</returns>
        public UpdateDefinition<TDocument> PushEach<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, IEnumerable<TItem> values, int? slice = null, int? position = null, SortDefinition<TItem> sort = null)
        {
            return PushEach(new ExpressionFieldDefinition<TDocument>(field), values, slice, position, sort);
        }

        /// <summary>
        /// Creates a field renaming operator.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="newName">The new name.</param>
        /// <returns>A field rename operator.</returns>
        public UpdateDefinition<TDocument> Rename(FieldDefinition<TDocument> field, string newName)
        {
            return new OperatorUpdateDefinition<TDocument>("$rename", field, newName);
        }

        /// <summary>
        /// Creates a field renaming operator.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="newName">The new name.</param>
        /// <returns>A field rename operator.</returns>
        public UpdateDefinition<TDocument> Rename(Expression<Func<TDocument, object>> field, string newName)
        {
            return Rename(new ExpressionFieldDefinition<TDocument>(field), newName);
        }

        /// <summary>
        /// Creates a set operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A set operator.</returns>
        public UpdateDefinition<TDocument> Set<TField>(FieldDefinition<TDocument, TField> field, TField value)
        {
            return new OperatorUpdateDefinition<TDocument, TField>("$set", field, value);
        }

        /// <summary>
        /// Creates a set operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A set operator.</returns>
        public UpdateDefinition<TDocument> Set<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return Set(new ExpressionFieldDefinition<TDocument, TField>(field), value);
        }

        /// <summary>
        /// Creates a set on insert operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A set on insert operator.</returns>
        public UpdateDefinition<TDocument> SetOnInsert<TField>(FieldDefinition<TDocument, TField> field, TField value)
        {
            return new OperatorUpdateDefinition<TDocument, TField>("$setOnInsert", field, value);
        }

        /// <summary>
        /// Creates a set on insert operator.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>A set on insert operator.</returns>
        public UpdateDefinition<TDocument> SetOnInsert<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return SetOnInsert(new ExpressionFieldDefinition<TDocument, TField>(field), value);
        }

        /// <summary>
        /// Creates an unset operator.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>An unset operator.</returns>
        public UpdateDefinition<TDocument> Unset(FieldDefinition<TDocument> field)
        {
            return new OperatorUpdateDefinition<TDocument>("$unset", field, 1);
        }

        /// <summary>
        /// Creates an unset operator.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>An unset operator.</returns>
        public UpdateDefinition<TDocument> Unset(Expression<Func<TDocument, object>> field)
        {
            return Unset(new ExpressionFieldDefinition<TDocument>(field));
        }
    }

    internal sealed class AddToSetUpdateDefinition<TDocument, TItem> : UpdateDefinition<TDocument>
    {
        private readonly FieldDefinition<TDocument> _field;
        private readonly List<TItem> _values;

        public AddToSetUpdateDefinition(FieldDefinition<TDocument> field, IEnumerable<TItem> values)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
            _values = Ensure.IsNotNull(values, nameof(values)).ToList();
        }

        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedField = _field.Render(documentSerializer, serializerRegistry);

            IBsonSerializer itemSerializer;
            if (renderedField.FieldSerializer != null)
            {
                var arraySerializer = renderedField.FieldSerializer as IBsonArraySerializer;
                BsonSerializationInfo itemSerializationInfo;
                if (arraySerializer == null || !arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                {
                    var message = string.Format("The serializer for field '{0}' must implement IBsonArraySerializer and provide item serialization info.", renderedField.FieldName);
                    throw new InvalidOperationException(message);
                }
                itemSerializer = itemSerializationInfo.Serializer;
            }
            else
            {
                itemSerializer = serializerRegistry.GetSerializer<TItem>();
            }

            var document = new BsonDocument();
            using (var bsonWriter = new BsonDocumentWriter(document))
            {
                var context = BsonSerializationContext.CreateRoot(bsonWriter);
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName("$addToSet");
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName(renderedField.FieldName);
                if (_values.Count == 1)
                {
                    itemSerializer.Serialize(context, _values[0]);
                }
                else
                {
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteName("$each");
                    bsonWriter.WriteStartArray();
                    foreach (var value in _values)
                    {
                        itemSerializer.Serialize(context, value);
                    }
                    bsonWriter.WriteEndArray();
                    bsonWriter.WriteEndDocument();
                }
                bsonWriter.WriteEndDocument();
                bsonWriter.WriteEndDocument();
            }

            return document;
        }
    }

    internal sealed class CombinedUpdateDefinition<TDocument> : UpdateDefinition<TDocument>
    {
        private readonly List<UpdateDefinition<TDocument>> _updates;

        public CombinedUpdateDefinition(IEnumerable<UpdateDefinition<TDocument>> updates)
        {
            _updates = Ensure.IsNotNull(updates, nameof(updates)).ToList();
        }

        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var document = new BsonDocument();

            foreach (var update in _updates)
            {
                var renderedUpdate = update.Render(documentSerializer, serializerRegistry);

                foreach (var element in renderedUpdate.Elements)
                {
                    BsonValue currentOperatorValue;
                    if (document.TryGetValue(element.Name, out currentOperatorValue))
                    {
                        // last one wins
                        document[element.Name] = ((BsonDocument)currentOperatorValue)
                            .Merge((BsonDocument)element.Value, overwriteExistingElements: true);
                    }
                    else
                    {
                        document.Add(element);
                    }
                }
            }
            return document;
        }
    }

    internal sealed class BitwiseOperatorUpdateDefinition<TDocument, TField> : UpdateDefinition<TDocument>
    {
        private readonly string _operatorName;
        private readonly FieldDefinition<TDocument, TField> _field;
        private readonly TField _value;

        public BitwiseOperatorUpdateDefinition(string operatorName, FieldDefinition<TDocument, TField> field, TField value)
        {
            _operatorName = Ensure.IsNotNull(operatorName, nameof(operatorName));
            _field = Ensure.IsNotNull(field, nameof(field));
            _value = value;
        }

        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedField = _field.Render(documentSerializer, serializerRegistry);

            var document = new BsonDocument();
            using (var bsonWriter = new BsonDocumentWriter(document))
            {
                var context = BsonSerializationContext.CreateRoot(bsonWriter);
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName("$bit");
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName(renderedField.FieldName);
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName(_operatorName);
                renderedField.ValueSerializer.Serialize(context, _value);
                bsonWriter.WriteEndDocument();
                bsonWriter.WriteEndDocument();
                bsonWriter.WriteEndDocument();
            }

            return document;
        }
    }

    internal sealed class OperatorUpdateDefinition<TDocument> : UpdateDefinition<TDocument>
    {
        private readonly string _operatorName;
        private readonly FieldDefinition<TDocument> _field;
        private readonly BsonValue _value;

        public OperatorUpdateDefinition(string operatorName, FieldDefinition<TDocument> field, BsonValue value)
        {
            _operatorName = Ensure.IsNotNull(operatorName, nameof(operatorName));
            _field = Ensure.IsNotNull(field, nameof(field));
            _value = value;
        }

        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedField = _field.Render(documentSerializer, serializerRegistry);
            return new BsonDocument(_operatorName, new BsonDocument(renderedField.FieldName, _value));
        }
    }

    internal sealed class OperatorUpdateDefinition<TDocument, TField> : UpdateDefinition<TDocument>
    {
        private readonly string _operatorName;
        private readonly FieldDefinition<TDocument, TField> _field;
        private readonly TField _value;

        public OperatorUpdateDefinition(string operatorName, FieldDefinition<TDocument, TField> field, TField value)
        {
            _operatorName = Ensure.IsNotNull(operatorName, nameof(operatorName));
            _field = Ensure.IsNotNull(field, nameof(field));
            _value = value;
        }

        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedField = _field.Render(documentSerializer, serializerRegistry);

            var document = new BsonDocument();
            using (var bsonWriter = new BsonDocumentWriter(document))
            {
                var context = BsonSerializationContext.CreateRoot(bsonWriter);
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName(_operatorName);
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName(renderedField.FieldName);
                renderedField.ValueSerializer.Serialize(context, _value);
                bsonWriter.WriteEndDocument();
                bsonWriter.WriteEndDocument();
            }

            return document;
        }
    }

    internal sealed class PullUpdateDefinition<TDocument, TItem> : UpdateDefinition<TDocument>
    {
        private readonly FieldDefinition<TDocument> _field;
        private readonly FilterDefinition<TItem> _filter;
        private readonly List<TItem> _values;

        public PullUpdateDefinition(FieldDefinition<TDocument> field, FilterDefinition<TItem> filter)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
            _filter = Ensure.IsNotNull(filter, nameof(filter));
        }

        public PullUpdateDefinition(FieldDefinition<TDocument> field, IEnumerable<TItem> values)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
            _values = Ensure.IsNotNull(values, nameof(values)).ToList();
        }

        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedField = _field.Render(documentSerializer, serializerRegistry);

            IBsonSerializer itemSerializer;
            if (renderedField.FieldSerializer != null)
            {
                var arraySerializer = renderedField.FieldSerializer as IBsonArraySerializer;
                BsonSerializationInfo itemSerializationInfo;
                if (arraySerializer == null || !arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                {
                    var message = string.Format("The serializer for field '{0}' must implement IBsonArraySerializer and provide item serialization info.", renderedField.FieldName);
                    throw new InvalidOperationException(message);
                }
                itemSerializer = itemSerializationInfo.Serializer;
            }
            else
            {
                itemSerializer = serializerRegistry.GetSerializer<TItem>();
            }

            if (_filter != null)
            {
                var renderedFilter = _filter.Render((IBsonSerializer<TItem>)itemSerializer, serializerRegistry);
                return new BsonDocument("$pull", new BsonDocument(renderedField.FieldName, renderedFilter));
            }
            else
            {
                var document = new BsonDocument();
                using (var bsonWriter = new BsonDocumentWriter(document))
                {
                    var context = BsonSerializationContext.CreateRoot(bsonWriter);
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteName(_values.Count == 1 ? "$pull" : "$pullAll");
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteName(renderedField.FieldName);
                    if (_values.Count == 1)
                    {
                        itemSerializer.Serialize(context, _values[0]);
                    }
                    else
                    {
                        bsonWriter.WriteStartArray();
                        foreach (var value in _values)
                        {
                            itemSerializer.Serialize(context, value);
                        }
                        bsonWriter.WriteEndArray();
                    }
                    bsonWriter.WriteEndDocument();
                    bsonWriter.WriteEndDocument();
                }
                return document;
            }
        }
    }

    internal sealed class PushUpdateDefinition<TDocument, TItem> : UpdateDefinition<TDocument>
    {
        private readonly FieldDefinition<TDocument> _field;
        private readonly int? _position;
        private readonly int? _slice;
        private SortDefinition<TItem> _sort;
        private readonly List<TItem> _values;

        public PushUpdateDefinition(FieldDefinition<TDocument> field, IEnumerable<TItem> values, int? slice = null, int? position = null, SortDefinition<TItem> sort = null)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
            _values = Ensure.IsNotNull(values, nameof(values)).ToList();
            _slice = slice;
            _position = position;
            _sort = sort;
        }

        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedField = _field.Render(documentSerializer, serializerRegistry);

            IBsonSerializer itemSerializer;
            if (renderedField.FieldSerializer != null)
            {
                var arraySerializer = renderedField.FieldSerializer as IBsonArraySerializer;
                BsonSerializationInfo itemSerializationInfo;
                if (arraySerializer == null || !arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                {
                    var message = string.Format("The serializer for field '{0}' must implement IBsonArraySerializer and provide item serialization info.", renderedField.FieldName);
                    throw new InvalidOperationException(message);
                }
                itemSerializer = itemSerializationInfo.Serializer;
            }
            else
            {
                itemSerializer = serializerRegistry.GetSerializer<TItem>();
            }

            var document = new BsonDocument();
            using (var bsonWriter = new BsonDocumentWriter(document))
            {
                var context = BsonSerializationContext.CreateRoot(bsonWriter);
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName("$push");
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName(renderedField.FieldName);
                if (!_slice.HasValue && !_position.HasValue && _sort == null && _values.Count == 1)
                {
                    itemSerializer.Serialize(context, _values[0]);
                }
                else
                {
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteName("$each");
                    bsonWriter.WriteStartArray();
                    foreach (var value in _values)
                    {
                        itemSerializer.Serialize(context, value);
                    }
                    bsonWriter.WriteEndArray();
                    if (_slice.HasValue)
                    {
                        bsonWriter.WriteName("$slice");
                        bsonWriter.WriteInt32(_slice.Value);
                    }
                    if (_position.HasValue)
                    {
                        bsonWriter.WriteName("$position");
                        bsonWriter.WriteInt32(_position.Value);
                    }
                    bsonWriter.WriteEndDocument();
                }
                bsonWriter.WriteEndDocument();
                bsonWriter.WriteEndDocument();
            }

            if (_sort != null)
            {
                document["$push"][renderedField.FieldName]["$sort"] = _sort.Render((IBsonSerializer<TItem>)itemSerializer, serializerRegistry);
            }

            return document;
        }
    }

}
