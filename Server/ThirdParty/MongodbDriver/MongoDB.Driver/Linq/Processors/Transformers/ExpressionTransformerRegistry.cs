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

namespace MongoDB.Driver.Linq.Processors.Transformers
{
    internal sealed class ExpressionTransformerRegistry
    {
        private readonly Dictionary<ExpressionType, List<Func<Expression, Expression>>> _transformations;

        public ExpressionTransformerRegistry()
        {
            _transformations = new Dictionary<ExpressionType, List<Func<Expression, Expression>>>();
        }

        public IEnumerable<Func<Expression, Expression>> GetTransformers(ExpressionType nodeType)
        {
            List<Func<Expression, Expression>> typedTransformations;
            if (!_transformations.TryGetValue(nodeType, out typedTransformations))
            {
                return Enumerable.Empty<Func<Expression, Expression>>();
            }

            return typedTransformations;
        }

        public void Register<TExpression>(IExpressionTransformer<TExpression> transformer) where TExpression : Expression
        {
            foreach (var nodeType in transformer.SupportedNodeTypes)
            {
                List<Func<Expression, Expression>> typedTransformations;
                if (!_transformations.TryGetValue(nodeType, out typedTransformations))
                {
                    _transformations[nodeType] = typedTransformations = new List<Func<Expression, Expression>>();
                }

                typedTransformations.Add(e => transformer.Transform((TExpression)e));
            }
        }
    }
}
