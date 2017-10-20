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
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver.Linq.Expressions.ResultOperators
{
    internal static class ResultTransformerHelper
    {
        public static LambdaExpression CreateAggregator(string methodName, Type sourceType)
        {
            var sourceParameter = Expression.Parameter(
                typeof(IEnumerable<>).MakeGenericType(sourceType),
                "source");

            return Expression.Lambda(
                Expression.Call(typeof(Enumerable),
                    methodName,
                    new[] { sourceType },
                    sourceParameter),
                sourceParameter);
        }

        public static LambdaExpression CreateAsyncAggregator(string methodName, Type sourceType)
        {
            var sourceParameter = Expression.Parameter(
                typeof(Task<>).MakeGenericType(typeof(IAsyncCursor<>).MakeGenericType(sourceType)),
                "source");

            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "ct");
            return Expression.Lambda(
                Expression.Call(
                    typeof(AsyncCursorHelper),
                    methodName,
                    new[] { sourceType },
                    sourceParameter,
                    cancellationTokenParameter),
                sourceParameter,
                cancellationTokenParameter);
        }
    }
}
