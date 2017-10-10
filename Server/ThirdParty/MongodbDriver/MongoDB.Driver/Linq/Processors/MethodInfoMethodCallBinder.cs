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
using System.Reflection;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors
{
    internal sealed class MethodInfoMethodCallBinder<TBindingContext> : IMethodCallBinder<TBindingContext>
        where TBindingContext : IBindingContext
    {
        private readonly Dictionary<MethodInfo, IMethodCallBinder<TBindingContext>> _binders;

        public MethodInfoMethodCallBinder()
        {
            _binders = new Dictionary<MethodInfo, IMethodCallBinder<TBindingContext>>();
        }

        public Expression Bind(PipelineExpression pipeline, TBindingContext bindingContext, MethodCallExpression node, IEnumerable<Expression> arguments)
        {
            IMethodCallBinder<TBindingContext> binder;
            if (!TryGetMethodCallBinder(node.Method, out binder))
            {
                return null;
            }

            return binder.Bind(pipeline, bindingContext, node, arguments);
        }

        public bool IsRegistered(MethodCallExpression node)
        {
            IMethodCallBinder<TBindingContext> binder;
            return TryGetMethodCallBinder(node.Method, out binder);
        }

        public void Register(IMethodCallBinder<TBindingContext> binder, IEnumerable<MethodInfo> methods)
        {
            foreach (var method in methods)
            {
                _binders[method] = binder;
            }
        }

        private bool TryGetMethodCallBinder(MethodInfo method, out IMethodCallBinder<TBindingContext> binder)
        {
            if (_binders.TryGetValue(method, out binder))
            {
                return true;
            }

            var methodDefinition = MethodHelper.GetMethodDefinition(method);
            return _binders.TryGetValue(methodDefinition, out binder);
        }
    }
}
