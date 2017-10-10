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
* 
*/

using System;
using System.Linq.Expressions;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq
{
    internal class FieldExpressionFlattener : ExtensionExpressionVisitor
    {
        public static Expression FlattenFields(Expression node)
        {
            var visitor = new FieldExpressionFlattener();
            return visitor.Visit(node);
        }

        public FieldExpressionFlattener()
        {
        }

        protected internal override Expression VisitArrayIndex(ArrayIndexExpression node)
        {
            var field = Visit(node.Array) as IFieldExpression;
            if (field != null)
            {
                var constantIndex = node.Index as ConstantExpression;
                if (constantIndex == null)
                {
                    throw new NotSupportedException($"Only a constant index is supported in the expression {node}.");
                }

                var index = constantIndex.Value.ToString();
                if (index == "-1")
                {
                    // We've treated -1 as meaning $ operator. We can't break this now,
                    // so, specifically when we are flattening fields names, this is 
                    // how we'll continue to treat -1.
                    index = "$";
                }

                return new FieldExpression(
                    field.AppendFieldName(index),
                    node.Serializer);
            }

            return node;
        }

        protected internal override Expression VisitDocumentWrappedField(FieldAsDocumentExpression node)
        {
            var field = Visit(node.Document) as IFieldExpression;
            if (field != null)
            {
                return new FieldExpression(
                    node.PrependFieldName(field.FieldName),
                    node.Serializer);
            }

            return node;
        }

        protected internal override Expression VisitField(FieldExpression node)
        {
            var document = Visit(node.Document) as IFieldExpression;
            if (document != null)
            {
                return new FieldExpression(
                    node.PrependFieldName(document.FieldName),
                    node.Serializer,
                    node.Original);
            }

            return node;
        }
    }
}
