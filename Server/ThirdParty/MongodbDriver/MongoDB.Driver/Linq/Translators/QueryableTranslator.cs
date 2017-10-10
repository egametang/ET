/* Copyright 2015-2016 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Expressions.ResultOperators;
using MongoDB.Driver.Support;

namespace MongoDB.Driver.Linq.Translators
{
    internal sealed class QueryableTranslator
    {
        public static QueryableTranslation Translate(Expression node, IBsonSerializerRegistry serializerRegistry, ExpressionTranslationOptions translationOptions)
        {
            var translator = new QueryableTranslator(serializerRegistry, translationOptions);
            translator.Translate(node);

            var outputType = translator._outputSerializer.ValueType;
            var modelType = typeof(AggregateQueryableExecutionModel<>).MakeGenericType(outputType);
            var modelTypeInfo = modelType.GetTypeInfo();
            var outputSerializerInterfaceType = typeof(IBsonSerializer<>).MakeGenericType(new[] { outputType });
            var constructorParameterTypes = new Type[] { typeof(IEnumerable<BsonDocument>), outputSerializerInterfaceType };
            var constructorInfo = modelTypeInfo.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(constructorParameterTypes))
                .Single();
            var constructorParameters = new object[] { translator._stages, translator._outputSerializer };
            var model = (QueryableExecutionModel)constructorInfo.Invoke(constructorParameters);

            return new QueryableTranslation(model, translator._resultTransformer);
        }

        private IBsonSerializer _outputSerializer;
        private readonly IBsonSerializerRegistry _serializerRegistry;
        private readonly List<BsonDocument> _stages;
        private IResultTransformer _resultTransformer;
        private ExpressionTranslationOptions _translationOptions;

        public QueryableTranslator(IBsonSerializerRegistry serializerRegistry, ExpressionTranslationOptions translationOptions)
        {
            _serializerRegistry = Ensure.IsNotNull(serializerRegistry, nameof(serializerRegistry));
            _translationOptions = translationOptions; // can be null
            _stages = new List<BsonDocument>();
        }

        private void Translate(Expression node)
        {
            var extensionExpression = node as ExtensionExpression;
            if (extensionExpression != null)
            {
                switch (extensionExpression.ExtensionType)
                {
                    case ExtensionExpressionType.Collection:
                        // we do nothing in this regard...
                        return;
                    case ExtensionExpressionType.GroupBy:
                        TranslateGroupBy((GroupByExpression)node);
                        return;
                    case ExtensionExpressionType.GroupByWithResultSelector:
                        TranslateGroupByWithResultSelector((GroupByWithResultSelectorExpression)node);
                        return;
                    case ExtensionExpressionType.GroupJoin:
                        TranslateGroupJoin((GroupJoinExpression)node);
                        return;
                    case ExtensionExpressionType.Join:
                        TranslateJoin((JoinExpression)node);
                        return;
                    case ExtensionExpressionType.OrderBy:
                        TranslateOrderBy((OrderByExpression)node);
                        return;
                    case ExtensionExpressionType.Pipeline:
                        TranslatePipeline((PipelineExpression)node);
                        return;
                    case ExtensionExpressionType.Sample:
                        TranslateSample((SampleExpression)node);
                        return;
                    case ExtensionExpressionType.Select:
                        TranslateSelect((SelectExpression)node);
                        return;
                    case ExtensionExpressionType.SelectMany:
                        TranslateSelectMany((SelectManyExpression)node);
                        return;
                    case ExtensionExpressionType.Skip:
                        TranslateSkip((SkipExpression)node);
                        return;
                    case ExtensionExpressionType.Take:
                        TranslateTake((TakeExpression)node);
                        return;
                    case ExtensionExpressionType.Where:
                        TranslateWhere((WhereExpression)node);
                        return;
                }
            }

            // TODO: better exception message
            var message = string.Format("The expression is not supported in a pipeline: {0}", node.ToString());
            throw new NotSupportedException(message);
        }

        private void TranslateGroupBy(GroupByExpression node)
        {
            Translate(node.Source);

            var groupValue = new BsonDocument();
            var idValue = AggregateLanguageTranslator.Translate(node.KeySelector, _translationOptions);
            groupValue.Add("_id", idValue);
            foreach (var accumulator in node.Accumulators)
            {
                var accumulatorValue = AggregateLanguageTranslator.Translate(accumulator, _translationOptions);
                groupValue.Add(accumulator.FieldName, accumulatorValue);
            }

            _stages.Add(new BsonDocument("$group", groupValue));
        }

        private void TranslateGroupByWithResultSelector(GroupByWithResultSelectorExpression node)
        {
            Translate(node.Source);

            var projection = TranslateProjectValue(node.Selector);
            _stages.Add(new BsonDocument("$group", projection));
        }

        private void TranslateGroupJoin(GroupJoinExpression node)
        {
            Translate(node.Source);

            var joined = node.Joined as CollectionExpression;
            if (joined == null)
            {
                throw new NotSupportedException("Only a collection is allowed to be joined.");
            }

            var localFieldValue = AggregateLanguageTranslator.Translate(node.SourceKeySelector, _translationOptions);
            if (localFieldValue.BsonType != BsonType.String)
            {
                throw new NotSupportedException("Could not translate the local field.");
            }

            var localField = localFieldValue.ToString().Substring(1); // remove '$'

            var foreignFieldValue = AggregateLanguageTranslator.Translate(node.JoinedKeySelector, _translationOptions);
            if (foreignFieldValue.BsonType != BsonType.String)
            {
                throw new NotSupportedException("Could not translate the foreign field.");
            }

            var foreignField = foreignFieldValue.ToString().Substring(1); // remove '$'

            _stages.Add(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", ((CollectionExpression)node.Joined).CollectionNamespace.CollectionName },
                { "localField", localField },
                { "foreignField", foreignField },
                { "as", node.JoinedItemName }
            }));
        }

        private void TranslateJoin(JoinExpression node)
        {
            Translate(node.Source);

            var joined = node.Joined as CollectionExpression;
            if (joined == null)
            {
                throw new NotSupportedException("Only a collection is allowed to be joined.");
            }

            var localFieldValue = AggregateLanguageTranslator.Translate(node.SourceKeySelector, _translationOptions);
            if (localFieldValue.BsonType != BsonType.String)
            {
                throw new NotSupportedException("Could not translate the local field.");
            }

            var localField = localFieldValue.ToString().Substring(1); // remove '$'

            var foreignFieldValue = AggregateLanguageTranslator.Translate(node.JoinedKeySelector, _translationOptions);
            if (foreignFieldValue.BsonType != BsonType.String)
            {
                throw new NotSupportedException("Could not translate the foreign field.");
            }

            var foreignField = foreignFieldValue.ToString().Substring(1); // remove '$'

            _stages.Add(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", ((CollectionExpression)node.Joined).CollectionNamespace.CollectionName },
                { "localField", localField },
                { "foreignField", foreignField },
                { "as", node.JoinedItemName }
            }));

            _stages.Add(new BsonDocument("$unwind", "$" + node.JoinedItemName));
        }

        private void TranslateOrderBy(OrderByExpression node)
        {
            Translate(node.Source);

            BsonDocument sort = new BsonDocument();
            foreach (var clause in node.Clauses)
            {
                var field = FieldExpressionFlattener.FlattenFields(clause.Expression) as IFieldExpression;
                if (field == null)
                {
                    throw new NotSupportedException("Only fields are allowed in a $sort.");
                }

                var direction = clause.Direction == SortDirection.Ascending ? 1 : -1;
                if (sort.Contains(field.FieldName))
                {
                    var message = string.Format("Redundant ordering fields are not supported: {0}.", field.FieldName);
                    throw new NotSupportedException(message);
                }

                sort.Add(field.FieldName, direction);
            }

            _stages.Add(new BsonDocument("$sort", sort));
        }

        private void TranslatePipeline(PipelineExpression node)
        {
            Translate(node.Source);

            var serializationExpression = node.Projector as ISerializationExpression;
            var fieldExpression = node.Projector as FieldExpression; // not IFieldExpression
            if (fieldExpression != null)
            {
                var info = new BsonSerializationInfo(fieldExpression.FieldName, fieldExpression.Serializer, fieldExpression.Serializer.ValueType);

                // We are projecting a field, however the server only responds
                // with documents. So we'll create a projector that reads a document
                // and then projects the field out of it.
                var parameter = Expression.Parameter(typeof(ProjectedObject), "document");
                var projector = Expression.Lambda(
                    Expression.Call(
                        parameter,
                        "GetValue",
                        new Type[] { info.Serializer.ValueType },
                        Expression.Constant(info.ElementName),
                        Expression.Constant(info.Serializer.ValueType.GetDefaultValue(), typeof(object))),
                    parameter);
                var innerSerializer = new ProjectedObjectDeserializer(new[] { info });
                _outputSerializer = (IBsonSerializer)Activator.CreateInstance(
                    typeof(ProjectingDeserializer<,>).MakeGenericType(typeof(ProjectedObject), info.Serializer.ValueType),
                    new object[]
                        {
                                innerSerializer,
                                projector.Compile()
                        });
            }
            else if (serializationExpression != null)
            {
                _outputSerializer = serializationExpression.Serializer;
            }
            else
            {
                throw new NotSupportedException();
            }

            _resultTransformer = node.ResultOperator as IResultTransformer;
        }

        private void TranslateSample(SampleExpression node)
        {
            Translate(node.Source);

            _stages.Add(new BsonDocument("$sample", new BsonDocument("size", (long)((ConstantExpression)node.Count).Value)));
        }

        private void TranslateSelect(SelectExpression node)
        {
            Translate(node.Source);

            var projectValue = TranslateProjectValue(node.Selector);
            _stages.Add(new BsonDocument("$project", projectValue));
        }

        private void TranslateSelectMany(SelectManyExpression node)
        {
            Translate(node.Source);

            var isLeftOuterJoin = false;
            IFieldExpression field;
            var collectionSelector = node.CollectionSelector as PipelineExpression;
            if (collectionSelector != null)
            {
                var defaultIfEmpty = collectionSelector.Source as DefaultIfEmptyExpression;
                if (defaultIfEmpty != null)
                {
                    isLeftOuterJoin = true;
                    field = defaultIfEmpty.Source as IFieldExpression;
                }
                else
                {
                    field = collectionSelector.Source as IFieldExpression;
                }
            }
            else
            {
                field = node.CollectionSelector as IFieldExpression;
            }

            if (field == null)
            {
                var message = string.Format("The collection selector must be a field: {0}", node.ToString());
                throw new NotSupportedException(message);
            }

            BsonValue unwindValue = "$" + field.FieldName;
            var groupJoin = node.Source as GroupJoinExpression;
            if (groupJoin != null && isLeftOuterJoin)
            {
                unwindValue = new BsonDocument
                {
                    { "path", unwindValue },
                    { "preserveNullAndEmptyArrays", true }
                };
            }

            _stages.Add(new BsonDocument("$unwind", unwindValue));

            var projectValue = TranslateProjectValue(node.ResultSelector);
            _stages.Add(new BsonDocument("$project", projectValue));
        }

        private void TranslateSkip(SkipExpression node)
        {
            Translate(node.Source);

            _stages.Add(new BsonDocument("$skip", (int)((ConstantExpression)node.Count).Value));
        }

        private void TranslateTake(TakeExpression node)
        {
            Translate(node.Source);

            _stages.Add(new BsonDocument("$limit", (int)((ConstantExpression)node.Count).Value));
        }

        private void TranslateWhere(WhereExpression node)
        {
            Translate(node.Source);

            var predicateValue = PredicateTranslator.Translate(node.Predicate, _serializerRegistry);
            _stages.Add(new BsonDocument("$match", predicateValue));
        }

        private BsonDocument TranslateProjectValue(Expression selector)
        {
            BsonDocument projectValue;
            var result = AggregateLanguageTranslator.Translate(selector, _translationOptions);
            if (result.BsonType == BsonType.String)
            {
                // this means we got back a field expression prefixed with a $ sign.
                projectValue = new BsonDocument(result.ToString().Substring(1), 1);
            }
            else if (result.BsonType == BsonType.Document)
            {
                projectValue = (BsonDocument)result;
            }
            else
            {
                throw new NotSupportedException($"The expression {selector.ToString()} is not supported.");
            }

            if (!projectValue.Contains("_id"))
            {
                projectValue.Add("_id", 0);
            }

            return projectValue;
        }
    }
}
