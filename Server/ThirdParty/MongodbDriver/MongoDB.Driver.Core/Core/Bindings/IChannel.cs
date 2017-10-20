/* Copyright 2013-2015 MongoDB Inc.
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// Represents a channel (similar to a connection but operates at the level of protocols rather than messages).
    /// </summary>
    public interface IChannel : IDisposable
    {
        /// <summary>
        /// Gets the connection description.
        /// </summary>
        /// <value>
        /// The connection description.
        /// </value>
        ConnectionDescription ConnectionDescription { get; }

        /// <summary>
        /// Executes a Command protocol.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="command">The command.</param>
        /// <param name="commandValidator">The command validator.</param>
        /// <param name="responseHandling">The response handling.</param>
        /// <param name="slaveOk">if set to <c>true</c> sets the SlaveOk bit to true in the command message sent to the server.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the Command protocol.</returns>
        TResult Command<TResult>(
            DatabaseNamespace databaseNamespace,
            BsonDocument command,
            IElementNameValidator commandValidator,
            Func<CommandResponseHandling> responseHandling,
            bool slaveOk,
            IBsonSerializer<TResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a Command protocol.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="command">The command.</param>
        /// <param name="commandValidator">The command validator.</param>
        /// <param name="responseHandling">The response handling.</param>
        /// <param name="slaveOk">if set to <c>true</c> sets the SlaveOk bit to true in the command message sent to the server.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the result of the Command protocol.</returns>
        Task<TResult> CommandAsync<TResult>(
            DatabaseNamespace databaseNamespace,
            BsonDocument command,
            IElementNameValidator commandValidator,
            Func<CommandResponseHandling> responseHandling,
            bool slaveOk,
            IBsonSerializer<TResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a Delete protocol.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="query">The query.</param>
        /// <param name="isMulti">if set to <c>true</c> all matching documents are deleted.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="writeConcern">The write concern.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the Delete protocol.</returns>
        WriteConcernResult Delete(
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            bool isMulti,
            MessageEncoderSettings messageEncoderSettings,
            WriteConcern writeConcern,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a Delete protocol.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="query">The query.</param>
        /// <param name="isMulti">if set to <c>true</c> all matching documents are deleted.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="writeConcern">The write concern.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the result of the Delete protocol.</returns>
        Task<WriteConcernResult> DeleteAsync(
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            bool isMulti,
            MessageEncoderSettings messageEncoderSettings,
            WriteConcern writeConcern,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a GetMore protocol.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="query">The query.</param>
        /// <param name="cursorId">The cursor identifier.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the GetMore protocol.</returns>
        CursorBatch<TDocument> GetMore<TDocument>(
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            long cursorId,
            int batchSize,
            IBsonSerializer<TDocument> serializer,
            MessageEncoderSettings messageEncoderSettings,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a GetMore protocol.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="query">The query.</param>
        /// <param name="cursorId">The cursor identifier.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the result of the GetMore protocol.</returns>
        Task<CursorBatch<TDocument>> GetMoreAsync<TDocument>(
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            long cursorId,
            int batchSize,
            IBsonSerializer<TDocument> serializer,
            MessageEncoderSettings messageEncoderSettings,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes an Insert protocol.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="writeConcern">The write concern.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="documentSource">The document source.</param>
        /// <param name="maxBatchCount">The maximum batch count.</param>
        /// <param name="maxMessageSize">Maximum size of the message.</param>
        /// <param name="continueOnError">if set to <c>true</c> the server will continue with subsequent Inserts even if errors occur.</param>
        /// <param name="shouldSendGetLastError">A delegate that determines whether to piggy-back a GetLastError messsage with the Insert message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the Insert protocol.</returns>
        WriteConcernResult Insert<TDocument>(
            CollectionNamespace collectionNamespace,
            WriteConcern writeConcern,
            IBsonSerializer<TDocument> serializer,
            MessageEncoderSettings messageEncoderSettings,
            BatchableSource<TDocument> documentSource,
            int? maxBatchCount,
            int? maxMessageSize,
            bool continueOnError,
            Func<bool> shouldSendGetLastError,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes an Insert protocol.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="writeConcern">The write concern.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="documentSource">The document source.</param>
        /// <param name="maxBatchCount">The maximum batch count.</param>
        /// <param name="maxMessageSize">Maximum size of the message.</param>
        /// <param name="continueOnError">if set to <c>true</c> the server will continue with subsequent Inserts even if errors occur.</param>
        /// <param name="shouldSendGetLastError">A delegate that determines whether to piggy-back a GetLastError messsage with the Insert message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the result of the Insert protocol.</returns>
        Task<WriteConcernResult> InsertAsync<TDocument>(
            CollectionNamespace collectionNamespace,
            WriteConcern writeConcern,
            IBsonSerializer<TDocument> serializer,
            MessageEncoderSettings messageEncoderSettings,
            BatchableSource<TDocument> documentSource,
            int? maxBatchCount,
            int? maxMessageSize,
            bool continueOnError,
            Func<bool> shouldSendGetLastError,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a KillCursors protocol.
        /// </summary>
        /// <param name="cursorIds">The cursor ids.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void KillCursors(
            IEnumerable<long> cursorIds,
            MessageEncoderSettings messageEncoderSettings,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a KillCursors protocol.
        /// </summary>
        /// <param name="cursorIds">The cursor ids.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task that represents the KillCursors protocol.</returns>
        Task KillCursorsAsync(
            IEnumerable<long> cursorIds,
            MessageEncoderSettings messageEncoderSettings,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a Query protocol.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="query">The query.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="queryValidator">The query validator.</param>
        /// <param name="skip">The number of documents to skip.</param>
        /// <param name="batchSize">The size of a batch.</param>
        /// <param name="slaveOk">if set to <c>true</c> sets the SlaveOk bit to true in the query message sent to the server.</param>
        /// <param name="partialOk">if set to <c>true</c> the server is allowed to return partial results if any shards are unavailable.</param>
        /// <param name="noCursorTimeout">if set to <c>true</c> the server will not timeout the cursor.</param>
        /// <param name="oplogReplay">if set to <c>true</c> the OplogReplay bit will be set.</param>
        /// <param name="tailableCursor">if set to <c>true</c> the query should return a tailable cursor.</param>
        /// <param name="awaitData">if set to <c>true</c> the server should await awhile before returning an empty batch for a tailable cursor.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the Insert protocol.</returns>
        CursorBatch<TDocument> Query<TDocument>(
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            BsonDocument fields,
            IElementNameValidator queryValidator,
            int skip,
            int batchSize,
            bool slaveOk,
            bool partialOk,
            bool noCursorTimeout,
            bool oplogReplay,
            bool tailableCursor,
            bool awaitData,
            IBsonSerializer<TDocument> serializer,
            MessageEncoderSettings messageEncoderSettings,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a Query protocol.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="query">The query.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="queryValidator">The query validator.</param>
        /// <param name="skip">The number of documents to skip.</param>
        /// <param name="batchSize">The size of a batch.</param>
        /// <param name="slaveOk">if set to <c>true</c> sets the SlaveOk bit to true in the query message sent to the server.</param>
        /// <param name="partialOk">if set to <c>true</c> the server is allowed to return partial results if any shards are unavailable.</param>
        /// <param name="noCursorTimeout">if set to <c>true</c> the server will not timeout the cursor.</param>
        /// <param name="oplogReplay">if set to <c>true</c> the OplogReplay bit will be set.</param>
        /// <param name="tailableCursor">if set to <c>true</c> the query should return a tailable cursor.</param>
        /// <param name="awaitData">if set to <c>true</c> the server should await awhile before returning an empty batch for a tailable cursor.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the result of the Insert protocol.</returns>
        Task<CursorBatch<TDocument>> QueryAsync<TDocument>(
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            BsonDocument fields,
            IElementNameValidator queryValidator,
            int skip,
            int batchSize,
            bool slaveOk,
            bool partialOk,
            bool noCursorTimeout,
            bool oplogReplay,
            bool tailableCursor,
            bool awaitData,
            IBsonSerializer<TDocument> serializer,
            MessageEncoderSettings messageEncoderSettings,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes an Update protocol.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="writeConcern">The write concern.</param>
        /// <param name="query">The query.</param>
        /// <param name="update">The update.</param>
        /// <param name="updateValidator">The update validator.</param>
        /// <param name="isMulti">if set to <c>true</c> the Update can affect multiple documents.</param>
        /// <param name="isUpsert">if set to <c>true</c> the document will be inserted if it is not found.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the Update protocol.</returns>
        WriteConcernResult Update(
            CollectionNamespace collectionNamespace,
            MessageEncoderSettings messageEncoderSettings,
            WriteConcern writeConcern,
            BsonDocument query,
            BsonDocument update,
            IElementNameValidator updateValidator,
            bool isMulti,
            bool isUpsert,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes an Update protocol.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="writeConcern">The write concern.</param>
        /// <param name="query">The query.</param>
        /// <param name="update">The update.</param>
        /// <param name="updateValidator">The update validator.</param>
        /// <param name="isMulti">if set to <c>true</c> the Update can affect multiple documents.</param>
        /// <param name="isUpsert">if set to <c>true</c> the document will be inserted if it is not found.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the result of the Update protocol.</returns>
        Task<WriteConcernResult> UpdateAsync(
            CollectionNamespace collectionNamespace,
            MessageEncoderSettings messageEncoderSettings,
            WriteConcern writeConcern,
            BsonDocument query,
            BsonDocument update,
            IElementNameValidator updateValidator,
            bool isMulti,
            bool isUpsert,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents a handle to a channel.
    /// </summary>
    public interface IChannelHandle : IChannel
    {
        /// <summary>
        /// Returns a new handle to the underlying channel.
        /// </summary>
        /// <returns>A channel handle.</returns>
        IChannelHandle Fork();
    }
}
