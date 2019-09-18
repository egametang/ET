/* Copyright 2015-present MongoDB Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Expressions.ResultOperators;
using MongoDB.Driver.Linq.Processors;
using MongoDB.Driver.Support;

namespace MongoDB.Driver.Linq.Translators
{
    internal sealed class AggregateLanguageTranslator
    {
        public static BsonValue Translate(Expression node, ExpressionTranslationOptions translationOptions)
        {
            var builder = new AggregateLanguageTranslator(translationOptions);
            return builder.TranslateValue(node);
        }

        private readonly AggregateStringTranslationMode _stringTranslationMode;

        private AggregateLanguageTranslator(ExpressionTranslationOptions translationOptions)
        {
            translationOptions = translationOptions ?? ExpressionTranslationOptions.Default;
            _stringTranslationMode = translationOptions.StringTranslationMode.GetValueOrDefault(AggregateStringTranslationMode.Bytes);
        }

        private BsonValue TranslateValue(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return TranslateAdd((BinaryExpression)node);
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return TranslateOperation((BinaryExpression)node, "$and", true);
                case ExpressionType.ArrayLength:
                    return TranslateArrayLength(node);
                case ExpressionType.Call:
                    return TranslateMethodCall((MethodCallExpression)node);
                case ExpressionType.Coalesce:
                    return TranslateOperation((BinaryExpression)node, "$ifNull", false);
                case ExpressionType.Conditional:
                    return TranslateConditional((ConditionalExpression)node);
                case ExpressionType.Constant:
                    return TranslateConstant(node);
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return TranslateValue(((UnaryExpression)node).Operand);
                case ExpressionType.Divide:
                    return TranslateOperation((BinaryExpression)node, "$divide", false);
                case ExpressionType.Equal:
                    return TranslateOperation((BinaryExpression)node, "$eq", false);
                case ExpressionType.GreaterThan:
                    return TranslateOperation((BinaryExpression)node, "$gt", false);
                case ExpressionType.GreaterThanOrEqual:
                    return TranslateOperation((BinaryExpression)node, "$gte", false);
                case ExpressionType.LessThan:
                    return TranslateOperation((BinaryExpression)node, "$lt", false);
                case ExpressionType.LessThanOrEqual:
                    return TranslateOperation((BinaryExpression)node, "$lte", false);
                case ExpressionType.MemberAccess:
                    return TranslateMemberAccess((MemberExpression)node);
                case ExpressionType.MemberInit:
                    return TranslateMemberInit((MemberInitExpression)node);
                case ExpressionType.Modulo:
                    return TranslateOperation((BinaryExpression)node, "$mod", false);
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return TranslateOperation((BinaryExpression)node, "$multiply", true);
                case ExpressionType.New:
                    return TranslateNew((NewExpression)node);
                case ExpressionType.NewArrayInit:
                    return TranslateNewArrayInit((NewArrayExpression)node);
                case ExpressionType.Not:
                    return TranslateNot((UnaryExpression)node);
                case ExpressionType.NotEqual:
                    return TranslateOperation((BinaryExpression)node, "$ne", false);
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return TranslateOperation((BinaryExpression)node, "$or", true);
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return TranslateOperation((BinaryExpression)node, "$subtract", false);
                case ExpressionType.Extension:
                    var extensionExpression = node as ExtensionExpression;
                    if (extensionExpression != null)
                    {
                        switch (extensionExpression.ExtensionType)
                        {
                            case ExtensionExpressionType.Accumulator:
                                return TranslateAccumulator((AccumulatorExpression)node);
                            case ExtensionExpressionType.AggregateExpression:
                                return TranslateAggregateExpression((AggregateExpressionExpression)node);
                            case ExtensionExpressionType.ArrayIndex:
                                return TranslateArrayIndex((ArrayIndexExpression)node);
                            case ExtensionExpressionType.Concat:
                                return TranslateConcat((ConcatExpression)node);
                            case ExtensionExpressionType.Except:
                                return TranslateExcept((ExceptExpression)node);
                            case ExtensionExpressionType.FieldAsDocument:
                                return TranslateDocumentWrappedField((FieldAsDocumentExpression)node);
                            case ExtensionExpressionType.Field:
                                return TranslateField((FieldExpression)node);
                            case ExtensionExpressionType.GroupingKey:
                                return TranslateGroupingKey((GroupingKeyExpression)node);
                            case ExtensionExpressionType.Intersect:
                                return TranslateIntersect((IntersectExpression)node);
                            case ExtensionExpressionType.Pipeline:
                                return TranslatePipeline((PipelineExpression)node);
                            case ExtensionExpressionType.Reverse:
                                return TranslateReverse((ReverseExpression)node);
                            case ExtensionExpressionType.Select:
                                return TranslateSelect((SelectExpression)node);
                            case ExtensionExpressionType.SerializedConstant:
                                return TranslateSerializedConstant((SerializedConstantExpression)node);
                            case ExtensionExpressionType.Skip:
                                return TranslateSkip((SkipExpression)node);
                            case ExtensionExpressionType.Take:
                                return TranslateTake((TakeExpression)node);
                            case ExtensionExpressionType.Union:
                                return TranslateUnion((UnionExpression)node);
                            case ExtensionExpressionType.Where:
                                return TranslateWhere((WhereExpression)node);
                            case ExtensionExpressionType.Zip:
                                return TranslateZip((ZipExpression)node);
                        }
                    }
                    break;
            }

            var message = string.Format("$project or $group does not support {0}.",
                node.ToString());
            throw new NotSupportedException(message);
        }

        private BsonValue TranslateAdd(BinaryExpression node)
        {
            var op = "$add";
            if (node.Left.Type == typeof(string))
            {
                op = "$concat";
            }

            return TranslateOperation(node, op, true);
        }

        private BsonValue TranslateAccumulator(AccumulatorExpression node)
        {
            switch (node.AccumulatorType)
            {
                case AccumulatorType.AddToSet:
                    return new BsonDocument("$addToSet", TranslateValue(node.Argument));
                case AccumulatorType.Average:
                    return new BsonDocument("$avg", TranslateValue(node.Argument));
                case AccumulatorType.First:
                    return new BsonDocument("$first", TranslateValue(node.Argument));
                case AccumulatorType.Last:
                    return new BsonDocument("$last", TranslateValue(node.Argument));
                case AccumulatorType.Max:
                    return new BsonDocument("$max", TranslateValue(node.Argument));
                case AccumulatorType.Min:
                    return new BsonDocument("$min", TranslateValue(node.Argument));
                case AccumulatorType.Push:
                    return new BsonDocument("$push", TranslateValue(node.Argument));
                case AccumulatorType.StandardDeviationPopulation:
                    return new BsonDocument("$stdDevPop", TranslateValue(node.Argument));
                case AccumulatorType.StandardDeviationSample:
                    return new BsonDocument("$stdDevSamp", TranslateValue(node.Argument));
                case AccumulatorType.Sum:
                    return new BsonDocument("$sum", TranslateValue(node.Argument));
            }

            // we should never ever get here.
            var message = string.Format("Unrecognized aggregation type in the expression tree {0}.",
                node.ToString());
            throw new MongoInternalException(message);
        }

        private BsonValue TranslateAggregateExpression(AggregateExpressionExpression node)
        {
            return TranslateValue(node.Expression);
        }

        private BsonValue TranslateArrayIndex(ArrayIndexExpression node)
        {
            return new BsonDocument("$arrayElemAt", new BsonArray
            {
                TranslateValue(node.Array),
                TranslateValue(node.Index)
            });
        }

        private BsonValue TranslateArrayLength(Expression node)
        {
            return new BsonDocument("$size", TranslateValue(((UnaryExpression)node).Operand));
        }

        private BsonValue TranslateConcat(ConcatExpression node)
        {
            var first = TranslateValue(node.Source);
            var second = TranslateValue(node.Other);

            return new BsonDocument("$concatArrays", new BsonArray
            {
                first,
                second
            });
        }

        private BsonValue TranslateConditional(ConditionalExpression node)
        {
            var condition = TranslateValue(node.Test);
            var truePart = TranslateValue(node.IfTrue);
            var falsePart = TranslateValue(node.IfFalse);

            return new BsonDocument("$cond", new BsonArray(new[] { condition, truePart, falsePart }));
        }

        private static BsonValue TranslateConstant(Expression node)
        {
            var value = BsonValue.Create(((ConstantExpression)node).Value);
            var stringValue = value as BsonString;
            if (stringValue != null && stringValue.Value.StartsWith("$"))
            {
                value = new BsonDocument("$literal", value);
            }
            // NOTE: there may be other instances where we should use a literal...
            // but I can't think of any yet.
            return value;
        }

        private BsonValue TranslateDocumentWrappedField(FieldAsDocumentExpression expression)
        {
            return new BsonDocument(expression.FieldName, TranslateValue(expression.Expression));
        }

        private BsonValue TranslateExcept(ExceptExpression node)
        {
            return new BsonDocument("$setDifference", new BsonArray(new[]
            {
                TranslateValue(node.Source),
                TranslateValue(node.Other)
            }));
        }

        private BsonValue TranslateField(FieldExpression expression)
        {
            if (expression.Document == null)
            {
                return "$" + expression.FieldName;
            }

            // 2 possibilities. 
            // 1. This is translatable into a single string:
            // 2. This has an array index operation in it which we must then use a $let expression for
            var parent = expression.Document;
            var currentName = expression.FieldName;
            while (parent != null)
            {
                var field = parent as IFieldExpression;
                if (field != null)
                {
                    currentName = field.FieldName + "." + currentName;
                    parent = field.Document;
                }
                else
                {
                    var array = parent as ArrayIndexExpression;
                    if (array != null)
                    {
                        return new BsonDocument("$let", new BsonDocument
                        {
                            { "vars", new BsonDocument("item", TranslateValue(parent)) },
                            { "in", "$$item." + currentName }
                        });
                    }

                    break;
                }
            }

            return "$" + currentName;
        }

        private BsonValue TranslateGroupingKey(GroupingKeyExpression node)
        {
            return TranslateValue(node.Expression);
        }

        private BsonValue TranslateIntersect(IntersectExpression node)
        {
            return new BsonDocument("$setIntersection", new BsonArray(new[]
            {
                TranslateValue(node.Source),
                TranslateValue(node.Other)
            }));
        }

        private BsonValue TranslateMemberAccess(MemberExpression node)
        {
            BsonValue result;
            if (node.Expression.Type == typeof(DateTime)
                && TryTranslateDateTimeMemberAccess(node, out result))
            {
                return result;
            }

            if (node.Expression.Type == typeof(string)
                && TryTranslateStringMemberAccess(node, out result))
            {
                return result;
            }

            var expressionType = node.Expression.Type;
            if (node.Expression != null
                && (expressionType.ImplementsInterface(typeof(ICollection<>))
                    || expressionType.ImplementsInterface(typeof(ICollection)))
                && node.Member.Name == "Count")
            {
                return new BsonDocument("$size", TranslateValue(node.Expression));
            }

            var message = string.Format("Member {0} of type {1} in the expression tree {2} cannot be translated.",
                node.Member.Name,
                node.Member.DeclaringType,
                node.ToString());
            throw new NotSupportedException(message);
        }

        private BsonValue TranslateMethodCall(MethodCallExpression node)
        {
            BsonValue result;

            if (node.Object == null)
            {
                if (node.Method.DeclaringType == typeof(string)
                    && TryTranslateStaticStringMethodCall(node, out result))
                {
                    return result;
                }

                if (node.Method.DeclaringType == typeof(Math)
                    && TryTranslateStaticMathMethodCall(node, out result))
                {
                    return result;
                }

                if (node.Method.DeclaringType == typeof(Enumerable)
                    && TryTranslateStaticEnumerableMethodCall(node, out result))
                {
                    return result;
                }

                if (node.Method.DeclaringType == typeof(DateTime)
                    && TryTranslateStaticDateTimeMethodCall(node, out result))
                {
                    return result;
                }
            }
            else
            {
                if (node.Object.Type == typeof(string)
                    && TryTranslateStringMethodCall(node, out result))
                {
                    return result;
                }

                if (node.Object.Type == typeof(DateTime)
                    && TryTranslateDateTimeCall(node, out result))
                {
                    return result;
                }

                if (node.Object.Type.GetTypeInfo().IsGenericType
                    && node.Object.Type.GetGenericTypeDefinition() == typeof(HashSet<>)
                    && TryTranslateHashSetMethodCall(node, out result))
                {
                    return result;
                }

                if (node.Method.Name == "CompareTo"
                    && (node.Object.Type.ImplementsInterface(typeof(IComparable<>))
                        || node.Object.Type.ImplementsInterface(typeof(IComparable))))
                {
                    return new BsonDocument("$cmp", new BsonArray(new[] { TranslateValue(node.Object), TranslateValue(node.Arguments[0]) }));
                }

                if (node.Method.Name == "Equals"
                    && node.Arguments.Count == 1)
                {
                    return new BsonDocument("$eq", new BsonArray(new[] { TranslateValue(node.Object), TranslateValue(node.Arguments[0]) }));
                }
            }

            var message = string.Format("{0} of type {1} is not supported in the expression tree {2}.",
                node.Method.Name,
                node.Method.DeclaringType,
                node.ToString());
            throw new NotSupportedException(message);
        }

        private BsonValue TranslateMemberInit(MemberInitExpression node)
        {
            var mapping = ProjectionMapper.Map(node);
            return TranslateMapping(mapping);
        }

        private BsonValue TranslateNew(NewExpression node)
        {
            if (node.Type == typeof(DateTime))
            {
                return TranslateNewDateTime(node);
            }
            var mapping = ProjectionMapper.Map(node);
            return TranslateMapping(mapping);
        }

        private BsonValue TranslateNewArrayInit(NewArrayExpression node)
        {
            var bsonArray = new BsonArray();
            foreach (var item in node.Expressions)
            {
                bsonArray.Add(TranslateValue(item));
            }
            return bsonArray;
        }

        private BsonValue TranslateNewDateTime(NewExpression node)
        {
            BsonValue year = null;
            BsonValue month = null;
            BsonValue day = null;
            BsonValue hour = null;
            BsonValue minute = null;
            BsonValue second = null;
            BsonValue millisecond = null;

            switch (node.Arguments.Count)
            {
                case 3:
                    year = TranslateValue(node.Arguments[0]);
                    month = TranslateValue(node.Arguments[1]);
                    day = TranslateValue(node.Arguments[2]);
                    break;
                case 6:
                    hour = TranslateValue(node.Arguments[3]);
                    minute = TranslateValue(node.Arguments[4]);
                    second = TranslateValue(node.Arguments[5]);
                    goto case 3;
                case 7:
                    if (node.Arguments[6].Type == typeof(int))
                    {
                        millisecond = TranslateValue(node.Arguments[6]);
                        goto case 6;
                    }
                    break;
            }

            if (year == null)
            { 
                throw new NotSupportedException($"The DateTime constructor {node} is not supported.");
            }

            return new BsonDocument("$dateFromParts", new BsonDocument
            {
                { "year", year, year != null },
                { "month", month, month != null },
                { "day", day, day != null },
                { "hour", hour, hour != null },
                { "minute", minute, minute != null },
                { "second", second, second != null },
                { "millisecond", millisecond, millisecond != null }
            });
        }

        private BsonValue TranslateMapping(ProjectionMapping mapping)
        {
            BsonDocument doc = new BsonDocument();
            bool hasId = false;
            foreach (var memberMapping in mapping.Members)
            {
                var value = TranslateValue(memberMapping.Expression);
                string name = memberMapping.Member.Name;
                if (!hasId && memberMapping.Expression is GroupingKeyExpression)
                {
                    name = "_id";
                    hasId = true;
                    doc.InsertAt(0, new BsonElement(name, value));
                }
                else
                {
                    doc.Add(name, value);
                }
            }

            return doc;
        }

        private BsonValue TranslateNot(UnaryExpression node)
        {
            var operand = TranslateValue(node.Operand);
            if (!operand.IsBsonArray)
            {
                operand = new BsonArray().Add(operand);
            }
            return new BsonDocument("$not", operand);
        }

        private BsonValue TranslateOperation(BinaryExpression node, string op, bool canBeFlattened)
        {
            var left = TranslateValue(node.Left);
            var right = TranslateValue(node.Right);

            // some operations take an array as the argument.
            // we want to flatten binary values into the top-level 
            // array if they are flattenable :).
            if (canBeFlattened && left.IsBsonDocument && left.AsBsonDocument.Contains(op) && left[op].IsBsonArray)
            {
                left[op].AsBsonArray.Add(right);
                return left;
            }

            return new BsonDocument(op, new BsonArray(new[] { left, right }));
        }

        private BsonValue TranslatePipeline(PipelineExpression node)
        {
            if (node.ResultOperator == null)
            {
                return TranslateValue(node.Source);
            }

            BsonValue result;
            if (TryTranslateAggregateResultOperator(node, out result) ||
                TryTranslateAllResultOperator(node, out result) ||
                TryTranslateAnyResultOperator(node, out result) ||
                TryTranslateAvgResultOperator(node, out result) ||
                TryTranslateContainsResultOperator(node, out result) ||
                TryTranslateCountResultOperator(node, out result) ||
                TryTranslateFirstResultOperator(node, out result) ||
                TryTranslateLastResultOperator(node, out result) ||
                TryTranslateMaxResultOperator(node, out result) ||
                TryTranslateMinResultOperator(node, out result) ||
                TryTranslateStdDevResultOperator(node, out result) ||
                TryTranslateSumResultOperator(node, out result))
            {
                if (result == null)
                {
                    // we successfully translated, but nothing to do.
                    return TranslateValue(node.Source);
                }

                return result;
            }

            var message = string.Format("The result operation {0} is not supported.", node.ResultOperator.GetType());
            throw new NotSupportedException(message);
        }

        private BsonValue TranslateReverse(ReverseExpression node)
        {
            return new BsonDocument("$reverseArray", TranslateValue(node.Source));
        }

        private BsonValue TranslateSelect(SelectExpression node)
        {
            var inputValue = TranslateValue(node.Source);
            var inValue = TranslateValue(FieldNamePrefixer.Prefix(node.Selector, "$" + node.ItemName));
            if (inputValue.BsonType == BsonType.String && inValue.BsonType == BsonType.String)
            {
                // if inputValue is a BsonString and inValue is a BsonString, 
                // then it is a simple field inclusion...
                // inValue is prefixed with a $${node.ItemName}, so we remove the itemName and the 2 $s.
                return inputValue.ToString() + inValue.ToString().Substring(node.ItemName.Length + 2);
            }

            return new BsonDocument("$map", new BsonDocument
            {
                { "input", inputValue },
                { "as", node.ItemName },
                { "in", inValue}
            });
        }

        private BsonValue TranslateSerializedConstant(SerializedConstantExpression node)
        {
            return node.SerializeValue(node.Type, node.Value);
        }

        private BsonValue TranslateSkip(SkipExpression node)
        {
            var message = "$project or $group only supports Skip when immediately followed by a Take.";
            throw new NotSupportedException(message);
        }

        private BsonValue TranslateTake(TakeExpression node)
        {
            var arguments = new BsonArray();
            var skipNode = node.Source as SkipExpression;
            if (skipNode != null)
            {
                arguments.Add(TranslateValue(skipNode.Source));
                arguments.Add(TranslateValue(skipNode.Count));
            }
            else
            {
                arguments.Add(TranslateValue(node.Source));
            }

            arguments.Add(TranslateValue(node.Count));

            return new BsonDocument("$slice", arguments);
        }

        private BsonValue TranslateUnion(UnionExpression node)
        {
            return new BsonDocument("$setUnion", new BsonArray(new[]
            {
                TranslateValue(node.Source),
                TranslateValue(node.Other)
            }));
        }

        private BsonValue TranslateWhere(WhereExpression node)
        {
            var inputValue = TranslateValue(node.Source);
            var condValue = TranslateValue(FieldNamePrefixer.Prefix(node.Predicate, "$" + node.ItemName));

            return new BsonDocument("$filter", new BsonDocument
            {
                { "input", inputValue },
                { "as", node.ItemName },
                { "cond", condValue }
            });
        }

        private BsonValue TranslateZip(ZipExpression node)
        {
            var inputs = new[] { TranslateValue(node.Source), TranslateValue(node.Other) };

            return new BsonDocument("$zip", new BsonDocument("inputs", new BsonArray(inputs)));
        }

        private bool TryTranslateAggregateResultOperator(PipelineExpression node, out BsonValue result)
        {
            result = null;
            var resultOperator = node.ResultOperator as AggregateResultOperator;
            if (resultOperator != null)
            {
                var input = TranslateValue(node.Source);
                var initialValue = TranslateValue(resultOperator.Seed);
                var inValue = TranslateValue(resultOperator.Reducer);

                result = new BsonDocument("$reduce", new BsonDocument
                {
                    { "input", input },
                    { "initialValue", initialValue },
                    { "in", inValue }
                });

                if (resultOperator.Finalizer != null)
                {
                    inValue = TranslateValue(resultOperator.Finalizer);
                    result = new BsonDocument("$let", new BsonDocument
                    {
                        { "vars", new BsonDocument(resultOperator.ItemName, result) },
                        { "in", inValue }
                    });
                }
                return true;
            }

            return false;
        }

        private bool TryTranslateAllResultOperator(PipelineExpression node, out BsonValue result)
        {
            var resultOperator = node.ResultOperator as AllResultOperator;
            if (resultOperator != null)
            {
                var whereExpression = node.Source as WhereExpression;

                if (whereExpression != null)
                {
                    var inValue = TranslateValue(FieldNamePrefixer.Prefix(whereExpression.Predicate, "$" + whereExpression.ItemName));

                    result = new BsonDocument("$map", new BsonDocument
                    {
                        { "input", TranslateValue(whereExpression.Source) },
                        { "as", whereExpression.ItemName },
                        { "in", inValue}
                    });

                    result = new BsonDocument("$allElementsTrue", result);
                    return true;
                }
            }

            result = null;
            return false;
        }

        private bool TryTranslateAnyResultOperator(PipelineExpression node, out BsonValue result)
        {
            var resultOperator = node.ResultOperator as AnyResultOperator;
            if (resultOperator != null)
            {
                var whereExpression = node.Source as WhereExpression;

                if (whereExpression == null)
                {
                    result = new BsonDocument("$gt", new BsonArray(new BsonValue[]
                    {
                        new BsonDocument("$size", TranslateValue(node.Source)),
                        0
                    }));
                    return true;
                }
                else
                {
                    var inValue = TranslateValue(FieldNamePrefixer.Prefix(whereExpression.Predicate, "$" + whereExpression.ItemName));

                    result = new BsonDocument("$map", new BsonDocument
                    {
                        { "input", TranslateValue(whereExpression.Source) },
                        { "as", whereExpression.ItemName },
                        { "in", inValue}
                    });

                    result = new BsonDocument("$anyElementTrue", result);
                    return true;
                }
            }

            result = null;
            return false;
        }

        private bool TryTranslateAvgResultOperator(PipelineExpression node, out BsonValue result)
        {
            var resultOperator = node.ResultOperator as AverageResultOperator;
            if (resultOperator != null)
            {
                result = new BsonDocument("$avg", TranslateValue(node.Source));
                return true;
            }

            result = null;
            return false;
        }

        private bool TryTranslateContainsResultOperator(PipelineExpression node, out BsonValue result)
        {
            var resultOperator = node.ResultOperator as ContainsResultOperator;
            if (resultOperator != null)
            {
                var source = TranslateValue(node.Source);
                var value = TranslateValue(resultOperator.Value);

                result = new BsonDocument("$anyElementTrue", new BsonDocument("$map", new BsonDocument
                {
                    { "input", source },
                    { "as", "x" },
                    { "in", new BsonDocument("$eq", new BsonArray(new [] { "$$x", value})) }
                }));
                return true;
            }

            result = null;
            return false;
        }

        private bool TryTranslateCountResultOperator(PipelineExpression node, out BsonValue result)
        {
            var resultOperator = node.ResultOperator as CountResultOperator;
            if (resultOperator != null)
            {
                result = new BsonDocument("$size", TranslateValue(node.Source));
                return true;
            }

            result = null;
            return false;
        }

        private bool TryTranslateDateTimeCall(MethodCallExpression node, out BsonValue result)
        {
            result = null;
            var field = TranslateValue(node.Object);

            switch (node.Method.Name)
            {
                case "ToString":
                    if (node.Arguments.Count == 1)
                    {
                        var format = TranslateValue(node.Arguments[0]);
                        result = new BsonDocument("$dateToString", new BsonDocument
                        {
                            { "format", format },
                            { "date", field }
                        });
                        return true;
                    }
                    break;
            }

            return false;
        }

        private bool TryTranslateDateTimeMemberAccess(MemberExpression node, out BsonValue result)
        {
            result = null;
            var field = TranslateValue(node.Expression);
            switch (node.Member.Name)
            {
                case "Day":
                    result = new BsonDocument("$dayOfMonth", field);
                    return true;
                case "DayOfWeek":
                    // The server's day of week values are 1 greater than
                    // .NET's DayOfWeek enum values
                    result = new BsonDocument("$subtract", new BsonArray
                        {
                            new BsonDocument("$dayOfWeek", field),
                            (BsonInt32)1
                        });
                    return true;
                case "DayOfYear":
                    result = new BsonDocument("$dayOfYear", field);
                    return true;
                case "Hour":
                    result = new BsonDocument("$hour", field);
                    return true;
                case "Millisecond":
                    result = new BsonDocument("$millisecond", field);
                    return true;
                case "Minute":
                    result = new BsonDocument("$minute", field);
                    return true;
                case "Month":
                    result = new BsonDocument("$month", field);
                    return true;
                case "Second":
                    result = new BsonDocument("$second", field);
                    return true;
                case "Year":
                    result = new BsonDocument("$year", field);
                    return true;
            }

            return false;
        }

        private bool TryTranslateStringMemberAccess(MemberExpression node, out BsonValue result)
        {
            result = null;
            var field = TranslateValue(node.Expression);
            switch (node.Member.Name)
            {
                case "Length":
                    var name = _stringTranslationMode == AggregateStringTranslationMode.CodePoints ?
                        "$strLenCP" :
                        "$strLenBytes";

                    result = new BsonDocument(name, field);
                    return true;
            }

            return false;
        }

        private bool TryTranslateFirstResultOperator(PipelineExpression node, out BsonValue result)
        {
            var resultOperator = node.ResultOperator as FirstResultOperator;
            if (resultOperator != null)
            {
                result = new BsonDocument("$arrayElemAt", new BsonArray
                {
                    TranslateValue(node.Source),
                    0
                });
                return true;
            }

            result = null;
            return false;
        }

        private bool TryTranslateHashSetMethodCall(MethodCallExpression node, out BsonValue result)
        {
            result = null;
            switch (node.Method.Name)
            {
                case "IsSubsetOf":
                    result = new BsonDocument("$setIsSubset", new BsonArray(new[]
                        {
                            TranslateValue(node.Object),
                            TranslateValue(node.Arguments[0])
                        }));
                    return true;
                case "SetEquals":
                    result = new BsonDocument("$setEquals", new BsonArray(new[]
                        {
                            TranslateValue(node.Object),
                            TranslateValue(node.Arguments[0])
                        }));
                    return true;
            }

            return false;
        }

        private bool TryTranslateLastResultOperator(PipelineExpression node, out BsonValue result)
        {
            var resultOperator = node.ResultOperator as LastResultOperator;
            if (resultOperator != null)
            {
                result = new BsonDocument("$arrayElemAt", new BsonArray
                {
                    TranslateValue(node.Source),
                    -1
                });
                return true;
            }

            result = null;
            return false;
        }

        private bool TryTranslateMaxResultOperator(PipelineExpression node, out BsonValue result)
        {
            var resultOperator = node.ResultOperator as MaxResultOperator;
            if (resultOperator != null)
            {
                result = new BsonDocument("$max", TranslateValue(node.Source));
                return true;
            }

            result = null;
            return false;
        }

        private bool TryTranslateMinResultOperator(PipelineExpression node, out BsonValue result)
        {
            var resultOperator = node.ResultOperator as MinResultOperator;
            if (resultOperator != null)
            {
                result = new BsonDocument("$min", TranslateValue(node.Source));
                return true;
            }

            result = null;
            return false;
        }

        private bool TryTranslateStaticDateTimeMethodCall(MethodCallExpression node, out BsonValue result)
        {
            result = null;
            switch (node.Method.Name)
            {
                case "Parse":
                    if (node.Arguments.Count == 1)
                    {
                        result = new BsonDocument("$dateFromString", new BsonDocument
                        {
                            { "dateString", TranslateValue(node.Arguments[0]) }
                        });
                        return true;
                    }
                    break;
            }

            return false;
        }

        private bool TryTranslateStaticEnumerableMethodCall(MethodCallExpression node, out BsonValue result)
        {
            result = null;
            switch (node.Method.Name)
            {
                case "Range":
                    var start = TranslateValue(node.Arguments[0]);
                    result = new BsonDocument("$range", new BsonArray
                    {
                        start,
                        new BsonDocument("$add", new BsonArray
                        {
                            start,
                            TranslateValue(node.Arguments[1])
                        })
                    });
                    return true;
            }

            return false;
        }

        private bool TryTranslateStaticMathMethodCall(MethodCallExpression node, out BsonValue result)
        {
            result = null;
            switch (node.Method.Name)
            {
                case "Abs":
                    result = new BsonDocument("$abs", TranslateValue(node.Arguments[0]));
                    return true;
                case "Ceiling":
                    result = new BsonDocument("$ceil", TranslateValue(node.Arguments[0]));
                    return true;
                case "Exp":
                    result = new BsonDocument("$exp", new BsonArray
                    {
                        TranslateValue(node.Arguments[0])
                    });
                    return true;
                case "Floor":
                    result = new BsonDocument("$floor", TranslateValue(node.Arguments[0]));
                    return true;
                case "Log":
                    if (node.Arguments.Count == 2)
                    {
                        result = new BsonDocument("$log", new BsonArray
                        {
                            TranslateValue(node.Arguments[0]),
                            TranslateValue(node.Arguments[1])
                        });
                    }
                    else
                    {
                        result = new BsonDocument("$ln", new BsonArray
                        {
                            TranslateValue(node.Arguments[0])
                        });
                    }
                    return true;
                case "Log10":
                    result = new BsonDocument("$log10", new BsonArray
                    {
                        TranslateValue(node.Arguments[0])
                    });
                    return true;
                case "Pow":
                    result = new BsonDocument("$pow", new BsonArray
                    {
                        TranslateValue(node.Arguments[0]),
                        TranslateValue(node.Arguments[1])
                    });
                    return true;
                case "Sqrt":
                    result = new BsonDocument("$sqrt", new BsonArray
                    {
                        TranslateValue(node.Arguments[0])
                    });
                    return true;
                case "Truncate":
                    result = new BsonDocument("$trunc", TranslateValue(node.Arguments[0]));
                    return true;
            }

            return false;
        }

        private bool TryTranslateStaticStringMethodCall(MethodCallExpression node, out BsonValue result)
        {
            result = null;
            switch (node.Method.Name)
            {
                case "IsNullOrEmpty":
                    var field = TranslateValue(node.Arguments[0]);
                    result = new BsonDocument("$or",
                        new BsonArray
                        {
                            new BsonDocument("$eq", new BsonArray { field, BsonNull.Value }),
                            new BsonDocument("$eq", new BsonArray { field, BsonString.Empty })
                        });
                    return true;
            }

            return false;
        }

        private bool TryTranslateStringMethodCall(MethodCallExpression node, out BsonValue result)
        {
            result = null;
            var field = TranslateValue(node.Object);
            switch (node.Method.Name)
            {
                case "Equals":
                    if (node.Arguments.Count == 2 && node.Arguments[1].NodeType == ExpressionType.Constant)
                    {
                        var comparisonType = (StringComparison)((ConstantExpression)node.Arguments[1]).Value;
                        switch (comparisonType)
                        {
                            case StringComparison.OrdinalIgnoreCase:
                                result = new BsonDocument("$eq",
                                    new BsonArray(new BsonValue[]
                                        {
                                            new BsonDocument("$strcasecmp", new BsonArray(new[] { field, TranslateValue(node.Arguments[0]) })),
                                            0
                                        }));
                                return true;
                            case StringComparison.Ordinal:
                                result = new BsonDocument("$eq", new BsonArray(new[] { field, TranslateValue(node.Arguments[0]) }));
                                return true;
                            default:
                                throw new NotSupportedException("Only Ordinal and OrdinalIgnoreCase are supported for string comparisons.");
                        }
                    }
                    break;
                case "IndexOf":
                    var indexOfArgs = new BsonArray { field };

                    if (node.Arguments.Count < 1 || node.Arguments.Count > 3)
                    {
                        return false;
                    }

                    if (node.Arguments[0].Type != typeof(char) && node.Arguments[0].Type != typeof(string))
                    {
                        return false;
                    }
                    var value = TranslateValue(node.Arguments[0]);
                    if (value.BsonType == BsonType.Int32)
                    {
                        value = new BsonString(new string((char)value.AsInt32, 1));
                    }
                    indexOfArgs.Add(value);

                    if (node.Arguments.Count > 1)
                    {
                        if (node.Arguments[1].Type != typeof(int))
                        {
                            return false;
                        }

                        var startIndex = TranslateValue(node.Arguments[1]);
                        indexOfArgs.Add(startIndex);
                    }

                    if (node.Arguments.Count > 2)
                    {
                        if (node.Arguments[2].Type != typeof(int))
                        {
                            return false;
                        }

                        var count = TranslateValue(node.Arguments[2]);
                        var endIndex = new BsonDocument("$add", new BsonArray { indexOfArgs[2], count });
                        indexOfArgs.Add(endIndex);
                    }

                    var indexOpName = _stringTranslationMode == AggregateStringTranslationMode.CodePoints ?
                            "$indexOfCP" :
                            "$indexOfBytes";

                    result = new BsonDocument(indexOpName, indexOfArgs);
                    return true;
                case "Split":
                    if (node.Arguments.Count < 1 || node.Arguments.Count > 2)
                    {
                        return false;
                    }
                    if (node.Arguments[0].Type != typeof(char[]) && node.Arguments[0].Type != typeof(string[]))
                    {
                        return false;
                    }
                    var separatorArray = TranslateValue(node.Arguments[0]) as BsonArray;
                    if (separatorArray == null || separatorArray.Count != 1)
                    {
                        return false;
                    }
                    var separator = separatorArray[0];
                    if (separator.BsonType == BsonType.Int32)
                    {
                        separator = new BsonString(new string((char)separator.AsInt32, 1));
                    }
                    if (node.Arguments.Count == 2)
                    {
                        var constantExpression = node.Arguments[1] as ConstantExpression;
                        if (constantExpression == null || constantExpression.Type != typeof(StringSplitOptions))
                        {
                            return false;
                        }
                        var options = (StringSplitOptions)constantExpression.Value;
                        if (options != StringSplitOptions.None)
                        {
                            return false;
                        }
                    }
                    result = new BsonDocument("$split", new BsonArray
                    {
                        field,
                        separator
                    });
                    return true;
                case "Substring":
                    if (node.Arguments.Count == 2)
                    {
                        var substrOpName = _stringTranslationMode == AggregateStringTranslationMode.CodePoints ?
                            "$substrCP" :
                            "$substr";
                        result = new BsonDocument(substrOpName, new BsonArray(new[]
                            {
                                field,
                                TranslateValue(node.Arguments[0]),
                                TranslateValue(node.Arguments[1])
                            }));
                        return true;
                    }
                    break;
                case "ToLower":
                case "ToLowerInvariant":
                    if (node.Arguments.Count == 0)
                    {
                        result = new BsonDocument("$toLower", field);
                        return true;
                    }
                    break;
                case "ToUpper":
                case "ToUpperInvariant":
                    if (node.Arguments.Count == 0)
                    {
                        result = new BsonDocument("$toUpper", field);
                        return true;
                    }
                    break;
            }

            return false;
        }

        private bool TryTranslateStdDevResultOperator(PipelineExpression node, out BsonValue result)
        {
            var resultOperator = node.ResultOperator as StandardDeviationResultOperator;
            if (resultOperator != null)
            {
                var name = resultOperator.IsSample ? "$stdDevSamp" : "$stdDevPop";
                result = new BsonDocument(name, TranslateValue(node.Source));
                return true;
            }

            result = null;
            return false;
        }

        private bool TryTranslateSumResultOperator(PipelineExpression node, out BsonValue result)
        {
            var resultOperator = node.ResultOperator as SumResultOperator;
            if (resultOperator != null)
            {
                result = new BsonDocument("$sum", TranslateValue(node.Source));
                return true;
            }

            result = null;
            return false;
        }
    }
}
