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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors.EmbeddedPipeline;
using MongoDB.Driver.Support;

namespace MongoDB.Driver.Linq.Processors
{
    internal sealed class SerializationBinder : ExtensionExpressionVisitor
    {
        public static Expression Bind(Expression node, IBindingContext context, bool isClientSideProjection = false)
        {
            var binder = new SerializationBinder(context, isClientSideProjection);
            return binder.Visit(node);
        }

        private readonly IBindingContext _bindingContext;
        private bool _isInEmbeddedPipeline;
        private readonly bool _isClientSideProjection;

        private SerializationBinder(IBindingContext bindingContext, bool isClientSideProjection)
        {
            _bindingContext = bindingContext;
            _isClientSideProjection = isClientSideProjection;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            Expression replacement;
            if (_bindingContext.TryGetExpressionMapping(node, out replacement))
            {
                return replacement;
            }

            return base.Visit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var newNode = base.VisitBinary(node);
            var binaryExpression = newNode as BinaryExpression;
            if (binaryExpression != null && binaryExpression.NodeType == ExpressionType.ArrayIndex)
            {
                var serializationExpression = binaryExpression.Left as ISerializationExpression;
                if (serializationExpression != null)
                {
                    var arraySerializer = serializationExpression.Serializer as IBsonArraySerializer;
                    BsonSerializationInfo itemSerializationInfo;
                    if (arraySerializer != null &&
                        arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                    {
                        newNode = new ArrayIndexExpression(
                            binaryExpression.Left,
                            binaryExpression.Right,
                            itemSerializationInfo.Serializer,
                            binaryExpression);
                    }
                }
            }

            return newNode;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.GetTypeInfo().IsGenericType &&
                node.Type.GetGenericTypeDefinition() == typeof(IMongoQueryable<>))
            {
                var queryable = (IMongoQueryable)node.Value;
                var provider = (IMongoQueryProvider)queryable.Provider;
                return new CollectionExpression(
                    provider.CollectionNamespace,
                    provider.CollectionDocumentSerializer);
            }

            return base.VisitConstant(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            // Don't visit the parameters. We cannot replace a parameter expression
            // with a document and we don't have a new parameter type to use because
            // we don't know why we are binding yet.
            return node.Update(
                Visit(node.Body),
                node.Parameters);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Expression newNode;
            if (_bindingContext.TryGetMemberMapping(node.Member, out newNode))
            {
                if (newNode is ISerializationExpression)
                {
                    return newNode;
                }

                var message = string.Format("Could not determine serialization information for member {0} in the expression tree {1}.",
                    node.Member,
                    node.ToString());
                throw new MongoInternalException(message);
            }

            newNode = base.VisitMember(node);
            var mex = newNode as MemberExpression;
            if (mex != null)
            {
                var serializationExpression = mex.Expression as ISerializationExpression;
                if (serializationExpression != null)
                {
                    var documentSerializer = serializationExpression.Serializer as IBsonDocumentSerializer;
                    BsonSerializationInfo memberSerializationInfo;
                    if (documentSerializer != null && documentSerializer.TryGetMemberSerializationInfo(node.Member.Name, out memberSerializationInfo))
                    {
                        if (memberSerializationInfo.ElementName == null)
                        {
                            newNode = new DocumentExpression(memberSerializationInfo.Serializer);
                        }
                        else
                        {
                            newNode = new FieldExpression(
                                mex.Expression,
                                memberSerializationInfo.ElementName,
                                memberSerializationInfo.Serializer,
                                mex);
                        }
                    }
                }
            }

            return newNode;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!_isClientSideProjection && EmbeddedPipelineBinder.SupportsNode(node))
            {
                return BindEmbeddedPipeline(node);
            }

            switch (node.Method.Name)
            {
                case "ElementAt":
                    return BindElementAt(node);
                case "get_Item":
                    return BindGetItem(node);
                case "Inject":
                    return BindInject(node);
            }

            // Select and SelectMany are the only supported client-side projection operators
            if (_isClientSideProjection &&
                ExpressionHelper.IsLinqMethod(node, "Select", "SelectMany") &&
                node.Arguments.Count == 2)
            {
                return BindClientSideProjector(node);
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var newNode = (UnaryExpression)base.VisitUnary(node);
            if (newNode.NodeType == ExpressionType.Convert || newNode.NodeType == ExpressionType.ConvertChecked || newNode.NodeType == ExpressionType.TypeAs)
            {
                // handle unnecessary convert expressions
                if (newNode.Method == null && !newNode.IsLiftedToNull && newNode.Type.GetTypeInfo().IsAssignableFrom(newNode.Operand.Type))
                {
                    return newNode.Operand;
                }

                // handle downcast convert expressions
                if (newNode.Operand.Type.GetTypeInfo().IsAssignableFrom(newNode.Type))
                {
                    var serializationExpression = newNode.Operand as SerializationExpression;
                    if (serializationExpression != null)
                    {
                        var serializer = _bindingContext.GetSerializer(newNode.Type, newNode);
                        switch (serializationExpression.ExtensionType)
                        {
                            case ExtensionExpressionType.ArrayIndex:
                                return new ArrayIndexExpression(
                                    ((ArrayIndexExpression)serializationExpression).Array,
                                    ((ArrayIndexExpression)serializationExpression).Index,
                                    serializer);
                            case ExtensionExpressionType.Collection:
                                return new CollectionExpression(
                                    ((CollectionExpression)serializationExpression).CollectionNamespace,
                                    serializer);
                            case ExtensionExpressionType.Document:
                                return new DocumentExpression(serializer);
                            case ExtensionExpressionType.Field:
                                return ((FieldExpression)serializationExpression).WithSerializer(serializer);
                            case ExtensionExpressionType.FieldAsDocument:
                                return new FieldAsDocumentExpression(
                                    ((FieldAsDocumentExpression)serializationExpression).Expression,
                                    ((FieldAsDocumentExpression)serializationExpression).FieldName,
                                    serializer);
                        }
                    }
                }
            }

            return newNode;
        }

        // private methods
        private Expression BindElementAt(MethodCallExpression node)
        {
            if (!ExpressionHelper.IsLinqMethod(node))
            {
                return base.VisitMethodCall(node);
            }

            var newNode = base.VisitMethodCall(node);
            var methodCallExpression = newNode as MethodCallExpression;
            if (node != newNode && methodCallExpression != null)
            {
                var serializationExpression = methodCallExpression.Arguments[0] as ISerializationExpression;
                if (serializationExpression != null)
                {
                    var arraySerializer = serializationExpression.Serializer as IBsonArraySerializer;
                    BsonSerializationInfo itemSerializationInfo;
                    if (arraySerializer != null && arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                    {
                        newNode = new ArrayIndexExpression(
                            methodCallExpression.Arguments[0],
                            methodCallExpression.Arguments[1],
                            itemSerializationInfo.Serializer,
                            methodCallExpression);
                    }
                }
            }

            return newNode;
        }

        private Expression BindEmbeddedPipeline(MethodCallExpression node)
        {
            var oldIsInEmbeddedPipeline = _isInEmbeddedPipeline;
            _isInEmbeddedPipeline = true;

            node = node.Update(
                Visit(node.Object),
                Visit(node.Arguments));

            _isInEmbeddedPipeline = oldIsInEmbeddedPipeline;
            if (_isInEmbeddedPipeline)
            {
                return node;
            }

            // we need to discover if this is rooted at an IMongoQueryable... If so, it 
            // gets processed as a top-level pipeline...

            return EmbeddedPipelineBinder.Bind(node, _bindingContext);
        }

        private Expression BindGetItem(MethodCallExpression node)
        {
            var newNode = base.VisitMethodCall(node);
            var methodCallExpression = newNode as MethodCallExpression;
            if (node == newNode ||
                methodCallExpression == null ||
                node.Object == null ||
                node.Arguments.Count != 1)
            {
                return newNode;
            }

            var serializationExpression = methodCallExpression.Object as ISerializationExpression;
            if (serializationExpression == null)
            {
                return newNode;
            }

            var indexExpression = methodCallExpression.Arguments[0];
            switch (Type.GetTypeCode(indexExpression.Type))
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    var arraySerializer = serializationExpression.Serializer as IBsonArraySerializer;
                    BsonSerializationInfo itemSerializationInfo;
                    if (arraySerializer != null && arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                    {
                        return new ArrayIndexExpression(
                            methodCallExpression.Object,
                            indexExpression,
                            itemSerializationInfo.Serializer,
                            methodCallExpression);
                    }
                    break;
                case TypeCode.String:
                    var index = indexExpression as ConstantExpression;
                    if (index != null)
                    {
                        var documentSerializer = serializationExpression.Serializer as IBsonDocumentSerializer;
                        BsonSerializationInfo memberSerializationInfo;
                        if (documentSerializer != null && documentSerializer.TryGetMemberSerializationInfo(index.Value.ToString(), out memberSerializationInfo))
                        {
                            return new FieldExpression(
                                methodCallExpression.Object,
                                memberSerializationInfo.ElementName,
                                memberSerializationInfo.Serializer,
                                methodCallExpression);
                        }
                    }
                    break;
            }

            return node; // return the original node because we can't translate this expression.
        }


        private Expression BindInject(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(LinqExtensions))
            {
                var arg = node.Arguments[0] as ConstantExpression;
                if (arg != null)
                {
                    var docType = node.Method.GetGenericArguments()[0];
                    var serializer = _bindingContext.GetSerializer(node.Method.GetGenericArguments()[0], arg);
                    var renderedFilter = (BsonDocument)typeof(FilterDefinition<>).MakeGenericType(docType)
                        .GetTypeInfo()
                        .GetMethod("Render")
                        .Invoke(arg.Value, new object[] { serializer, _bindingContext.SerializerRegistry });

                    return new InjectedFilterExpression(renderedFilter);
                }
            }

            return node;
        }

        private Expression BindClientSideProjector(MethodCallExpression node)
        {
            var arguments = new List<Expression>();
            arguments.Add(Visit(node.Arguments[0]));

            // we need to make sure that the serialization info for the parameter
            // is the item serialization from the parent IBsonArraySerializer
            var fieldExpression = arguments[0] as IFieldExpression;
            if (fieldExpression != null)
            {
                var arraySerializer = fieldExpression.Serializer as IBsonArraySerializer;
                BsonSerializationInfo itemSerializationInfo;
                if (arraySerializer != null && arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                {
                    var lambda = ExpressionHelper.GetLambda(node.Arguments[1]);
                    _bindingContext.AddExpressionMapping(
                        lambda.Parameters[0],
                        new FieldExpression(
                            fieldExpression.FieldName,
                            itemSerializationInfo.Serializer,
                            lambda.Parameters[0]));
                }
            }

            arguments.Add(Visit(node.Arguments[1]));

            if (node.Arguments[0] != arguments[0] || node.Arguments[1] != arguments[1])
            {
                node = Expression.Call(node.Method, arguments.ToArray());
            }

            return node;
        }
    }
}