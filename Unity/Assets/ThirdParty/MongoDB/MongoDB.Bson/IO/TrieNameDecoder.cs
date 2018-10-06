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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a Trie-based name decoder that also provides a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class TrieNameDecoder<TValue> : INameDecoder
    {
        // private fields
        private bool _found;
        private readonly BsonTrie<TValue> _trie;
        private TValue _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TrieNameDecoder{TValue}"/> class.
        /// </summary>
        /// <param name="trie">The trie.</param>
        public TrieNameDecoder(BsonTrie<TValue> trie)
        {
            _trie = trie;
        }

        // public properties
        /// <summary>
        /// Gets a value indicating whether this <see cref="TrieNameDecoder{TValue}"/> is found.
        /// </summary>
        /// <value>
        ///   <c>true</c> if found; otherwise, <c>false</c>.
        /// </value>
        public bool Found
        {
            get { return _found; }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public TValue Value
        {
            get { return _value; }
        }

        // public methods
        /// <summary>
        /// Reads the name.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>
        /// The name.
        /// </returns>
        public string Decode(BsonStream stream, UTF8Encoding encoding)
        {
            BsonTrieNode<TValue> node;
            var oldPosition = stream.Position;
            if (_trie.TryGetNode(stream, out node))
            {
                if (node.HasValue)
                {
                    _found = true;
                    _value = node.Value;
                    return node.ElementName;
                }

                stream.Position = oldPosition;
            }

            return stream.ReadCString(encoding);
        }

        /// <summary>
        /// Informs the decoder of an already decoded name (so the decoder can change state if necessary).
        /// </summary>
        /// <param name="name">The name.</param>
        public void Inform(string name)
        {
            _found = _trie.TryGetValue(name, out _value);
        }
    }
}
