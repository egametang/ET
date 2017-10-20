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
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a write concern.
    /// </summary>
    public sealed class WriteConcern : IEquatable<WriteConcern>, IConvertibleToBsonDocument
    {
        #region static
        // static fields
        private static readonly WriteConcern __acknowledged = new WriteConcern();
        private static readonly WriteConcern __unacknowledged = new WriteConcern(0);
        private static readonly WriteConcern __w1 = new WriteConcern(1);
        private static readonly WriteConcern __w2 = new WriteConcern(2);
        private static readonly WriteConcern __w3 = new WriteConcern(3);
        private static readonly WriteConcern __wMajority = new WriteConcern("majority");

        // static properties
        /// <summary>
        /// Gets an instance of WriteConcern that represents an acknowledged write concern.
        /// </summary>
        /// <value>
        /// An instance of WriteConcern that represents an acknowledged write concern.
        /// </value>
        public static WriteConcern Acknowledged
        {
            get { return __acknowledged; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern that represents an unacknowledged write concern.
        /// </summary>
        /// <value>
        /// An instance of WriteConcern that represents an unacknowledged write concern.
        /// </value>
        public static WriteConcern Unacknowledged
        {
            get { return __unacknowledged; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern that represents a W1 write concern.
        /// </summary>
        /// <value>
        /// An instance of WriteConcern that represents a W1 write concern.
        /// </value>
        public static WriteConcern W1
        {
            get { return __w1; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern that represents a W2 write concern.
        /// </summary>
        /// <value>
        /// An instance of WriteConcern that represents a W2 write concern.
        /// </value>
        public static WriteConcern W2
        {
            get { return __w2; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern that represents a W3 write concern.
        /// </summary>
        /// <value>
        /// An instance of WriteConcern that represents a W3 write concern.
        /// </value>
        public static WriteConcern W3
        {
            get { return __w3; }
        }

        /// <summary>
        /// Gets an instance of WriteConcern that represents a majority write concern.
        /// </summary>
        /// <value>
        /// An instance of WriteConcern that represents a majority write concern.
        /// </value>
        public static WriteConcern WMajority
        {
            get { return __wMajority; }
        }

        /// <summary>
        /// Creates a write concern from a document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>A write concern.</returns>
        public static WriteConcern FromBsonDocument(BsonDocument document)
        {
            var writeConcern = WriteConcern.Acknowledged;

            BsonValue w;
            if (document.TryGetValue("w", out w))
            {
                writeConcern = writeConcern.With(w: WriteConcern.WValue.Parse(w.ToString()));
            }

            BsonValue wTimeout;
            if (document.TryGetValue("wtimeout", out wTimeout))
            {
                writeConcern = writeConcern.With(wTimeout: TimeSpan.FromMilliseconds(wTimeout.ToDouble()));
            }

            BsonValue fsync;
            if (document.TryGetValue("fsync", out fsync))
            {
                writeConcern = writeConcern.With(fsync: fsync.ToBoolean());
            }

            BsonValue j;
            if (document.TryGetValue("j", out j))
            {
                writeConcern = writeConcern.With(journal: j.ToBoolean());
            }

            return writeConcern;
        }
        #endregion

        // fields
        private readonly bool? _fsync;
        private readonly bool? _journal;
        private readonly WValue _w;
        private readonly TimeSpan? _wTimeout;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteConcern"/> class.
        /// </summary>
        /// <param name="w">The w value.</param>
        /// <param name="wTimeout">The wtimeout value.</param>
        /// <param name="fsync">The fsync value .</param>
        /// <param name="journal">The journal value.</param>
        public WriteConcern(
            int w,
            Optional<TimeSpan?> wTimeout = default(Optional<TimeSpan?>),
            Optional<bool?> fsync = default(Optional<bool?>),
            Optional<bool?> journal = default(Optional<bool?>))
            : this(new WCount(Ensure.IsGreaterThanOrEqualToZero(w, nameof(w))), wTimeout, fsync, journal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteConcern"/> class.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="wTimeout">The wtimeout value.</param>
        /// <param name="fsync">The fsync value .</param>
        /// <param name="journal">The journal value.</param>
        public WriteConcern(
            string mode,
            Optional<TimeSpan?> wTimeout = default(Optional<TimeSpan?>),
            Optional<bool?> fsync = default(Optional<bool?>),
            Optional<bool?> journal = default(Optional<bool?>))
            : this(new WMode(Ensure.IsNotNullOrEmpty(mode, nameof(mode))), wTimeout, fsync, journal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteConcern"/> class.
        /// </summary>
        /// <param name="w">The w value.</param>
        /// <param name="wTimeout">The wtimeout value.</param>
        /// <param name="fsync">The fsync value .</param>
        /// <param name="journal">The journal value.</param>
        public WriteConcern(
            Optional<WValue> w = default(Optional<WValue>),
            Optional<TimeSpan?> wTimeout = default(Optional<TimeSpan?>),
            Optional<bool?> fsync = default(Optional<bool?>),
            Optional<bool?> journal = default(Optional<bool?>))
        {
            _w = w.WithDefault(null);
            _wTimeout = Ensure.IsNullOrGreaterThanZero(wTimeout.WithDefault(null), "wTimeout");
            _fsync = fsync.WithDefault(null);
            _journal = journal.WithDefault(null);
        }

        // properties
        /// <summary>
        /// Gets the fsync value.
        /// </summary>
        /// <value>
        /// The fsync value.
        /// </value>
        public bool? FSync
        {
            get { return _fsync; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is an acknowledged write concern.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is an acknowledged write concern; otherwise, <c>false</c>.
        /// </value>
        public bool IsAcknowledged
        {
            get
            {
                if (_w == null || !_w.Equals((WValue)0))
                {
                    return true;
                }

                return _journal.GetValueOrDefault(false) || _fsync.GetValueOrDefault(false);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this write concern will use the default on the server.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is the default; otherwise, <c>false</c>.
        /// </value>
        public bool IsServerDefault
        {
            get
            {
                return _w == null &&
                    !_wTimeout.HasValue &&
                    !_fsync.HasValue &&
                    !_journal.HasValue;
            }
        }

        /// <summary>
        /// Gets the journal value.
        /// </summary>
        /// <value>
        /// The journal value.
        /// </value>
        public bool? Journal
        {
            get { return _journal; }
        }

        /// <summary>
        /// Gets the w value.
        /// </summary>
        /// <value>
        /// The w value.
        /// </value>
        public WValue W
        {
            get { return _w; }
        }

        /// <summary>
        /// Gets the wtimeout value.
        /// </summary>
        /// <value>
        /// The wtimeout value.
        /// </value>
        public TimeSpan? WTimeout
        {
            get { return _wTimeout; }
        }

        // methods
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as WriteConcern);
        }

        /// <inheritdoc/>
        public bool Equals(WriteConcern other)
        {
            if (other == null)
            {
                return false;
            }

            return _fsync == other._fsync &&
                _journal == other._journal &&
                object.Equals(_w, other._w) &&
                _wTimeout == other._wTimeout;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(_fsync)
                .Hash(_journal)
                .Hash(_w)
                .Hash(_wTimeout)
                .GetHashCode();
        }

        /// <summary>
        /// Converts this write concern to a BsonDocument suitable to be sent to the server.
        /// </summary>
        /// <returns>
        /// A BsonDocument.
        /// </returns>
        public BsonDocument ToBsonDocument()
        {
            return new BsonDocument
            {
                { "w", () => _w.ToBsonValue(), _w != null }, // optional
                { "wtimeout", () => _wTimeout.Value.TotalMilliseconds, _wTimeout.HasValue }, // optional
                { "fsync", () => _fsync.Value, _fsync.HasValue }, // optional
                { "j", () => _journal.Value, _journal.HasValue } // optional
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var parts = new List<string>();
            if (_w != null)
            {
                if (_w is WMode)
                {
                    parts.Add(string.Format("w : \"{0}\"", _w));
                }
                else
                {
                    parts.Add(string.Format("w : {0}", _w));
                }
            }
            if (_wTimeout != null)
            {
                parts.Add(string.Format("wtimeout : {0}", TimeSpanParser.ToString(_wTimeout.Value)));
            }
            if (_fsync != null)
            {
                parts.Add(string.Format("fsync : {0}", _fsync.Value ? "true" : "false"));
            }
            if (_journal != null)
            {
                parts.Add(string.Format("journal : {0}", _journal.Value ? "true" : "false"));
            }

            if (parts.Count == 0)
            {
                return "{ }";
            }
            else
            {
                return string.Format("{{ {0} }}", string.Join(", ", parts.ToArray()));
            }
        }

        /// <summary>
        /// Returns a new instance of WriteConcern with some values changed.
        /// </summary>
        /// <param name="w">The w value.</param>
        /// <param name="wTimeout">The wtimeout value.</param>
        /// <param name="fsync">The fsync value.</param>
        /// <param name="journal">The journal value.</param>
        /// <returns>A WriteConcern.</returns>
        public WriteConcern With(
            int w,
            Optional<TimeSpan?> wTimeout = default(Optional<TimeSpan?>),
            Optional<bool?> fsync = default(Optional<bool?>),
            Optional<bool?> journal = default(Optional<bool?>))
        {
            return With(new WCount(w), wTimeout, fsync, journal);
        }

        /// <summary>
        /// Returns a new instance of WriteConcern with some values changed.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="wTimeout">The wtimeout value.</param>
        /// <param name="fsync">The fsync value.</param>
        /// <param name="journal">The journal value.</param>
        /// <returns>A WriteConcern.</returns>
        public WriteConcern With(
            string mode,
            Optional<TimeSpan?> wTimeout = default(Optional<TimeSpan?>),
            Optional<bool?> fsync = default(Optional<bool?>),
            Optional<bool?> journal = default(Optional<bool?>))
        {
            return With(new WMode(mode), wTimeout, fsync, journal);
        }

        /// <summary>
        /// Returns a new instance of WriteConcern with some values changed.
        /// </summary>
        /// <param name="w">The w value.</param>
        /// <param name="wTimeout">The wtimeout value.</param>
        /// <param name="fsync">The fsync value.</param>
        /// <param name="journal">The journal value.</param>
        /// <returns>A WriteConcern.</returns>
        public WriteConcern With(
            Optional<WValue> w = default(Optional<WValue>),
            Optional<TimeSpan?> wTimeout = default(Optional<TimeSpan?>),
            Optional<bool?> fsync = default(Optional<bool?>),
            Optional<bool?> journal = default(Optional<bool?>))
        {
            if (w.Replaces(_w) ||
                wTimeout.Replaces(_wTimeout) ||
                fsync.Replaces(_fsync) ||
                journal.Replaces(_journal))
            {
                return new WriteConcern(
                    w: w.WithDefault(_w),
                    wTimeout: wTimeout.WithDefault(_wTimeout),
                    fsync: fsync.WithDefault(_fsync),
                    journal: journal.WithDefault(_journal));
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Represents the base class for w values.
        /// </summary>
        public abstract class WValue : IEquatable<WValue>
        {
            #region static
            // static methods
            /// <summary>
            /// Parses the specified value.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <returns>A WValue.</returns>
            public static WValue Parse(string value)
            {
                int n;
                if (int.TryParse(value, out n))
                {
                    return new WCount(n);
                }
                else
                {
                    return new WMode(value);
                }
            }

            // static operators
            /// <summary>
            /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="WValue"/>.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <returns>
            /// The result of the conversion.
            /// </returns>
            public static implicit operator WValue(int value)
            {
                return new WCount(value);
            }

            /// <summary>
            /// Performs an implicit conversion from Nullable{Int32} to <see cref="WValue"/>.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <returns>
            /// The result of the conversion.
            /// </returns>
            public static implicit operator WValue(int? value)
            {
                return value.HasValue ? new WCount(value.Value) : null;
            }

            /// <summary>
            /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="WValue"/>.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <returns>
            /// The result of the conversion.
            /// </returns>
            public static implicit operator WValue(string value)
            {
                return (value == null) ? null : new WMode(value);
            }
            #endregion

            // constructors
            internal WValue()
            {
            }

            // methods
            /// <inheritdoc/>
            public bool Equals(WValue other)
            {
                return Equals((object)other);
            }

            /// <summary>
            /// Converts this WValue to a BsonValue suitable to be included in a BsonDocument representing a write concern.
            /// </summary>
            /// <returns>A BsonValue.</returns>
            public abstract BsonValue ToBsonValue();
        }

        /// <summary>
        /// Represents a numeric WValue.
        /// </summary>
        public sealed class WCount : WValue, IEquatable<WCount>
        {
            // fields
            private readonly int _value;

            // constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="WCount"/> class.
            /// </summary>
            /// <param name="w">The w value.</param>
            public WCount(int w)
            {
                _value = Ensure.IsGreaterThanOrEqualToZero(w, nameof(w));
            }

            // properties
            /// <summary>
            /// Gets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public int Value
            {
                get { return _value; }
            }

            // methods
            /// <inheritdoc/>
            public override bool Equals(object obj)
            {
                return Equals(obj as WCount);
            }

            /// <inheritdoc/>
            public bool Equals(WCount other)
            {
                if (other == null)
                {
                    return false;
                }
                return _value == other._value;
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }

            /// <inheritdoc/>
            public override BsonValue ToBsonValue()
            {
                return (BsonInt32)_value;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return _value.ToString();
            }
        }

        /// <summary>
        /// Represents a mode string WValue.
        /// </summary>
        public sealed class WMode : WValue, IEquatable<WMode>
        {
            #region static
            // static fields
            private static readonly WMode __majority = new WMode("majority");

            // static properties
            /// <summary>
            /// Gets an instance of WValue that represents the majority mode.
            /// </summary>
            /// <value>
            /// An instance of WValue that represents the majority mode.
            /// </value>
            public static WMode Majority
            {
                get { return __majority; }
            }
            #endregion

            // fields
            private readonly string _value;

            // constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="WMode"/> class.
            /// </summary>
            /// <param name="mode">The mode.</param>
            public WMode(string mode)
            {
                _value = Ensure.IsNotNullOrEmpty(mode, nameof(mode));
            }

            // properties
            /// <summary>
            /// Gets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public string Value
            {
                get { return _value; }
            }

            // methods
            /// <inheritdoc/>
            public override bool Equals(object obj)
            {
                return Equals(obj as WMode);
            }

            /// <inheritdoc/>
            public bool Equals(WMode other)
            {
                if (other == null)
                {
                    return false;
                }
                return _value == other._value;
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }

            /// <inheritdoc/>
            public override BsonValue ToBsonValue()
            {
                return new BsonString(_value);
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return _value;
            }
        }
    }
}
