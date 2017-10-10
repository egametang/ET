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
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.Linq.Processors
{
    internal interface IBindingContext
    {
        IBsonSerializerRegistry SerializerRegistry { get; }

        void AddCorrelatingId(Expression node, Guid correlatingId);

        void AddExpressionMapping(Expression original, Expression replacement);

        void AddMemberMapping(MemberInfo member, Expression replacement);

        Expression Bind(Expression node);

        IBsonSerializer GetSerializer(Type type, Expression node);

        bool TryGetCorrelatingId(Expression node, out Guid correlatingId);

        bool TryGetExpressionMapping(Expression original, out Expression replacement);

        bool TryGetMemberMapping(MemberInfo member, out Expression replacement);
    }
}
