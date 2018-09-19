/* Copyright 2010-2014 MongoDB Inc.
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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a JSON token type.
    /// </summary>
    public enum JsonTokenType
    {
        /// <summary>
        /// An invalid token.
        /// </summary>
        Invalid,
        /// <summary>
        /// A begin array token (a '[').
        /// </summary>
        BeginArray,
        /// <summary>
        /// A begin object token (a '{').
        /// </summary>
        BeginObject,
        /// <summary>
        /// An end array token (a ']').
        /// </summary>
        EndArray,
        /// <summary>
        /// A left parenthesis (a '(').
        /// </summary>
        LeftParen,
        /// <summary>
        /// A right parenthesis (a ')').
        /// </summary>
        RightParen,
        /// <summary>
        /// An end object token (a '}').
        /// </summary>
        EndObject,
        /// <summary>
        /// A colon token (a ':').
        /// </summary>
        Colon,
        /// <summary>
        /// A comma token (a ',').
        /// </summary>
        Comma,
        /// <summary>
        /// A DateTime token.
        /// </summary>
        DateTime,
        /// <summary>
        /// A Double token.
        /// </summary>
        Double,
        /// <summary>
        /// An Int32 token.
        /// </summary>
        Int32,
        /// <summary>
        /// And Int64 token.
        /// </summary>
        Int64,
        /// <summary>
        /// An ObjectId token.
        /// </summary>
        ObjectId,
        /// <summary>
        /// A regular expression token.
        /// </summary>
        RegularExpression,
        /// <summary>
        /// A string token.
        /// </summary>
        String,
        /// <summary>
        /// An unquoted string token.
        /// </summary>
        UnquotedString,
        /// <summary>
        /// An end of file token.
        /// </summary>
        EndOfFile
    }

    /// <summary>
    /// Represents a JSON token.
    /// </summary>
    public class JsonToken
    {
        // private fields
        private JsonTokenType _type;
        private string _lexeme;

        // constructors
        /// <summary>
        /// Initializes a new instance of the JsonToken class.
        /// </summary>
        /// <param name="type">The token type.</param>
        /// <param name="lexeme">The lexeme.</param>
        public JsonToken(JsonTokenType type, string lexeme)
        {
            _type = type;
            _lexeme = lexeme;
        }

        // public properties
        /// <summary>
        /// Gets the token type.
        /// </summary>
        public JsonTokenType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the lexeme.
        /// </summary>
        public string Lexeme
        {
            get { return _lexeme; }
        }

        /// <summary>
        /// Gets the value of a DateTime token.
        /// </summary>
        public virtual BsonDateTime DateTimeValue
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the value of a Double token.
        /// </summary>
        public virtual double DoubleValue
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the value of an Int32 token.
        /// </summary>
        public virtual int Int32Value
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the value of an Int64 token.
        /// </summary>
        public virtual long Int64Value
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether this token is number.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this token is number; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsNumber
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the value of an ObjectId token.
        /// </summary>
        public virtual ObjectId ObjectIdValue
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the value of a regular expression token.
        /// </summary>
        public virtual BsonRegularExpression RegularExpressionValue
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the value of a string token.
        /// </summary>
        public virtual string StringValue
        {
            get { throw new NotSupportedException(); }
        }
    }

    /// <summary>
    /// Represents a DateTime JSON token.
    /// </summary>
    public class DateTimeJsonToken : JsonToken
    {
        // private fields
        private BsonDateTime _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the DateTimeJsonToken class.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="value">The DateTime value.</param>
        public DateTimeJsonToken(string lexeme, BsonDateTime value)
            : base(JsonTokenType.DateTime, lexeme)
        {
            _value = value;
        }

        // public properties
        /// <summary>
        /// Gets the value of a DateTime token.
        /// </summary>
        public override BsonDateTime DateTimeValue
        {
            get { return _value; }
        }
    }

    /// <summary>
    /// Represents a Double JSON token.
    /// </summary>
    public class DoubleJsonToken : JsonToken
    {
        // private fields
        private double _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the DoubleJsonToken class.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="value">The Double value.</param>
        public DoubleJsonToken(string lexeme, double value)
            : base(JsonTokenType.Double, lexeme)
        {
            _value = value;
        }

        // public properties
        /// <summary>
        /// Gets the value of a Double token.
        /// </summary>
        public override double DoubleValue
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the value of an Int32 token.
        /// </summary>
        public override int Int32Value
        {
            get { return (int)_value; }
        }

        /// <summary>
        /// Gets the value of an Int64 token.
        /// </summary>
        public override long Int64Value
        {
            get { return (long)_value; }
        }

        /// <summary>
        /// Gets a value indicating whether this token is number.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this token is number; otherwise, <c>false</c>.
        /// </value>
        public override bool IsNumber
        {
            get { return true; }
        }
    }

    /// <summary>
    /// Represents an Int32 JSON token.
    /// </summary>
    public class Int32JsonToken : JsonToken
    {
        // private fields
        private int _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the Int32JsonToken class.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="value">The Int32 value.</param>
        public Int32JsonToken(string lexeme, int value)
            : base(JsonTokenType.Int32, lexeme)
        {
            _value = value;
        }

        // public properties
        /// <summary>
        /// Gets the value of a Double token.
        /// </summary>
        public override double DoubleValue
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the value of an Int32 token.
        /// </summary>
        public override int Int32Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the value of an Int32 token as an Int64.
        /// </summary>
        public override long Int64Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets a value indicating whether this token is number.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this token is number; otherwise, <c>false</c>.
        /// </value>
        public override bool IsNumber
        {
            get { return true; }
        }
    }

    /// <summary>
    /// Represents an Int64 JSON token.
    /// </summary>
    public class Int64JsonToken : JsonToken
    {
        // private fields
        private long _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the Int64JsonToken class.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="value">The Int64 value.</param>
        public Int64JsonToken(string lexeme, long value)
            : base(JsonTokenType.Int64, lexeme)
        {
            _value = value;
        }

        // public properties
        /// <summary>
        /// Gets the value of a Double token.
        /// </summary>
        public override double DoubleValue
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the value of an Int32 token.
        /// </summary>
        public override int Int32Value
        {
            get { return (int)_value; }
        }

        /// <summary>
        /// Gets the value of an Int64 token.
        /// </summary>
        public override long Int64Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets a value indicating whether this token is number.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this token is number; otherwise, <c>false</c>.
        /// </value>
        public override bool IsNumber
        {
            get { return true; }
        }
    }

    /// <summary>
    /// Represents an ObjectId JSON token.
    /// </summary>
    public class ObjectIdJsonToken : JsonToken
    {
        // private fields
        private ObjectId _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the ObjectIdJsonToken class.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="value">The ObjectId value.</param>
        public ObjectIdJsonToken(string lexeme, ObjectId value)
            : base(JsonTokenType.ObjectId, lexeme)
        {
            _value = value;
        }

        // public properties
        /// <summary>
        /// Gets the value of an ObjectId token.
        /// </summary>
        public override ObjectId ObjectIdValue
        {
            get { return _value; }
        }
    }

    /// <summary>
    /// Represents a regular expression JSON token.
    /// </summary>
    public class RegularExpressionJsonToken : JsonToken
    {
        // private fields
        private BsonRegularExpression _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the RegularExpressionJsonToken class.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="value">The BsonRegularExpression value.</param>
        public RegularExpressionJsonToken(string lexeme, BsonRegularExpression value)
            : base(JsonTokenType.RegularExpression, lexeme)
        {
            _value = value;
        }

        // public properties
        /// <summary>
        /// Gets the value of a regular expression token.
        /// </summary>
        public override BsonRegularExpression RegularExpressionValue
        {
            get { return _value; }
        }
    }

    /// <summary>
    /// Represents a String JSON token.
    /// </summary>
    public class StringJsonToken : JsonToken
    {
        // private fields
        private string _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the StringJsonToken class.
        /// </summary>
        /// <param name="type">The token type.</param>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="value">The String value.</param>
        public StringJsonToken(JsonTokenType type, string lexeme, string value)
            : base(type, lexeme)
        {
            _value = value;
        }

        // public properties
        /// <summary>
        /// Gets the value of an String token.
        /// </summary>
        public override string StringValue
        {
            get { return _value; }
        }
    }
}
