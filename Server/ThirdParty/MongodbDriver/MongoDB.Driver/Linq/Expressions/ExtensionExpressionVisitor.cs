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

namespace MongoDB.Driver.Linq.Expressions
{
    internal class ExtensionExpressionVisitor : ExpressionVisitor
    {
        protected internal virtual Expression VisitExtensionExpression(ExtensionExpression node)
        {
            throw new NotSupportedException(string.Format("{0} is an unknown expression.", node.GetType()));
        }

        protected internal virtual Expression VisitAccumulator(AccumulatorExpression node)
        {
            return node.Update(Visit(node.Argument));
        }

        protected internal virtual Expression VisitAggregateExpression(AggregateExpressionExpression node)
        {
            return node.Update(Visit(node.Expression));
        }

        protected internal virtual Expression VisitArrayIndex(ArrayIndexExpression node)
        {
            return node.Update(
                Visit(node.Array),
                Visit(node.Index),
                node.Original);
        }

        protected internal virtual Expression VisitCollection(CollectionExpression node)
        {
            return node;
        }

        protected internal virtual Expression VisitInjectedFilter(InjectedFilterExpression node)
        {
            return node;
        }

        protected internal virtual Expression VisitConcat(ConcatExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Other));
        }

        protected internal virtual Expression VisitCorrelated(CorrelatedExpression node)
        {
            return node.Update(Visit(node.Expression));
        }

        protected internal virtual Expression VisitDefaultIfEmpty(DefaultIfEmptyExpression node)
        {
            return node.Update(Visit(node.Source));
        }

        protected internal virtual Expression VisitDistinct(DistinctExpression node)
        {
            return node.Update(Visit(node.Source));
        }

        protected internal virtual Expression VisitDocument(DocumentExpression node)
        {
            return node;
        }

        protected internal virtual Expression VisitDocumentWrappedField(FieldAsDocumentExpression node)
        {
            return node.Update(
                Visit(node.Expression));
        }

        protected internal virtual Expression VisitExcept(ExceptExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Other));
        }

        protected internal virtual Expression VisitField(FieldExpression node)
        {
            return node.Update(
                Visit(node.Document),
                node.Original);
        }

        protected internal virtual Expression VisitGroupBy(GroupByExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.KeySelector),
                VisitAndConvert(node.Accumulators, nameof(VisitGroupBy)));
        }

        protected internal virtual Expression VisitGroupByWithResultSelector(GroupByWithResultSelectorExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Selector));
        }

        protected internal virtual Expression VisitGroupingKey(GroupingKeyExpression node)
        {
            return node.Update(Visit(node.Expression));
        }

        protected internal virtual Expression VisitGroupJoin(GroupJoinExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Joined),
                Visit(node.SourceKeySelector),
                Visit(node.JoinedKeySelector));
        }

        protected internal virtual Expression VisitIntersect(IntersectExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Other));
        }

        protected internal virtual Expression VisitJoin(JoinExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Joined),
                Visit(node.SourceKeySelector),
                Visit(node.JoinedKeySelector));
        }

        protected internal virtual Expression VisitOrderBy(OrderByExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Clauses, VisitOrderByClause));
        }

        protected internal virtual OrderByClause VisitOrderByClause(OrderByClause clause)
        {
            return clause.Update(Visit(clause.Expression));
        }

        protected internal virtual Expression VisitPipeline(PipelineExpression node)
        {
            return node.Update(
                Visit(node.Source),
                VisitAndConvert(node.Projector, nameof(VisitPipeline)),
                VisitResultOperator(node.ResultOperator));
        }

        protected internal virtual ResultOperator VisitResultOperator(ResultOperator resultOperator)
        {
            if (resultOperator == null)
            {
                return resultOperator;
            }

            return resultOperator.Update(this);
        }

        protected internal virtual Expression VisitReverse(ReverseExpression node)
        {
            return node.Update(Visit(node.Source));
        }

        protected internal virtual Expression VisitSample(SampleExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Count));
        }

        protected internal virtual Expression VisitSelect(SelectExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Selector));
        }

        protected internal virtual Expression VisitSelectMany(SelectManyExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.CollectionSelector),
                Visit(node.ResultSelector));
        }

        protected internal virtual Expression VisitSerializedConstant(SerializedConstantExpression node)
        {
            return node;
        }

        protected internal virtual Expression VisitSkip(SkipExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Count));
        }

        protected internal virtual Expression VisitTake(TakeExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Count));
        }

        protected internal virtual Expression VisitUnion(UnionExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Other));
        }

        protected internal virtual Expression VisitWhere(WhereExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Predicate));
        }

        protected internal virtual Expression VisitZip(ZipExpression node)
        {
            return node.Update(
                Visit(node.Source),
                Visit(node.Other));
        }
    }
}
