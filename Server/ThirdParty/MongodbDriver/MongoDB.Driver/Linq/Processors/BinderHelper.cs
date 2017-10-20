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
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors.EmbeddedPipeline;

namespace MongoDB.Driver.Linq.Processors
{
    internal static class BinderHelper
    {
        public static SelectExpression BindSelect(PipelineExpression pipeline, IBindingContext bindingContext, LambdaExpression lambda)
        {
            bindingContext.AddExpressionMapping(lambda.Parameters[0], pipeline.Projector);

            var selector = bindingContext.Bind(lambda.Body);

            return new SelectExpression(
                pipeline.Source,
                lambda.Parameters[0].Name,
                selector);
        }

        public static WhereExpression BindWhere(PipelineExpression pipeline, IBindingContext bindingContext, LambdaExpression lambda)
        {
            bindingContext.AddExpressionMapping(lambda.Parameters[0], pipeline.Projector);

            var predicate = bindingContext.Bind(lambda.Body);

            return new WhereExpression(
                pipeline.Source,
                lambda.Parameters[0].Name,
                predicate);
        }
    }
}
