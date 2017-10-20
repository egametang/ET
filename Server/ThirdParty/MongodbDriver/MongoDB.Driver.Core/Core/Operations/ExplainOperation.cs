/* Copyright 2010-2015 MongoDB Inc.
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
    /// Represents an explain operation.
    /// </summary>
    public class ExplainOperation : IReadOperation<BsonDocument>, IWriteOperation<BsonDocument>
    {
        // fields
        private readonly DatabaseNamespace _databaseNamespace;
        private readonly BsonDocument _command;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private ExplainVerbosity _verbosity;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExplainOperation"/> class.
        /// </summary>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="command">The command.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ExplainOperation(DatabaseNamespace databaseNamespace, BsonDocument command, MessageEncoderSettings messageEncoderSettings)
        {
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            _command = Ensure.IsNotNull(command, nameof(command));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
            _verbosity = ExplainVerbosity.QueryPlanner;
        }

        // properties
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
        /// Gets the command to be explained.
        /// </summary>
        /// <value>
        /// The command to be explained.
        /// </value>
        public BsonDocument Command
        {
            get { return _command; }
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
        /// Gets or sets the verbosity.
        /// </summary>
        /// <value>
        /// The verbosity.
        /// </value>
        public ExplainVerbosity Verbosity
        {
            get { return _verbosity; }
            set { _verbosity = value; }
        }

        // public methods
        /// <inheritdoc/>
        public BsonDocument Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            var operation = CreateReadOperation();
            return operation.Execute(binding, cancellationToken);
        }

        /// <inheritdoc/>
        public BsonDocument Execute(IWriteBinding binding, CancellationToken cancellationToken)
        {
            var operation = CreateWriteOperation();
            return operation.Execute(binding, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<BsonDocument> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            var operation = CreateReadOperation();
            return operation.ExecuteAsync(binding, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<BsonDocument> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            var operation = CreateWriteOperation();
            return operation.ExecuteAsync(binding, cancellationToken);
        }

        // private methods
        private static string ConvertVerbosityToString(ExplainVerbosity verbosity)
        {
            switch (verbosity)
            {
                case ExplainVerbosity.AllPlansExecution:
                    return "allPlansExecution";
                case ExplainVerbosity.ExecutionStats:
                    return "executionStats";
                case ExplainVerbosity.QueryPlanner:
                    return "queryPlanner";
                default:
                    var message = string.Format("Unsupported explain verbosity: {0}.", verbosity.ToString());
                    throw new InvalidOperationException(message);
            }
        }

        internal BsonDocument CreateCommand()
        {
            return new BsonDocument
            {
                { "explain", _command },
                { "verbosity", ConvertVerbosityToString(_verbosity) }
            };
        }

        private ReadCommandOperation<BsonDocument> CreateReadOperation()
        {
            var command = CreateCommand();
            return new ReadCommandOperation<BsonDocument>(
                _databaseNamespace,
                command,
                BsonDocumentSerializer.Instance,
                _messageEncoderSettings);
        }

        private WriteCommandOperation<BsonDocument> CreateWriteOperation()
        {
            var command = CreateCommand();
            return new WriteCommandOperation<BsonDocument>(
                _databaseNamespace,
                command,
                BsonDocumentSerializer.Instance,
                _messageEncoderSettings);
        }
    }
}