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

namespace MongoDB.Driver.Linq.Processors.EmbeddedPipeline.MethodCallBinders
{
    internal sealed class OfTypeBinder : IMethodCallBinder<EmbeddedPipelineBindingContext>
    {
        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            return MethodHelper.GetEnumerableAndQueryableMethodDefinitions("OfType");
        }

        public Expression Bind(PipelineExpression pipeline, EmbeddedPipelineBindingContext bindingContext, MethodCallExpression node, IEnumerable<Expression> arguments)
        {
            var newType = node.Method.GetGenericArguments()[0];

            var serializer = bindingContext.GetSerializer(newType, pipeline.Projector);

            var projector = pipeline.Projector;
            var fieldProjector = projector as IFieldExpression;
            if (fieldProjector != null)
            {
                projector = new FieldExpression(
                    fieldProjector.FieldName,
                    serializer);
            }
            else
            {
                projector = new DocumentExpression(serializer);
            }

            return new PipelineExpression(
                new WhereExpression(
                    pipeline.Source,
                    "__p",
                    Expression.TypeIs(pipeline.Projector, newType)),
                projector);
        }
    }
}
