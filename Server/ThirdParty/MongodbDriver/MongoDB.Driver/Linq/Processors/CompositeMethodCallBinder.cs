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

using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors
{
    internal sealed class CompositeMethodCallBinder<TBindingContext> : IMethodCallBinder<TBindingContext>
        where TBindingContext : IBindingContext
    {
        private readonly MethodInfoMethodCallBinder<TBindingContext> _infoBinder;
        private readonly NameBasedMethodCallBinder<TBindingContext> _nameBinder;

        public CompositeMethodCallBinder(MethodInfoMethodCallBinder<TBindingContext> infoBinder, NameBasedMethodCallBinder<TBindingContext> nameBinder)
        {
            _infoBinder = Ensure.IsNotNull(infoBinder, nameof(infoBinder));
            _nameBinder = Ensure.IsNotNull(nameBinder, nameof(nameBinder));
        }

        public bool IsRegistered(MethodCallExpression node)
        {
            return _infoBinder.IsRegistered(node) || _nameBinder.IsRegistered(node);
        }

        public Expression Bind(PipelineExpression pipeline, TBindingContext bindingContext, MethodCallExpression node, IEnumerable<Expression> arguments)
        {
            var result = _infoBinder.Bind(pipeline, bindingContext, node, arguments);
            if (result == null)
            {
                result = _nameBinder.Bind(pipeline, bindingContext, node, arguments);
            }

            return result;
        }
    }
}