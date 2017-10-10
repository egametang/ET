/* Copyright 2015 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a read concern.
    /// </summary>
    public sealed class ReadConcern : IEquatable<ReadConcern>, IConvertibleToBsonDocument
    {
        private static readonly ReadConcern __default = new ReadConcern();
        private static readonly ReadConcern __linearizable = new ReadConcern(ReadConcernLevel.Linearizable);
        private static readonly ReadConcern __local = new ReadConcern(ReadConcernLevel.Local);
        private static readonly ReadConcern __majority = new ReadConcern(ReadConcernLevel.Majority);

        /// <summary>
        /// Gets a default read concern.
        /// </summary>
        public static ReadConcern Default => __default;

        /// <summary>
        /// Gets a linearizable read concern.
        /// </summary>
        public static ReadConcern Linearizable => __linearizable;

        /// <summary>
        /// Gets a local read concern.
        /// </summary>
        public static ReadConcern Local => __local;

        /// <summary>
        /// Gets a majority read concern.
        /// </summary>
        public static ReadConcern Majority => __majority;

        /// <summary>
        /// Creates a read concern from a document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>A read concern.</returns>
        public static ReadConcern FromBsonDocument(BsonDocument document)
        {
            var readConcern = ReadConcern.Default;

            BsonValue levelValue;
            if (document.TryGetValue("level", out levelValue))
            {
                var level = (ReadConcernLevel)Enum.Parse(typeof(ReadConcernLevel), (string)levelValue, true);
                switch (level)
                {
                    case ReadConcernLevel.Linearizable:
                        return ReadConcern.Linearizable;
                    case ReadConcernLevel.Local:
                        return ReadConcern.Local;
                    case ReadConcernLevel.Majority:
                        return ReadConcern.Majority;
                    default:
                        throw new NotSupportedException($"The level {level} is not supported.");
                }
            }

            return readConcern;
        }

        private readonly ReadConcernLevel? _level;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadConcern" /> class.
        /// </summary>
        /// <param name="level">The level.</param>
        public ReadConcern(Optional<ReadConcernLevel?> level = default(Optional<ReadConcernLevel?>))
        {
            _level = level.WithDefault(null);
        }

        /// <summary>
        /// Gets a value indicating whether this is the server's default read concern.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is default; otherwise, <c>false</c>.
        /// </value>
        public bool IsServerDefault
        {
            get { return !_level.HasValue; }
        }

        /// <summary>
        /// Gets the level.
        /// </summary>
        public ReadConcernLevel? Level
        {
            get { return _level; }
        }

        // methods
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as ReadConcern);
        }

        /// <inheritdoc/>
        public bool Equals(ReadConcern other)
        {
            if (other == null)
            {
                return false;
            }

            return _level == other._level;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(_level)
                .GetHashCode();
        }

        /// <summary>
        /// Converts this read concern to a BsonDocument suitable to be sent to the server.
        /// </summary>
        /// <returns>
        /// A BsonDocument.
        /// </returns>
        public BsonDocument ToBsonDocument()
        {
            string level = null;
            if (_level.HasValue)
            {
                switch (_level.Value)
                {
                    case ReadConcernLevel.Linearizable:
                        level = "linearizable";
                        break;
                    case ReadConcernLevel.Local:
                        level = "local";
                        break;
                    case ReadConcernLevel.Majority:
                        level = "majority";
                        break;
                }
            }

            return new BsonDocument
            {
                { "level", () => level, level != null }
            };
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToBsonDocument().ToString();
        }

        /// <summary>
        /// Returns a new instance of ReadConcern with some values changed.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>
        /// A ReadConcern.
        /// </returns>
        public ReadConcern With(Optional<ReadConcernLevel?> level = default(Optional<ReadConcernLevel?>))
        {
            if (level.Replaces(_level))
            {
                return new ReadConcern(level.WithDefault(_level));
            }
            else
            {
                return this;
            }
        }
    }
}
