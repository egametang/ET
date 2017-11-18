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
using System.IO;
using System.Text;
using UnityEngine;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a mapping from a set of UTF8 encoded strings to a set of elementName/value pairs, implemented as a trie.
    /// </summary>
    /// <typeparam name="TValue">The type of the BsonTrie values.</typeparam>
    public class BsonTrie<TValue>
    {
        // private fields
        private readonly BsonTrieNode<TValue> _root;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonTrie class.
        /// </summary>
        public BsonTrie()
        {
            _root = new BsonTrieNode<TValue>(0);
        }

        // public properties
        /// <summary>
        /// Gets the root node.
        /// </summary>
        public BsonTrieNode<TValue> Root
        {
            get
            {
                return _root;
            }
        }

        // public methods
        /// <summary>
        /// Adds the specified elementName (after encoding as a UTF8 byte sequence) and value to the trie.
        /// </summary>
        /// <param name="elementName">The element name to add.</param>
        /// <param name="value">The value to add. The value can be null for reference types.</param>
        public void Add(string elementName, TValue value)
        {
            var utf8 = Utf8Encodings.Strict.GetBytes(elementName);

            var node = _root;
            foreach (var keyByte in utf8)
            {
                var child = node.GetChild(keyByte);
                if (child == null)
                {
                    child = new BsonTrieNode<TValue>(keyByte);
                    node.AddChild(child);
                }
                node = child;
            }

            node.SetValue(elementName, value);
        }

        /// <summary>
        /// Gets the node associated with the specified element name.
        /// </summary>
        /// <param name="utf8">The element name.</param>
        /// <param name="node">
        /// When this method returns, contains the node associated with the specified element name, if the key is found;
        /// otherwise, null. This parameter is passed unitialized.
        /// </param>
        /// <returns>True if the node was found; otherwise, false.</returns>
        public bool TryGetNode(ArraySegment<byte> utf8, out BsonTrieNode<TValue> node)
        {
            node = _root;
            for (var i = 0; node != null && i < utf8.Count; i++)
            {
                var keyByte = utf8.Array[utf8.Offset + i];
                node = node.GetChild(keyByte);
            }

            return node != null;
        }

        /// <summary>
        /// Tries to get the node associated with a name read from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="node">The node.</param>
        /// <returns>
        /// True if the node was found.
        /// If the node was found the stream is advanced over the name, otherwise
        /// the stream is repositioned to the beginning of the name.
        /// </returns>
        public bool TryGetNode(BsonStream stream, out BsonTrieNode<TValue> node)
        {
            var position = stream.Position;
            var utf8 = stream.ReadCStringBytes();

            if (TryGetNode(utf8, out node))
            {
                return true;
            }

            stream.Position = position;
            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified element name.
        /// </summary>
        /// <param name="utf8">The element name.</param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified element name, if the key is found;
        /// otherwise, the default value for the type of the value parameter. This parameter is passed unitialized.
        /// </param>
        /// <returns>True if the value was found; otherwise, false.</returns>
        public bool TryGetValue(ArraySegment<byte> utf8, out TValue value)
        {
            BsonTrieNode<TValue> node;
            if (TryGetNode(utf8, out node) && node.HasValue)
            {
                value = node.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified element name.
        /// </summary>
        /// <param name="elementName">The element name.</param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified element name, if the key is found;
        /// otherwise, the default value for the type of the value parameter. This parameter is passed unitialized.
        /// </param>
        /// <returns>True if the value was found; otherwise, false.</returns>
        public bool TryGetValue(string elementName, out TValue value)
        {
            var bytes = Utf8Encodings.Strict.GetBytes(elementName);
            var utf8 = new ArraySegment<byte>(bytes, 0, bytes.Length);
            return TryGetValue(utf8, out value);
        }
    }

    /// <summary>
    /// Represents a node in a BsonTrie.
    /// </summary>
    /// <typeparam name="TValue">The type of the BsonTrie values.</typeparam>
    public sealed class BsonTrieNode<TValue>
    {
        // private fields
        private readonly byte _keyByte;
        private string _elementName;
        private TValue _value;
        private BsonTrieNode<TValue> _onlyChild; // used when there is only one child
        private BsonTrieNode<TValue>[] _children; // used when there are two or more children
        private byte[] _childrenIndexes; // maps key bytes into indexes into the _children array
        private byte _minChildKeyByte; // key byte value of first element in _childrenIndexes

        // constructors
        internal BsonTrieNode(byte keyByte)
        {
            _keyByte = keyByte;
        }

        /// <summary>
        /// Gets whether this node has a value.
        /// </summary>
        public bool HasValue
        {
            get
            {
                return _elementName != null;
            }
        }

        /// <summary>
        /// Gets the element name for this node.
        /// </summary>
        public string ElementName
        {
            get
            {
                if (_elementName == null)
                {
                    throw new InvalidOperationException("BsonTrieNode doesn't have a value.");
                }

                return _elementName;
            }
        }

        /// <summary>
        /// Gets the value for this node.
        /// </summary>
        public TValue Value
        {
            get
            {
                if (_elementName == null)
                {
                    throw new InvalidOperationException("BsonTrieNode doesn't have a value.");
                }

                return _value;
            }
        }

        // public methods
        /// <summary>
        /// Gets the child of this node for a given key byte.
        /// </summary>
        /// <param name="keyByte">The key byte.</param>
        /// <returns>The child node if it exists; otherwise, null.</returns>
        public BsonTrieNode<TValue> GetChild(byte keyByte)
        {
            if (_onlyChild != null)
            {
                // optimization for nodes that have only one child
                if (_onlyChild._keyByte == keyByte)
                {
                    return _onlyChild;
                }
            }
            else if (_children != null)
            {
                // 这里做了修改，il2cpp uint跟int比较有bug
                int index = keyByte - _minChildKeyByte;
                if (index < 0)
                {
                    return null;
                }
                if (index < _childrenIndexes.Length)
                {
                    index = _childrenIndexes[index];
                    if (index < _children.Length)
                    {
                        return _children[index];
                    }
                }
            }
            return null;
        }

        // internal methods
        internal void AddChild(BsonTrieNode<TValue> child)
        {
            if (GetChild(child._keyByte) != null)
            {
                throw new ArgumentException("BsonTrieNode already contains a child with the same keyByte.");
            }

            if (_children != null)
            {
                // add a new child to the existing _children
                var children = new BsonTrieNode<TValue>[_children.Length + 1];
                Array.Copy(_children, children, _children.Length);
                children[children.Length - 1] = child;

                var childrenIndexes = _childrenIndexes;
                var minChildKeyByte = _minChildKeyByte;
                var maxChildKeyByte = _minChildKeyByte + _childrenIndexes.Length - 1;

                // if new keyByte doesn't fall within existing min/max range expand the range
                if (child._keyByte < minChildKeyByte)
                {
                    // grow the indexes on the min side
                    minChildKeyByte = child._keyByte;
                    childrenIndexes = new byte[maxChildKeyByte - minChildKeyByte + 1];
                    var sizeDelta = childrenIndexes.Length - _childrenIndexes.Length;
                    for (var i = 0; i < sizeDelta; i++)
                    {
                        childrenIndexes[i] = 255;
                    }
                    Array.Copy(_childrenIndexes, 0, childrenIndexes, sizeDelta, _childrenIndexes.Length);
                }
                else if (child._keyByte > maxChildKeyByte)
                {
                    // grow the indexes on the max side
                    maxChildKeyByte = child._keyByte;
                    childrenIndexes = new byte[maxChildKeyByte - minChildKeyByte + 1];
                    Array.Copy(_childrenIndexes, 0, childrenIndexes, 0, _childrenIndexes.Length);
                    for (var i = _childrenIndexes.Length; i < childrenIndexes.Length; i++)
                    {
                        childrenIndexes[i] = 255;
                    }
                }
                childrenIndexes[child._keyByte - minChildKeyByte] = (byte)(children.Length - 1);

                _children = children;
                _childrenIndexes = childrenIndexes;
                _minChildKeyByte = minChildKeyByte;
            }
            else if (_onlyChild != null)
            {
                // switch from having an _onlyChild to having two _children
                var children = new BsonTrieNode<TValue>[2];
                children[0] = _onlyChild;
                children[1] = child;

                var minChildKeyByte = _onlyChild._keyByte;
                var maxChildKeyByte = child._keyByte;
                if (minChildKeyByte > maxChildKeyByte)
                {
                    minChildKeyByte = child._keyByte;
                    maxChildKeyByte = _onlyChild._keyByte;
                }

                var childrenIndexes = new byte[maxChildKeyByte - minChildKeyByte + 1];
                for (var i = 0; i < childrenIndexes.Length; i++)
                {
                    childrenIndexes[i] = 255;
                }
                childrenIndexes[_onlyChild._keyByte - minChildKeyByte] = 0;
                childrenIndexes[child._keyByte - minChildKeyByte] = 1;

                _onlyChild = null;
                _children = children;
                _childrenIndexes = childrenIndexes;
                _minChildKeyByte = minChildKeyByte;
            }
            else
            {
                _onlyChild = child;
            }
        }

        internal void SetValue(string elementName, TValue value)
        {
            if (elementName == null)
            {
                throw new ArgumentNullException("elementName");
            }
            if (_elementName != null)
            {
                throw new InvalidOperationException("BsonTrieNode already has a value.");
            }

            _elementName = elementName;
            _value = value;
        }
    }
}
