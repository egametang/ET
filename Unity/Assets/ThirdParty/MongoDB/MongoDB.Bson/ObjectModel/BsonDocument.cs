/* Copyright 2010-2016 MongoDB Inc.
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
#if NET45
    [Serializable]
#endif
    public class BsonDocument : BsonValue, IComparable<BsonDocument>, IConvertibleToBsonDocument, IEnumerable<BsonElement>, IEquatable<BsonDocument>
    {
        // constants
        private const int __indexesThreshold = 8; // the _indexes dictionary will not be created until the document grows to contain 8 elements

        // private fields
        // use a list and a dictionary because we want to preserve the order in which the elements were added
        // if duplicate names are present only the first one will be in the dictionary (the others can only be accessed by index)
        private readonly List<BsonElement> _elements = new List<BsonElement>();
        private Dictionary<string, int> _indexes = null; // maps names to indexes into elements list (not created until there are enough elements to justify it)
        private bool _allowDuplicateNames;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDocument class.
        /// </summary>
        public BsonDocument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class specifying whether duplicate element names are allowed
        /// (allowing duplicate element names is not recommended).
        /// </summary>
        /// <param name="allowDuplicateNames">Whether duplicate element names are allowed.</param>
        public BsonDocument(bool allowDuplicateNames)
        {
            _allowDuplicateNames = allowDuplicateNames;
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds one element.
        /// </summary>
        /// <param name="element">An element to add to the document.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BsonDocument(BsonElement element)
        {
            Add(element);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">A dictionary to initialize the document from.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BsonDocument(Dictionary<string, object> dictionary)
        {
            AddRange(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">A dictionary to initialize the document from.</param>
        /// <param name="keys">A list of keys to select values from the dictionary.</param>
        [Obsolete("Use BsonDocument(IEnumerable<BsonElement> elements) instead.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BsonDocument(Dictionary<string, object> dictionary, IEnumerable<string> keys)
        {
            Add(dictionary, keys);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">A dictionary to initialize the document from.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BsonDocument(IEnumerable<KeyValuePair<string, object>> dictionary)
        {
            AddRange(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">A dictionary to initialize the document from.</param>
        /// <param name="keys">A list of keys to select values from the dictionary.</param>
        [Obsolete("Use BsonDocument(IEnumerable<BsonElement> elements) instead.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BsonDocument(IDictionary<string, object> dictionary, IEnumerable<string> keys)
        {
            Add(dictionary, keys);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">A dictionary to initialize the document from.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BsonDocument(IDictionary dictionary)
        {
            AddRange(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">A dictionary to initialize the document from.</param>
        /// <param name="keys">A list of keys to select values from the dictionary.</param>
        [Obsolete("Use BsonDocument(IEnumerable<BsonElement> elements) instead.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BsonDocument(IDictionary dictionary, IEnumerable keys)
        {
            Add(dictionary, keys);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds new elements from a list of elements.
        /// </summary>
        /// <param name="elements">A list of elements to add to the document.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BsonDocument(IEnumerable<BsonElement> elements)
        {
            AddRange(elements);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and adds one or more elements.
        /// </summary>
        /// <param name="elements">One or more elements to add to the document.</param>
        [Obsolete("Use BsonDocument(IEnumerable<BsonElement> elements) instead.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BsonDocument(params BsonElement[] elements)
        {
            Add(elements);
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocument class and creates and adds a new element.
        /// </summary>
        /// <param name="name">The name of the element to add to the document.</param>
        /// <param name="value">The value of the element to add to the document.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BsonDocument(string name, BsonValue value)
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

        /// <summary>
        /// Gets the BsonType of this BsonValue.
        /// </summary>
        public override BsonType BsonType
        {
            get { return BsonType.Document; }
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
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _elements[index] = new BsonElement(_elements[index].Name, value);
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
                var index = IndexOfName(name);
                if (index != -1)
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
                var index = IndexOfName(name);
                if (index != -1)
                {
                    _elements[index] = new BsonElement(name, value);
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
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return (BsonDocument)BsonTypeMapper.MapToBsonValue(value, BsonType.Document);
        }

        /// <summary>
        /// Parses a JSON string and returns a BsonDocument.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>A BsonDocument.</returns>
        public static BsonDocument Parse(string json)
        {
            using (var jsonReader = new JsonReader(json))
            {
                var context = BsonDeserializationContext.CreateRoot(jsonReader);
                var document = BsonDocumentSerializer.Instance.Deserialize(context);
                if (!jsonReader.IsAtEndOfFile())
                {
                    throw new FormatException("String contains extra non-whitespace characters beyond the end of the document.");
                }
                return document;
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
            var isDuplicate = IndexOfName(element.Name) != -1;
            if (isDuplicate && !_allowDuplicateNames)
            {
                var message = string.Format("Duplicate element name '{0}'.", element.Name);
                throw new InvalidOperationException(message);
            }
            else
            {
                _elements.Add(element);
                if (!isDuplicate)
                {
                    if (_indexes == null)
                    {
                        RebuildIndexes();
                    }
                    else
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
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            foreach (var key in keys)
            {
                Add(key, BsonTypeMapper.MapToBsonValue(dictionary[key]));
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
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            foreach (var key in keys)
            {
                if (key == null)
                {
                    throw new ArgumentException("keys", "A key passed to BsonDocument.Add is null.");
                }
                if (key.GetType() != typeof(string))
                {
                    throw new ArgumentOutOfRangeException("keys", "A key passed to BsonDocument.Add is not a string.");
                }
                Add((string)key, BsonTypeMapper.MapToBsonValue(dictionary[key]));
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
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Add(new BsonElement(name, value));

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

            if (condition)
            {
                // don't check for null value unless condition is true
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

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
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key == null)
                {
                    throw new ArgumentException("keys", "A key passed to BsonDocument.AddRange is null.");
                }
                if (entry.Key.GetType() != typeof(string))
                {
                    throw new ArgumentOutOfRangeException("dictionary", "One or more keys in the dictionary passed to BsonDocument.AddRange is not a string.");
                }
                Add((string)entry.Key, BsonTypeMapper.MapToBsonValue(entry.Value));
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
            if (elements == null)
            {
                throw new ArgumentNullException("elements");
            }

            foreach (var element in elements)
            {
                Add(element);
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
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            foreach (var entry in dictionary)
            {
                Add(entry.Key, BsonTypeMapper.MapToBsonValue(entry.Value));
            }

            return this;
        }

        /// <summary>
        /// Clears the document (removes all elements).
        /// </summary>
        public virtual void Clear()
        {
            _elements.Clear();
            _indexes = null;
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
            return IndexOfName(name) != -1;
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
            var index = IndexOfName(name);
            if (index != -1)
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

            var index = IndexOfName(name);
            if (index != -1)
            {
                return _elements[index].Value;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the index of an element.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <returns>The index of the element, or -1 if the element is not found.</returns>
        public virtual int IndexOfName(string name)
        {
            if (_indexes == null)
            {
                var count = _elements.Count;
                for (var index = 0; index < count; index++)
                {
                    if (_elements[index].Name == name)
                    {
                        return index;
                    }
                }

                return -1;
            }
            else
            {
                int index;
                if (_indexes.TryGetValue(name, out index))
                {
                    return index;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Inserts a new element at a specified position.
        /// </summary>
        /// <param name="index">The position of the new element.</param>
        /// <param name="element">The element.</param>
        public virtual void InsertAt(int index, BsonElement element)
        {
            var isDuplicate = IndexOfName(element.Name) != -1;
            if (isDuplicate && !_allowDuplicateNames)
            {
                var message = string.Format("Duplicate element name '{0}' not allowed.", element.Name);
                throw new InvalidOperationException(message);
            }
            else
            {
                _elements.Insert(index, element);
                RebuildIndexes();
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
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            foreach (BsonElement element in document)
            {
                if (overwriteExistingElements || !Contains(element.Name))
                {
                    this[element.Name] = element.Value;
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

            if (_allowDuplicateNames)
            {
                var count = _elements.Count;
                var removedAny = false;
                for (var i = count - 1; i >= 0; i--)
                {
                    if (_elements[i].Name == name)
                    {
                        _elements.RemoveAt(i);
                        removedAny = true;
                    }
                }

                if (removedAny)
                {
                    RebuildIndexes();
                }
            }
            else
            {
                var index = IndexOfName(name);
                if (index != -1)
                {
                    _elements.RemoveAt(index);
                    RebuildIndexes();
                }
            }
        }

        /// <summary>
        /// Removes an element from this document.
        /// </summary>
        /// <param name="index">The zero based index of the element to remove.</param>
        public virtual void RemoveAt(int index)
        {
            _elements.RemoveAt(index);
            RebuildIndexes();
        }

        /// <summary>
        /// Removes an element from this document.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public virtual void RemoveElement(BsonElement element)
        {
            if (_elements.Remove(element))
            {
                RebuildIndexes();
            }
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
        /// Sets an element of the document (replacing the existing element at that position).
        /// </summary>
        /// <param name="index">The zero based index of the element to replace.</param>
        /// <param name="element">The new element.</param>
        /// <returns>The document.</returns>
        public virtual BsonDocument SetElement(int index, BsonElement element)
        {
            var oldName = _elements[index].Name;
            _elements[index] = element;

            if (element.Name != oldName)
            {
                RebuildIndexes();
            }

            return this;
        }

        /// <summary>
        /// Sets an element of the document (replaces any existing element with the same name or adds a new element if an element with the same name is not found).
        /// </summary>
        /// <param name="element">The new element.</param>
        /// <returns>The document.</returns>
        public virtual BsonDocument SetElement(BsonElement element)
        {
            var index = IndexOfName(element.Name);
            if (index != -1)
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
            var index = IndexOfName(name);
            if (index != -1)
            {
                value = _elements[index];
                return true;
            }
            else
            {
                value = default(BsonElement);
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
            var index = IndexOfName(name);
            if (index != -1)
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

        // private methods
        private void RebuildIndexes()
        {
            if (_elements.Count < __indexesThreshold)
            {
                _indexes = null;
                return;
            }

            if (_indexes == null)
            {
                _indexes = new Dictionary<string, int>();
            }
            else
            {
                _indexes.Clear();
            }

            // process the elements in reverse order so that in case of duplicates the dictionary ends up pointing at the first one
            var count = _elements.Count;
            for (int index = count - 1; index >= 0; index--)
            {
                BsonElement element = _elements[index];
                _indexes[element.Name] = index;
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
