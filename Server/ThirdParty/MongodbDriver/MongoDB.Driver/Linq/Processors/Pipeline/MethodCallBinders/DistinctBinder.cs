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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.Pipeline.MethodCallBinders
{
    internal sealed class DistinctBinder : IMethodCallBinder<PipelineBindingContext>
    {
        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            yield return MethodHelper.GetMethodDefinition(() => Enumerable.Distinct<object>(null));
            yield return MethodHelper.GetMethodDefinition(() => Queryable.Distinct<object>(null));
        }

        public Expression Bind(PipelineExpression pipeline, PipelineBindingContext bindingContext, MethodCallExpression node, IEnumerable<Expression> arguments)
        {
            var source = pipeline.Source;
            Expression keySelector;
            var previousSelectExpression = source as SelectExpression;
            if (previousSelectExpression != null)
            {
                keySelector = previousSelectExpression.Selector;
                source = previousSelectExpression.Source;

                var fieldAsDocumentExpression = keySelector as FieldAsDocumentExpression;
                if (fieldAsDocumentExpression != null)
                {
                    keySelector = fieldAsDocumentExpression.Expression;
                }
            }
            else if (pipeline.Projector is FieldExpression)
            {
                keySelector = pipeline.Projector;
            }
            else
            {
                var currentProjector = (ISerializationExpression)pipeline.Projector;
                keySelector = new FieldExpression("$ROOT", currentProjector.Serializer);
            }

            var serializer = bindingContext.GetSerializer(keySelector.Type, keySelector);

            return new PipelineExpression(
                new GroupByExpression(
                    source,
                    keySelector),
                new FieldExpression("_id", serializer));
        }
    }
}
