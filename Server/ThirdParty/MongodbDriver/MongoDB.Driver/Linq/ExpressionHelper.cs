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

using System.Linq;
using System.Linq.Expressions;

namespace MongoDB.Driver.Linq
{
    internal static class ExpressionHelper
    {
        public static LambdaExpression GetLambda(Expression node)
        {
            if (node.NodeType == ExpressionType.Constant && ((ConstantExpression)node).Value is LambdaExpression)
            {
                return (LambdaExpression)((ConstantExpression)node).Value;
            }

            return (LambdaExpression)StripQuotes(node);
        }

        public static bool IsLambda(Expression node)
        {
            return StripQuotes(node).NodeType == ExpressionType.Lambda;
        }

        public static bool IsLambda(Expression node, int parameterCount)
        {
            var lambda = StripQuotes(node);
            return lambda.NodeType == ExpressionType.Lambda &&
                ((LambdaExpression)lambda).Parameters.Count == parameterCount;
        }

        public static bool IsLinqMethod(MethodCallExpression node, params string[] names)
        {
            if (node == null)
            {
                return false;
            }

            if (node.Method.DeclaringType != typeof(Enumerable) &&
                node.Method.DeclaringType != typeof(Queryable) &&
                node.Method.DeclaringType != typeof(MongoQueryable))
            {
                return false;
            }

            if (names == null || names.Length == 0)
            {
                return true;
            }

            return names.Contains(node.Method.Name);
        }

        public static bool TryGetExpression<T>(Expression node, out T value)
            where T : class
        {
            while (node.NodeType == ExpressionType.Convert ||
                node.NodeType == ExpressionType.ConvertChecked ||
                node.NodeType == ExpressionType.Quote)
            {
                node = ((UnaryExpression)node).Operand;
            }

            value = node as T;
            return value != null;
        }

        private static Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }
    }
}
