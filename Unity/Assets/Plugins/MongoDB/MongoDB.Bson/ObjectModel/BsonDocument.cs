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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Shared;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON document.
    /// </summary>
    [Serializable]
    public class BsonDocument : BsonValue, IBsonSerializable, IComparable<BsonDocument>, IConvertibleToBsonDocument, IEnumerable<BsonElement>, IEquatable<BsonDocument>
    {
        // private fields
        // use a list and a dictionary because we want to preserve the order in which the elements were added
        // if duplicate names are present only the first one will be in the dictionary (the others can only be accessed by index)
        private List<BsonElement> _elements = new List<BsonElement>();
        private Dictionary<string, int> _indexes = new Dictionary<string, int>(); // maps names to indexes into elements list
        private bool _allowDuplicateNames;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDocument class.
        /// </summary>
        public BsonDocument()
            : base(BsonType.Document)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class specifying whether duplicate element names are allowed
        /// (allowing duplicate element names is not recommended).
        /// </summary>
        /// <param name="allowDuplicateNames">Whether duplicate element names are allowed.</param>
        public BsonDocument(bool allowDuplicateNames)
            : base(BsonType.Document)
        {
            _allowDuplicateNames = allowDuplicateNames;
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds one element.
        /// </summary>
        /// <param name="element">An element to add to the document.</param>
        public BsonDocument(BsonElement element)
            : base(BsonType.Document)
        {
            Add(element);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">A dictionary to initialize the document from.</param>
        public BsonDocument(Dictionary<string, object> dictionary)
            : base(BsonType.Document)
        {
            AddRange(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">A dictionary to initialize the document from.</param>
        /// <param name="keys">A list of keys to select values from the dictionary.</param>
        [Obsolete("Use BsonDocument(IEnumerable<BsonElement> elements) instead.")]
        public BsonDocument(Dictionary<string, object> dictionary, IEnumerable<string> keys)
            : base(BsonType.Document)
        {
            Add(dictionary, keys);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">A dictionary to initialize the document from.</param>
        public BsonDocument(IEnumerable<KeyValuePair<string, object>> dictionary)
            : base(BsonType.Document)
        {
            AddRange(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">A dictionary to initialize the document from.</param>
        /// <param name="keys">A list of keys to select values from the dictionary.</param>
        [Obsolete("Use BsonDocument(IEnumerable<BsonElement> elements) instead.")]
        public BsonDocument(IDictionary<string, object> dictionary, IEnumerable<string> keys)
            : base(BsonType.Document)
        {
            Add(dictionary, keys);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">A dictionary to initialize the document from.</param>
        public BsonDocument(IDictionary dictionary)
            : base(BsonType.Document)
        {
            AddRange(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">A dictionary to initialize the document from.</param>
        /// <param name="keys">A list of keys to select values from the dictionary.</param>
        [Obsolete("Use BsonDocument(IEnumerable<BsonElement> elements) instead.")]
        public BsonDocument(IDictionary dictionary, IEnumerable keys)
            : base(BsonType.Document)
        {
            Add(dictionary, keys);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a list of elements.
        /// </summary>
        /// <param name="elements">A list of elements to add to the document.</param>
        public BsonDocument(IEnumerable<BsonElement> elements)
            : base(BsonType.Document)
        {
            AddRange(elements);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds one or more elements.
        /// </summary>
        /// <param name="elements">One or more elements to add to the document.</param>
        [Obsolete("Use BsonDocument(IEnumerable<BsonElement> elements) instead.")]
        public BsonDocument(params BsonElement[] elements)
            : base(BsonType.Document)
        {
            Add(elements);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and creates and adds a new element.
        /// </summary>
        /// <param name="name">The name of the element to add to the document.</param>
        /// <param name="value">The value of the element to add to the document.</param>
        public BsonDocument(string name, BsonValue value)
            : base(BsonType.Document)
        {
            Add(name, value);
        }

        // public operators
        /// <summary>
        /// Compares two BsonDocument values.
        /// </summary>
        /// <param name="lhs">The first BsonDocument.</param>
        /// <param name="rhs">The other BsonDocument.</param>
        /// <returns>True if the two BsonDocument values are not equal according to ==.</returns>
        public static bool operator !=(BsonDocument lhs, BsonDocument rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two BsonDocument values.
        /// </summary>
        /// <param name="lhs">The first BsonDocument.</param>
        /// <param name="rhs">The other BsonDocument.</param>
        /// <returns>True if the two BsonDocument values are equal according to ==.</returns>
        public static bool operator ==(BsonDocument lhs, BsonDocument rhs)
        {
            return object.Equals(lhs, rhs); // handles lhs == null correctly
        }

        // public properties
        /// <summary>
        /// Gets or sets whether to allow duplicate names (allowing duplicate names is not recommended).
        /// </summary>
        public bool AllowDuplicateNames
        {
            get { return _allowDuplicateNames; }
            set { _allowDuplicateNames = value; }
        }

        // ElementCount could be greater than the number of Names if allowDuplicateNames is true
        /// <summary>
        /// Gets the number of elements.
        /// </summary>
        public virtual int ElementCount
        {
            get { return _elements.Count; }
        }

        /// <summary>
        /// Gets the elements.
        /// </summary>
        public virtual IEnumerable<BsonElement> Elements
        {
            get { return _elements; }
        }

        /// <summary>
        /// Gets the element names.
        /// </summary>
        public virtual IEnumerable<string> Names
        {
            get { return _elements.Select(e => e.Name); }
        }

        /// <summary>
        /// Gets the raw values (see BsonValue.RawValue).
        /// </summary>
        [Obsolete("Use Values instead.")]
        public virtual IEnumerable<object> RawValues
        {
            get { return _elements.Select(e => e.Value.RawValue); }
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        public virtual IEnumerable<BsonValue> Values
        {
            get { return _elements.Select(e => e.Value); }
        }

        // public indexers
        // note: the return type of the indexers is BsonValue and NOT BsonElement so that we can write code like:
        //     BsonDocument car;
        //     car["color"] = "red"; // changes value of existing element or adds new element
        //         note: we are using implicit conversion from string to BsonValue
        // to convert the returned BsonValue to a .NET type you have two approaches (explicit cast or As method):
        //     string color = (string) car["color"]; // throws exception if value is not a string (returns null if not found)
        //     string color = car["color"].AsString; // throws exception if value is not a string (results in a NullReferenceException if not found)
        //     string color = car["color", "none"].AsString; // throws exception if value is not a string (default to "none" if not found)
        // the second approach offers a more fluent interface (with fewer parenthesis!)
        //     string name = car["brand"].AsBsonSymbol.Name;
        //     string name = ((BsonSymbol) car["brand"]).Name; // the extra parenthesis are required and harder to read
        // there are also some conversion methods (and note that ToBoolean uses the JavaScript definition of truthiness)
        //     bool ok = result["ok"].ToBoolean(); // works whether ok is false, true, 0, 0.0, 1, 1.0, "", "xyz", BsonNull.Value, etc...
        //     bool ok = result["ok", false].ToBoolean(); // defaults to false if ok element is not found
        //     int n = result["n"].ToInt32(); // works whether n is Int32, Int64, Double or String (if it can be parsed)
        //     long n = result["n"].ToInt64(); // works whether n is Int32, Int64, Double or String (if it can be parsed)
        //     double d = result["n"].ToDouble(); // works whether d is Int32, Int64, Double or String (if it can be parsed)
        // to work in terms of BsonElements use Add, GetElement and SetElement
        //     car.Add(new BsonElement("color", "red")); // might throw exception if allowDuplicateNames is false
        //     car.SetElement(new BsonElement("color", "red")); // replaces existing element or adds new element
        //     BsonElement colorElement = car.GetElement("color"); // returns null if element "color" is not found

        /// <summary>
        /// Gets or sets a value by position.
        /// </summary>
        /// <param name="index">The position.</param>
        /// <returns>The value.</returns>
        public override BsonValue this[int index]
        {
            get { return _elements[index].Value; }
            set {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _elements[index].Value = value;
            }
        }

        /// <summary>
        /// Gets the value of an element or a default value if the element is not found.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="defaultValue">The default value to return if the element is not found.</param>
        /// <returns>Teh value of the element or a default value if the element is not found.</returns>
        [Obsolete("Use GetValue(string name, BsonValue defaultValue) instead.")]
        public virtual BsonValue this[string name, BsonValue defaultValue]
        {
            get { return GetValue(name, defaultValue); }
        }

        /// <summary>
        /// Gets or sets a value by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The value.</returns>
        public override BsonValue this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }
                int index;
                if (_indexes.TryGetValue(name, out index))
                {
                    return _elements[index].Value;
                }
                else
                {
                    string message = string.Format("Element '{0}' not found.", name);
                    throw new KeyNotFoundException(message);
                }
            }
            set
            {
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                int index;
                if (_indexes.TryGetValue(name, out index))
                {
                    _elements[index].Value = value;
                }
                else
                {
                    Add(new BsonElement(name, value));
                }
            }
        }

        // public static methods
        /// <summary>
        /// Creates a new BsonDocument by mapping an object to a BsonDocument.
        /// </summary>
        /// <param name="value">The object to be mapped to a BsonDocument.</param>
        /// <returns>A BsonDocument.</returns>
        public new static BsonDocument Create(object value)
        {
            if (value != null)
            {
                return (BsonDocument)BsonTypeMapper.MapToBsonValue(value, BsonType.Document);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a JSON string and returns a BsonDocument.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>A BsonDocument.</returns>
        public static BsonDocument Parse(string json)
        {
            using (var bsonReader = BsonReader.Create(json))
            {
                return (BsonDocument)BsonDocumentSerializer.Instance.Deserialize(bsonReader, typeof(BsonDocument), null);
            }
        }

        /// <summary>
        /// Reads a BsonDocument from a BsonBuffer.
        /// </summary>
        /// <param name="buffer">The BsonBuffer.</param>
        /// <returns>A BsonDocument.</returns>
        [Obsolete("Use BsonSerializer.Deserialize<BsonDocument> instead.")]
        public static BsonDocument ReadFrom(BsonBuffer buffer)
        {
            using (BsonReader bsonReader = BsonReader.Create(buffer))
            {
                return BsonSerializer.Deserialize<BsonDocument>(bsonReader);
            }
        }

        /// <summary>
        /// Reads a BsonDocument from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <returns>A BsonDocument.</returns>
        [Obsolete("Use BsonSerializer.Deserialize<BsonDocument> instead.")]
        public static new BsonDocument ReadFrom(BsonReader bsonReader)
        {
            return BsonSerializer.Deserialize<BsonDocument>(bsonReader);
        }

        /// <summary>
        /// Reads a BsonDocument from a byte array.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>A BsonDocument.</returns>
        [Obsolete("Use BsonSerializer.Deserialize<BsonDocument> instead.")]
        public static BsonDocument ReadFrom(byte[] bytes)
        {
            return BsonSerializer.Deserialize<BsonDocument>(bytes);
        }

        /// <summary>
        /// Reads a BsonDocument from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>A BsonDocument.</returns>
        [Obsolete("Use BsonSerializer.Deserialize<BsonDocument> instead.")]
        public static BsonDocument ReadFrom(Stream stream)
        {
            return BsonSerializer.Deserialize<BsonDocument>(stream);
        }

        /// <summary>
        /// Reads a BsonDocument from a file.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <returns>A BsonDocument.</returns>
        [Obsolete("Use BsonSerializer.Deserialize<BsonDocument> instead.")]
        public static BsonDocument ReadFrom(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                return BsonSerializer.Deserialize<BsonDocument>(stream);
            }
        }

        // public methods
        /// <summary>
        /// Adds an element to the document.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public virtual BsonDocument Add(BsonElement element)
        {
            if (element != null)
            {
                bool found;
                int index;
                if ((found = _indexes.TryGetValue(element.Name, out index)) && !_allowDuplicateNames)
                {
                    var message = string.Format("Duplicate element name '{0}'.", element.Name);
                    throw new InvalidOperationException(message);
                }
                else
                {
                    _elements.Add(element);
                    if (!found)
                    {
                        _indexes.Add(element.Name, _elements.Count - 1); // index of the newly added element
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange instead.")]
        public virtual BsonDocument Add(Dictionary<string, object> dictionary)
        {
            return AddRange(dictionary);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keys">Which keys of the hash table to add.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange(IEnumerable<BsonElement> elements) instead.")]
        public virtual BsonDocument Add(Dictionary<string, object> dictionary, IEnumerable<string> keys)
        {
            return Add((IDictionary<string, object>)dictionary, keys);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange instead.")]
        public virtual BsonDocument Add(IDictionary<string, object> dictionary)
        {
            return AddRange(dictionary);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keys">Which keys of the hash table to add.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange(IEnumerable<BsonElement> elements) instead.")]
        public virtual BsonDocument Add(IDictionary<string, object> dictionary, IEnumerable<string> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            if (dictionary != null)
            {
                foreach (var key in keys)
                {
                    Add(key, BsonTypeMapper.MapToBsonValue(dictionary[key]));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange instead.")]
        public virtual BsonDocument Add(IDictionary dictionary)
        {
            return AddRange(dictionary);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keys">Which keys of the hash table to add.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange(IEnumerable<BsonElement> elements) instead.")]
        public virtual BsonDocument Add(IDictionary dictionary, IEnumerable keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            if (dictionary != null)
            {
                foreach (var key in keys)
                {
                    if (key.GetType() != typeof(string))
                    {
                        throw new ArgumentOutOfRangeException("keys", "A key passed to BsonDocument.Add is not a string.");
                    }
                    Add((string)key, BsonTypeMapper.MapToBsonValue(dictionary[key]));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds a list of elements to the document.
        /// </summary>
        /// <param name="elements">The list of elements.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange instead.")]
        public virtual BsonDocument Add(IEnumerable<BsonElement> elements)
        {
            return AddRange(elements);
        }

        /// <summary>
        /// Adds a list of elements to the document.
        /// </summary>
        /// <param name="elements">The list of elements.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange(IEnumerable<BsonElement> elements) instead.")]
        public virtual BsonDocument Add(params BsonElement[] elements)
        {
            return AddRange((IEnumerable<BsonElement>)elements);
        }

        /// <summary>
        /// Creates and adds an element to the document.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The value of the element.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public virtual BsonDocument Add(string name, BsonValue value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value != null)
            {
                Add(new BsonElement(name, value));
            }
            return this;
        }

        /// <summary>
        /// Creates and adds an element to the document, but only if the condition is true.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The value of the element.</param>
        /// <param name="condition">Whether to add the element to the document.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public virtual BsonDocument Add(string name, BsonValue value, bool condition)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value != null && condition)
            {
                Add(new BsonElement(name, value));
            }
            return this;
        }

        /// <summary>
        /// Creates and adds an element to the document, but only if the condition is true.
        /// If the condition is false the value factory is not called at all.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="valueFactory">A delegate called to compute the value of the element if condition is true.</param>
        /// <param name="condition">Whether to add the element to the document.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public virtual BsonDocument Add(string name, Func<BsonValue> valueFactory, bool condition)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (valueFactory == null)
            {
                throw new ArgumentNullException("valueFactory");
            }
            if (condition)
            {
                Add(new BsonElement(name, valueFactory()));
            }
            return this;
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public virtual BsonDocument AddRange(Dictionary<string, object> dictionary)
        {
            return AddRange((IEnumerable<KeyValuePair<string, object>>)dictionary);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public virtual BsonDocument AddRange(IDictionary dictionary)
        {
            if (dictionary != null)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.Key.GetType() != typeof(string))
                    {
                        throw new ArgumentOutOfRangeException("dictionary", "One or more keys in the dictionary passed to BsonDocument.AddRange is not a string.");
                    }
                    Add((string)entry.Key, BsonTypeMapper.MapToBsonValue(entry.Value));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds a list of elements to the document.
        /// </summary>
        /// <param name="elements">The list of elements.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public virtual BsonDocument AddRange(IEnumerable<BsonElement> elements)
        {
            if (elements != null)
            {
                foreach (var element in elements)
                {
                    Add(element);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public virtual BsonDocument AddRange(IEnumerable<KeyValuePair<string, object>> dictionary)
        {
            if (dictionary != null)
            {
                foreach (var entry in dictionary)
                {
                    Add(entry.Key, BsonTypeMapper.MapToBsonValue(entry.Value));
                }
            }
            return this;
        }

        /// <summary>
        /// Clears the document (removes all elements).
        /// </summary>
        public virtual void Clear()
        {
            _elements.Clear();
            _indexes.Clear();
        }

        /// <summary>
        /// Creates a shallow clone of the document (see also DeepClone).
        /// </summary>
        /// <returns>A shallow clone of the document.</returns>
        public override BsonValue Clone()
        {
            BsonDocument clone = new BsonDocument();
            foreach (BsonElement element in _elements)
            {
                clone.Add(element.Clone());
            }
            return clone;
        }

        /// <summary>
        /// Compares this document to another document.
        /// </summary>
        /// <param name="rhs">The other document.</param>
        /// <returns>A 32-bit signed integer that indicates whether this document is less than, equal to, or greather than the other.</returns>
        public virtual int CompareTo(BsonDocument rhs)
        {
            if (rhs == null) { return 1; }

            // lhs and rhs might be subclasses of BsonDocument
            using (var lhsEnumerator = Elements.GetEnumerator())
            using (var rhsEnumerator = rhs.Elements.GetEnumerator())
            {
                while (true)
                {
                    var lhsHasNext = lhsEnumerator.MoveNext();
                    var rhsHasNext = rhsEnumerator.MoveNext();
                    if (!lhsHasNext && !rhsHasNext) { return 0; }
                    if (!lhsHasNext) { return -1; }
                    if (!rhsHasNext) { return 1; }

                    var lhsElement = lhsEnumerator.Current;
                    var rhsElement = rhsEnumerator.Current;
                    var result = lhsElement.Name.CompareTo(rhsElement.Name);
                    if (result != 0) { return result; }
                    result = lhsElement.Value.CompareTo(rhsElement.Value);
                    if (result != 0) { return result; }
                }
            }
        }

        /// <summary>
        /// Compares the BsonDocument to another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonDocument is less than, equal to, or greather than the other BsonValue.</returns>
        public override int CompareTo(BsonValue other)
        {
            if (other == null) { return 1; }
            var otherDocument = other as BsonDocument;
            if (otherDocument != null)
            {
                return CompareTo(otherDocument);
            }
            return CompareTypeTo(other);
        }

        /// <summary>
        /// Tests whether the document contains an element with the specified name.
        /// </summary>
        /// <param name="name">The name of the element to look for.</param>
        /// <returns>True if the document contains an element with the specified name.</returns>
        public virtual bool Contains(string name)
        {
            return _indexes.ContainsKey(name);
        }

        /// <summary>
        /// Tests whether the document contains an element with the specified value.
        /// </summary>
        /// <param name="value">The value of the element to look for.</param>
        /// <returns>True if the document contains an element with the specified value.</returns>
        public virtual bool ContainsValue(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return _elements.Any(e => e.Value == value);
        }

        /// <summary>
        /// Creates a deep clone of the document (see also Clone).
        /// </summary>
        /// <returns>A deep clone of the document.</returns>
        public override BsonValue DeepClone()
        {
            BsonDocument clone = new BsonDocument();
            foreach (BsonElement element in _elements)
            {
                clone.Add(element.DeepClone());
            }
            return clone;
        }

        /// <summary>
        /// Deserializes the document from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object (ignored, but should be BsonDocument).</param>
        /// <param name="options">The serialization options (ignored).</param>
        /// <returns>The document (which has now been initialized by deserialization), or null.</returns>
        [Obsolete("Deserialize was intended to be private and will become private in a future release.")]
        public virtual object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            return BsonDocumentSerializer.Instance.Deserialize(bsonReader, nominalType, options);
        }

        /// <summary>
        /// Gets the Id of the document.
        /// </summary>
        /// <param name="id">The Id of the document (the RawValue if it has one, otherwise the element Value).</param>
        /// <param name="idNominalType">The nominal type of the Id.</param>
        /// <param name="idGenerator">The IdGenerator for the Id (or null).</param>
        /// <returns>True (a BsonDocument either has an Id member or one can be added).</returns>
        [Obsolete("GetDocumentId was intended to be private and will become private in a future release. Use document[\"_id\"] or document.GetValue(\"_id\") instead.")]
        public virtual bool GetDocumentId(out object id, out Type idNominalType, out IIdGenerator idGenerator)
        {
            var idProvider = (IBsonIdProvider)BsonDocumentSerializer.Instance;
            return idProvider.GetDocumentId(this, out id, out idNominalType, out idGenerator);
        }

        /// <summary>
        /// Compares this document to another document.
        /// </summary>
        /// <param name="obj">The other document.</param>
        /// <returns>True if the two documents are equal.</returns>
        public bool Equals(BsonDocument obj)
        {
            return Equals((object)obj); // handles obj == null correctly
        }

        /// <summary>
        /// Compares this BsonDocument to another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is a BsonDocument and equal to this one.</returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null) || !(obj is BsonDocument)) { return false; }

            // lhs and rhs might be subclasses of BsonDocument
            var rhs = (BsonDocument)obj;
            return Elements.SequenceEqual(rhs.Elements);
        }

        /// <summary>
        /// Gets an element of this document.
        /// </summary>
        /// <param name="index">The zero based index of the element.</param>
        /// <returns>The element.</returns>
        public virtual BsonElement GetElement(int index)
        {
            return _elements[index];
        }

        /// <summary>
        /// Gets an element of this document.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <returns>A BsonElement.</returns>
        public virtual BsonElement GetElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            int index;
            if (_indexes.TryGetValue(name, out index))
            {
                return _elements[index];
            }
            else
            {
                string message = string.Format("Element '{0}' not found.", name);
                throw new KeyNotFoundException(message);
            }
        }

        /// <summary>
        /// Gets an enumerator that can be used to enumerate the elements of this document.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public virtual IEnumerator<BsonElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(BsonType)
                .HashElements(Elements)
                .GetHashCode();
        }

        /// <summary>
        /// Gets the value of an element.
        /// </summary>
        /// <param name="index">The zero based index of the element.</param>
        /// <returns>The value of the element.</returns>
        public virtual BsonValue GetValue(int index)
        {
            return this[index];
        }

        /// <summary>
        /// Gets the value of an element.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <returns>The value of the element.</returns>
        public virtual BsonValue GetValue(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return this[name];
        }

        /// <summary>
        /// Gets the value of an element or a default value if the element is not found.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="defaultValue">The default value returned if the element is not found.</param>
        /// <returns>The value of the element or the default value if the element is not found.</returns>
        public virtual BsonValue GetValue(string name, BsonValue defaultValue)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            int index;
            if (_indexes.TryGetValue(name, out index))
            {
                return _elements[index].Value;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Inserts a new element at a specified position.
        /// </summary>
        /// <param name="index">The position of the new element.</param>
        /// <param name="element">The element.</param>
        public virtual void InsertAt(int index, BsonElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (_indexes.ContainsKey(element.Name) && !_allowDuplicateNames)
            {
                var message = string.Format("Duplicate element name '{0}' not allowed.", element.Name);
                throw new InvalidOperationException(message);
            }
            else
            {
                _elements.Insert(index, element);
                RebuildDictionary();
            }
        }

        /// <summary>
        /// Merges another document into this one. Existing elements are not overwritten.
        /// </summary>
        /// <param name="document">The other document.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public virtual BsonDocument Merge(BsonDocument document)
        {
            Merge(document, false); // don't overwriteExistingElements
            return this;
        }

        /// <summary>
        /// Merges another document into this one, specifying whether existing elements are overwritten.
        /// </summary>
        /// <param name="document">The other document.</param>
        /// <param name="overwriteExistingElements">Whether to overwrite existing elements.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public virtual BsonDocument Merge(BsonDocument document, bool overwriteExistingElements)
        {
            if (document != null)
            {
                foreach (BsonElement element in document)
                {
                    if (overwriteExistingElements || !Contains(element.Name))
                    {
                        this[element.Name] = element.Value;
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Removes an element from this document (if duplicate element names are allowed
        /// then all elements with this name will be removed).
        /// </summary>
        /// <param name="name">The name of the element to remove.</param>
        public virtual void Remove(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (_indexes.ContainsKey(name))
            {
                _elements.RemoveAll(e => e.Name == name);
                RebuildDictionary();
            }
        }

        /// <summary>
        /// Removes an element from this document.
        /// </summary>
        /// <param name="index">The zero based index of the element to remove.</param>
        public virtual void RemoveAt(int index)
        {
            _elements.RemoveAt(index);
            RebuildDictionary();
        }

        /// <summary>
        /// Removes an element from this document.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public virtual void RemoveElement(BsonElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            _elements.Remove(element);
            RebuildDictionary();
        }

        /// <summary>
        /// Serializes this document to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The writer.</param>
        /// <param name="nominalType">The nominalType.</param>
        /// <param name="options">The serialization options (can be null).</param>
        [Obsolete("Serialize was intended to be private and will become private in a future release.")]
        public virtual void Serialize(BsonWriter bsonWriter, Type nominalType, IBsonSerializationOptions options)
        {
            BsonDocumentSerializer.Instance.Serialize(bsonWriter, nominalType, this, options);
        }

        /// <summary>
        /// Sets the value of an element.
        /// </summary>
        /// <param name="index">The zero based index of the element whose value is to be set.</param>
        /// <param name="value">The new value.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public virtual BsonDocument Set(int index, BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this[index] = value;
            return this;
        }

        /// <summary>
        /// Sets the value of an element (an element will be added if no element with this name is found).
        /// </summary>
        /// <param name="name">The name of the element whose value is to be set.</param>
        /// <param name="value">The new value.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public virtual BsonDocument Set(string name, BsonValue value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this[name] = value;
            return this;
        }

        /// <summary>
        /// Sets the document Id.
        /// </summary>
        /// <param name="id">The value of the Id.</param>
        [Obsolete("SetDocumentId was intended to be private and will become private in a future release. Use document[\"_id\"] = value or document.Set(\"_id\", value) instead.")]
        public virtual void SetDocumentId(object id)
        {
            var idProvider = (IBsonIdProvider)BsonDocumentSerializer.Instance;
            idProvider.SetDocumentId(this, id);
        }

        /// <summary>
        /// Sets an element of the document (replacing the existing element at that position).
        /// </summary>
        /// <param name="index">The zero based index of the element to replace.</param>
        /// <param name="element">The new element.</param>
        /// <returns>The document.</returns>
        public virtual BsonDocument SetElement(int index, BsonElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            _elements[index] = element;
            RebuildDictionary();
            return this;
        }

        /// <summary>
        /// Sets an element of the document (replaces any existing element with the same name or adds a new element if an element with the same name is not found).
        /// </summary>
        /// <param name="element">The new element.</param>
        /// <returns>The document.</returns>
        public virtual BsonDocument SetElement(BsonElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            int index;
            if (_indexes.TryGetValue(element.Name, out index))
            {
                _elements[index] = element;
            }
            else
            {
                Add(element);
            }
            return this;
        }

        /// <summary>
        /// Converts the BsonDocument to a Dictionary&lt;string, object&gt;.
        /// </summary>
        /// <returns>A dictionary.</returns>
        public Dictionary<string, object> ToDictionary()
        {
            var options = new BsonTypeMapperOptions
            {
                DuplicateNameHandling = DuplicateNameHandling.ThrowException,
                MapBsonArrayTo = typeof(object[]), // TODO: should this be List<object>?
                MapBsonDocumentTo = typeof(Dictionary<string, object>),
                MapOldBinaryToByteArray = false
            };
            return (Dictionary<string, object>)BsonTypeMapper.MapToDotNetValue(this, options);
        }

        /// <summary>
        /// Converts the BsonDocument to a Hashtable.
        /// </summary>
        /// <returns>A hashtable.</returns>
        public Hashtable ToHashtable()
        {
            var options = new BsonTypeMapperOptions
            {
                DuplicateNameHandling = DuplicateNameHandling.ThrowException,
                MapBsonArrayTo = typeof(object[]), // TODO: should this be ArrayList?
                MapBsonDocumentTo = typeof(Hashtable),
                MapOldBinaryToByteArray = false
            };
            return (Hashtable)BsonTypeMapper.MapToDotNetValue(this, options);
        }

        /// <summary>
        /// Returns a string representation of the document.
        /// </summary>
        /// <returns>A string representation of the document.</returns>
        public override string ToString()
        {
            return this.ToJson();
        }

        /// <summary>
        /// Tries to get an element of this document.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The element.</param>
        /// <returns>True if an element with that name was found.</returns>
        public virtual bool TryGetElement(string name, out BsonElement value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            int index;
            if (_indexes.TryGetValue(name, out index))
            {
                value = _elements[index];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to get the value of an element of this document.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The value of the element.</param>
        /// <returns>True if an element with that name was found.</returns>
        public virtual bool TryGetValue(string name, out BsonValue value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            int index;
            if (_indexes.TryGetValue(name, out index))
            {
                value = _elements[index].Value;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Writes the document to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The writer.</param>
        [Obsolete("Use BsonSerializer.Serialize<BsonDocument> instead.")]
        public new void WriteTo(BsonWriter bsonWriter)
        {
            BsonSerializer.Serialize(bsonWriter, this);
        }

        /// <summary>
        /// Writes the document to a BsonBuffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        [Obsolete("Use BsonSerializer.Serialize<BsonDocument> instead.")]
        public void WriteTo(BsonBuffer buffer)
        {
            using (BsonWriter bsonWriter = BsonWriter.Create(buffer))
            {
                BsonSerializer.Serialize(bsonWriter, this);
            }
        }

        /// <summary>
        /// Writes the document to a Stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        [Obsolete("Use BsonSerializer.Serialize<BsonDocument> instead.")]
        public void WriteTo(Stream stream)
        {
            using (BsonWriter bsonWriter = BsonWriter.Create(stream))
            {
                BsonSerializer.Serialize(bsonWriter, this);
            }
        }

        /// <summary>
        /// Writes the document to a file.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        [Obsolete("Use BsonSerializer.Serialize<BsonDocument> instead.")]
        public void WriteTo(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                using (BsonWriter bsonWriter = BsonWriter.Create(stream))
                {
                    BsonSerializer.Serialize(bsonWriter, this);
                }
            }
        }

        // private methods
        private void RebuildDictionary()
        {
            _indexes.Clear();
            for (int index = 0; index < _elements.Count; index++)
            {
                BsonElement element = _elements[index];
                if (!_indexes.ContainsKey(element.Name))
                {
                    _indexes.Add(element.Name, index);
                }
            }
        }

        // explicit interface implementations
        BsonDocument IConvertibleToBsonDocument.ToBsonDocument()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
