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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
#if NET45
using System.Runtime.Serialization;
#endif
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents a mapping between a class and a BSON document.
    /// </summary>
    public class BsonClassMap
    {
        // private static fields
        private readonly static Dictionary<Type, BsonClassMap> __classMaps = new Dictionary<Type, BsonClassMap>();
        private readonly static Queue<Type> __knownTypesQueue = new Queue<Type>();

        private static readonly MethodInfo __getUninitializedObjectMethodInfo =
            typeof(string)
            .GetTypeInfo()
            .Assembly
            .GetType("System.Runtime.Serialization.FormatterServices")
            .GetTypeInfo()
            ?.GetMethod("GetUninitializedObject", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        private static int __freezeNestingLevel = 0;

        // private fields
        private readonly Type _classType;
        private readonly List<BsonCreatorMap> _creatorMaps;
        private readonly IConventionPack _conventionPack;
        private readonly bool _isAnonymous;
        private readonly List<BsonMemberMap> _allMemberMaps; // includes inherited member maps
        private readonly ReadOnlyCollection<BsonMemberMap> _allMemberMapsReadonly;
        private readonly List<BsonMemberMap> _declaredMemberMaps; // only the members declared in this class
        private readonly BsonTrie<int> _elementTrie;

        private bool _frozen; // once a class map has been frozen no further changes are allowed
        private BsonClassMap _baseClassMap; // null for class object and interfaces
        private volatile IDiscriminatorConvention _discriminatorConvention;
        private Func<object> _creator;
        private string _discriminator;
        private bool _discriminatorIsRequired;
        private bool _hasRootClass;
        private bool _isRootClass;
        private BsonMemberMap _idMemberMap;
        private bool _ignoreExtraElements;
        private bool _ignoreExtraElementsIsInherited;
        private BsonMemberMap _extraElementsMemberMap;
        private int _extraElementsMemberIndex = -1;
        private List<Type> _knownTypes = new List<Type>();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonClassMap class.
        /// </summary>
        /// <param name="classType">The class type.</param>
        public BsonClassMap(Type classType)
        {
            _classType = classType;
            _creatorMaps = new List<BsonCreatorMap>();
            _conventionPack = ConventionRegistry.Lookup(classType);
            _isAnonymous = IsAnonymousType(classType);
            _allMemberMaps = new List<BsonMemberMap>();
            _allMemberMapsReadonly = _allMemberMaps.AsReadOnly();
            _declaredMemberMaps = new List<BsonMemberMap>();
            _elementTrie = new BsonTrie<int>();

            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonClassMap"/> class.
        /// </summary>
        /// <param name="classType">Type of the class.</param>
        /// <param name="baseClassMap">The base class map.</param>
        public BsonClassMap(Type classType, BsonClassMap baseClassMap)
            : this(classType)
        {
            _baseClassMap = baseClassMap;
        }

        // public properties
        /// <summary>
        /// Gets all the member maps (including maps for inherited members).
        /// </summary>
        public ReadOnlyCollection<BsonMemberMap> AllMemberMaps
        {
            get { return _allMemberMapsReadonly; }
        }

        /// <summary>
        /// Gets the base class map.
        /// </summary>
        public BsonClassMap BaseClassMap
        {
            get { return _baseClassMap; }
        }

        /// <summary>
        /// Gets the class type.
        /// </summary>
        public Type ClassType
        {
            get { return _classType; }
        }

        /// <summary>
        /// Gets the constructor maps.
        /// </summary>
        public IEnumerable<BsonCreatorMap> CreatorMaps
        {
            get { return _creatorMaps; }
        }

        /// <summary>
        /// Gets the conventions used for auto mapping.
        /// </summary>
        public IConventionPack ConventionPack
        {
            get { return _conventionPack; }
        }

        /// <summary>
        /// Gets the declared member maps (only for members declared in this class).
        /// </summary>
        public IEnumerable<BsonMemberMap> DeclaredMemberMaps
        {
            get { return _declaredMemberMaps; }
        }

        /// <summary>
        /// Gets the discriminator.
        /// </summary>
        public string Discriminator
        {
            get { return _discriminator; }
        }

        /// <summary>
        /// Gets whether a discriminator is required when serializing this class.
        /// </summary>
        public bool DiscriminatorIsRequired
        {
            get { return _discriminatorIsRequired; }
        }

        /// <summary>
        /// Gets the member map of the member used to hold extra elements.
        /// </summary>
        public BsonMemberMap ExtraElementsMemberMap
        {
            get { return _extraElementsMemberMap; }
        }

        /// <summary>
        /// Gets whether this class map has any creator maps.
        /// </summary>
        public bool HasCreatorMaps
        {
            get { return _creatorMaps.Count > 0; }
        }

        /// <summary>
        /// Gets whether this class has a root class ancestor.
        /// </summary>
        public bool HasRootClass
        {
            get { return _hasRootClass; }
        }

        /// <summary>
        /// Gets the Id member map (null if none).
        /// </summary>
        public BsonMemberMap IdMemberMap
        {
            get { return _idMemberMap; }
        }

        /// <summary>
        /// Gets whether extra elements should be ignored when deserializing.
        /// </summary>
        public bool IgnoreExtraElements
        {
            get { return _ignoreExtraElements; }
        }

        /// <summary>
        /// Gets whether the IgnoreExtraElements value should be inherited by derived classes.
        /// </summary>
        public bool IgnoreExtraElementsIsInherited
        {
            get { return _ignoreExtraElementsIsInherited; }
        }

        /// <summary>
        /// Gets whether this class is anonymous.
        /// </summary>
        public bool IsAnonymous
        {
            get { return _isAnonymous; }
        }

        /// <summary>
        /// Gets whether the class map is frozen.
        /// </summary>
        public bool IsFrozen
        {
            get { return _frozen; }
        }

        /// <summary>
        /// Gets whether this class is a root class.
        /// </summary>
        public bool IsRootClass
        {
            get { return _isRootClass; }
        }

        /// <summary>
        /// Gets the known types of this class.
        /// </summary>
        public IEnumerable<Type> KnownTypes
        {
            get { return _knownTypes; }
        }

        // internal properties
        /// <summary>
        /// Gets the element name to member index trie.
        /// </summary>
        internal BsonTrie<int> ElementTrie
        {
            get { return _elementTrie; }
        }

        /// <summary>
        /// Gets the member index of the member used to hold extra elements.
        /// </summary>
        internal int ExtraElementsMemberMapIndex
        {
            get { return _extraElementsMemberIndex; }
        }

        // public static methods
        /// <summary>
        /// Gets the type of a member.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The type of the member.</returns>
        public static Type GetMemberInfoType(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }

            if (memberInfo is FieldInfo)
            {
                return ((FieldInfo)memberInfo).FieldType;
            }
            else if (memberInfo is PropertyInfo)
            {
                return ((PropertyInfo)memberInfo).PropertyType;
            }

            throw new NotSupportedException("Only field and properties are supported at this time.");
        }

        /// <summary>
        /// Gets all registered class maps.
        /// </summary>
        /// <returns>All registered class maps.</returns>
        public static IEnumerable<BsonClassMap> GetRegisteredClassMaps()
        {
            BsonSerializer.ConfigLock.EnterReadLock();
            try
            {
                return __classMaps.Values.ToList(); // return a copy for thread safety
            }
            finally
            {
                BsonSerializer.ConfigLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Checks whether a class map is registered for a type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if there is a class map registered for the type.</returns>
        public static bool IsClassMapRegistered(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            BsonSerializer.ConfigLock.EnterReadLock();
            try
            {
                return __classMaps.ContainsKey(type);
            }
            finally
            {
                BsonSerializer.ConfigLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Looks up a class map (will AutoMap the class if no class map is registered).
        /// </summary>
        /// <param name="classType">The class type.</param>
        /// <returns>The class map.</returns>
        public static BsonClassMap LookupClassMap(Type classType)
        {
            if (classType == null)
            {
                throw new ArgumentNullException("classType");
            }

            BsonSerializer.ConfigLock.EnterReadLock();
            try
            {
                BsonClassMap classMap;
                if (__classMaps.TryGetValue(classType, out classMap))
                {
                    if (classMap.IsFrozen)
                    {
                        return classMap;
                    }
                }
            }
            finally
            {
                BsonSerializer.ConfigLock.ExitReadLock();
            }

            BsonSerializer.ConfigLock.EnterWriteLock();
            try
            {
                BsonClassMap classMap;
                if (!__classMaps.TryGetValue(classType, out classMap))
                {
                    // automatically create a classMap for classType and register it
                    var classMapDefinition = typeof(BsonClassMap<>);
                    var classMapType = classMapDefinition.MakeGenericType(classType);
                    classMap = (BsonClassMap)Activator.CreateInstance(classMapType);
                    classMap.AutoMap();
                    RegisterClassMap(classMap);
                }
                return classMap.Freeze();
            }
            finally
            {
                BsonSerializer.ConfigLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Creates and registers a class map.
        /// </summary>
        /// <typeparam name="TClass">The class.</typeparam>
        /// <returns>The class map.</returns>
        public static BsonClassMap<TClass> RegisterClassMap<TClass>()
        {
            return RegisterClassMap<TClass>(cm => { cm.AutoMap(); });
        }

        /// <summary>
        /// Creates and registers a class map.
        /// </summary>
        /// <typeparam name="TClass">The class.</typeparam>
        /// <param name="classMapInitializer">The class map initializer.</param>
        /// <returns>The class map.</returns>
        public static BsonClassMap<TClass> RegisterClassMap<TClass>(Action<BsonClassMap<TClass>> classMapInitializer)
        {
            var classMap = new BsonClassMap<TClass>(classMapInitializer);
            RegisterClassMap(classMap);
            return classMap;
        }

        /// <summary>
        /// Registers a class map.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        public static void RegisterClassMap(BsonClassMap classMap)
        {
            if (classMap == null)
            {
                throw new ArgumentNullException("classMap");
            }

            BsonSerializer.ConfigLock.EnterWriteLock();
            try
            {
                // note: class maps can NOT be replaced (because derived classes refer to existing instance)
                __classMaps.Add(classMap.ClassType, classMap);
                BsonSerializer.RegisterDiscriminator(classMap.ClassType, classMap.Discriminator);
            }
            finally
            {
                BsonSerializer.ConfigLock.ExitWriteLock();
            }
        }

        // public methods
        /// <summary>
        /// Automaps the class.
        /// </summary>
        public void AutoMap()
        {
            if (_frozen) { ThrowFrozenException(); }
            AutoMapClass();
        }

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <returns>An object.</returns>
        public object CreateInstance()
        {
            if (!_frozen) { ThrowNotFrozenException(); }
            var creator = GetCreator();
            return creator.Invoke();
        }

        /// <summary>
        /// Freezes the class map.
        /// </summary>
        /// <returns>The frozen class map.</returns>
        public BsonClassMap Freeze()
        {
            BsonSerializer.ConfigLock.EnterReadLock();
            try
            {
                if (_frozen)
                {
                    return this;
                }
            }
            finally
            {
                BsonSerializer.ConfigLock.ExitReadLock();
            }

            BsonSerializer.ConfigLock.EnterWriteLock();
            try
            {
                if (!_frozen)
                {
                    __freezeNestingLevel++;
                    try
                    {
                        var baseType = _classType.GetTypeInfo().BaseType;
                        if (baseType != null)
                        {
                            if (_baseClassMap == null)
                            {
                                _baseClassMap = LookupClassMap(baseType);
                            }
                            _discriminatorIsRequired |= _baseClassMap._discriminatorIsRequired;
                            _hasRootClass |= (_isRootClass || _baseClassMap.HasRootClass);
                            _allMemberMaps.AddRange(_baseClassMap.AllMemberMaps);
                            if (_baseClassMap.IgnoreExtraElements && _baseClassMap.IgnoreExtraElementsIsInherited)
                            {
                                _ignoreExtraElements = true;
                                _ignoreExtraElementsIsInherited = true;
                            }
                        }
                        _allMemberMaps.AddRange(_declaredMemberMaps);

                        if (_idMemberMap == null)
                        {
                            // see if we can inherit the idMemberMap from our base class
                            if (_baseClassMap != null)
                            {
                                _idMemberMap = _baseClassMap.IdMemberMap;
                            }
                        }
                        else
                        {
                            if (_idMemberMap.ClassMap == this)
                            {
                                // conventions could have set this to an improper value
                                _idMemberMap.SetElementName("_id");
                            }
                        }

                        if (_extraElementsMemberMap == null)
                        {
                            // see if we can inherit the extraElementsMemberMap from our base class
                            if (_baseClassMap != null)
                            {
                                _extraElementsMemberMap = _baseClassMap.ExtraElementsMemberMap;
                            }
                        }

                        _extraElementsMemberIndex = -1;
                        for (int memberIndex = 0; memberIndex < _allMemberMaps.Count; ++memberIndex)
                        {
                            var memberMap = _allMemberMaps[memberIndex];
                            int conflictingMemberIndex;
                            if (!_elementTrie.TryGetValue(memberMap.ElementName, out conflictingMemberIndex))
                            {
                                _elementTrie.Add(memberMap.ElementName, memberIndex);
                            }
                            else
                            {
                                var conflictingMemberMap = _allMemberMaps[conflictingMemberIndex];
                                var fieldOrProperty = (memberMap.MemberInfo is FieldInfo) ? "field" : "property";
                                var conflictingFieldOrProperty = (conflictingMemberMap.MemberInfo is FieldInfo) ? "field" : "property";
                                var conflictingType = conflictingMemberMap.MemberInfo.DeclaringType;

                                string message;
                                if (conflictingType == _classType)
                                {
                                    message = string.Format(
                                        "The {0} '{1}' of type '{2}' cannot use element name '{3}' because it is already being used by {4} '{5}'.",
                                        fieldOrProperty, memberMap.MemberName, _classType.FullName, memberMap.ElementName, conflictingFieldOrProperty, conflictingMemberMap.MemberName);
                                }
                                else
                                {
                                    message = string.Format(
                                        "The {0} '{1}' of type '{2}' cannot use element name '{3}' because it is already being used by {4} '{5}' of type '{6}'.",
                                        fieldOrProperty, memberMap.MemberName, _classType.FullName, memberMap.ElementName, conflictingFieldOrProperty, conflictingMemberMap.MemberName, conflictingType.FullName);
                                }
                                throw new BsonSerializationException(message);
                            }
                            if (memberMap == _extraElementsMemberMap)
                            {
                                _extraElementsMemberIndex = memberIndex;
                            }
                        }

                        // mark this classMap frozen before we start working on knownTypes
                        // because we might get back to this same classMap while processing knownTypes
                        foreach (var creatorMap in _creatorMaps)
                        {
                            creatorMap.Freeze();
                        }
                        foreach (var memberMap in _declaredMemberMaps)
                        {
                            memberMap.Freeze();
                        }
                        _frozen = true;

                        // use a queue to postpone processing of known types until we get back to the first level call to Freeze
                        // this avoids infinite recursion when going back down the inheritance tree while processing known types
                        foreach (var knownType in _knownTypes)
                        {
                            __knownTypesQueue.Enqueue(knownType);
                        }

                        // if we are back to the first level go ahead and process any queued known types
                        if (__freezeNestingLevel == 1)
                        {
                            while (__knownTypesQueue.Count != 0)
                            {
                                var knownType = __knownTypesQueue.Dequeue();
                                LookupClassMap(knownType); // will AutoMap and/or Freeze knownType if necessary
                            }
                        }
                    }
                    finally
                    {
                        __freezeNestingLevel--;
                    }
                }
            }
            finally
            {
                BsonSerializer.ConfigLock.ExitWriteLock();
            }
            return this;
        }

        /// <summary>
        /// Gets a member map (only considers members declared in this class).
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <returns>The member map (or null if the member was not found).</returns>
        public BsonMemberMap GetMemberMap(string memberName)
        {
            if (memberName == null)
            {
                throw new ArgumentNullException("memberName");
            }

            // can be called whether frozen or not
            return _declaredMemberMaps.Find(m => m.MemberName == memberName);
        }

        /// <summary>
        /// Gets the member map for a BSON element.
        /// </summary>
        /// <param name="elementName">The name of the element.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap GetMemberMapForElement(string elementName)
        {
            if (elementName == null)
            {
                throw new ArgumentNullException("elementName");
            }

            if (!_frozen) { ThrowNotFrozenException(); }
            int memberIndex;
            if (!_elementTrie.TryGetValue(elementName, out memberIndex))
            {
                return null;
            }
            var memberMap = _allMemberMaps[memberIndex];
            return memberMap;
        }

        /// <summary>
        /// Creates a creator map for a constructor and adds it to the class map.
        /// </summary>
        /// <param name="constructorInfo">The constructor info.</param>
        /// <returns>The creator map (so method calls can be chained).</returns>
        public BsonCreatorMap MapConstructor(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
            {
                throw new ArgumentNullException("constructorInfo");
            }
            EnsureMemberInfoIsForThisClass(constructorInfo);

            if (_frozen) { ThrowFrozenException(); }
            var creatorMap = _creatorMaps.FirstOrDefault(m => m.MemberInfo == constructorInfo);
            if (creatorMap == null)
            {
                var @delegate = new CreatorMapDelegateCompiler().CompileConstructorDelegate(constructorInfo);
                creatorMap = new BsonCreatorMap(this, constructorInfo, @delegate);
                _creatorMaps.Add(creatorMap);
            }
            return creatorMap;
        }

        /// <summary>
        /// Creates a creator map for a constructor and adds it to the class map.
        /// </summary>
        /// <param name="constructorInfo">The constructor info.</param>
        /// <param name="argumentNames">The argument names.</param>
        /// <returns>The creator map (so method calls can be chained).</returns>
        public BsonCreatorMap MapConstructor(ConstructorInfo constructorInfo, params string[] argumentNames)
        {
            var creatorMap = MapConstructor(constructorInfo);
            creatorMap.SetArguments(argumentNames);
            return creatorMap;
        }

        /// <summary>
        /// Creates a creator map and adds it to the class.
        /// </summary>
        /// <param name="delegate">The delegate.</param>
        /// <returns>The factory method map (so method calls can be chained).</returns>
        public BsonCreatorMap MapCreator(Delegate @delegate)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException("delegate");
            }

            if (_frozen) { ThrowFrozenException(); }
            var creatorMap = new BsonCreatorMap(this, null, @delegate);
            _creatorMaps.Add(creatorMap);
            return creatorMap;
        }

        /// <summary>
        /// Creates a creator map and adds it to the class.
        /// </summary>
        /// <param name="delegate">The delegate.</param>
        /// <param name="argumentNames">The argument names.</param>
        /// <returns>The factory method map (so method calls can be chained).</returns>
        public BsonCreatorMap MapCreator(Delegate @delegate, params string[] argumentNames)
        {
            var creatorMap = MapCreator(@delegate);
            creatorMap.SetArguments(argumentNames);
            return creatorMap;
        }

        /// <summary>
        /// Creates a member map for the extra elements field and adds it to the class map.
        /// </summary>
        /// <param name="fieldName">The name of the extra elements field.</param>
        /// <returns>The member map (so method calls can be chained).</returns>
        public BsonMemberMap MapExtraElementsField(string fieldName)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }

            if (_frozen) { ThrowFrozenException(); }
            var fieldMap = MapField(fieldName);
            SetExtraElementsMember(fieldMap);
            return fieldMap;
        }

        /// <summary>
        /// Creates a member map for the extra elements member and adds it to the class map.
        /// </summary>
        /// <param name="memberInfo">The member info for the extra elements member.</param>
        /// <returns>The member map (so method calls can be chained).</returns>
        public BsonMemberMap MapExtraElementsMember(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }

            if (_frozen) { ThrowFrozenException(); }
            var memberMap = MapMember(memberInfo);
            SetExtraElementsMember(memberMap);
            return memberMap;
        }

        /// <summary>
        /// Creates a member map for the extra elements property and adds it to the class map.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The member map (so method calls can be chained).</returns>
        public BsonMemberMap MapExtraElementsProperty(string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (_frozen) { ThrowFrozenException(); }
            var propertyMap = MapProperty(propertyName);
            SetExtraElementsMember(propertyMap);
            return propertyMap;
        }

        /// <summary>
        /// Creates a creator map for a factory method and adds it to the class.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <returns>The creator map (so method calls can be chained).</returns>
        public BsonCreatorMap MapFactoryMethod(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }
            EnsureMemberInfoIsForThisClass(methodInfo);

            if (_frozen) { ThrowFrozenException(); }
            var creatorMap = _creatorMaps.FirstOrDefault(m => m.MemberInfo == methodInfo);
            if (creatorMap == null)
            {
                var @delegate = new CreatorMapDelegateCompiler().CompileFactoryMethodDelegate(methodInfo);
                creatorMap = new BsonCreatorMap(this, methodInfo, @delegate);
                _creatorMaps.Add(creatorMap);
            }
            return creatorMap;
        }

        /// <summary>
        /// Creates a creator map for a factory method and adds it to the class.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <param name="argumentNames">The argument names.</param>
        /// <returns>The creator map (so method calls can be chained).</returns>
        public BsonCreatorMap MapFactoryMethod(MethodInfo methodInfo, params string[] argumentNames)
        {
            var creatorMap = MapFactoryMethod(methodInfo);
            creatorMap.SetArguments(argumentNames);
            return creatorMap;
        }

        /// <summary>
        /// Creates a member map for a field and adds it to the class map.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The member map (so method calls can be chained).</returns>
        public BsonMemberMap MapField(string fieldName)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }

            if (_frozen) { ThrowFrozenException(); }
            var fieldInfo = _classType.GetTypeInfo().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (fieldInfo == null)
            {
                var message = string.Format("The class '{0}' does not have a field named '{1}'.", _classType.FullName, fieldName);
                throw new BsonSerializationException(message);
            }
            return MapMember(fieldInfo);
        }

        /// <summary>
        /// Creates a member map for the Id field and adds it to the class map.
        /// </summary>
        /// <param name="fieldName">The name of the Id field.</param>
        /// <returns>The member map (so method calls can be chained).</returns>
        public BsonMemberMap MapIdField(string fieldName)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }

            if (_frozen) { ThrowFrozenException(); }
            var fieldMap = MapField(fieldName);
            SetIdMember(fieldMap);
            return fieldMap;
        }

        /// <summary>
        /// Creates a member map for the Id member and adds it to the class map.
        /// </summary>
        /// <param name="memberInfo">The member info for the Id member.</param>
        /// <returns>The member map (so method calls can be chained).</returns>
        public BsonMemberMap MapIdMember(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }

            if (_frozen) { ThrowFrozenException(); }
            var memberMap = MapMember(memberInfo);
            SetIdMember(memberMap);
            return memberMap;
        }

        /// <summary>
        /// Creates a member map for the Id property and adds it to the class map.
        /// </summary>
        /// <param name="propertyName">The name of the Id property.</param>
        /// <returns>The member map (so method calls can be chained).</returns>
        public BsonMemberMap MapIdProperty(string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (_frozen) { ThrowFrozenException(); }
            var propertyMap = MapProperty(propertyName);
            SetIdMember(propertyMap);
            return propertyMap;
        }

        /// <summary>
        /// Creates a member map for a member and adds it to the class map.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The member map (so method calls can be chained).</returns>
        public BsonMemberMap MapMember(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }
            if (!(memberInfo is FieldInfo) && !(memberInfo is PropertyInfo))
            {
                throw new ArgumentException("MemberInfo must be either a FieldInfo or a PropertyInfo.", "memberInfo");
            }
            EnsureMemberInfoIsForThisClass(memberInfo);

            if (_frozen) { ThrowFrozenException(); }
            var memberMap = _declaredMemberMaps.Find(m => m.MemberInfo == memberInfo);
            if (memberMap == null)
            {
                memberMap = new BsonMemberMap(this, memberInfo);
                _declaredMemberMaps.Add(memberMap);
            }
            return memberMap;
        }

        /// <summary>
        /// Creates a member map for a property and adds it to the class map.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The member map (so method calls can be chained).</returns>
        public BsonMemberMap MapProperty(string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (_frozen) { ThrowFrozenException(); }
            var propertyInfo = _classType.GetTypeInfo().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (propertyInfo == null)
            {
                var message = string.Format("The class '{0}' does not have a property named '{1}'.", _classType.FullName, propertyName);
                throw new BsonSerializationException(message);
            }
            return MapMember(propertyInfo);
        }

        /// <summary>
        /// Resets the class map back to its initial state.
        /// </summary>
        public void Reset()
        {
            if (_frozen) { ThrowFrozenException(); }

            _creatorMaps.Clear();
            _creator = null;
            _declaredMemberMaps.Clear();
            _discriminator = _classType.Name;
            _discriminatorIsRequired = false;
            _extraElementsMemberMap = null;
            _idMemberMap = null;
            _ignoreExtraElements = true; // TODO: should this really be false?
            _ignoreExtraElementsIsInherited = false;
            _isRootClass = false;
            _knownTypes.Clear();
        }

        /// <summary>
        /// Sets the creator for the object.
        /// </summary>
        /// <param name="creator">The creator.</param>
        /// <returns>The class map (so method calls can be chained).</returns>
        public BsonClassMap SetCreator(Func<object> creator)
        {
            _creator = creator;
            return this;
        }

        /// <summary>
        /// Sets the discriminator.
        /// </summary>
        /// <param name="discriminator">The discriminator.</param>
        public void SetDiscriminator(string discriminator)
        {
            if (discriminator == null)
            {
                throw new ArgumentNullException("discriminator");
            }

            if (_frozen) { ThrowFrozenException(); }
            _discriminator = discriminator;
        }

        /// <summary>
        /// Sets whether a discriminator is required when serializing this class.
        /// </summary>
        /// <param name="discriminatorIsRequired">Whether a discriminator is required.</param>
        public void SetDiscriminatorIsRequired(bool discriminatorIsRequired)
        {
            if (_frozen) { ThrowFrozenException(); }
            _discriminatorIsRequired = discriminatorIsRequired;
        }

        /// <summary>
        /// Sets the member map of the member used to hold extra elements.
        /// </summary>
        /// <param name="memberMap">The extra elements member map.</param>
        public void SetExtraElementsMember(BsonMemberMap memberMap)
        {
            if (memberMap == null)
            {
                throw new ArgumentNullException("memberMap");
            }
            EnsureMemberMapIsForThisClass(memberMap);

            if (_frozen) { ThrowFrozenException(); }
            if (memberMap.MemberType != typeof(BsonDocument) && !typeof(IDictionary<string, object>).GetTypeInfo().IsAssignableFrom(memberMap.MemberType))
            {
                var message = string.Format("Type of ExtraElements member must be BsonDocument or implement IDictionary<string, object>.");
                throw new InvalidOperationException(message);
            }

            _extraElementsMemberMap = memberMap;
        }

        /// <summary>
        /// Adds a known type to the class map.
        /// </summary>
        /// <param name="type">The known type.</param>
        public void AddKnownType(Type type)
        {
            if (!_classType.GetTypeInfo().IsAssignableFrom(type))
            {
                string message = string.Format("Class {0} cannot be assigned to Class {1}.  Ensure that known types are derived from the mapped class.", type.FullName, _classType.FullName);
                throw new ArgumentNullException("type", message);
            }

            if (_frozen) { ThrowFrozenException(); }
            _knownTypes.Add(type);
        }

        /// <summary>
        /// Sets the Id member.
        /// </summary>
        /// <param name="memberMap">The Id member (null if none).</param>
        public void SetIdMember(BsonMemberMap memberMap)
        {
            if (memberMap != null)
            {
                EnsureMemberMapIsForThisClass(memberMap);
            }

            if (_frozen) { ThrowFrozenException(); }

            _idMemberMap = memberMap;
        }

        /// <summary>
        /// Sets whether extra elements should be ignored when deserializing.
        /// </summary>
        /// <param name="ignoreExtraElements">Whether extra elements should be ignored when deserializing.</param>
        public void SetIgnoreExtraElements(bool ignoreExtraElements)
        {
            if (_frozen) { ThrowFrozenException(); }
            _ignoreExtraElements = ignoreExtraElements;
        }

        /// <summary>
        /// Sets whether the IgnoreExtraElements value should be inherited by derived classes.
        /// </summary>
        /// <param name="ignoreExtraElementsIsInherited">Whether the IgnoreExtraElements value should be inherited by derived classes.</param>
        public void SetIgnoreExtraElementsIsInherited(bool ignoreExtraElementsIsInherited)
        {
            if (_frozen) { ThrowFrozenException(); }
            _ignoreExtraElementsIsInherited = ignoreExtraElementsIsInherited;
        }

        /// <summary>
        /// Sets whether this class is a root class.
        /// </summary>
        /// <param name="isRootClass">Whether this class is a root class.</param>
        public void SetIsRootClass(bool isRootClass)
        {
            if (_frozen) { ThrowFrozenException(); }
            _isRootClass = isRootClass;
        }

        /// <summary>
        /// Removes a creator map for a constructor from the class map.
        /// </summary>
        /// <param name="constructorInfo">The constructor info.</param>
        public void UnmapConstructor(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
            {
                throw new ArgumentNullException("constructorInfo");
            }
            EnsureMemberInfoIsForThisClass(constructorInfo);

            if (_frozen) { ThrowFrozenException(); }
            var creatorMap = _creatorMaps.FirstOrDefault(m => m.MemberInfo == constructorInfo);
            if (creatorMap != null)
            {
                _creatorMaps.Remove(creatorMap);
            }
        }

        /// <summary>
        /// Removes a creator map for a factory method from the class map.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        public void UnmapFactoryMethod(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }
            EnsureMemberInfoIsForThisClass(methodInfo);

            if (_frozen) { ThrowFrozenException(); }
            var creatorMap = _creatorMaps.FirstOrDefault(m => m.MemberInfo == methodInfo);
            if (creatorMap != null)
            {
                _creatorMaps.Remove(creatorMap);
            }
        }

        /// <summary>
        /// Removes the member map for a field from the class map.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public void UnmapField(string fieldName)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }

            if (_frozen) { ThrowFrozenException(); }
            var fieldInfo = _classType.GetTypeInfo().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (fieldInfo == null)
            {
                var message = string.Format("The class '{0}' does not have a field named '{1}'.", _classType.FullName, fieldName);
                throw new BsonSerializationException(message);
            }
            UnmapMember(fieldInfo);
        }

        /// <summary>
        /// Removes a member map from the class map.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        public void UnmapMember(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }
            EnsureMemberInfoIsForThisClass(memberInfo);

            if (_frozen) { ThrowFrozenException(); }
            var memberMap = _declaredMemberMaps.Find(m => m.MemberInfo == memberInfo);
            if (memberMap != null)
            {
                _declaredMemberMaps.Remove(memberMap);
                if (_idMemberMap == memberMap)
                {
                    _idMemberMap = null;
                }
                if (_extraElementsMemberMap == memberMap)
                {
                    _extraElementsMemberMap = null;
                }
            }
        }

        /// <summary>
        /// Removes the member map for a property from the class map.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        public void UnmapProperty(string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (_frozen) { ThrowFrozenException(); }
            var propertyInfo = _classType.GetTypeInfo().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (propertyInfo == null)
            {
                var message = string.Format("The class '{0}' does not have a property named '{1}'.", _classType.FullName, propertyName);
                throw new BsonSerializationException(message);
            }
            UnmapMember(propertyInfo);
        }

        // internal methods
        /// <summary>
        /// Gets the discriminator convention for the class.
        /// </summary>
        /// <returns>The discriminator convention for the class.</returns>
        internal IDiscriminatorConvention GetDiscriminatorConvention()
        {
            // return a cached discriminator convention when possible
            var discriminatorConvention = _discriminatorConvention;
            if (discriminatorConvention == null)
            {
                // it's possible but harmless for multiple threads to do the initial lookup at the same time
                discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(_classType);
                _discriminatorConvention = discriminatorConvention;
            }
            return discriminatorConvention;
        }

        // private methods
        private void AutoMapClass()
        {
            new ConventionRunner(_conventionPack).Apply(this);

            OrderMembers();
            foreach (var memberMap in _declaredMemberMaps)
            {
                TryFindShouldSerializeMethod(memberMap);
            }
        }

        private void OrderMembers()
        {
            // only auto map properties declared in this class (and not in base classes)
            var hasOrderedElements = false;
            var hasUnorderedElements = false;
            foreach (var memberMap in _declaredMemberMaps)
            {
                if (memberMap.Order != int.MaxValue)
                {
                    hasOrderedElements |= true;
                }
                else
                {
                    hasUnorderedElements |= true;
                }
            }

            if (hasOrderedElements)
            {
                if (hasUnorderedElements)
                {
                    // split out the unordered elements and add them back at the end (because Sort is unstable, see online help)
                    var unorderedElements = new List<BsonMemberMap>(_declaredMemberMaps.Where(pm => pm.Order == int.MaxValue));
                    _declaredMemberMaps.RemoveAll(m => m.Order == int.MaxValue);
                    _declaredMemberMaps.Sort((x, y) => x.Order.CompareTo(y.Order));
                    _declaredMemberMaps.AddRange(unorderedElements);
                }
                else
                {
                    _declaredMemberMaps.Sort((x, y) => x.Order.CompareTo(y.Order));
                }
            }
        }

        private void TryFindShouldSerializeMethod(BsonMemberMap memberMap)
        {
            // see if the class has a method called ShouldSerializeXyz where Xyz is the name of this member
            var shouldSerializeMethod = GetShouldSerializeMethod(memberMap.MemberInfo);
            if (shouldSerializeMethod != null)
            {
                memberMap.SetShouldSerializeMethod(shouldSerializeMethod);
            }
        }

        private void EnsureMemberInfoIsForThisClass(MemberInfo memberInfo)
        {
            if (memberInfo.DeclaringType != _classType)
            {
                var message = string.Format(
                    "The memberInfo argument must be for class {0}, but was for class {1}.",
                    _classType.Name,
                    memberInfo.DeclaringType.Name);
                throw new ArgumentOutOfRangeException("memberInfo", message);
            }
        }

        private void EnsureMemberMapIsForThisClass(BsonMemberMap memberMap)
        {
            if (memberMap.ClassMap != this)
            {
                var message = string.Format(
                    "The memberMap argument must be for class {0}, but was for class {1}.",
                    _classType.Name,
                    memberMap.ClassMap.ClassType.Name);
                throw new ArgumentOutOfRangeException("memberMap", message);
            }
        }

        private Func<object> GetCreator()
        {
            if (_creator == null)
            {
                Expression body;
                var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                var classTypeInfo = _classType.GetTypeInfo();
                ConstructorInfo defaultConstructor = classTypeInfo.GetConstructors(bindingFlags)
                    .Where(c => c.GetParameters().Length == 0)
                    .SingleOrDefault();
                #if ENABLE_IL2CPP

                if (defaultConstructor != null)
                {
                    _creator = () => defaultConstructor.Invoke(null);

                }
                else if(__getUninitializedObjectMethodInfo != null)
                {

                    _creator = () => __getUninitializedObjectMethodInfo.Invoke(null, new object[] { this._classType });
                }
                else
                {
                    var message = $"Type '{_classType.GetType().Name}' does not have a default constructor.";
                    throw new BsonSerializationException(message);
                }
                #else
                if (defaultConstructor != null)
                {
                    // lambdaExpression = () => (object) new TClass()
                    body = Expression.New(defaultConstructor);
                }
                else if (__getUninitializedObjectMethodInfo != null)
                {
                    // lambdaExpression = () => FormatterServices.GetUninitializedObject(classType)
                    body = Expression.Call(__getUninitializedObjectMethodInfo, Expression.Constant(_classType));
                }
                else
                {
                    var message = $"Type '{_classType.GetType().Name}' does not have a default constructor.";
                    throw new BsonSerializationException(message);
                }

                var lambdaExpression = Expression.Lambda<Func<object>>(body);
                _creator = lambdaExpression.Compile();
                #endif
            }
            return _creator;
        }

        private Func<object, bool> GetShouldSerializeMethod(MemberInfo memberInfo)
        {
            var shouldSerializeMethodName = "ShouldSerialize" + memberInfo.Name;
            var shouldSerializeMethodInfo = _classType.GetTypeInfo().GetMethod(shouldSerializeMethodName, new Type[] { });
            if (shouldSerializeMethodInfo != null &&
                shouldSerializeMethodInfo.IsPublic &&
                shouldSerializeMethodInfo.ReturnType == typeof(bool))
            {
                // lambdaExpression = (obj) => ((TClass) obj).ShouldSerializeXyz()
                var objParameter = Expression.Parameter(typeof(object), "obj");
                var lambdaExpression = Expression.Lambda<Func<object, bool>>(Expression.Call(Expression.Convert(objParameter, _classType), shouldSerializeMethodInfo), objParameter);
                return lambdaExpression.Compile();
            }
            else
            {
                return null;
            }
        }

        private bool IsAnonymousType(Type type)
        {
            // don't test for too many things in case implementation details change in the future
            var typeInfo = type.GetTypeInfo();
            return
                typeInfo.GetCustomAttributes<CompilerGeneratedAttribute>(false).Any() &&
                typeInfo.IsGenericType &&
                type.Name.Contains("Anon"); // don't check for more than "Anon" so it works in mono also
        }

        private void ThrowFrozenException()
        {
            var message = string.Format("Class map for {0} has been frozen and no further changes are allowed.", _classType.FullName);
            throw new InvalidOperationException(message);
        }

        private void ThrowNotFrozenException()
        {
            var message = string.Format("Class map for {0} has been not been frozen yet.", _classType.FullName);
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Represents a mapping between a class and a BSON document.
    /// </summary>
    /// <typeparam name="TClass">The class.</typeparam>
    public class BsonClassMap<TClass> : BsonClassMap
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonClassMap class.
        /// </summary>
        public BsonClassMap()
            : base(typeof(TClass))
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonClassMap class.
        /// </summary>
        /// <param name="classMapInitializer">The class map initializer.</param>
        public BsonClassMap(Action<BsonClassMap<TClass>> classMapInitializer)
            : base(typeof(TClass))
        {
            classMapInitializer(this);
        }

        // public methods
        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <returns>An instance.</returns>
        public new TClass CreateInstance()
        {
            return (TClass)base.CreateInstance();
        }

        /// <summary>
        /// Gets a member map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="memberLambda">A lambda expression specifying the member.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap GetMemberMap<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            var memberName = GetMemberNameFromLambda(memberLambda);
            return GetMemberMap(memberName);
        }

        /// <summary>
        /// Creates a creator map and adds it to the class map.
        /// </summary>
        /// <param name="creatorLambda">Lambda expression specifying the creator code and parameters to use.</param>
        /// <returns>The member map.</returns>
        public BsonCreatorMap MapCreator(Expression<Func<TClass, TClass>> creatorLambda)
        {
            if (creatorLambda == null)
            {
                throw new ArgumentNullException("creatorLambda");
            }

            IEnumerable<MemberInfo> arguments;
            var @delegate = new CreatorMapDelegateCompiler().CompileCreatorDelegate(creatorLambda, out arguments);
            var creatorMap = MapCreator(@delegate);
            creatorMap.SetArguments(arguments);
            return creatorMap;
        }

        /// <summary>
        /// Creates a member map for the extra elements field and adds it to the class map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="fieldLambda">A lambda expression specifying the extra elements field.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap MapExtraElementsField<TMember>(Expression<Func<TClass, TMember>> fieldLambda)
        {
            var fieldMap = MapField(fieldLambda);
            SetExtraElementsMember(fieldMap);
            return fieldMap;
        }

        /// <summary>
        /// Creates a member map for the extra elements member and adds it to the class map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="memberLambda">A lambda expression specifying the extra elements member.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap MapExtraElementsMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            var memberMap = MapMember(memberLambda);
            SetExtraElementsMember(memberMap);
            return memberMap;
        }

        /// <summary>
        /// Creates a member map for the extra elements property and adds it to the class map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="propertyLambda">A lambda expression specifying the extra elements property.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap MapExtraElementsProperty<TMember>(Expression<Func<TClass, TMember>> propertyLambda)
        {
            var propertyMap = MapProperty(propertyLambda);
            SetExtraElementsMember(propertyMap);
            return propertyMap;
        }

        /// <summary>
        /// Creates a member map for a field and adds it to the class map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="fieldLambda">A lambda expression specifying the field.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap MapField<TMember>(Expression<Func<TClass, TMember>> fieldLambda)
        {
            return MapMember(fieldLambda);
        }

        /// <summary>
        /// Creates a member map for the Id field and adds it to the class map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="fieldLambda">A lambda expression specifying the Id field.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap MapIdField<TMember>(Expression<Func<TClass, TMember>> fieldLambda)
        {
            var fieldMap = MapField(fieldLambda);
            SetIdMember(fieldMap);
            return fieldMap;
        }

        /// <summary>
        /// Creates a member map for the Id member and adds it to the class map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="memberLambda">A lambda expression specifying the Id member.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap MapIdMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            var memberMap = MapMember(memberLambda);
            SetIdMember(memberMap);
            return memberMap;
        }

        /// <summary>
        /// Creates a member map for the Id property and adds it to the class map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="propertyLambda">A lambda expression specifying the Id property.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap MapIdProperty<TMember>(Expression<Func<TClass, TMember>> propertyLambda)
        {
            var propertyMap = MapProperty(propertyLambda);
            SetIdMember(propertyMap);
            return propertyMap;
        }

        /// <summary>
        /// Creates a member map and adds it to the class map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="memberLambda">A lambda expression specifying the member.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap MapMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            var memberInfo = GetMemberInfoFromLambda(memberLambda);
            return MapMember(memberInfo);
        }

        /// <summary>
        /// Creates a member map for the Id property and adds it to the class map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="propertyLambda">A lambda expression specifying the Id property.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap MapProperty<TMember>(Expression<Func<TClass, TMember>> propertyLambda)
        {
            return MapMember(propertyLambda);
        }

        /// <summary>
        /// Removes the member map for a field from the class map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="fieldLambda">A lambda expression specifying the field.</param>
        public void UnmapField<TMember>(Expression<Func<TClass, TMember>> fieldLambda)
        {
            UnmapMember(fieldLambda);
        }

        /// <summary>
        /// Removes a member map from the class map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="memberLambda">A lambda expression specifying the member.</param>
        public void UnmapMember<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            var memberInfo = GetMemberInfoFromLambda(memberLambda);
            UnmapMember(memberInfo);
        }

        /// <summary>
        /// Removes a member map for a property from the class map.
        /// </summary>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="propertyLambda">A lambda expression specifying the property.</param>
        public void UnmapProperty<TMember>(Expression<Func<TClass, TMember>> propertyLambda)
        {
            UnmapMember(propertyLambda);
        }

        // private static methods
        private static MethodInfo[] GetPropertyAccessors(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetAccessors(true);
        }

        private static MemberInfo GetMemberInfoFromLambda<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            var body = memberLambda.Body;
            MemberExpression memberExpression;
            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    memberExpression = (MemberExpression)body;
                    break;
                case ExpressionType.Convert:
                    var convertExpression = (UnaryExpression)body;
                    memberExpression = (MemberExpression)convertExpression.Operand;
                    break;
                default:
                    throw new BsonSerializationException("Invalid lambda expression");
            }
            var memberInfo = memberExpression.Member;
            if (memberInfo is PropertyInfo)
            {
                if (memberInfo.DeclaringType.GetTypeInfo().IsInterface)
                {
                    memberInfo = FindPropertyImplementation((PropertyInfo)memberInfo, typeof(TClass));
                }
            }
            else if (!(memberInfo is FieldInfo))
            {
                throw new BsonSerializationException("Invalid lambda expression");
            }
            return memberInfo;
        }

        private static string GetMemberNameFromLambda<TMember>(Expression<Func<TClass, TMember>> memberLambda)
        {
            return GetMemberInfoFromLambda(memberLambda).Name;
        }

        private static PropertyInfo FindPropertyImplementation(PropertyInfo interfacePropertyInfo, Type actualType)
        {
            var interfaceType = interfacePropertyInfo.DeclaringType;

#if NETSTANDARD1_5 || NETSTANDARD1_6
            var actualTypeInfo = actualType.GetTypeInfo();
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var actualTypePropertyInfos = actualTypeInfo.GetMembers(bindingFlags).OfType<PropertyInfo>();

            var explicitlyImplementedPropertyName = $"{interfacePropertyInfo.DeclaringType.FullName}.{interfacePropertyInfo.Name}".Replace("+", ".");
            var explicitlyImplementedPropertyInfo = actualTypePropertyInfos
                .Where(p => p.Name == explicitlyImplementedPropertyName)
                .SingleOrDefault();
            if (explicitlyImplementedPropertyInfo != null)
            {
                return explicitlyImplementedPropertyInfo;
            }

            var implicitlyImplementedPropertyInfo = actualTypePropertyInfos
                .Where(p => p.Name == interfacePropertyInfo.Name && p.PropertyType == interfacePropertyInfo.PropertyType)
                .SingleOrDefault();
            if (implicitlyImplementedPropertyInfo != null)
            {
                return implicitlyImplementedPropertyInfo;
            }

            throw new BsonSerializationException($"Unable to find property info for property: '{interfacePropertyInfo.Name}'.");
#else
            // An interface map must be used because because there is no
            // other officially documented way to derive the explicitly
            // implemented property name.
            var interfaceMap = actualType.GetInterfaceMap(interfaceType);

            var interfacePropertyAccessors = GetPropertyAccessors(interfacePropertyInfo);

            var actualPropertyAccessors = interfacePropertyAccessors.Select(interfacePropertyAccessor =>
            {
                var index = Array.IndexOf<MethodInfo>(interfaceMap.InterfaceMethods, interfacePropertyAccessor);

                return interfaceMap.TargetMethods[index];
            });

            // Binding must be done by accessor methods because interface
            // maps only map accessor methods and do not map properties.
            return actualType.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Single(propertyInfo =>
                {
                    // we are looking for a property that implements all the required accessors
                    var propertyAccessors = GetPropertyAccessors(propertyInfo);
                    return actualPropertyAccessors.All(x => propertyAccessors.Contains(x));
                });
#endif
        }
    }
}
