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

using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq
{
    internal sealed class QueryableTranslation
    {
        private readonly QueryableExecutionModel _model;
        private readonly IResultTransformer _resultTransformer;

        public QueryableTranslation(QueryableExecutionModel model, IResultTransformer resultTransformer)
        {
            _model = Ensure.IsNotNull(model, nameof(model));
            _resultTransformer = resultTransformer;
        }

        public QueryableExecutionModel Model
        {
            get { return _model; }
        }

        public IResultTransformer ResultTransformer
        {
            get { return _resultTransformer; }
        }
    }
}
