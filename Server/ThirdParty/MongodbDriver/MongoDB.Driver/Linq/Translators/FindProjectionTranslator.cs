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
* 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors;
using MongoDB.Driver.Support;

namespace MongoDB.Driver.Linq.Translators
{
    internal sealed class FindProjectionTranslator : ExtensionExpressionVisitor
    {
        public static RenderedProjectionDefinition<TProjection> Translate<TDocument, TProjection>(Expression<Func<TDocument, TProjection>> projector, IBsonSerializer<TDocument> parameterSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var bindingContext = new PipelineBindingContext(serializerRegistry);
            var documentExpression = new DocumentExpression(parameterSerializer);
            bindingContext.AddExpressionMapping(projector.Parameters[0], documentExpression);

            var node = PartialEvaluator.Evaluate(projector.Body);
            node = Transformer.Transform(node);
            node = bindingContext.Bind(node, isClientSideProjection: true);
            node = FieldExpressionFlattener.FlattenFields(node);

            BsonDocument projectionDocument = null;
            IBsonSerializer<TProjection> serializer;
            if (node is DocumentExpression)
            {
                serializer = new ProjectingDeserializer<TDocument, TProjection>(parameterSerializer, projector.Compile());
            }
            else
            {
                var candidateFields = SerializationExpressionGatherer.Gather(node);
                var fields = GetUniqueFieldsByHierarchy(candidateFields);
                var serializationInfo = fields.Select(x => new BsonSerializationInfo(x.FieldName, x.Serializer, x.Serializer.ValueType)).ToList();

                var projectedObjectExpression = Expression.Parameter(typeof(ProjectedObject), "document");
                var translator = new FindProjectionTranslator(documentExpression, projectedObjectExpression, fields);
                var translated = translator.Visit(node);
                if (translator._fullDocument)
                {
                    serializer = new ProjectingDeserializer<TDocument, TProjection>(parameterSerializer, projector.Compile());
                }
                else
                {
                    var newProjector = Expression.Lambda<Func<ProjectedObject, TProjection>>(
                        translated,
                        projectedObjectExpression);

                    projectionDocument = GetProjectionDocument(serializationInfo);
                    var projectedObjectSerializer = new ProjectedObjectDeserializer(serializationInfo);
                    serializer = new ProjectingDeserializer<ProjectedObject, TProjection>(projectedObjectSerializer, newProjector.Compile());
                }
            }

            return new RenderedProjectionDefinition<TProjection>(projectionDocument, serializer);
        }

        private readonly IReadOnlyList<IFieldExpression> _fields;
        private bool _fullDocument;
        private readonly DocumentExpression _documentExpression;
        private readonly ParameterExpression _parameterExpression;

        private FindProjectionTranslator(DocumentExpression documentExpression, ParameterExpression parameterExpression, IReadOnlyList<IFieldExpression> fields)
        {
            _documentExpression = Ensure.IsNotNull(documentExpression, nameof(documentExpression));
            _parameterExpression = Ensure.IsNotNull(parameterExpression, nameof(parameterExpression));
            _fields = fields;
        }

        protected internal override Expression VisitField(FieldExpression node)
        {
            if (!_fields.Any(x => x.FieldName == node.FieldName
                && x.Serializer.ValueType == node.Serializer.ValueType))
            {
                // we need to unwind this call...
                return Visit(node.Original);
            }

            return Expression.Call(
                _parameterExpression,
                "GetValue",
                new[] { node.Type },
                Expression.Constant(node.FieldName),
                Expression.Constant(node.Serializer.ValueType.GetDefaultValue(), typeof(object)));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!ExpressionHelper.IsLinqMethod(node, "Select", "SelectMany"))
            {
                return base.VisitMethodCall(node);
            }

            var source = node.Arguments[0] as IFieldExpression;
            if (source != null && !_fields.Any(x => x.FieldName == source.FieldName))
            {
                // We are projecting off an embedded array, but we have selected the entire
                // array and not just values within it.
                var selector = (LambdaExpression)Visit((LambdaExpression)node.Arguments[1]);
                var nestedParameter = Expression.Parameter(_parameterExpression.Type, selector.Parameters[0].Name);
                var nestedBody = new ProjectedObjectFieldReplacer().Replace(selector.Body, source.FieldName, nestedParameter);

                var newSourceType = typeof(IEnumerable<>).MakeGenericType(nestedParameter.Type);
                var newSource =
                    Expression.Call(
                        typeof(Enumerable),
                        "Cast",
                        new[] { typeof(ProjectedObject) },
                        Expression.Call(
                            _parameterExpression,
                            "GetValue",
                            new[] { typeof(IEnumerable<object>) },
                            Expression.Constant(source.FieldName),
                            Expression.Constant(newSourceType.GetDefaultValue(), typeof(object))));

                return Expression.Call(
                    typeof(Enumerable),
                    node.Method.Name,
                    new Type[] { nestedParameter.Type, node.Method.GetGenericArguments()[1] },
                    newSource,
                    Expression.Lambda(
                        nestedBody,
                        nestedParameter));
            }

            return base.VisitMethodCall(node);
        }

        protected internal override Expression VisitDocument(DocumentExpression node)
        {
            if (node == _documentExpression)
            {
                _fullDocument = true;
            }

            return base.VisitDocument(node);
        }

        private static BsonDocument GetProjectionDocument(IEnumerable<BsonSerializationInfo> used)
        {
            var includeId = false;
            var document = new BsonDocument();
            foreach (var u in used)
            {
                if (u.ElementName == "_id")
                {
                    includeId = true;
                }
                document.Add(u.ElementName, 1);
            }

            if (!includeId)
            {
                document.Add("_id", 0);
            }
            return document;
        }

        private static IReadOnlyList<IFieldExpression> GetUniqueFieldsByHierarchy(IEnumerable<IFieldExpression> usedFields)
        {
            // we want to leave out subelements when the parent element exists
            // for instance, if we have asked for both "d" and "d.e", we only want to send { "d" : 1 } to the server
            // 1) group all the used fields by their element name.
            // 2) order them by their element name in ascending order
            // 3) if any groups are prefixed by the current groups element, then skip it.

            var uniqueFields = new List<IFieldExpression>();
            var skippedFields = new List<string>();
            var referenceGroups = new Queue<IGrouping<string, IFieldExpression>>(usedFields.GroupBy(x => x.FieldName).OrderBy(x => x.Key));
            while (referenceGroups.Count > 0)
            {
                var referenceGroup = referenceGroups.Dequeue();
                if (!skippedFields.Contains(referenceGroup.Key))
                {
                    var hierarchicalReferenceGroups = referenceGroups.Where(x => x.Key.StartsWith(referenceGroup.Key));
                    uniqueFields.AddRange(referenceGroup);
                    skippedFields.AddRange(hierarchicalReferenceGroups.Select(x => x.Key));
                }
            }

            return uniqueFields.GroupBy(x => x.FieldName).Select(x => x.First()).ToList();
        }

        private class SerializationExpressionGatherer : ExtensionExpressionVisitor
        {
            public static IReadOnlyList<IFieldExpression> Gather(Expression node)
            {
                var gatherer = new SerializationExpressionGatherer();
                gatherer.Visit(node);
                return gatherer._fieldExpressions;
            }

            private List<IFieldExpression> _fieldExpressions;

            private SerializationExpressionGatherer()
            {
                _fieldExpressions = new List<IFieldExpression>();
            }

            protected internal override Expression VisitField(FieldExpression node)
            {
                _fieldExpressions.Add(node);
                return node;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (!ExpressionHelper.IsLinqMethod(node, "Select", "SelectMany"))
                {
                    return base.VisitMethodCall(node);
                }

                var source = node.Arguments[0] as IFieldExpression;
                if (source != null)
                {
                    var fields = Gather(node.Arguments[1]);
                    if (fields.Any(x => x.FieldName.StartsWith(source.FieldName)))
                    {
                        _fieldExpressions.AddRange(fields);
                        return node;
                    }
                }

                return base.VisitMethodCall(node);
            }
        }

        /// <summary>
        /// This guy is going to replace calls like store.GetValue("d.y") with nestedStore.GetValue("y").
        /// </summary>
        private class ProjectedObjectFieldReplacer : ExtensionExpressionVisitor
        {
            private string _keyPrefix;
            private Expression _source;

            public Expression Replace(Expression node, string keyPrefix, Expression source)
            {
                _keyPrefix = keyPrefix;
                _source = source;
                return Visit(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Object == null || node.Object.Type != typeof(ProjectedObject) || node.Method.Name != "GetValue")
                {
                    return base.VisitMethodCall(node);
                }

                var currentKey = (string)((ConstantExpression)node.Arguments[0]).Value;

                if (!currentKey.StartsWith(_keyPrefix))
                {
                    return base.VisitMethodCall(node);
                }

                var newElementName = currentKey;
                if (currentKey.Length > _keyPrefix.Length)
                {
                    newElementName = currentKey.Remove(0, _keyPrefix.Length + 1);
                }

                var defaultValue = ((ConstantExpression)node.Arguments[1]).Value;
                return Expression.Call(
                    _source,
                    "GetValue",
                    new[] { node.Type },
                    Expression.Constant(newElementName),
                    Expression.Constant(defaultValue, typeof(object)));
            }
        }
    }
}