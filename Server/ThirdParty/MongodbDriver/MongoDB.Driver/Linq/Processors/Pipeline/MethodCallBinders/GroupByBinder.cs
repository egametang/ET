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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.Pipeline.MethodCallBinders
{
    internal sealed class GroupByBinder : IMethodCallBinder<PipelineBindingContext>
    {
        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            yield return MethodHelper.GetMethodDefinition(() => Enumerable.GroupBy<object, object>(null, null));
            yield return MethodHelper.GetMethodDefinition(() => Queryable.GroupBy<object, object>(null, null));
        }

        public Expression Bind(PipelineExpression pipeline, PipelineBindingContext bindingContext, MethodCallExpression node, IEnumerable<Expression> arguments)
        {
            var keySelector = BindKey(pipeline, bindingContext, arguments.First());

            var projectedType = typeof(IGrouping<,>).MakeGenericType(keySelector.Type, pipeline.Projector.Type);
            var groupExpression = new GroupByExpression(
                typeof(IEnumerable<>).MakeGenericType(projectedType),
                pipeline.Source,
                keySelector,
                Enumerable.Empty<AccumulatorExpression>());

            var projector = BuildProjector(bindingContext, keySelector, pipeline.Projector);

            var correlatingId = Guid.NewGuid();
            bindingContext.AddCorrelatingId(projector, correlatingId);
            return new PipelineExpression(
                new CorrelatedExpression(correlatingId, groupExpression),
                projector);
        }

        private Expression BindKey(PipelineExpression pipeline, PipelineBindingContext bindingContext, Expression node)
        {
            var lambda = ExpressionHelper.GetLambda(node);
            bindingContext.AddExpressionMapping(lambda.Parameters[0], pipeline.Projector);
            return bindingContext.Bind(lambda.Body);
        }

        private DocumentExpression BuildProjector(PipelineBindingContext bindingContext, Expression idSelector, Expression elementSelector)
        {
            var elementSerializer = bindingContext.GetSerializer(elementSelector.Type, elementSelector);
            var flattenedElementField = FieldExpressionFlattener.FlattenFields(elementSelector);
            var elementFieldName = (flattenedElementField as IFieldExpression)?.FieldName;

            var serializerType = typeof(GroupingDeserializer<,>).MakeGenericType(
                idSelector.Type,
                elementSelector.Type);
            var serializer = (IBsonSerializer)Activator.CreateInstance(
                serializerType,
                bindingContext.GetSerializer(idSelector.Type, idSelector),
                bindingContext.GetSerializer(elementSelector.Type, elementSelector),
                elementFieldName);

            return new DocumentExpression(serializer);
        }

        private class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
        {
            private readonly TKey _key;

            public Grouping(TKey key)
            {
                _key = key;
            }

            public TKey Key
            {
                get { return _key; }
            }

            public IEnumerator<TElement> GetEnumerator()
            {
                yield break;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class GroupingDeserializer<TKey, TElement> : SerializerBase<IGrouping<TKey, TElement>>, IBsonDocumentSerializer, IBsonArraySerializer
        {
            private readonly string _elementFieldName;
            private readonly IBsonSerializer _elementSerializer;
            private readonly IBsonSerializer _idSerializer;

            public GroupingDeserializer(IBsonSerializer idSerializer, IBsonSerializer elementSerializer, string elementFieldName)
            {
                _idSerializer = idSerializer;
                _elementSerializer = elementSerializer;
                _elementFieldName = elementFieldName;
            }

            public override IGrouping<TKey, TElement> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                var reader = context.Reader;
                TKey key = default(TKey);
                reader.ReadStartDocument();
                while (context.Reader.ReadBsonType() != 0)
                {
                    var fieldName = reader.ReadName();
                    if (fieldName == "_id")
                    {
                        key = (TKey)_idSerializer.Deserialize(context);
                    }
                    else
                    {
                        reader.SkipValue();
                    }
                }
                reader.ReadEndDocument();
                return new Grouping<TKey, TElement>(key);
            }

            public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
            {
                if (memberName == "Key")
                {
                    serializationInfo = new BsonSerializationInfo("_id", _idSerializer, _idSerializer.ValueType);
                    return true;
                }

                serializationInfo = null;
                return false;
            }

            public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
            {
                serializationInfo = new BsonSerializationInfo(_elementFieldName, _elementSerializer, typeof(TElement));
                return true;
            }
        }
    }
}
