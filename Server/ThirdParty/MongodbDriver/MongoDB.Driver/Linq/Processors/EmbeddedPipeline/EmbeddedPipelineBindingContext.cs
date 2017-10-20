/* Copyright 2015 MongoDB Inc.
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

namespace MongoDB.Driver.Linq.Processors.EmbeddedPipeline
{
    internal sealed class EmbeddedPipelineBindingContext : IBindingContext
    {
        private readonly Dictionary<Expression, Guid> _correlationMapping;
        private readonly Dictionary<Expression, Expression> _expressionMapping;
        private readonly Dictionary<MemberInfo, Expression> _memberMapping;
        private readonly IBindingContext _parent;

        public EmbeddedPipelineBindingContext(IBindingContext parent)
        {
            _parent = Ensure.IsNotNull(parent, nameof(parent));

            _correlationMapping = new Dictionary<Expression, Guid>();
            _expressionMapping = new Dictionary<Expression, Expression>();
            _memberMapping = new Dictionary<MemberInfo, Expression>();
        }

        public IBsonSerializerRegistry SerializerRegistry
        {
            get { return _parent.SerializerRegistry; }
        }

        public void AddCorrelatingId(Expression node, Guid correlatingId)
        {
            _correlationMapping.Add(node, correlatingId);
        }

        public void AddExpressionMapping(Expression original, Expression replacement)
        {
            _expressionMapping[original] = replacement;
        }

        public void AddMemberMapping(MemberInfo member, Expression replacement)
        {
            _memberMapping[member] = replacement;
        }

        public Expression Bind(Expression node)
        {
            return SerializationBinder.Bind(node, this);
        }

        public IBsonSerializer GetSerializer(Type type, Expression node)
        {
            return _parent.GetSerializer(type, node);
        }

        public bool TryGetCorrelatingId(Expression node, out Guid correlatingId)
        {
            return _correlationMapping.TryGetValue(node, out correlatingId) ||
                _parent.TryGetCorrelatingId(node, out correlatingId);
        }

        public bool TryGetExpressionMapping(Expression original, out Expression replacement)
        {
            return _expressionMapping.TryGetValue(original, out replacement) ||
                _parent.TryGetExpressionMapping(original, out replacement);
        }

        public bool TryGetMemberMapping(MemberInfo member, out Expression replacement)
        {
            return _memberMapping.TryGetValue(member, out replacement) ||
                _parent.TryGetMemberMapping(member, out replacement);
        }
    }
}
