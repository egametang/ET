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

using System.Linq.Expressions;
using MongoDB.Driver.Linq.Processors.Transformers;

namespace MongoDB.Driver.Linq.Processors
{
    internal sealed class Transformer : ExpressionVisitor
    {
        private static ExpressionTransformerRegistry __registry;

        static Transformer()
        {
            __registry = new ExpressionTransformerRegistry();
            __registry.Register(new NullableTransformer());
            __registry.Register(new EqualsAnyBooleanTransformer());
            __registry.Register(new FirstLastNormalizingTransformer());
            __registry.Register(new SelectSelectCombiningTransformer());
            __registry.Register(new ConstantOnRightTransformer());
            __registry.Register(new CollectionConstructorTransformer());
            __registry.Register(new VBStringIndexComparisonTransformer());
            __registry.Register(new VBCompareStringTransformer());
            __registry.Register(new VBNothingConversionRemovalTransformer());
            __registry.Register(new VBCoalesceTransformer());
            __registry.Register(new VBInformationIsNothingTransformer());
        }

        public static Expression Transform(Expression node)
        {
            var transformer = new Transformer();
            return transformer.Visit(node);
        }

        private Transformer()
        {
        }

        public override Expression Visit(Expression node)
        {
            var visited = base.Visit(node);

            if (visited == null)
            {
                return visited;
            }

            var transformers = __registry.GetTransformers(visited.NodeType);

            foreach (var transformer in transformers)
            {
                var transformed = transformer(visited);
                if (transformed != visited)
                {
                    // we'll start over and apply the transformations to
                    // the new node
                    return Visit(transformed);
                }
            }

            return visited;
        }
    }
}