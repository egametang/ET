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
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors.EmbeddedPipeline.MethodCallBinders;
using MongoDB.Driver.Support;

namespace MongoDB.Driver.Linq.Processors.EmbeddedPipeline
{
    internal sealed class EmbeddedPipelineBinder : PipelineBinderBase<EmbeddedPipelineBindingContext>
    {
        private readonly static CompositeMethodCallBinder<EmbeddedPipelineBindingContext> __methodCallBinder;

        static EmbeddedPipelineBinder()
        {
            var infoBinder = new MethodInfoMethodCallBinder<EmbeddedPipelineBindingContext>();
            infoBinder.Register(new AggregateBinder(), AggregateBinder.GetSupportedMethods());
            infoBinder.Register(new AllBinder(), AllBinder.GetSupportedMethods());
            infoBinder.Register(new AnyBinder(), AnyBinder.GetSupportedMethods());
            infoBinder.Register(new AsQueryableBinder(), AsQueryableBinder.GetSupportedMethods());
            infoBinder.Register(new AverageBinder(), AverageBinder.GetSupportedMethods());
            infoBinder.Register(new ConcatBinder(), ConcatBinder.GetSupportedMethods());
            infoBinder.Register(new DefaultIfEmptyBinder(), DefaultIfEmptyBinder.GetSupportedMethods());
            infoBinder.Register(new DistinctBinder(), DistinctBinder.GetSupportedMethods());
            infoBinder.Register(new ExceptBinder(), ExceptBinder.GetSupportedMethods());
            infoBinder.Register(new FirstBinder(), FirstBinder.GetSupportedMethods());
            infoBinder.Register(new IntersectBinder(), IntersectBinder.GetSupportedMethods());
            infoBinder.Register(new LastBinder(), LastBinder.GetSupportedMethods());
            infoBinder.Register(new MaxBinder(), MaxBinder.GetSupportedMethods());
            infoBinder.Register(new MinBinder(), MinBinder.GetSupportedMethods());
            infoBinder.Register(new OfTypeBinder(), OfTypeBinder.GetSupportedMethods());
            infoBinder.Register(new ReverseBinder(), ReverseBinder.GetSupportedMethods());
            infoBinder.Register(new SelectBinder(), SelectBinder.GetSupportedMethods());
            infoBinder.Register(new SkipBinder(), SkipBinder.GetSupportedMethods());
            infoBinder.Register(new StandardDeviationBinder(), StandardDeviationBinder.GetSupportedMethods());
            infoBinder.Register(new SumBinder(), SumBinder.GetSupportedMethods());
            infoBinder.Register(new TakeBinder(), TakeBinder.GetSupportedMethods());
            infoBinder.Register(new ToArrayBinder(), ToArrayBinder.GetSupportedMethods());
            infoBinder.Register(new ToHashSetBinder(), ToHashSetBinder.GetSupportedMethods());
            infoBinder.Register(new ToListBinder(), ToListBinder.GetSupportedMethods());
            infoBinder.Register(new UnionBinder(), UnionBinder.GetSupportedMethods());
            infoBinder.Register(new WhereBinder(), WhereBinder.GetSupportedMethods());
            infoBinder.Register(new ZipBinder(), ZipBinder.GetSupportedMethods());

            var nameBinder = new NameBasedMethodCallBinder<EmbeddedPipelineBindingContext>();
            nameBinder.Register(new ContainsBinder(), ContainsBinder.IsSupported, ContainsBinder.SupportedMethodNames);
            nameBinder.Register(new CountBinder(), CountBinder.IsSupported, CountBinder.SupportedMethodNames);

            __methodCallBinder = new CompositeMethodCallBinder<EmbeddedPipelineBindingContext>(
                infoBinder,
                nameBinder);
        }

        public static bool SupportsNode(MethodCallExpression node)
        {
            return __methodCallBinder.IsRegistered(node);
        }

        public static Expression Bind(Expression node, IBindingContext parent)
        {
            var bindingContext = new EmbeddedPipelineBindingContext(parent);
            var binder = new EmbeddedPipelineBinder(bindingContext);

            var bound = binder.Bind(node);
            bound = AccumulatorBinder.Bind(bound, bindingContext);
            bound = CorrelatedGroupRewriter.Rewrite(bound);
            return MultipleWhereMerger.Merge(bound);
        }

        public EmbeddedPipelineBinder(EmbeddedPipelineBindingContext bindingContext)
            : base(bindingContext, __methodCallBinder)
        {
        }

        protected override Expression BindNonMethodCall(Expression node)
        {
            var serializationExpression = node as ISerializationExpression;
            if (serializationExpression != null)
            {
                var arraySerializer = serializationExpression.Serializer as IBsonArraySerializer;
                BsonSerializationInfo itemSerializationInfo;
                if (arraySerializer != null && arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                {
                    if (itemSerializationInfo.ElementName == null)
                    {
                        return new PipelineExpression(
                            node,
                            new DocumentExpression(itemSerializationInfo.Serializer));
                    }
                    else
                    {
                        return new PipelineExpression(
                            node,
                            new FieldExpression(itemSerializationInfo.ElementName, itemSerializationInfo.Serializer));
                    }
                }
            }
            else if (node.NodeType == ExpressionType.Constant)
            {
                var sequenceType = node.Type.GetSequenceElementType();
                if (sequenceType != null)
                {
                    var serializer = BindingContext.GetSerializer(sequenceType, node);
                    return new PipelineExpression(
                        node,
                        new DocumentExpression(serializer));
                }
            }

            var message = string.Format("The expression tree is not supported: {0}",
                            node.ToString());

            throw new NotSupportedException(message);
        }
    }
}