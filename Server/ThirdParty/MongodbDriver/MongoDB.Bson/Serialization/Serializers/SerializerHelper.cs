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
using System.Linq;
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a helper for serializers.
    /// </summary>
    public class SerializerHelper
    {
        // private fields
        private readonly long _allMemberFlags;
        private readonly long _extraMemberFlag;
        private readonly Member[] _members;
        private readonly long _requiredMemberFlags;
        private readonly BsonTrie<long> _trie;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerHelper"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public SerializerHelper(params Member[] members)
        {
            if (members == null)
            {
                throw new ArgumentNullException("members");
            }
            if (members.Length > 64)
            {
                throw new ArgumentException("SerializerHelper supports a maximum of 64 members.", "members");
            }

            _members = members;
            _trie = new BsonTrie<long>();

            foreach (var member in members)
            {
                _allMemberFlags |= member.Flag;
                if (!member.IsOptional)
                {
                    _requiredMemberFlags |= member.Flag;
                }

                if (member.ElementName == "*")
                {
                    _extraMemberFlag = member.Flag;
                }
                else
                {
                    _trie.Add(member.ElementName, member.Flag);
                }
            }
        }

        // public methods
        /// <summary>
        /// Deserializes the members.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="memberHandler">The member handler.</param>
        /// <returns>The found member flags.</returns>
        public long DeserializeMembers(BsonDeserializationContext context, Action<string, long> memberHandler)
        {
            var reader = context.Reader;
            var foundMemberFlags = 0L;

            reader.ReadStartDocument();
            while (reader.ReadBsonType() != 0)
            {
                var trieDecoder = new TrieNameDecoder<long>(_trie);
                var elementName = reader.ReadName(trieDecoder);

                long memberFlag;
                if (trieDecoder.Found)
                {
                    memberFlag = trieDecoder.Value;
                }
                else
                {
                    if (_extraMemberFlag == 0)
                    {
                        throw new BsonSerializationException(string.Format(
                            "Invalid element: '{0}'.", elementName));
                    }
                    else
                    {
                        memberFlag = _extraMemberFlag;
                    }
                }

                memberHandler(elementName, memberFlag);
                foundMemberFlags |= memberFlag;
            }
            reader.ReadEndDocument();

            var missingRequiredMemberFlags = _requiredMemberFlags & ~foundMemberFlags;
            if (missingRequiredMemberFlags != 0)
            {
                var missingRequiredMember = FindFirstMissingRequiredMember(missingRequiredMemberFlags);
                throw new BsonSerializationException(string.Format(
                   "Missing element: '{0}'.", missingRequiredMember.ElementName));
            }

            return foundMemberFlags;
        }

        // private methods
        private Member FindFirstMissingRequiredMember(long missingRequiredMemberFlags)
        {
            foreach (var member in _members)
            {
                if ((member.Flag & missingRequiredMemberFlags) != 0)
                {
                    return member;
                }
            }
            throw new BsonInternalException();
        }

        // nested types
        /// <summary>
        /// Represents information about a member.
        /// </summary>
        public class Member
        {
            // private static fields
            private static readonly long[] __validFlags = Enumerable.Range(0, 64).Select(i => 1L << i).ToArray();

            // private fields
            private readonly string _elementName;
            private readonly long _flag;
            private readonly bool _isOptional;

            // constuctors
            /// <summary>
            /// Initializes a new instance of the <see cref="Member" /> class.
            /// </summary>
            /// <param name="elementName">The name of the element.</param>
            /// <param name="flag">The flag.</param>
            /// <param name="isOptional">Whether the member is optional.</param>
            public Member(
                string elementName,
                long flag,
                bool isOptional = false)
            {
                if (string.IsNullOrEmpty(elementName))
                {
                    throw new ArgumentException(string.Format("Invalid element name: '{0}'.", elementName));
                }
                if (!__validFlags.Contains(flag))
                {
                    throw new ArgumentException(string.Format("Invalid member flag: {0:x}.", flag));
                }

                _elementName = elementName;
                _flag = flag;
                _isOptional = isOptional;
            }

            // public properties
            /// <summary>
            /// Gets the flag.
            /// </summary>
            /// <value>
            /// The flag.
            /// </value>
            public long Flag
            {
                get { return _flag; }
            }

            /// <summary>
            /// Gets the name of the element.
            /// </summary>
            /// <value>
            /// The name of the element.
            /// </value>
            public string ElementName
            {
                get { return _elementName; }
            }

            /// <summary>
            /// Gets a value indicating whether this member is optional.
            /// </summary>
            /// <value>Whether this member is optional.</value>
            public bool IsOptional
            {
                get { return _isOptional; }
            }
        }
    }
}
