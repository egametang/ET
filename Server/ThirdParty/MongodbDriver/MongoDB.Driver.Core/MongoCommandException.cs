/* Copyright 2013-present MongoDB Inc.
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
#if NET452
using System.Runtime.Serialization;
#endif
using MongoDB.Bson;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a MongoDB command exception.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class MongoCommandException : MongoServerException
    {
        #region static
        private static void AddErrorLabelsFromCommandResult(MongoCommandException exception, BsonDocument result)
        {
            // note: make a best effort to extract the error labels from the result, but never throw an exception
            if (result != null)
            {
                BsonValue errorLabels;
                if (result.TryGetValue("errorLabels", out errorLabels) && errorLabels.IsBsonArray)
                {
                    foreach (var errorLabel in errorLabels.AsBsonArray)
                    {
                        if (errorLabel.IsString)
                        {
                            exception.AddErrorLabel(errorLabel.AsString);
                        }
                    }
                }
            }
        }
        #endregion

        // fields
        private readonly BsonDocument _command;
        private readonly BsonDocument _result;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCommandException"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="command">The command.</param>
        public MongoCommandException(ConnectionId connectionId, string message, BsonDocument command)
            : this(connectionId, message, command, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCommandException"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="command">The command.</param>
        /// <param name="result">The command result.</param>
        public MongoCommandException(ConnectionId connectionId, string message, BsonDocument command, BsonDocument result)
            : base(connectionId, message)
        {
            _command = command; // can be null
            _result = result; // can be null

            AddErrorLabelsFromCommandResult(this, result);
        }

#if NET452
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCommandException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        protected MongoCommandException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _command = (BsonDocument)info.GetValue("_command", typeof(BsonDocument));
            _result = (BsonDocument)info.GetValue("_result", typeof(BsonDocument));
        }
#endif

        // properties
        /// <summary>
        /// Gets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public int Code
        {
            get { return _result.GetValue("code", -1).ToInt32(); }
        }

        /// <summary>
        /// Gets the name of the error code.
        /// </summary>
        /// <value>
        /// The name of the error code.
        /// </value>
        public string CodeName
        {
            get { return _result.GetValue("codeName", null)?.AsString; }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public BsonDocument Command
        {
            get { return _command; }
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage
        {
            get { return _result.GetValue("errmsg", "Unknown error.").AsString; }
        }

        /// <summary>
        /// Gets the command result.
        /// </summary>
        /// <value>
        /// The command result.
        /// </value>
        public BsonDocument Result
        {
            get { return _result; }
        }

        // methods
#if NET452
        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_command", _command);
            info.AddValue("_result", _result);
        }
#endif
    }
}
