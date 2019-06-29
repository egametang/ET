/* Copyright 2015-present MongoDB Inc.
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

namespace MongoDB.Driver.Linq.Expressions
{
    internal enum ExtensionExpressionType
    {
        Pipeline,

        // Sequence
        Concat,
        DefaultIfEmpty,
        Distinct,
        Except,
        GroupBy,
        GroupByWithResultSelector,
        GroupJoin,
        Intersect,
        Join,
        OrderBy,
        Reverse,
        Sample,
        Select,
        SelectMany,
        Skip,
        Take,
        Union,
        Where,
        Zip,

        // Bindings
        Accumulator,
        AggregateExpression,
        ArrayIndex,
        Collection,
        Document,
        FieldAsDocument,
        Field,
        GroupingKey,
        InjectedFilter,
        SerializedConstant,

        // Temporary
        Correlated,
    }
}
