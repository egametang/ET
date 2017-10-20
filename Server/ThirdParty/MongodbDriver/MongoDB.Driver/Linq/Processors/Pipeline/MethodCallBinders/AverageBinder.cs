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
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Expressions.ResultOperators;

namespace MongoDB.Driver.Linq.Processors.Pipeline.MethodCallBinders
{
    internal sealed class AverageBinder : SelectingResultOperatorBinderBase
    {
        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            return MethodHelper.GetEnumerableAndQueryableMethodDefinitions("Average");
        }

        protected override ResultOperator CreateResultOperator(Type resultType, IBsonSerializer serializer)
        {
            return new AverageResultOperator(resultType, serializer);
        }

        protected override AccumulatorType GetAccumulatorType()
        {
            return AccumulatorType.Average;
        }
    }
}
