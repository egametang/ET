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
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents an eval operation.
    /// </summary>
    public class EvalOperation : IWriteOperation<BsonValue>
    {
        // fields
        private IEnumerable<BsonValue> _args;
        private readonly DatabaseNamespace _databaseNamespace;
        private readonly BsonJavaScript _function;
        private TimeSpan? _maxTime;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private bool? _noLock;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EvalOperation"/> class.
        /// </summary>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="function">The JavaScript function.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public EvalOperation(
            DatabaseNamespace databaseNamespace,
            BsonJavaScript function,
            MessageEncoderSettings messageEncoderSettings)
        {
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            _function = Ensure.IsNotNull(function, nameof(function));
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
        /// <summary>
        /// Gets or sets the arguments to the JavaScript function.
        /// </summary>
        /// <value>
        /// The arguments to the JavaScript function.
        /// </value>
        public IEnumerable<BsonValue> Args
        {
            get { return _args; }
            set { _args = value; }
        }

        /// <summary>
        /// Gets the database namespace.
        /// </summary>
        /// <value>
        /// The database namespace.
        /// </value>
        public DatabaseNamespace DatabaseNamespace
        {
            get { return _databaseNamespace; }
        }

        /// <summary>
        /// Gets the JavaScript function.
        /// </summary>
        /// <value>
        /// The JavaScript function.
        /// </value>
        public BsonJavaScript Function
        {
            get { return _function; }
        }

        /// <summary>
        /// Gets or sets the maximum time the server should spend on this operation.
        /// </summary>
        /// <value>
        /// The maximum time the server should spend on this operation.
        /// </value>
        public TimeSpan? MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = Ensure.IsNullOrInfiniteOrGreaterThanOrEqualToZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets the message encoder settings.
        /// </summary>
        /// <value>
        /// The message encoder settings.
        /// </value>
        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server should not take a global write lock before evaluating the JavaScript function.
        /// </summary>
        /// <value>
        /// A value indicating whether the server should not take a global write lock before evaluating the JavaScript function.
        /// </value>
        public bool? NoLock
        {
            get { return _noLock; }
            set { _noLock = value; }
        }

        // public methods
        /// <inheritdoc/>
        public BsonValue Execute(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));
            var operation = CreateOperation();
            var result = operation.Execute(binding, cancellationToken);
            return result["retval"];
        }

        /// <inheritdoc/>
        public async Task<BsonValue> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));
            var operation = CreateOperation();
            var result = await operation.ExecuteAsync(binding, cancellationToken).ConfigureAwait(false);
            return result["retval"];
        }

        // private methods
        internal BsonDocument CreateCommand()
        {
            return new BsonDocument
            {
                { "$eval", _function },
                { "args", () => new BsonArray(_args), _args != null },
                { "nolock", () => _noLock.Value, _noLock.HasValue },
                { "maxTimeMS", () => _maxTime.Value.TotalMilliseconds, _maxTime.HasValue }
            };
        }

        private WriteCommandOperation<BsonDocument> CreateOperation()
        {
            var command = CreateCommand();
            return new WriteCommandOperation<BsonDocument>(_databaseNamespace, command, BsonDocumentSerializer.Instance, _messageEncoderSettings);
        }
    }
}
