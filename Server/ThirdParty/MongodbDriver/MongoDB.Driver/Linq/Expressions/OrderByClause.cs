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

namespace MongoDB.Driver.Linq.Expressions
{
    internal sealed class OrderByClause
    {
        private readonly SortDirection _direction;
        private readonly Expression _expression;

        public OrderByClause(Expression expression, SortDirection direction)
        {
            _expression = expression;
            _direction = direction;
        }

        public SortDirection Direction
        {
            get { return _direction; }
        }

        public Expression Expression
        {
            get { return _expression; }
        }

        public override string ToString()
        {
            return _expression.ToString() + " " + _direction.ToString();
        }

        public OrderByClause Update(Expression expression)
        {
            if (expression != _expression)
            {
                return new OrderByClause(expression, _direction);
            }

            return this;
        }
    }
}