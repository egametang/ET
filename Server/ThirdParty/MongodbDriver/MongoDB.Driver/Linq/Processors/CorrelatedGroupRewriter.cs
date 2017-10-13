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
    internal sealed class CorrelatedGroupRewriter : ExtensionExpressionVisitor
    {
        public static Expression Rewrite(Expression node)
        {
            var rewriter = new CorrelatedGroupRewriter();
            return rewriter.Visit(node);
        }

        private ILookup<Guid, CorrelatedExpression> _accumulatorLookup;
        private readonly Dictionary<CorrelatedExpression, Expression> _accumulatorReplacementMap;

        private CorrelatedGroupRewriter()
        {
            _accumulatorReplacementMap = new Dictionary<CorrelatedExpression, Expression>();
        }

        protected internal override Expression VisitCorrelated(CorrelatedExpression node)
        {
            if (node.Expression is AccumulatorExpression)
            {
                return VisitCorrelatedAccumulator(node);
            }

            if (node.Expression is GroupByExpression)
            {
                return VisitCorrelatedGroup(node);
            }

            return base.VisitCorrelated(node);
        }

        private Expression VisitCorrelatedAccumulator(CorrelatedExpression node)
        {
            Expression mapped;
            if (_accumulatorReplacementMap.TryGetValue(node, out mapped))
            {
                return mapped;
            }

            return base.VisitCorrelated(node);
        }

        private Expression VisitCorrelatedGroup(CorrelatedExpression node)
        {
            var groupExpression = (GroupByExpression)node.Expression;
            if (_accumulatorLookup != null && _accumulatorLookup.Contains(node.CorrelationId))
            {
                var oldAccumulatorLookup = _accumulatorLookup;
                var source = Visit(groupExpression.Source);
                _accumulatorLookup = oldAccumulatorLookup;

                var accumulators = new List<AccumulatorExpression>();
                var fieldExpressions = new List<FieldExpression>();
                var comparer = new ExpressionComparer();
                foreach (var correlatedAccumulator in _accumulatorLookup[node.CorrelationId])
                {
                    var index = accumulators.FindIndex(x => comparer.Compare((Expression)x, correlatedAccumulator.Expression));

                    FieldExpression fieldExpression;
                    if (index == -1)
                    {
                        var accumulator = (AccumulatorExpression)correlatedAccumulator.Expression;

                        // TODO: might not need to do any renames...
                        accumulator = new AccumulatorExpression(
                            accumulator.Type,
                            "__agg" + accumulators.Count,
                            accumulator.Serializer,
                            accumulator.AccumulatorType,
                            accumulator.Argument);

                        accumulators.Add(accumulator);
                        fieldExpression = new FieldExpression(accumulator.FieldName, accumulator.Serializer);
                        fieldExpressions.Add(fieldExpression);
                    }
                    else
                    {
                        fieldExpression = fieldExpressions[index];
                    }

                    _accumulatorReplacementMap[correlatedAccumulator] = fieldExpression;
                }

                groupExpression = new GroupByExpression(
                    groupExpression.Type,
                    source,
                    groupExpression.KeySelector,
                    accumulators.AsReadOnly());
            }

            return Visit(groupExpression);
        }

        protected internal override Expression VisitPipeline(PipelineExpression node)
        {
            _accumulatorLookup = AccumulatorGatherer.Gather(node.Source).ToLookup(x => x.CorrelationId);

            return base.VisitPipeline(node);
        }

        private class AccumulatorGatherer : ExtensionExpressionVisitor
        {
            public static List<CorrelatedExpression> Gather(Expression node)
            {
                var gatherer = new AccumulatorGatherer();
                gatherer.Visit(node);
                return gatherer._accumulators;
            }

            private readonly List<CorrelatedExpression> _accumulators;

            public AccumulatorGatherer()
            {
                _accumulators = new List<CorrelatedExpression>();
            }

            protected internal override Expression VisitCorrelated(CorrelatedExpression node)
            {
                if (node.Expression is AccumulatorExpression)
                {
                    _accumulators.Add(node);
                    return node;
                }

                return base.VisitCorrelated(node);
            }
        }
    }
}
