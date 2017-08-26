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
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// A helper class used to create and compile delegates for creator maps.
    /// </summary>
    public class CreatorMapDelegateCompiler : ExpressionVisitor
    {
        // private fields
        private ParameterExpression _prototypeParameter;
        private Dictionary<MemberInfo, ParameterExpression> _parameters;

        // public methods
        /// <summary>
        /// Creates and compiles a delegate that calls a constructor.
        /// </summary>
        /// <param name="constructorInfo">The constructor.</param>
        /// <returns>A delegate that calls the constructor.</returns>
        public Delegate CompileConstructorDelegate(ConstructorInfo constructorInfo)
        {
            // build and compile the following delegate:
            // (p1, p2, ...) => new TClass(p1, p2, ...)

            var parameters = constructorInfo.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
            var body = Expression.New(constructorInfo, parameters);
            var lambda = Expression.Lambda(body, parameters);
            return lambda.Compile();
        }

        /// <summary>
        /// Creates and compiles a delegate from a lambda expression.
        /// </summary>
        /// <typeparam name="TClass">The type of the class.</typeparam>
        /// <param name="creatorLambda">The lambda expression.</param>
        /// <param name="arguments">The arguments for the delegate's parameters.</param>
        /// <returns>A delegate.</returns>
        public Delegate CompileCreatorDelegate<TClass>(Expression<Func<TClass, TClass>> creatorLambda, out IEnumerable<MemberInfo> arguments)
        {
            // transform c => expression (where c is the prototype parameter)
            // to (p1, p2, ...) => expression' where expression' is expression with every c.X replaced by p#

            _prototypeParameter = creatorLambda.Parameters[0];
            _parameters = new Dictionary<MemberInfo, ParameterExpression>();
            var body = Visit(creatorLambda.Body);
            var lambda = Expression.Lambda(body, _parameters.Values.ToArray());
            var @delegate = lambda.Compile();

            arguments = _parameters.Keys.ToArray();
            return @delegate;
        }

        /// <summary>
        /// Creates and compiles a delegate that calls a factory method.
        /// </summary>
        /// <param name="methodInfo">the method.</param>
        /// <returns>A delegate that calls the factory method.</returns>
        public Delegate CompileFactoryMethodDelegate(MethodInfo methodInfo)
        {
            // build and compile the following delegate:
            // (p1, p2, ...) => factoryMethod(p1, p2, ...)

            var parameters = methodInfo.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
            var body = Expression.Call(methodInfo, parameters);
            var lambda = Expression.Lambda(body, parameters);
            return lambda.Compile();
        }

        // protected methods
        /// <summary>
        /// Visits a MemberExpression.
        /// </summary>
        /// <param name="node">The MemberExpression.</param>
        /// <returns>The MemberExpression (possibly modified).</returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == _prototypeParameter)
            {
                var memberInfo = node.Member;

                ParameterExpression parameter;
                if (!_parameters.TryGetValue(memberInfo, out parameter))
                {
                    var parameterName = string.Format("_p{0}_", _parameters.Count + 1); // avoid naming conflicts with body
                    parameter = Expression.Parameter(node.Type, parameterName);
                    _parameters.Add(memberInfo, parameter);
                }

                return parameter;
            }

            return base.VisitMember(node);
        }

        /// <summary>
        /// Visits a ParameterExpression.
        /// </summary>
        /// <param name="node">The ParameterExpression.</param>
        /// <returns>The ParameterExpression (possibly modified).</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == _prototypeParameter)
            {
                throw new BsonSerializationException("The only operations allowed on the prototype parameter are accessing a field or property.");
            }

            return base.VisitParameter(node);
        }
    }
}
