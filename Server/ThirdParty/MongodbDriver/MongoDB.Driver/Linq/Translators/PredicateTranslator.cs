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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Expressions.ResultOperators;
using MongoDB.Driver.Linq.Processors;
using MongoDB.Driver.Support;

namespace MongoDB.Driver.Linq.Translators
{
    internal sealed class PredicateTranslator
    {
        #region static
        // private static fields
        private static readonly FilterDefinitionBuilder<BsonDocument> __builder = new FilterDefinitionBuilder<BsonDocument>();

        // public static methods
        public static BsonDocument Translate<TDocument>(Expression<Func<TDocument, bool>> predicate, IBsonSerializer<TDocument> parameterSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var parameterExpression = new DocumentExpression(parameterSerializer);
            var context = new PipelineBindingContext(serializerRegistry);
            context.AddExpressionMapping(predicate.Parameters[0], parameterExpression);

            var node = PartialEvaluator.Evaluate(predicate.Body);
            node = Transformer.Transform(node);
            node = context.Bind(node);

            return Translate(node, serializerRegistry);
        }

        public static BsonDocument Translate(Expression node, IBsonSerializerRegistry serializerRegistry)
        {
            var translator = new PredicateTranslator(serializerRegistry);
            node = FieldExpressionFlattener.FlattenFields(node);
            return translator.Translate(node)
                .Render(serializerRegistry.GetSerializer<BsonDocument>(), serializerRegistry);
        }
        #endregion

        // private fields
        private readonly IBsonSerializerRegistry _serializerRegistry;

        // constructors
        private PredicateTranslator(IBsonSerializerRegistry serializerRegistry)
        {
            _serializerRegistry = serializerRegistry;
        }

        // private methods
        private FilterDefinition<BsonDocument> Translate(Expression node)
        {
            FilterDefinition<BsonDocument> filter = null;

            switch (node.NodeType)
            {
                case ExpressionType.And:
                    filter = TranslateAnd((BinaryExpression)node);
                    break;
                case ExpressionType.AndAlso:
                    filter = TranslateAndAlso((BinaryExpression)node);
                    break;
                case ExpressionType.ArrayIndex:
                    filter = TranslateBoolean(node);
                    break;
                case ExpressionType.Call:
                    filter = TranslateMethodCall((MethodCallExpression)node);
                    break;
                case ExpressionType.Constant:
                    filter = TranslateConstant((ConstantExpression)node);
                    break;
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    filter = TranslateComparison((BinaryExpression)node);
                    break;
                case ExpressionType.MemberAccess:
                    filter = TranslateBoolean(node);
                    break;
                case ExpressionType.Not:
                    filter = TranslateNot((UnaryExpression)node);
                    break;
                case ExpressionType.Or:
                    filter = TranslateOr((BinaryExpression)node);
                    break;
                case ExpressionType.OrElse:
                    filter = TranslateOrElse((BinaryExpression)node);
                    break;
                case ExpressionType.TypeIs:
                    filter = TranslateTypeIsQuery((TypeBinaryExpression)node);
                    break;
                case ExpressionType.Extension:
                    var mongoExpression = node as ExtensionExpression;
                    if (mongoExpression != null)
                    {
                        switch (mongoExpression.ExtensionType)
                        {
                            case ExtensionExpressionType.FieldAsDocument:
                            case ExtensionExpressionType.Field:
                                if (mongoExpression.Type == typeof(bool))
                                {
                                    filter = TranslateBoolean(mongoExpression);
                                }
                                break;
                            case ExtensionExpressionType.InjectedFilter:
                                return TranslateInjectedFilter((InjectedFilterExpression)node);
                            case ExtensionExpressionType.Pipeline:
                                filter = TranslatePipeline((PipelineExpression)node);
                                break;
                        }
                    }
                    break;
            }

            if (filter == null)
            {
                var message = string.Format("Unsupported filter: {0}.", node);
                throw new ArgumentException(message);
            }

            return filter;
        }

        // private methods
        private FilterDefinition<BsonDocument> TranslateAndAlso(BinaryExpression node)
        {
            return __builder.And(Translate(node.Left), Translate(node.Right));
        }

        private FilterDefinition<BsonDocument> TranslateAnd(BinaryExpression node)
        {
            if (node.Left.Type == typeof(bool) && node.Right.Type == typeof(bool))
            {
                return TranslateAndAlso(node);
            }

            return null;
        }

        private bool CanAnyBeRenderedWithoutElemMatch(Expression node)
        {
            switch (node.NodeType)
            {
                // this doesn't cover all cases, but absolutely covers
                // the most common ones. This is opt-in behavior, so
                // when someone else discovers an Any query that shouldn't
                // be rendered with $elemMatch, we'll have to add it in.
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    return true;
                case ExpressionType.Call:
                    var callNode = (MethodCallExpression)node;
                    switch (callNode.Method.Name)
                    {
                        case "Contains":
                        case "StartsWith":
                        case "EndsWith":
                            return true;
                        default:
                            return false;
                    }
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Not:
                    var unaryExpression = (UnaryExpression)node;
                    return CanAnyBeRenderedWithoutElemMatch(unaryExpression.Operand);
                case ExpressionType.Extension:
                    var pipelineExpression = node as PipelineExpression;
                    if (pipelineExpression != null)
                    {
                        var source = pipelineExpression.Source as ISerializationExpression;
                        return source == null;
                    }

                    return false;
                default:
                    return false;
            }
        }

        private FilterDefinition<BsonDocument> TranslateArrayLength(Expression variableNode, ExpressionType operatorType, ConstantExpression constantNode)
        {
            var allowedOperators = new[]
            {
                ExpressionType.Equal,
                ExpressionType.NotEqual,
                ExpressionType.GreaterThan,
                ExpressionType.GreaterThanOrEqual,
                ExpressionType.LessThan,
                ExpressionType.LessThanOrEqual
            };

            if (!allowedOperators.Contains(operatorType))
            {
                return null;
            }

            if (constantNode.Type != typeof(int))
            {
                return null;
            }
            var value = ToInt32(constantNode);

            IFieldExpression fieldExpression = null;

            var unaryExpression = variableNode as UnaryExpression;
            if (unaryExpression != null && unaryExpression.NodeType == ExpressionType.ArrayLength)
            {
                TryGetFieldExpression(unaryExpression.Operand, out fieldExpression);
            }

            var memberExpression = variableNode as MemberExpression;
            if (memberExpression != null && memberExpression.Member.Name == "Count")
            {
                TryGetFieldExpression(memberExpression.Expression, out fieldExpression);
            }

            var pipelineExpression = variableNode as PipelineExpression;
            if (pipelineExpression != null && pipelineExpression.ResultOperator != null && pipelineExpression.ResultOperator is CountResultOperator)
            {
                TryGetFieldExpression(pipelineExpression.Source, out fieldExpression);
            }

            if (fieldExpression != null)
            {
                switch (operatorType)
                {
                    case ExpressionType.Equal:
                        return __builder.Size(fieldExpression.FieldName, value);
                    case ExpressionType.NotEqual:
                        return __builder.Not(__builder.Size(fieldExpression.FieldName, value));
                    case ExpressionType.GreaterThan:
                        return __builder.SizeGt(fieldExpression.FieldName, value);
                    case ExpressionType.GreaterThanOrEqual:
                        return __builder.SizeGte(fieldExpression.FieldName, value);
                    case ExpressionType.LessThan:
                        return __builder.SizeLt(fieldExpression.FieldName, value);
                    case ExpressionType.LessThanOrEqual:
                        return __builder.SizeLte(fieldExpression.FieldName, value);
                }
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateBitwiseComparison(Expression variableExpression, ExpressionType operatorType, ConstantExpression constantExpression)
        {
            var binaryExpression = variableExpression as BinaryExpression;
            if (binaryExpression == null ||
                binaryExpression.NodeType != ExpressionType.And ||
                binaryExpression.Right.NodeType != ExpressionType.Constant ||
                (operatorType != ExpressionType.Equal && operatorType != ExpressionType.NotEqual))
            {
                return null;
            }

            var field = GetFieldExpression(binaryExpression.Left);

            var maskExpression = (ConstantExpression)binaryExpression.Right;
            var value = field.SerializeValue(maskExpression.Type, maskExpression.Value).ToInt64();
            var comparison = Convert.ToInt64(constantExpression.Value);

            if (value == comparison)
            {
                if (operatorType == ExpressionType.Equal)
                {
                    return __builder.BitsAllSet(field.FieldName, value);
                }
                else
                {
                    return __builder.BitsAnyClear(field.FieldName, value);
                }
            }
            else if (comparison == 0)
            {
                if (operatorType == ExpressionType.Equal)
                {
                    return __builder.BitsAllClear(field.FieldName, value);
                }
                else
                {
                    return __builder.BitsAnySet(field.FieldName, value);
                }
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateBoolean(bool value)
        {
            if (value)
            {
                return new BsonDocument(); // empty query matches all documents
            }
            else
            {
                return __builder.Type("_id", (BsonType)(-1)); // matches no documents (and uses _id index when used at top level)
            }
        }

        private FilterDefinition<BsonDocument> TranslateBoolean(Expression expression)
        {
            if (expression.Type == typeof(bool))
            {
                var constantExpression = expression as ConstantExpression;
                if (constantExpression != null)
                {
                    return TranslateBoolean((bool)constantExpression.Value);
                }

                var fieldExpression = GetFieldExpression(expression);
                return new BsonDocument(fieldExpression.FieldName, true);
            }
            return null;
        }

        private FilterDefinition<BsonDocument> TranslateComparison(BinaryExpression binaryExpression)
        {
            // the constant could be on either side
            var variableExpression = binaryExpression.Left;
            var constantExpression = binaryExpression.Right as ConstantExpression;
            var operatorType = binaryExpression.NodeType;

            if (constantExpression == null)
            {
                return null;
            }

            var query = TranslateArrayLength(variableExpression, operatorType, constantExpression);
            if (query != null)
            {
                return query;
            }

            query = TranslateMod(variableExpression, operatorType, constantExpression);
            if (query != null)
            {
                return query;
            }

            query = TranslateCompareTo(variableExpression, operatorType, constantExpression);
            if (query != null)
            {
                return query;
            }

            query = TranslateStringIndexOfQuery(variableExpression, operatorType, constantExpression);
            if (query != null)
            {
                return query;
            }

            query = TranslateStringIndexQuery(variableExpression, operatorType, constantExpression);
            if (query != null)
            {
                return query;
            }

            query = TranslateStringLengthQuery(variableExpression, operatorType, constantExpression);
            if (query != null)
            {
                return query;
            }

            query = TranslateStringCaseInsensitiveComparisonQuery(variableExpression, operatorType, constantExpression);
            if (query != null)
            {
                return query;
            }

            query = TranslateTypeComparisonQuery(variableExpression, operatorType, constantExpression);
            if (query != null)
            {
                return query;
            }

            query = TranslateBitwiseComparison(variableExpression, operatorType, constantExpression);
            if (query != null)
            {
                return query;
            }

            return TranslateComparison(variableExpression, operatorType, constantExpression);
        }

        private FilterDefinition<BsonDocument> TranslateCompareTo(Expression variableExpression, ExpressionType operatorType, ConstantExpression constantExpression)
        {
            if (constantExpression.Type != typeof(int) || ((int)constantExpression.Value) != 0)
            {
                return null;
            }

            var call = variableExpression as MethodCallExpression;
            if (call == null || call.Object == null || call.Method.Name != "CompareTo" || call.Arguments.Count != 1)
            {
                return null;
            }

            constantExpression = call.Arguments[0] as ConstantExpression;
            if (constantExpression == null)
            {
                return null;
            }

            return TranslateComparison(call.Object, operatorType, constantExpression);
        }

        private FilterDefinition<BsonDocument> TranslateComparison(Expression variableExpression, ExpressionType operatorType, ConstantExpression constantExpression)
        {
            var value = constantExpression.Value;

            var methodCallExpression = variableExpression as MethodCallExpression;
            if (methodCallExpression != null && value is bool)
            {
                var boolValue = (bool)value;
                var query = this.TranslateMethodCall(methodCallExpression);

                var isTrueComparison = (boolValue && operatorType == ExpressionType.Equal)
                                        || (!boolValue && operatorType == ExpressionType.NotEqual);

                return isTrueComparison ? query : __builder.Not(query);
            }

            var fieldExpression = GetFieldExpression(variableExpression);

            var valueSerializer = FieldValueSerializerHelper.GetSerializerForValueType(fieldExpression.Serializer, _serializerRegistry, constantExpression.Type, value);
            var serializedValue = valueSerializer.ToBsonValue(value);

            switch (operatorType)
            {
                case ExpressionType.Equal: return __builder.Eq(fieldExpression.FieldName, serializedValue);
                case ExpressionType.GreaterThan: return __builder.Gt(fieldExpression.FieldName, serializedValue);
                case ExpressionType.GreaterThanOrEqual: return __builder.Gte(fieldExpression.FieldName, serializedValue);
                case ExpressionType.LessThan: return __builder.Lt(fieldExpression.FieldName, serializedValue);
                case ExpressionType.LessThanOrEqual: return __builder.Lte(fieldExpression.FieldName, serializedValue);
                case ExpressionType.NotEqual: return __builder.Ne(fieldExpression.FieldName, serializedValue);
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateConstant(ConstantExpression constantExpression)
        {
            var value = constantExpression.Value;
            if (value != null && value.GetType() == typeof(bool))
            {
                return TranslateBoolean((bool)value);
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateContainsKey(MethodCallExpression methodCallExpression)
        {
            var dictionaryType = methodCallExpression.Object.Type;
            var implementedInterfaces = new List<Type>(dictionaryType.GetTypeInfo().GetInterfaces());
            if (dictionaryType.GetTypeInfo().IsInterface)
            {
                implementedInterfaces.Add(dictionaryType);
            }

            Type dictionaryGenericInterface = null;
            Type dictionaryInterface = null;
            foreach (var implementedInterface in implementedInterfaces)
            {
                if (implementedInterface.GetTypeInfo().IsGenericType)
                {
                    if (implementedInterface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    {
                        dictionaryGenericInterface = implementedInterface;
                    }
                }
                else if (implementedInterface == typeof(IDictionary))
                {
                    dictionaryInterface = implementedInterface;
                }
            }

            if (dictionaryGenericInterface == null && dictionaryInterface == null)
            {
                return null;
            }

            var arguments = methodCallExpression.Arguments.ToArray();
            if (arguments.Length != 1)
            {
                return null;
            }

            var constantExpression = arguments[0] as ConstantExpression;
            if (constantExpression == null)
            {
                return null;
            }
            var key = constantExpression.Value;

            var fieldExpression = GetFieldExpression(methodCallExpression.Object);
            var serializer = fieldExpression.Serializer;

            var dictionarySerializer = serializer as IBsonDictionarySerializer;
            if (dictionarySerializer == null)
            {
                var message = string.Format(
                    "{0} in a LINQ query is only supported for members that are serialized using a serializer that implements IBsonDictionarySerializer.",
                    methodCallExpression.Method.Name); // could be Contains (for IDictionary) or ContainsKey (for IDictionary<TKey, TValue>)
                throw new NotSupportedException(message);
            }

            var keySerializer = dictionarySerializer.KeySerializer;
            var keySerializationInfo = new BsonSerializationInfo(
                null, // elementName
                keySerializer,
                keySerializer.ValueType);
            var serializedKey = keySerializationInfo.SerializeValue(key);

            var dictionaryRepresentation = dictionarySerializer.DictionaryRepresentation;
            switch (dictionaryRepresentation)
            {
                case DictionaryRepresentation.ArrayOfDocuments:
                    return __builder.Eq(fieldExpression.FieldName + ".k", serializedKey);
                case DictionaryRepresentation.Document:
                    return __builder.Exists(fieldExpression.FieldName + "." + serializedKey.AsString);
                default:
                    var message = string.Format(
                        "{0} in a LINQ query is only supported for DictionaryRepresentation ArrayOfDocuments or Document, not {1}.",
                        methodCallExpression.Method.Name, // could be Contains (for IDictionary) or ContainsKey (for IDictionary<TKey, TValue>)
                        dictionaryRepresentation);
                    throw new NotSupportedException(message);
            }
        }

        private FilterDefinition<BsonDocument> TranslateContains(MethodCallExpression methodCallExpression)
        {
            // handle IDictionary Contains the same way as IDictionary<TKey, TValue> ContainsKey
            if (methodCallExpression.Object != null && typeof(IDictionary).GetTypeInfo().IsAssignableFrom(methodCallExpression.Object.Type))
            {
                return TranslateContainsKey(methodCallExpression);
            }

            if (methodCallExpression.Method.DeclaringType == typeof(string))
            {
                return TranslateStringQuery(methodCallExpression);
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateEquals(MethodCallExpression methodCallExpression)
        {
            var arguments = methodCallExpression.Arguments.ToArray();

            // assume that static and instance Equals mean the same thing for all classes (i.e. an equality test)
            Expression firstExpression = null;
            Expression secondExpression = null;
            if (methodCallExpression.Object == null)
            {
                // static Equals method
                if (arguments.Length == 2)
                {
                    firstExpression = arguments[0];
                    secondExpression = arguments[1];
                }
            }
            else
            {
                // instance Equals method
                if (arguments.Length == 1)
                {
                    firstExpression = methodCallExpression.Object;
                    secondExpression = arguments[0];
                }
            }

            if (firstExpression != null && secondExpression != null)
            {
                // the constant could be either expression
                var variableExpression = firstExpression;
                var constantExpression = secondExpression as ConstantExpression;
                if (constantExpression == null)
                {
                    constantExpression = firstExpression as ConstantExpression;
                    variableExpression = secondExpression;
                }

                if (constantExpression == null)
                {
                    return null;
                }

                if (variableExpression.Type == typeof(Type) && constantExpression.Type == typeof(Type))
                {
                    return TranslateTypeComparisonQuery(variableExpression, ExpressionType.Equal, constantExpression);
                }

                return TranslateComparison(variableExpression, ExpressionType.Equal, constantExpression);
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateHasFlag(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Object == null)
            {
                return null;
            }

            var field = GetFieldExpression(methodCallExpression.Object);
            var flagExpression = (ConstantExpression)methodCallExpression.Arguments[0];
            var value = field.SerializeValue(flagExpression.Type, flagExpression.Value).ToInt64();

            return __builder.BitsAllSet(field.FieldName, value);
        }

        private FilterDefinition<BsonDocument> TranslateIn(MethodCallExpression methodCallExpression)
        {
            var methodDeclaringType = methodCallExpression.Method.DeclaringType;
            var methodDeclaringTypeInfo = methodDeclaringType.GetTypeInfo();
            var arguments = methodCallExpression.Arguments.ToArray();
            IFieldExpression fieldExpression = null;
            ConstantExpression valuesExpression = null;
            if (methodDeclaringType == typeof(Enumerable) || methodDeclaringType == typeof(Queryable))
            {
                if (arguments.Length == 2)
                {
                    fieldExpression = GetFieldExpression(arguments[1]);
                    valuesExpression = arguments[0] as ConstantExpression;
                }
            }
            else
            {
                if (methodDeclaringTypeInfo.IsGenericType)
                {
                    methodDeclaringType = methodDeclaringType.GetGenericTypeDefinition();
                    methodDeclaringTypeInfo = methodDeclaringType.GetTypeInfo();
                }

                bool contains = methodDeclaringType == typeof(ICollection<>) || methodDeclaringTypeInfo.GetInterface("ICollection`1") != null;
                if (contains && arguments.Length == 1)
                {
                    fieldExpression = GetFieldExpression(arguments[0]);
                    valuesExpression = methodCallExpression.Object as ConstantExpression;
                }
            }

            if (fieldExpression != null && valuesExpression != null)
            {
                var ienumerableInterfaceType = valuesExpression.Type.FindIEnumerable();
                var itemType = ienumerableInterfaceType.GetTypeInfo().GetGenericArguments()[0];
                var serializedValues = fieldExpression.SerializeValues(itemType, (IEnumerable)valuesExpression.Value);
                return __builder.In(fieldExpression.FieldName, serializedValues);
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateInjectedFilter(InjectedFilterExpression node)
        {
            return new BsonDocumentFilterDefinition<BsonDocument>(node.Filter);
        }

        private FilterDefinition<BsonDocument> TranslateIsMatch(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(Regex))
            {
                var arguments = methodCallExpression.Arguments.ToArray();
                var obj = methodCallExpression.Object;
                if (obj == null)
                {
                    if (arguments.Length == 2 || arguments.Length == 3)
                    {
                        var fieldExpression = GetFieldExpression(arguments[0]);
                        var patternExpression = arguments[1] as ConstantExpression;
                        if (patternExpression != null)
                        {
                            var pattern = patternExpression.Value as string;
                            if (pattern != null)
                            {
                                var options = RegexOptions.None;
                                if (arguments.Length == 3)
                                {
                                    var optionsExpression = arguments[2] as ConstantExpression;
                                    if (optionsExpression == null || optionsExpression.Type != typeof(RegexOptions))
                                    {
                                        return null;
                                    }
                                    options = (RegexOptions)optionsExpression.Value;
                                }
                                var regex = new Regex(pattern, options);
                                return __builder.Regex(fieldExpression.FieldName, regex);
                            }
                        }
                    }
                }
                else
                {
                    var regexExpression = obj as ConstantExpression;
                    if (regexExpression != null && arguments.Length == 1)
                    {
                        var serializationInfo = GetFieldExpression(arguments[0]);
                        var regex = regexExpression.Value as Regex;
                        if (regex != null)
                        {
                            return __builder.Regex(serializationInfo.FieldName, regex);
                        }
                    }
                }
            }
            return null;
        }

        private FilterDefinition<BsonDocument> TranslateIsNullOrEmpty(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(string) && methodCallExpression.Object == null)
            {
                var arguments = methodCallExpression.Arguments.ToArray();
                var fieldExpression = GetFieldExpression(arguments[0]);
                return __builder.In<string>(fieldExpression.FieldName, new string[] { null, "" });
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateMethodCall(MethodCallExpression methodCallExpression)
        {
            switch (methodCallExpression.Method.Name)
            {
                case "Contains": return TranslateContains(methodCallExpression);
                case "ContainsKey": return TranslateContainsKey(methodCallExpression);
                case "EndsWith": return TranslateStringQuery(methodCallExpression);
                case "Equals": return TranslateEquals(methodCallExpression);
                case "HasFlag": return TranslateHasFlag(methodCallExpression);
                case "In": return TranslateIn(methodCallExpression);
                case "IsMatch": return TranslateIsMatch(methodCallExpression);
                case "IsNullOrEmpty": return TranslateIsNullOrEmpty(methodCallExpression);
                case "StartsWith": return TranslateStringQuery(methodCallExpression);
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateMod(Expression variableExpression, ExpressionType operatorType, ConstantExpression constantExpression)
        {
            if (operatorType != ExpressionType.Equal && operatorType != ExpressionType.NotEqual)
            {
                return null;
            }

            if (constantExpression.Type != typeof(int) && constantExpression.Type != typeof(long))
            {
                return null;
            }
            var value = ToInt64(constantExpression);

            var modBinaryExpression = variableExpression as BinaryExpression;
            if (modBinaryExpression != null && modBinaryExpression.NodeType == ExpressionType.Modulo)
            {
                var fieldExpression = GetFieldExpression(modBinaryExpression.Left);
                var modulusExpression = modBinaryExpression.Right as ConstantExpression;
                if (modulusExpression != null)
                {
                    var modulus = ToInt64(modulusExpression);
                    if (operatorType == ExpressionType.Equal)
                    {
                        return __builder.Mod(fieldExpression.FieldName, modulus, value);
                    }
                    else
                    {
                        return __builder.Not(__builder.Mod(fieldExpression.FieldName, modulus, value));
                    }
                }
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateNot(UnaryExpression unaryExpression)
        {
            var filter = Translate(unaryExpression.Operand);
            return __builder.Not(filter);
        }

        private FilterDefinition<BsonDocument> TranslateOrElse(BinaryExpression binaryExpression)
        {
            return __builder.Or(Translate(binaryExpression.Left), Translate(binaryExpression.Right));
        }

        private FilterDefinition<BsonDocument> TranslateOr(BinaryExpression binaryExpression)
        {
            if (binaryExpression.Left.Type == typeof(bool) && binaryExpression.Right.Type == typeof(bool))
            {
                return TranslateOrElse(binaryExpression);
            }

            return null;
        }


        private FilterDefinition<BsonDocument> TranslatePipeline(PipelineExpression node)
        {
            if (node.ResultOperator is AllResultOperator)
            {
                return TranslatePipelineAll(node);
            }
            if (node.ResultOperator is AnyResultOperator)
            {
                return TranslatePipelineAny(node);
            }
            if (node.ResultOperator is ContainsResultOperator)
            {
                return TranslatePipelineContains(node);
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslatePipelineAll(PipelineExpression node)
        {
            var whereExpression = node.Source as WhereExpression;
            if (whereExpression == null)
            {
                return null;
            }

            var constant = whereExpression.Source as ConstantExpression;
            if (constant == null)
            {
                return null;
            }

            var embeddedPipeline = whereExpression.Predicate as PipelineExpression;
            if (!(embeddedPipeline?.ResultOperator is ContainsResultOperator))
            {
                return null;
            }

            var fieldExpression = embeddedPipeline.Source as IFieldExpression;
            if (fieldExpression == null)
            {
                return null;
            }

            var arraySerializer = fieldExpression.Serializer as IBsonArraySerializer;
            if (arraySerializer == null)
            {
                return null;
            }

            BsonSerializationInfo itemSerializationInfo;
            if (!arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
            {
                return null;
            }

            var serializedValues = itemSerializationInfo.SerializeValues((IEnumerable)constant.Value);
            return __builder.All(fieldExpression.FieldName, serializedValues);
        }

        private FilterDefinition<BsonDocument> TranslatePipelineAny(PipelineExpression node)
        {
            var fieldExpression = node.Source as IFieldExpression;
            if (fieldExpression != null)
            {
                return __builder.And(
                        __builder.Ne(fieldExpression.FieldName, BsonNull.Value),
                        __builder.Not(__builder.Size(fieldExpression.FieldName, 0)));
            }

            var whereExpression = node.Source as WhereExpression;
            if (whereExpression == null)
            {
                return null;
            }

            fieldExpression = whereExpression.Source as IFieldExpression;
            if (fieldExpression == null)
            {
                if (whereExpression.Source is ConstantExpression)
                {
                    return TranslatePipelineAnyScalar(node);
                }
                return null;
            }

            FilterDefinition<BsonDocument> filter;
            var renderWithoutElemMatch = CanAnyBeRenderedWithoutElemMatch(whereExpression.Predicate);

            if (renderWithoutElemMatch)
            {
                var predicate = FieldNamePrefixer.Prefix(whereExpression.Predicate, fieldExpression.FieldName);
                filter = Translate(predicate);
            }
            else
            {
                var predicate = DocumentToFieldConverter.Convert(whereExpression.Predicate);
                filter = __builder.ElemMatch(fieldExpression.FieldName, Translate(predicate));
                if (!(fieldExpression.Serializer is IBsonDocumentSerializer))
                {
                    filter = new ScalarElementMatchFilterDefinition<BsonDocument>(filter);
                }
            }

            return filter;
        }

        private FilterDefinition<BsonDocument> TranslatePipelineAnyScalar(PipelineExpression node)
        {
            var whereExpression = node.Source as WhereExpression;
            if (whereExpression == null)
            {
                return null;
            }

            var constant = whereExpression.Source as ConstantExpression;
            if (constant == null)
            {
                return null;
            }

            var embeddedPipeline = whereExpression.Predicate as PipelineExpression;
            if (!(embeddedPipeline?.ResultOperator is ContainsResultOperator))
            {
                return null;
            }

            var fieldExpression = embeddedPipeline.Source as IFieldExpression;
            if (fieldExpression == null)
            {
                return null;
            }

            var arraySerializer = fieldExpression.Serializer as IBsonArraySerializer;
            if (arraySerializer == null)
            {
                return null;
            }

            BsonSerializationInfo itemSerializationInfo;
            if (!arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
            {
                return null;
            }

            var serializedValues = itemSerializationInfo.SerializeValues((IEnumerable)constant.Value);
            return __builder.In(fieldExpression.FieldName, serializedValues);
        }

        private FilterDefinition<BsonDocument> TranslatePipelineContains(PipelineExpression node)
        {
            var value = ((ContainsResultOperator)node.ResultOperator).Value;
            var constantExpression = node.Source as ConstantExpression;
            IFieldExpression field;
            if (constantExpression != null)
            {
                field = value as IFieldExpression;
                if (field != null)
                {
                    var ienumerableInterfaceType = constantExpression.Type.FindIEnumerable();
                    var itemType = ienumerableInterfaceType.GetTypeInfo().GetGenericArguments()[0];
                    var serializedValues = field.SerializeValues(itemType, (IEnumerable)constantExpression.Value);
                    return __builder.In(field.FieldName, serializedValues);
                }
            }
            else
            {
                constantExpression = value as ConstantExpression;
                field = node.Source as IFieldExpression;
                if (constantExpression != null && field != null)
                {
                    var arraySerializer = field.Serializer as IBsonArraySerializer;
                    BsonSerializationInfo itemSerializationInfo;
                    if (arraySerializer != null && arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                    {
                        var serializedValue = itemSerializationInfo.SerializeValue(constantExpression.Value);
                        return __builder.Eq(field.FieldName, serializedValue);
                    }
                }
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateStringIndexOfQuery(Expression variableExpression, ExpressionType operatorType, ConstantExpression constantExpression)
        {
            // TODO: support other comparison operators
            if (operatorType != ExpressionType.Equal)
            {
                return null;
            }

            if (constantExpression.Type != typeof(int))
            {
                return null;
            }
            var index = ToInt32(constantExpression);

            var methodCallExpression = variableExpression as MethodCallExpression;
            if (methodCallExpression != null &&
                (methodCallExpression.Method.Name == "IndexOf" || methodCallExpression.Method.Name == "IndexOfAny") &&
                methodCallExpression.Method.DeclaringType == typeof(string))
            {
                var fieldExpression = GetFieldExpression(methodCallExpression.Object);

                object value;
                var startIndex = -1;
                var count = -1;

                var args = methodCallExpression.Arguments.ToArray();
                switch (args.Length)
                {
                    case 3:
                        var countExpression = args[2] as ConstantExpression;
                        if (countExpression == null)
                        {
                            return null;
                        }
                        count = ToInt32(countExpression);
                        goto case 2;
                    case 2:
                        var startIndexExpression = args[1] as ConstantExpression;
                        if (startIndexExpression == null)
                        {
                            return null;
                        }
                        startIndex = ToInt32(startIndexExpression);
                        goto case 1;
                    case 1:
                        var valueExpression = args[0] as ConstantExpression;
                        if (valueExpression == null)
                        {
                            return null;
                        }
                        value = valueExpression.Value;
                        break;
                    default:
                        return null;
                }

                string pattern = null;
                if (value.GetType() == typeof(char) || value.GetType() == typeof(char[]))
                {
                    char[] chars;
                    if (value.GetType() == typeof(char))
                    {
                        chars = new char[] { (char)value };
                    }
                    else
                    {
                        chars = (char[])value;
                    }
                    var positiveClass = string.Join("", chars.Select(c => (c == '-') ? "\\-" : (c == ']') ? "\\]" : Regex.Escape(c.ToString())).ToArray());
                    var negativeClass = "[^" + positiveClass + "]";
                    if (chars.Length > 1)
                    {
                        positiveClass = "[" + positiveClass + "]";
                    }

                    if (startIndex == -1)
                    {
                        // the regex for: IndexOf(c) == index 
                        // is: /^[^c]{index}c/
                        pattern = string.Format("^{0}{{{1}}}{2}", negativeClass, index, positiveClass);
                    }
                    else
                    {
                        if (count == -1)
                        {
                            // the regex for: IndexOf(c, startIndex) == index
                            // is: /^.{startIndex}[^c]{index - startIndex}c/
                            pattern = string.Format("^.{{{0}}}{1}{{{2}}}{3}", startIndex, negativeClass, index - startIndex, positiveClass);
                        }
                        else
                        {
                            if (index >= startIndex + count)
                            {
                                // index is outside of the substring so no match is possible
                                return TranslateBoolean(false);
                            }
                            else
                            {
                                // the regex for: IndexOf(c, startIndex, count) == index
                                // is: /^.{startIndex}(?=.{count})[^c]{index - startIndex}c/
                                pattern = string.Format("^.{{{0}}}(?=.{{{1}}}){2}{{{3}}}{4}", startIndex, count, negativeClass, index - startIndex, positiveClass);
                            }
                        }
                    }
                }
                else if (value.GetType() == typeof(string))
                {
                    var escapedString = Regex.Escape((string)value);
                    if (startIndex == -1)
                    {
                        // the regex for: IndexOf(s) == index 
                        // is: /^(?!.{0,index - 1}s).{index}s/
                        pattern = string.Format("^(?!.{{0,{2}}}{0}).{{{1}}}{0}", escapedString, index, index - 1);
                    }
                    else
                    {
                        if (count == -1)
                        {
                            // the regex for: IndexOf(s, startIndex) == index
                            // is: /^.{startIndex}(?!.{0, index - startIndex - 1}s).{index - startIndex}s/
                            pattern = string.Format("^.{{{1}}}(?!.{{0,{2}}}{0}).{{{3}}}{0}", escapedString, startIndex, index - startIndex - 1, index - startIndex);
                        }
                        else
                        {
                            var unescapedLength = ((string)value).Length;
                            if (unescapedLength > startIndex + count - index)
                            {
                                // substring isn't long enough to match
                                return TranslateBoolean(false);
                            }
                            else
                            {
                                // the regex for: IndexOf(s, startIndex, count) == index
                                // is: /^.{startIndex}(?=.{count})(?!.{0,index - startIndex - 1}s).{index - startIndex)s/
                                pattern = string.Format("^.{{{1}}}(?=.{{{2}}})(?!.{{0,{3}}}{0}).{{{4}}}{0}", escapedString, startIndex, count, index - startIndex - 1, index - startIndex);
                            }
                        }
                    }
                }

                if (pattern != null)
                {
                    return __builder.Regex(fieldExpression.FieldName, new BsonRegularExpression(pattern, "s"));
                }
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateStringIndexQuery(Expression variableExpression, ExpressionType operatorType, ConstantExpression constantExpression)
        {
            var unaryExpression = variableExpression as UnaryExpression;
            if (unaryExpression == null)
            {
                return null;
            }

            if (unaryExpression.NodeType != ExpressionType.Convert || unaryExpression.Type != typeof(int))
            {
                return null;
            }

            var methodCallExpression = unaryExpression.Operand as MethodCallExpression;
            if (methodCallExpression == null)
            {
                return null;
            }

            var method = methodCallExpression.Method;
            if (method.DeclaringType != typeof(string) || method.Name != "get_Chars")
            {
                return null;
            }

            var stringExpression = methodCallExpression.Object;
            if (stringExpression == null)
            {
                return null;
            }

            var args = methodCallExpression.Arguments.ToArray();
            if (args.Length != 1)
            {
                return null;
            }

            var indexExpression = args[0] as ConstantExpression;
            if (indexExpression == null)
            {
                return null;
            }

            if (constantExpression.Type != typeof(int))
            {
                return null;
            }

            var index = ToInt32(indexExpression);
            var value = ToInt32(constantExpression);

            var c = new string((char)value, 1);
            var positiveClass = (c == "-") ? "\\-" : (c == "]") ? "\\]" : Regex.Escape(c);
            var negativeClass = "[^" + positiveClass + "]";

            string characterClass;
            switch (operatorType)
            {
                case ExpressionType.Equal:
                    characterClass = positiveClass;
                    break;
                case ExpressionType.NotEqual:
                    characterClass = negativeClass;
                    break;
                default:
                    return null; // TODO: suport other comparison operators?
            }
            var pattern = string.Format("^.{{{0}}}{1}", index, characterClass);

            var fieldExpression = GetFieldExpression(stringExpression);
            return __builder.Regex(fieldExpression.FieldName, new BsonRegularExpression(pattern, "s"));
        }

        private FilterDefinition<BsonDocument> TranslateStringLengthQuery(Expression variableExpression, ExpressionType operatorType, ConstantExpression constantExpression)
        {
            if (constantExpression.Type != typeof(int))
            {
                return null;
            }
            var value = ToInt32(constantExpression);

            IFieldExpression fieldExpression = null;

            var memberExpression = variableExpression as MemberExpression;
            if (memberExpression != null && memberExpression.Member.Name == "Length")
            {
                TryGetFieldExpression(memberExpression.Expression, out fieldExpression);
            }

            var methodCallExpression = variableExpression as MethodCallExpression;
            if (methodCallExpression != null && methodCallExpression.Method.Name == "Count" && methodCallExpression.Method.DeclaringType == typeof(Enumerable))
            {
                var args = methodCallExpression.Arguments.ToArray();
                if (args.Length == 1 && args[0].Type == typeof(string))
                {
                    TryGetFieldExpression(args[0], out fieldExpression);
                }
            }

            if (fieldExpression != null)
            {
                string regex = null;
                switch (operatorType)
                {
                    case ExpressionType.NotEqual:
                    case ExpressionType.Equal: regex = @"/^.{" + value.ToString() + "}$/s"; break;
                    case ExpressionType.GreaterThan: regex = @"/^.{" + (value + 1).ToString() + ",}$/s"; break;
                    case ExpressionType.GreaterThanOrEqual: regex = @"/^.{" + value.ToString() + ",}$/s"; break;
                    case ExpressionType.LessThan: regex = @"/^.{0," + (value - 1).ToString() + "}$/s"; break;
                    case ExpressionType.LessThanOrEqual: regex = @"/^.{0," + value.ToString() + "}$/s"; break;
                }
                if (regex != null)
                {
                    if (operatorType == ExpressionType.NotEqual)
                    {
                        return __builder.Not(__builder.Regex(fieldExpression.FieldName, regex));
                    }
                    else
                    {
                        return __builder.Regex(fieldExpression.FieldName, regex);
                    }
                }
            }

            return null;
        }

        private FilterDefinition<BsonDocument> TranslateStringCaseInsensitiveComparisonQuery(Expression variableExpression, ExpressionType operatorType, ConstantExpression constantExpression)
        {
            var methodExpression = variableExpression as MethodCallExpression;
            if (methodExpression == null)
            {
                return null;
            }

            var methodName = methodExpression.Method.Name;
            if ((methodName != "ToLower" && methodName != "ToUpper" && methodName != "ToLowerInvariant" && methodName != "ToUpperInvariant") ||
                methodExpression.Object == null ||
                methodExpression.Type != typeof(string) ||
                methodExpression.Arguments.Count != 0)
            {
                return null;
            }

            if (operatorType != ExpressionType.Equal && operatorType != ExpressionType.NotEqual)
            {
                return null;
            }

            var fieldExpression = GetFieldExpression(methodExpression.Object);
            var serializedValue = fieldExpression.SerializeValue(constantExpression.Type, constantExpression.Value);

            if (serializedValue.IsString)
            {
                var stringValue = serializedValue.AsString;
                var stringValueCaseMatches =
                    methodName == "ToLower" && stringValue == stringValue.ToLowerInvariant() ||
                    methodName == "ToLowerInvariant" && stringValue == stringValue.ToLowerInvariant() ||
                    methodName == "ToUpper" && stringValue == stringValue.ToUpperInvariant() ||
                    methodName == "ToUpperInvariant" && stringValue == stringValue.ToUpperInvariant();

                if (stringValueCaseMatches)
                {
                    string pattern = "/^" + Regex.Escape(stringValue) + "$/i";
                    var regex = new BsonRegularExpression(pattern);

                    if (operatorType == ExpressionType.Equal)
                    {
                        return __builder.Regex(fieldExpression.FieldName, regex);
                    }
                    else
                    {
                        return __builder.Not(__builder.Regex(fieldExpression.FieldName, regex));
                    }
                }
                else
                {
                    if (operatorType == ExpressionType.Equal)
                    {
                        // == "mismatched case" matches no documents
                        return TranslateBoolean(false);
                    }
                    else
                    {
                        // != "mismatched case" matches all documents
                        return TranslateBoolean(true);
                    }
                }
            }
            else if (serializedValue.IsBsonNull)
            {
                if (operatorType == ExpressionType.Equal)
                {
                    return __builder.Eq(fieldExpression.FieldName, BsonNull.Value);
                }
                else
                {
                    return __builder.Ne(fieldExpression.FieldName, BsonNull.Value);
                }
            }
            else
            {
                var message = string.Format("When using {0} in a LINQ string comparison the value being compared to must serialize as a string.", methodName);
                throw new ArgumentException(message);
            }
        }

        private FilterDefinition<BsonDocument> TranslateStringQuery(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType != typeof(string))
            {
                return null;
            }

            var arguments = methodCallExpression.Arguments.ToArray();
            if (arguments.Length != 1)
            {
                return null;
            }

            var stringExpression = methodCallExpression.Object;
            var constantExpression = arguments[0] as ConstantExpression;
            if (constantExpression == null)
            {
                return null;
            }

            var pattern = Regex.Escape((string)constantExpression.Value);
            switch (methodCallExpression.Method.Name)
            {
                case "Contains": pattern = ".*" + pattern + ".*"; break;
                case "EndsWith": pattern = ".*" + pattern; break;
                case "StartsWith": pattern = pattern + ".*"; break; // query optimizer will use index for rooted regular expressions
                default: return null;
            }

            var caseInsensitive = false;
            MethodCallExpression stringMethodCallExpression;
            while ((stringMethodCallExpression = stringExpression as MethodCallExpression) != null)
            {
                var trimStart = false;
                var trimEnd = false;
                Expression trimCharsExpression = null;
                switch (stringMethodCallExpression.Method.Name)
                {
                    case "ToLower":
                    case "ToLowerInvariant":
                    case "ToUpper":
                    case "ToUpperInvariant":
                        caseInsensitive = true;
                        break;
                    case "Trim":
                        trimStart = true;
                        trimEnd = true;
                        trimCharsExpression = stringMethodCallExpression.Arguments.FirstOrDefault();
                        break;
                    case "TrimEnd":
                        trimEnd = true;
                        trimCharsExpression = stringMethodCallExpression.Arguments.First();
                        break;
                    case "TrimStart":
                        trimStart = true;
                        trimCharsExpression = stringMethodCallExpression.Arguments.First();
                        break;
                    default:
                        return null;
                }

                if (trimStart || trimEnd)
                {
                    var trimCharsPattern = GetTrimCharsPattern(trimCharsExpression);
                    if (trimCharsPattern == null)
                    {
                        return null;
                    }

                    if (trimStart)
                    {
                        pattern = trimCharsPattern + pattern;
                    }
                    if (trimEnd)
                    {
                        pattern = pattern + trimCharsPattern;
                    }
                }

                stringExpression = stringMethodCallExpression.Object;
            }

            pattern = "^" + pattern + "$";
            if (pattern.StartsWith("^.*"))
            {
                pattern = pattern.Substring(3);
            }
            if (pattern.EndsWith(".*$"))
            {
                pattern = pattern.Substring(0, pattern.Length - 3);
            }

            var fieldExpression = GetFieldExpression(stringExpression);
            var options = caseInsensitive ? "is" : "s";
            return __builder.Regex(fieldExpression.FieldName, new BsonRegularExpression(pattern, options));
        }

        private FilterDefinition<BsonDocument> TranslateTypeComparisonQuery(Expression variableExpression, ExpressionType operatorType, ConstantExpression constantExpression)
        {
            if (operatorType != ExpressionType.Equal)
            {
                // TODO: support NotEqual?
                return null;
            }

            if (constantExpression.Type != typeof(Type))
            {
                return null;
            }
            var actualType = (Type)constantExpression.Value;

            var methodCallExpression = variableExpression as MethodCallExpression;
            if (methodCallExpression == null)
            {
                return null;
            }
            if (methodCallExpression.Method.Name != "GetType" || methodCallExpression.Object == null)
            {
                return null;
            }
            if (methodCallExpression.Arguments.Count != 0)
            {
                return null;
            }

            var fieldExpression = GetFieldExpression(methodCallExpression.Object);
            var nominalType = fieldExpression.Serializer.ValueType;

            var discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(nominalType);
            var discriminator = discriminatorConvention.GetDiscriminator(nominalType, actualType);
            if (discriminator == null)
            {
                return TranslateBoolean(true);
            }

            var elementName = discriminatorConvention.ElementName;
            if (fieldExpression.FieldName != null)
            {
                elementName = string.Format("{0}.{1}", fieldExpression.FieldName, elementName);
            }

            if (discriminator.IsBsonArray)
            {
                var discriminatorArray = discriminator.AsBsonArray;
                var queries = new FilterDefinition<BsonDocument>[discriminatorArray.Count + 1];
                queries[0] = __builder.Size(elementName, discriminatorArray.Count);
                for (var i = 0; i < discriminatorArray.Count; i++)
                {
                    queries[i + 1] = __builder.Eq(string.Format("{0}.{1}", elementName, i), discriminatorArray[i]);
                }
                return __builder.And(queries);
            }
            else
            {
                return __builder.And(
                    __builder.Exists(elementName + ".0", false), // trick to check that element is not an array
                    __builder.Eq(elementName, discriminator));
            }
        }

        private FilterDefinition<BsonDocument> TranslateTypeIsQuery(TypeBinaryExpression typeBinaryExpression)
        {
            var nominalType = typeBinaryExpression.Expression.Type;
            var actualType = typeBinaryExpression.TypeOperand;

            var discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(nominalType);
            var discriminator = discriminatorConvention.GetDiscriminator(nominalType, actualType);
            if (discriminator == null)
            {
                return TranslateBoolean(true);
            }

            if (discriminator.IsBsonArray)
            {
                discriminator = discriminator[discriminator.AsBsonArray.Count - 1];
            }

            var elementName = discriminatorConvention.ElementName;
            IFieldExpression fieldExpression;
            if (TryGetFieldExpression(typeBinaryExpression.Expression, out fieldExpression) && !string.IsNullOrEmpty(fieldExpression.FieldName))
            {
                elementName = string.Format("{0}.{1}", fieldExpression.FieldName, elementName);
            }
            return __builder.Eq(elementName, discriminator);
        }

        private string GetTrimCharsPattern(Expression trimCharsExpression)
        {
            if (trimCharsExpression == null)
            {
                return "\\s*";
            }

            var constantExpression = trimCharsExpression as ConstantExpression;
            if (constantExpression == null || !constantExpression.Type.IsArray || constantExpression.Type.GetElementType() != typeof(char))
            {
                return null;
            }

            var trimChars = (char[])constantExpression.Value;
            if (trimChars.Length == 0)
            {
                return "\\s*";
            }

            // build a pattern that matches the characters to be trimmed
            var characterClass = string.Join("", trimChars.Select(c => (c == '-') ? "\\-" : (c == ']') ? "\\]" : Regex.Escape(c.ToString())).ToArray());
            if (trimChars.Length > 1)
            {
                characterClass = "[" + characterClass + "]";
            }
            return characterClass + "*";
        }

        private int ToInt32(Expression expression)
        {
            if (expression.Type != typeof(int))
            {
                throw new ArgumentOutOfRangeException("expression", "Expected an Expression of Type Int32.");
            }

            var constantExpression = expression as ConstantExpression;
            if (constantExpression == null)
            {
                throw new ArgumentOutOfRangeException("expression", "Expected a ConstantExpression.");
            }

            return (int)constantExpression.Value;
        }

        private long ToInt64(Expression expression)
        {
            if (expression.Type != typeof(int) && expression.Type != typeof(long))
            {
                throw new ArgumentOutOfRangeException("expression", "Expected an Expression of Type Int32 or Int64.");
            }

            var constantExpression = expression as ConstantExpression;
            if (constantExpression == null)
            {
                throw new ArgumentOutOfRangeException("expression", "Expected a ConstantExpression.");
            }

            if (expression.Type == typeof(int))
            {
                return (long)(int)constantExpression.Value;
            }
            else
            {
                return (long)constantExpression.Value;
            }
        }

        private bool TryGetFieldExpression(Expression expression, out IFieldExpression fieldExpression)
        {
            return ExpressionHelper.TryGetExpression(expression, out fieldExpression);
        }

        private IFieldExpression GetFieldExpression(Expression expression)
        {
            IFieldExpression fieldExpression;
            if (!TryGetFieldExpression(expression, out fieldExpression))
            {
                var message = string.Format("{0} is not supported.",
                    expression.ToString());
                throw new InvalidOperationException(message);
            }

            return fieldExpression;
        }

        // nested types
        private class DocumentToFieldConverter : ExtensionExpressionVisitor
        {
            public static Expression Convert(Expression node)
            {
                var visitor = new DocumentToFieldConverter();
                return visitor.Visit(node);
            }

            protected internal override Expression VisitDocument(DocumentExpression node)
            {
                return new FieldExpression("", node.Serializer);
            }

            protected internal override Expression VisitPipeline(PipelineExpression node)
            {
                return node;
            }
        }
    }
}
