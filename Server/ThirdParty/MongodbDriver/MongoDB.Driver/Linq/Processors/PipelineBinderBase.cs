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
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Support;

namespace MongoDB.Driver.Linq.Processors
{
    internal abstract class PipelineBinderBase<TBindingContext> where TBindingContext : class, IBindingContext
    {
        private readonly TBindingContext _bindingContext;
        private readonly IMethodCallBinder<TBindingContext> _methodCallBinder;

        protected PipelineBinderBase(TBindingContext bindingContext, IMethodCallBinder<TBindingContext> methodCallBinder)
        {
            _bindingContext = Ensure.IsNotNull(bindingContext, nameof(bindingContext));
            _methodCallBinder = Ensure.IsNotNull(methodCallBinder, nameof(methodCallBinder));
        }

        protected TBindingContext BindingContext
        {
            get { return _bindingContext; }
        }

        protected abstract Expression BindNonMethodCall(Expression node);

        protected Expression Bind(Expression node)
        {
            if (node.Type == typeof(void))
            {
                var message = string.Format("Expressions of type void are not supported: {0}", node.ToString());
                throw new NotSupportedException(message);
            }

            node = RemoveUnnecessaries(node);
            var methodCall = node as MethodCallExpression;

            if (methodCall != null)
            {
                return BindMethodCall((MethodCallExpression)node);
            }

            return BindNonMethodCall(node);
        }

        private PipelineExpression BindPipeline(Expression node)
        {
            var bound = Bind(node);
            var pipeline = bound as PipelineExpression;
            if (pipeline == null)
            {
                var message = string.Format("Expected a PipelineExpression, but found a {0} in the expression tree: {1}.",
                    bound.GetType(),
                    node.ToString());
                throw new NotSupportedException(message);
            }

            return pipeline;
        }

        private Expression BindMethodCall(MethodCallExpression node)
        {
            Expression source;
            IEnumerable<Expression> arguments;

            if (node.Object != null)
            {
                source = node.Object;
                arguments = node.Arguments;
            }
            else
            {
                // assuming an extension method...
                source = node.Arguments[0];
                arguments = node.Arguments.Skip(1);
            }

            var pipeline = BindPipeline(source);
            var result = _methodCallBinder.Bind(pipeline, _bindingContext, node, arguments);
            if (result == null)
            {
                var message = string.Format("The method {0} is not supported in the expression tree: {1}.",
                    node.Method.Name,
                    node.ToString());
                throw new NotSupportedException(message);
            }

            return result;
        }

        private Expression RemoveUnnecessaries(Expression node)
        {
            while (node.NodeType == ExpressionType.Convert ||
                node.NodeType == ExpressionType.ConvertChecked ||
                node.NodeType == ExpressionType.Quote)
            {
                node = ((UnaryExpression)node).Operand;
            }

            return node;
        }
    }
}