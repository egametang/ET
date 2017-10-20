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
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.Linq.Processors.Pipeline.MethodCallBinders
{
    internal class JoinSerializer<T> : SerializerBase<T>, IBsonDocumentSerializer
    {
        private readonly IBsonSerializer _sourceSerializer;
        private readonly string _sourceMemberName;
        private readonly IBsonSerializer _joinedSerializer;
        private readonly string _joinedFieldName;
        private readonly string _joinedMemberName;
        private readonly Func<object, object, object> _creator;

        public JoinSerializer(IBsonSerializer sourceSerializer, string sourceMemberName, IBsonSerializer joinedSerializer, string joinedMemberName, string joinedFieldName, Func<object, object, object> creator)
        {
            _sourceSerializer = sourceSerializer;
            _sourceMemberName = sourceMemberName;
            _joinedSerializer = joinedSerializer;
            _joinedFieldName = joinedFieldName;
            _joinedMemberName = joinedMemberName;
            _creator = creator;
        }

        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bookmark = context.Reader.GetBookmark();
            object original;
            using (var originalReader = new FieldHidingBsonReader(context.Reader, fieldName => fieldName == _joinedFieldName))
            {
                var childContext = BsonDeserializationContext.CreateRoot(originalReader);
                original = _sourceSerializer.Deserialize(childContext);
            }

            context.Reader.ReturnToBookmark(bookmark);
            object joined = null;
            context.Reader.ReadStartDocument();
            while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = context.Reader.ReadName();
                if (name != _joinedFieldName)
                {
                    context.Reader.SkipValue();
                }
                else
                {
                    joined = _joinedSerializer.Deserialize(context);
                }
            }
            context.Reader.ReadEndDocument();

            return (T)_creator(original, joined);
        }

        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            if (memberName == _sourceMemberName)
            {
                serializationInfo = new BsonSerializationInfo(null, _sourceSerializer, _sourceSerializer.ValueType);
                return true;
            }

            if (memberName == _joinedMemberName)
            {
                serializationInfo = new BsonSerializationInfo(_joinedFieldName, _joinedSerializer, _joinedSerializer.ValueType);
                return true;
            }

            serializationInfo = null;
            return false;
        }

        private class FieldHidingBsonReader : IBsonReader
        {
            private readonly IBsonReader _parent;
            private readonly Func<string, bool> _shouldHide;
            private string _currentName;
            private BsonReaderState? _currentState;
            private int _currentDepth;

            public FieldHidingBsonReader(IBsonReader parent, Func<string, bool> shouldHide)
            {
                _parent = parent;
                _shouldHide = shouldHide;
                _currentDepth = -1;
            }

            public BsonType CurrentBsonType
            {
                get { return _parent.CurrentBsonType; }
            }

            public BsonReaderState State
            {
                get
                {
                    return _currentDepth == 0 ?
                      _currentState.GetValueOrDefault(_parent.State) :
                      _parent.State;
                }
            }

            public void Close()
            {
                // we don't close
            }

            public void Dispose()
            {
                // we don't dispose
            }

            public BsonReaderBookmark GetBookmark()
            {
                var parentBookmark = _parent.GetBookmark();
                return new FieldHiderBookmark(parentBookmark, _currentState, _currentName, _currentDepth);
            }

            public BsonType GetCurrentBsonType()
            {
                return _parent.GetCurrentBsonType();
            }

            public bool IsAtEndOfFile()
            {
                return _parent.IsAtEndOfFile();
            }

            public BsonBinaryData ReadBinaryData()
            {
                return _parent.ReadBinaryData();
            }

            public bool ReadBoolean()
            {
                return _parent.ReadBoolean();
            }

            public BsonType ReadBsonType()
            {
                var type = _parent.ReadBsonType();
                if (_currentDepth == 0)
                {
                    while (type != BsonType.EndOfDocument)
                    {
                        var name = _parent.ReadName();
                        if (!_shouldHide(name))
                        {
                            _currentName = name;
                            _currentState = BsonReaderState.Name;
                            break;
                        }

                        _currentName = null;
                        _currentState = null;
                        _parent.SkipValue();
                        type = _parent.ReadBsonType();
                    }
                }

                return type;
            }

            public byte[] ReadBytes()
            {
                return _parent.ReadBytes();
            }

            public long ReadDateTime()
            {
                return _parent.ReadDateTime();
            }

            public Decimal128 ReadDecimal128()
            {
                return _parent.ReadDecimal128();
            }

            public double ReadDouble()
            {
                return _parent.ReadDouble();
            }

            public void ReadEndArray()
            {
                _currentDepth--;
                _parent.ReadEndArray();
            }

            public void ReadEndDocument()
            {
                _currentDepth--;
                _parent.ReadEndDocument();
            }

            public int ReadInt32()
            {
                return _parent.ReadInt32();
            }

            public long ReadInt64()
            {
                return _parent.ReadInt64();
            }

            public string ReadJavaScript()
            {
                return _parent.ReadJavaScript();
            }

            public string ReadJavaScriptWithScope()
            {
                return _parent.ReadJavaScriptWithScope();
            }

            public void ReadMaxKey()
            {
                _parent.ReadMaxKey();
            }

            public void ReadMinKey()
            {
                _parent.ReadMinKey();
            }

            public string ReadName(INameDecoder nameDecoder)
            {
                if (_currentDepth == 0)
                {
                    var name = _currentName;
                    if (name != null)
                    {
                        _currentName = null;
                        _currentState = null;
                        nameDecoder.Inform(name);
                        return name;
                    }
                }

                return _parent.ReadName(nameDecoder);
            }

            public void ReadNull()
            {
                _parent.ReadNull();
            }

            public ObjectId ReadObjectId()
            {
                return _parent.ReadObjectId();
            }

            public IByteBuffer ReadRawBsonArray()
            {
                return _parent.ReadRawBsonArray();
            }

            public IByteBuffer ReadRawBsonDocument()
            {
                return _parent.ReadRawBsonDocument();
            }

            public BsonRegularExpression ReadRegularExpression()
            {
                return _parent.ReadRegularExpression();
            }

            public void ReadStartArray()
            {
                _currentDepth++;
                _parent.ReadStartArray();
            }

            public void ReadStartDocument()
            {
                _currentDepth++;
                _parent.ReadStartDocument();
            }

            public string ReadString()
            {
                return _parent.ReadString();
            }

            public string ReadSymbol()
            {
                return _parent.ReadSymbol();
            }

            public long ReadTimestamp()
            {
                return _parent.ReadTimestamp();
            }

            public void ReadUndefined()
            {
                _parent.ReadUndefined();
            }

            public void ReturnToBookmark(BsonReaderBookmark bookmark)
            {
                var fieldHiderBookmark = (FieldHiderBookmark)bookmark;
                _parent.ReturnToBookmark(fieldHiderBookmark._parentBookmark);

                _currentDepth = fieldHiderBookmark._currentDepth;
                _currentName = fieldHiderBookmark._currentName;
                _currentState = fieldHiderBookmark._currentState;
            }

            public void SkipName()
            {
                if (_currentDepth == 0 && _currentName != null)
                {
                    _currentName = null;
                    _currentState = null;
                }
                else
                {
                    _parent.SkipName();
                }
            }

            public void SkipValue()
            {
                _parent.SkipValue();
            }

            private class FieldHiderBookmark : BsonReaderBookmark
            {
                internal readonly int _currentDepth;
                internal readonly string _currentName;
                internal readonly BsonReaderState? _currentState;
                internal readonly BsonReaderBookmark _parentBookmark;

                public FieldHiderBookmark(BsonReaderBookmark parentBookmark, BsonReaderState? currentState, string currentName, int currentDepth)
                    : base(currentState ?? parentBookmark.State, parentBookmark.CurrentBsonType, currentName ?? parentBookmark.CurrentName)
                {
                    _currentState = currentState;
                    _currentName = currentName;
                    _currentDepth = currentDepth;
                    _parentBookmark = parentBookmark;
                }
            }
        }
    }
}
