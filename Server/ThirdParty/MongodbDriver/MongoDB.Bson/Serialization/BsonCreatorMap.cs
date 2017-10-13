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
using System.Linq;
using System.Reflection;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents a mapping to a delegate and its arguments.
    /// </summary>
    public class BsonCreatorMap
    {
        // private fields
        private readonly BsonClassMap _classMap;
        private readonly MemberInfo _memberInfo; // null if there is no corresponding constructor or factory method
        private readonly Delegate _delegate;
        private bool _isFrozen;
        private List<MemberInfo> _arguments; // the members that define the values for the delegate's parameters

        // these values are set when Freeze is called
        private List<string> _elementNames; // the element names of the serialized arguments
        private Dictionary<string, object> _defaultValues; // not all arguments have default values

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonCreatorMap class.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        /// <param name="memberInfo">The member info (null if none).</param>
        /// <param name="delegate">The delegate.</param>
        public BsonCreatorMap(BsonClassMap classMap, MemberInfo memberInfo, Delegate @delegate)
        {
            if (classMap == null)
            {
                throw new ArgumentNullException("classMap");
            }
            if (@delegate == null)
            {
                throw new ArgumentNullException("delegate");
            }

            _classMap = classMap;
            _memberInfo = memberInfo;
            _delegate = @delegate;
        }

        // public properties
        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public IEnumerable<MemberInfo> Arguments
        {
            get { return _arguments; }
        }

        /// <summary>
        /// Gets the class map that this creator map belongs to.
        /// </summary>
        public BsonClassMap ClassMap
        {
            get { return _classMap; }
        }

        /// <summary>
        /// Gets the delegeate
        /// </summary>
        public Delegate Delegate
        {
            get { return _delegate; }
        }

        /// <summary>
        /// Gets the element names.
        /// </summary>
        public IEnumerable<string> ElementNames
        {
            get
            {
                if (!_isFrozen) { ThrowNotFrozenException(); }
                return _elementNames;
            }
        }

        /// <summary>
        /// Gets the member info (null if none).
        /// </summary>
        public MemberInfo MemberInfo
        {
            get { return _memberInfo; }
        }

        // public methods
        /// <summary>
        /// Freezes the creator map.
        /// </summary>
        public void Freeze()
        {
            if (!_isFrozen)
            {
                var allMemberMaps = _classMap.AllMemberMaps;

                var elementNames = new List<string>();
                var defaultValues = new Dictionary<string, object>();

                var expectedArgumentsCount = GetExpectedArgumentsCount();
                if (_arguments != null)
                {
                    if (_arguments.Count != expectedArgumentsCount)
                    {
                        throw new BsonSerializationException($"Creator map for class {_classMap.ClassType.FullName} has {expectedArgumentsCount} arguments, not {_arguments.Count}.");
                    }

                    foreach (var argument in _arguments)
                    {
                        var memberMap = allMemberMaps.FirstOrDefault(m => IsSameMember(m.MemberInfo, argument));
                        if (memberMap == null)
                        {
                            var message = string.Format("Member '{0}' is not mapped.", argument.Name);
                            throw new BsonSerializationException(message);
                        }
                        elementNames.Add(memberMap.ElementName);
                        if (memberMap.IsDefaultValueSpecified)
                        {
                            defaultValues.Add(memberMap.ElementName, memberMap.DefaultValue);
                        }
                    }
                }
                else
                {
                    if (expectedArgumentsCount != 0)
                    {
                        throw new BsonSerializationException($"Creator map for class {_classMap.ClassType.FullName} has {expectedArgumentsCount} arguments, but none are configured.");
                    }
                }

                _elementNames = elementNames;
                _defaultValues = defaultValues;
                _isFrozen = true;
            }
        }

        /// <summary>
        /// Gets whether there is a default value for a missing element.
        /// </summary>
        /// <param name="elementName">The element name.</param>
        /// <returns>True if there is a default value for element name; otherwise, false.</returns>
        public bool HasDefaultValue(string elementName)
        {
            if (!_isFrozen) { ThrowNotFrozenException(); }
            return _defaultValues.ContainsKey(elementName);
        }

        /// <summary>
        /// Sets the arguments for the creator map.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The creator map.</returns>
        public BsonCreatorMap SetArguments(IEnumerable<MemberInfo> arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            if (_isFrozen) { ThrowFrozenException(); }
            var argumentsList = arguments.ToList(); // only enumerate once

            var expectedArgumentsCount = GetExpectedArgumentsCount();
            if (argumentsList.Count != expectedArgumentsCount)
            {
                throw new ArgumentException($"Creator map for class {_classMap.ClassType.FullName} has {expectedArgumentsCount} arguments, not {argumentsList.Count}.", nameof(arguments));
            }

            _arguments = argumentsList;
            return this;
        }

        /// <summary>
        /// Sets the arguments for the creator map.
        /// </summary>
        /// <param name="argumentNames">The argument names.</param>
        /// <returns>The creator map.</returns>
        public BsonCreatorMap SetArguments(IEnumerable<string> argumentNames)
        {
            if (argumentNames == null)
            {
                throw new ArgumentNullException("argumentNames");
            }
            if (_isFrozen) { ThrowFrozenException(); }

            var classTypeInfo = _classMap.ClassType.GetTypeInfo();
            var arguments = new List<MemberInfo>();
            foreach (var argumentName in argumentNames)
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var memberInfos = classTypeInfo.GetMembers(bindingFlags)
                    .Where(m => m.Name == argumentName && (m is FieldInfo || m is PropertyInfo))
                    .ToArray();
                if (memberInfos.Length == 0)
                {
                    var message = string.Format("Class '{0}' does not have a member named '{1}'.", _classMap.ClassType.FullName, argumentName);
                    throw new BsonSerializationException(message);
                }
                else if (memberInfos.Length > 1)
                {
                    var message = string.Format("Class '{0}' has more than one member named '{1}'.", _classMap.ClassType.FullName, argumentName);
                    throw new BsonSerializationException(message);
                }
                arguments.Add(memberInfos[0]);
            }

            SetArguments(arguments);
            return this;
        }

        // internal methods
        internal object CreateInstance(Dictionary<string, object> values)
        {
            var arguments = new List<object>();

            // get the values for the arguments to be passed to the creator delegate
            foreach (var elementName in _elementNames)
            {
                object argument;
                if (values.TryGetValue(elementName, out argument))
                {
                    values.Remove(elementName);
                }
                else if (!_defaultValues.TryGetValue(elementName, out argument))
                {
                    // shouldn't happen unless there is a bug in ChooseBestCreator
                    throw new BsonInternalException();
                }
                arguments.Add(argument);
            }

            return _delegate.DynamicInvoke(arguments.ToArray());
        }

        // private methods
        private int GetExpectedArgumentsCount()
        {
            var constructorInfo = _memberInfo as ConstructorInfo;
            if (constructorInfo != null)
            {
                return constructorInfo.GetParameters().Length;
            }

            var methodInfo = _memberInfo as MethodInfo;
            if (methodInfo != null)
            {
                return methodInfo.GetParameters().Length;
            }

            var delegateParameters = _delegate.GetMethodInfo().GetParameters();
            if (delegateParameters.Length == 0)
            {
                return 0;
            }
            else
            {
                // check if delegate is closed over its first parameter
                if (_delegate.Target != null && _delegate.Target.GetType() == delegateParameters[0].ParameterType)
                {
                    return delegateParameters.Length - 1;
                }
                else
                {
                    return delegateParameters.Length;
                }
            }
        }

        private bool IsSameMember(MemberInfo a, MemberInfo b)
        {
            // two MemberInfos refer to the same member if the Module and MetadataToken are equal
            return a.Module == b.Module && a.MetadataToken == b.MetadataToken;
        }

        private void ThrowFrozenException()
        {
            throw new InvalidOperationException("BsonCreatorMap is frozen.");
        }

        private void ThrowNotFrozenException()
        {
            throw new InvalidOperationException("BsonCreatorMap is not frozen.");
        }
    }
}
