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
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.Pipeline.MethodCallBinders
{
    internal abstract class SelectingResultOperatorBinderBase : IMethodCallBinder<PipelineBindingContext>
    {
        public Expression Bind(PipelineExpression pipeline, PipelineBindingContext bindingContext, MethodCallExpression node, IEnumerable<Expression> arguments)
        {
            var source = pipeline.Source;
            Expression argument;
            if (arguments.Any())
            {
                var lambda = ExpressionHelper.GetLambda(arguments.Single());
                bindingContext.AddExpressionMapping(lambda.Parameters[0], pipeline.Projector);

                argument = bindingContext.Bind(lambda.Body);
            }
            else
            {
                var selectExpression = source as SelectExpression;
                if (selectExpression != null)
                {
                    source = selectExpression.Source;
                    argument = selectExpression.Selector;
                    var fieldAsDocumentExpression = argument as FieldAsDocumentExpression;
                    if (fieldAsDocumentExpression != null)
                    {
                        argument = fieldAsDocumentExpression.Expression;
                    }
                }
                else
                {
                    argument = pipeline.Projector;
                }
            }

            var serializer = bindingContext.GetSerializer(node.Type, argument);

            var accumulator = new AccumulatorExpression(
                node.Type,
                "__result",
                serializer,
                GetAccumulatorType(),
                argument);

            source = new GroupByExpression(
                node.Type,
                source,
                Expression.Constant(1),
                new[] { accumulator });

            return new PipelineExpression(
                source,
                new FieldExpression(accumulator.FieldName, accumulator.Serializer),
                CreateResultOperator(
                    node.Type,
                    bindingContext.GetSerializer(node.Type, node)));
        }

        protected abstract AccumulatorType GetAccumulatorType();

        protected abstract ResultOperator CreateResultOperator(Type resultType, IBsonSerializer serializer);
    }
}
