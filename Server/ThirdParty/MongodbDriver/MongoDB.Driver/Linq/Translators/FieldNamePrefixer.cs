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
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Translators
{
    internal class FieldNamePrefixer : ExtensionExpressionVisitor
    {
        public static Expression Prefix(Expression node, string prefix)
        {
            var replacer = new FieldNamePrefixer(prefix);
            return replacer.Visit(node);
        }

        private readonly string _prefix;

        private FieldNamePrefixer(string prefix)
        {
            _prefix = prefix;
        }

        protected internal override Expression VisitDocument(DocumentExpression node)
        {
            return new FieldExpression(
                _prefix,
                node.Serializer);
        }

        protected internal override Expression VisitField(FieldExpression node)
        {
            if (node.Document is IFieldExpression || node.Document is ArrayIndexExpression)
            {
                return node.Update(
                    Visit(node.Document),
                    node.Original);
            }

            return new FieldExpression(
                node.Document,
                node.PrependFieldName(_prefix),
                node.Serializer,
                node.Original);
        }

        protected internal override Expression VisitPipeline(PipelineExpression node)
        {
            var source = node.Source;
            var sourcedSource = source as ISourcedExpression;
            while (sourcedSource != null)
            {
                source = sourcedSource.Source;
                sourcedSource = source as ISourcedExpression;
            }

            var newSource = Visit(source);
            PipelineExpression newNode = node;
            if (newSource != source)
            {
                newNode = (PipelineExpression)ExpressionReplacer.Replace(node, source, newSource);
            }

            return newNode.Update(
                newNode.Source,
                newNode.Projector,
                VisitResultOperator(newNode.ResultOperator));
        }
    }
}