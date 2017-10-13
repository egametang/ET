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
using System.Linq.Expressions;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Expressions.ResultOperators;

namespace MongoDB.Driver.Linq.Processors
{
    internal sealed class AccumulatorBinder : ExtensionExpressionVisitor
    {
        public static Expression Bind(Expression node, IBindingContext bindingContext)
        {
            var binder = new AccumulatorBinder(bindingContext);
            return binder.Visit(node);
        }

        private readonly IBindingContext _bindingContext;
        private int _count;

        public AccumulatorBinder(IBindingContext bindingContext)
        {
            _bindingContext = Ensure.IsNotNull(bindingContext, nameof(bindingContext));
        }

        protected internal override Expression VisitAccumulator(AccumulatorExpression node)
        {
            return node;
        }

        protected internal override Expression VisitPipeline(PipelineExpression node)
        {
            Guid correlationId;
            if (TryGetCorrelatedGroup(node.Source, out correlationId))
            {
                AccumulatorType accumulatorType;
                Expression argument;
                if (TryGetAccumulatorTypeAndArgument(node, out accumulatorType, out argument))
                {
                    var accumulator = new AccumulatorExpression(
                        node.Type,
                        "__agg" + _count++,
                        _bindingContext.GetSerializer(node.Type, argument),
                        accumulatorType,
                        argument);

                    return new CorrelatedExpression(
                        correlationId,
                        accumulator);
                }
            }

            return node;
        }

        private bool TryGetCorrelatedGroup(Expression source, out Guid correlationId)
        {
            while (source != null)
            {
                if (_bindingContext.TryGetCorrelatingId(source, out correlationId))
                {
                    return true;
                }

                var newSource = source as ISourcedExpression;
                if (newSource != null)
                {
                    source = newSource.Source;
                }
                else
                {
                    var correlatedExpression = source as CorrelatedExpression;
                    if (correlatedExpression != null)
                    {
                        newSource = correlatedExpression.Expression as ISourcedExpression;
                        if (newSource != null)
                        {
                            source = newSource.Source;
                        }
                        else
                        {
                            source = null;
                        }
                    }
                    else
                    {
                        source = null;
                    }
                }
            }

            correlationId = Guid.Empty;
            return false;
        }

        private bool TryGetAccumulatorTypeAndArgument(PipelineExpression node, out AccumulatorType accumulatorType, out Expression argument)
        {
            if (node.ResultOperator == null)
            {
                var distinct = node.Source as DistinctExpression;
                if (distinct != null)
                {
                    accumulatorType = AccumulatorType.AddToSet;
                    argument = GetAccumulatorArgument(distinct.Source);
                    return true;
                }

                accumulatorType = AccumulatorType.Push;
                argument = GetAccumulatorArgument(node.Source);
                return true;
            }

            var resultOperator = node.ResultOperator;
            if (resultOperator is AverageResultOperator)
            {
                accumulatorType = AccumulatorType.Average;
                argument = GetAccumulatorArgument(node.Source);
                return true;
            }
            if (resultOperator is CountResultOperator)
            {
                accumulatorType = AccumulatorType.Sum;
                argument = Expression.Constant(1);
                return true;
            }
            if (resultOperator is FirstResultOperator)
            {
                accumulatorType = AccumulatorType.First;
                argument = GetAccumulatorArgument(node.Source);
                return true;
            }
            if (resultOperator is LastResultOperator)
            {
                accumulatorType = AccumulatorType.Last;
                argument = GetAccumulatorArgument(node.Source);
                return true;
            }
            if (resultOperator is MaxResultOperator)
            {
                accumulatorType = AccumulatorType.Max;
                argument = GetAccumulatorArgument(node.Source);
                return true;
            }
            if (resultOperator is MinResultOperator)
            {
                accumulatorType = AccumulatorType.Min;
                argument = GetAccumulatorArgument(node.Source);
                return true;
            }
            if (resultOperator is StandardDeviationResultOperator)
            {
                var isSample = ((StandardDeviationResultOperator)resultOperator).IsSample;
                accumulatorType = isSample ? AccumulatorType.StandardDeviationSample : AccumulatorType.StandardDeviationPopulation;
                argument = GetAccumulatorArgument(node.Source);
                return true;
            }
            if (resultOperator is SumResultOperator)
            {
                accumulatorType = AccumulatorType.Sum;
                argument = GetAccumulatorArgument(node.Source);
                return true;
            }
            if (resultOperator is ArrayResultOperator)
            {
                accumulatorType = AccumulatorType.Push;
                argument = GetAccumulatorArgument(node.Source);
                return true;
            }
            if (resultOperator is HashSetResultOperator)
            {
                accumulatorType = AccumulatorType.AddToSet;
                argument = GetAccumulatorArgument(node.Source);
                return true;
            }
            if (resultOperator is ListResultOperator)
            {
                accumulatorType = AccumulatorType.Push;
                argument = GetAccumulatorArgument(node.Source);
                return true;
            }

            accumulatorType = 0;
            argument = null;
            return false;
        }

        private Expression GetAccumulatorArgument(Expression node)
        {
            // we are looking for a Map
            var select = node as SelectExpression;
            if (select != null)
            {
                return select.Selector;
            }

            throw new NotSupportedException();
        }
    }
}
