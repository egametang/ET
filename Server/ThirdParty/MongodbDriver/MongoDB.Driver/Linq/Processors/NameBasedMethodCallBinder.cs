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
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors
{
    internal sealed class NameBasedMethodCallBinder<TBindingContext> : IMethodCallBinder<TBindingContext> where TBindingContext : IBindingContext
    {
        private readonly Dictionary<string, List<KeyValuePair<Registration, IMethodCallBinder<TBindingContext>>>> _binders;

        public NameBasedMethodCallBinder()
        {
            _binders = new Dictionary<string, List<KeyValuePair<Registration, IMethodCallBinder<TBindingContext>>>>();
        }

        public Expression Bind(PipelineExpression pipeline, TBindingContext context, MethodCallExpression node, IEnumerable<Expression> arguments)
        {
            List<KeyValuePair<Registration, IMethodCallBinder<TBindingContext>>> namedBinders;
            if (!_binders.TryGetValue(node.Method.Name, out namedBinders))
            {
                return null;
            }

            var binder = namedBinders.Where(x => x.Key.Filter(node)).Select(x => x.Value).FirstOrDefault();
            if (binder == null)
            {
                return null;
            }

            return binder.Bind(pipeline, context, node, arguments);
        }

        public bool IsRegistered(MethodCallExpression node)
        {
            List<KeyValuePair<Registration, IMethodCallBinder<TBindingContext>>> namedBinders;
            if (!_binders.TryGetValue(node.Method.Name, out namedBinders))
            {
                return false;
            }

            return namedBinders.Any(x => x.Key.Filter(node));
        }

        public void Register(IMethodCallBinder<TBindingContext> binder, params string[] names)
        {
            Register(binder, node => true, names);
        }

        public void Register(IMethodCallBinder<TBindingContext> binder, Func<MethodCallExpression, bool> filter, params string[] names)
        {
            if (names == null)
            {
                return;
            }

            foreach (var name in names)
            {
                List<KeyValuePair<Registration, IMethodCallBinder<TBindingContext>>> namedBinders;
                if (!_binders.TryGetValue(name, out namedBinders))
                {
                    _binders[name] = namedBinders = new List<KeyValuePair<Registration, IMethodCallBinder<TBindingContext>>>();
                }

                namedBinders.Add(
                    new KeyValuePair<Registration, IMethodCallBinder<TBindingContext>>(
                        new Registration(name, filter),
                        binder));
            }
        }

        private class Registration
        {
            private readonly string _name;
            private readonly Func<MethodCallExpression, bool> _filter;

            public Registration(string name, Func<MethodCallExpression, bool> filter)
            {
                _name = name;
                _filter = filter;
            }

            public string Name
            {
                get { return _name; }
            }

            public Func<MethodCallExpression, bool> Filter
            {
                get { return _filter; }
            }
        }


    }
}
