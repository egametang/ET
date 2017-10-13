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
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors.Pipeline.MethodCallBinders;

namespace MongoDB.Driver.Linq.Processors.Pipeline
{
    internal sealed class PipelineBinder : PipelineBinderBase<PipelineBindingContext>
    {
        private readonly static MethodInfoMethodCallBinder<PipelineBindingContext> __methodCallBinder;

        static PipelineBinder()
        {
            __methodCallBinder = new MethodInfoMethodCallBinder<PipelineBindingContext>();
            __methodCallBinder.Register(new AnyBinder(), AnyBinder.GetSupportedMethods());
            __methodCallBinder.Register(new AverageBinder(), AverageBinder.GetSupportedMethods());
            __methodCallBinder.Register(new CountBinder(), CountBinder.GetSupportedMethods());
            __methodCallBinder.Register(new DistinctBinder(), DistinctBinder.GetSupportedMethods());
            __methodCallBinder.Register(new FirstBinder(), FirstBinder.GetSupportedMethods());
            __methodCallBinder.Register(new GroupByBinder(), GroupByBinder.GetSupportedMethods());
            __methodCallBinder.Register(new GroupByWithResultSelectorBinder(), GroupByWithResultSelectorBinder.GetSupportedMethods());
            __methodCallBinder.Register(new JoinBinder(), JoinBinder.GetSupportedMethods());
            __methodCallBinder.Register(new MaxBinder(), MaxBinder.GetSupportedMethods());
            __methodCallBinder.Register(new MinBinder(), MinBinder.GetSupportedMethods());
            __methodCallBinder.Register(new OfTypeBinder(), OfTypeBinder.GetSupportedMethods());
            __methodCallBinder.Register(new OrderByBinder(), OrderByBinder.GetSupportedMethods());
            __methodCallBinder.Register(new SampleBinder(), SampleBinder.GetSupportedMethods());
            __methodCallBinder.Register(new SelectBinder(), SelectBinder.GetSupportedMethods());
            __methodCallBinder.Register(new SelectManyBinder(), SelectManyBinder.GetSupportedMethods());
            __methodCallBinder.Register(new SingleBinder(), SingleBinder.GetSupportedMethods());
            __methodCallBinder.Register(new SkipBinder(), SkipBinder.GetSupportedMethods());
            __methodCallBinder.Register(new StandardDeviationPopulationBinder(), StandardDeviationPopulationBinder.GetSupportedMethods());
            __methodCallBinder.Register(new StandardDeviationSampleBinder(), StandardDeviationSampleBinder.GetSupportedMethods());
            __methodCallBinder.Register(new SumBinder(), SumBinder.GetSupportedMethods());
            __methodCallBinder.Register(new TakeBinder(), TakeBinder.GetSupportedMethods());
            __methodCallBinder.Register(new ThenByBinder(), ThenByBinder.GetSupportedMethods());
            __methodCallBinder.Register(new WhereBinder(), WhereBinder.GetSupportedMethods());
        }

        public static Expression Bind(Expression node, IBsonSerializerRegistry serializerRegistry)
        {
            var bindingContext = new PipelineBindingContext(serializerRegistry);
            var binder = new PipelineBinder(bindingContext);

            node = binder.Bind(node);
            node = AccumulatorBinder.Bind(node, bindingContext);
            node = CorrelatedGroupRewriter.Rewrite(node);
            return node;
        }

        private PipelineBinder(PipelineBindingContext bindingContext)
            : base(bindingContext, __methodCallBinder)
        {
        }

        protected override Expression BindNonMethodCall(Expression node)
        {
            if (node.NodeType == ExpressionType.Constant &&
                node.Type.GetTypeInfo().IsGenericType &&
                node.Type.GetGenericTypeDefinition() == typeof(IMongoQueryable<>))
            {
                var queryable = (IMongoQueryable)((ConstantExpression)node).Value;
                var provider = (IMongoQueryProvider)queryable.Provider;
                return new PipelineExpression(
                    new CollectionExpression(provider.CollectionNamespace, provider.CollectionDocumentSerializer),
                    new DocumentExpression(provider.CollectionDocumentSerializer));
            }

            var message = string.Format("The expression tree is not supported: {0}",
                            node.ToString());

            throw new NotSupportedException(message);
        }
    }
}