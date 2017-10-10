/* Copyright 2015-2017 MongoDB Inc.
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
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors
{
    internal sealed class PipelineBindingContext : IBindingContext
    {
        private readonly Dictionary<Expression, Guid> _correlationMapping;
        private readonly Dictionary<Expression, Expression> _expressionMapping;
        private readonly Dictionary<MemberInfo, Expression> _memberMapping;
        private readonly IBsonSerializerRegistry _serializerRegistry;

        public PipelineBindingContext(IBsonSerializerRegistry serializerRegistry)
        {
            _serializerRegistry = Ensure.IsNotNull(serializerRegistry, nameof(serializerRegistry));
            _correlationMapping = new Dictionary<Expression, Guid>();
            _expressionMapping = new Dictionary<Expression, Expression>();
            _memberMapping = new Dictionary<MemberInfo, Expression>();
        }

        public IBsonSerializerRegistry SerializerRegistry
        {
            get { return _serializerRegistry; }
        }

        public void AddCorrelatingId(Expression node, Guid correlatingId)
        {
            Ensure.IsNotNull(node, nameof(node));

            _correlationMapping.Add(node, correlatingId);
        }

        public void AddExpressionMapping(Expression original, Expression replacement)
        {
            Ensure.IsNotNull(original, nameof(original));
            Ensure.IsNotNull(replacement, nameof(replacement));

            _expressionMapping[original] = replacement;
        }

        public void AddMemberMapping(MemberInfo member, Expression replacement)
        {
            Ensure.IsNotNull(member, nameof(member));
            Ensure.IsNotNull(replacement, nameof(replacement));

            _memberMapping[member] = replacement;
        }

        public Expression Bind(Expression node)
        {
            return Bind(node, false);
        }

        public Expression Bind(Expression node, bool isClientSideProjection)
        {
            Ensure.IsNotNull(node, nameof(node));

            return SerializationBinder.Bind(node, this, isClientSideProjection);
        }

        public IBsonSerializer GetSerializer(Type type, Expression node)
        {
            Ensure.IsNotNull(type, nameof(type));

            IBsonSerializer serializer;
            if (node != null && PreviouslyUsedSerializerFinder.TryFindSerializer(node, type, out serializer))
            {
                return serializer;
            }
            else if (node == null || type != node.Type)
            {
                serializer = _serializerRegistry.GetSerializer(type);
                if (node != null)
                {
                    var childConfigurableSerializer = serializer as IChildSerializerConfigurable;
                    if (childConfigurableSerializer != null)
                    {
                        var nodeSerializer = GetSerializer(node.Type, node);
                        var deepestChildSerializer = SerializerHelper.GetDeepestChildSerializer(childConfigurableSerializer);

                        if (nodeSerializer.ValueType == deepestChildSerializer.ValueType)
                        {
                            serializer = SerializerHelper.RecursiveConfigureChildSerializer(childConfigurableSerializer, nodeSerializer);
                        }
                    }
                }
            }
            else
            {
                serializer = SerializerBuilder.Build(node, _serializerRegistry);
            }

            return serializer;
        }

        public SerializationExpression BindProjector(ref Expression selector)
        {
            var projector = selector as SerializationExpression;
            if (selector.NodeType == ExpressionType.MemberInit || selector.NodeType == ExpressionType.New)
            {
                var serializer = SerializerBuilder.Build(selector, _serializerRegistry);

                projector = new DocumentExpression(serializer);
            }
            else if (projector == null || projector is PipelineExpression || projector is IFieldExpression || projector is ArrayIndexExpression)
            {
                var newFieldName = "__fld0";
                if (projector is IFieldExpression)
                {
                    // We don't have to do this, but it makes the output a little nicer.
                    newFieldName = ((IFieldExpression)projector).FieldName;
                }

                // the output of a $project stage must be a document, so 
                // if this isn't already a serialization expression and it's not
                // a new expression or member init, then we need to create an 
                // artificial field to project the computation into.
                var serializer = GetSerializer(selector.Type, selector);
                selector = new FieldAsDocumentExpression(selector, newFieldName, serializer);
                projector = new FieldExpression(newFieldName, serializer);
            }

            return projector;
        }

        public bool TryGetCorrelatingId(Expression node, out Guid correlatingId)
        {
            return _correlationMapping.TryGetValue(node, out correlatingId);
        }

        public bool TryGetExpressionMapping(Expression original, out Expression replacement)
        {
            Ensure.IsNotNull(original, nameof(original));

            return _expressionMapping.TryGetValue(original, out replacement);
        }

        public bool TryGetMemberMapping(MemberInfo member, out Expression replacement)
        {
            Ensure.IsNotNull(member, nameof(member));

            return _memberMapping.TryGetValue(member, out replacement);
        }
    }
}
