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
using MongoDB.Driver.Core.Misc;
#if NET452
using System.Runtime.Serialization;
#endif

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a MongoDB exception.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class MongoException : Exception
    {
        // private fields
        private readonly List<string> _errorLabels = new List<string>();

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public MongoException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public MongoException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if NET452
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        public MongoException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _errorLabels = (List<string>)info.GetValue(nameof(_errorLabels), typeof(List<String>));
        }
#endif

        // public properties
        /// <summary>
        /// Gets the error labels.
        /// </summary>
        /// <value>
        /// The error labels.
        /// </value>
        public IReadOnlyList<string> ErrorLabels => _errorLabels;

        // public methods
        /// <summary>
        /// Adds an error label.
        /// </summary>
        /// <param name="errorLabel">The error label.</param>
        public void AddErrorLabel(string errorLabel)
        {
            Ensure.IsNotNull(errorLabel, nameof(errorLabel));
            if (!_errorLabels.Contains(errorLabel))
            {
                _errorLabels.Add(errorLabel);
            }
        }

        /// <summary>
        /// Determines whether the exception has some error label.
        /// </summary>
        /// <param name="errorLabel">The error label.</param>
        /// <returns>
        ///   <c>true</c> if the exception has some error label; otherwise, <c>false</c>.
        /// </returns>
        public bool HasErrorLabel(string errorLabel)
        {
            return _errorLabels.Contains(errorLabel);
        }

        /// <summary>
        /// Removes the error label.
        /// </summary>
        /// <param name="errorLabel">The error label.</param>
        public void RemoveErrorLabel(string errorLabel)
        {
            _errorLabels.Remove(errorLabel);
        }

#if NET452
        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(_errorLabels), _errorLabels);
        }
#endif
    }
}
