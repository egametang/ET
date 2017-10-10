/* Copyright 2010-2016 MongoDB Inc.
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    internal class FindFluent<TDocument, TProjection> : FindFluentBase<TDocument, TProjection>
    {
        // private fields
        private readonly IMongoCollection<TDocument> _collection;
        private FilterDefinition<TDocument> _filter;
        private readonly FindOptions<TDocument, TProjection> _options;

        // constructors
        public FindFluent(IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options)
        {
            _collection = Ensure.IsNotNull(collection, nameof(collection));
            _filter = Ensure.IsNotNull(filter, nameof(filter));
            _options = Ensure.IsNotNull(options, nameof(options));
        }

        // public properties
        public override FilterDefinition<TDocument> Filter
        {
            get { return _filter; }
            set { _filter = Ensure.IsNotNull(value, nameof(value)); }
        }

        public override FindOptions<TDocument, TProjection> Options
        {
            get { return _options; }
        }

        // public methods
        public override IFindFluent<TDocument, TResult> As<TResult>(IBsonSerializer<TResult> resultSerializer)
        {
            var projection = Builders<TDocument>.Projection.As<TResult>(resultSerializer);
            return Project(projection);
        }

        public override long Count(CancellationToken cancellationToken)
        {
            var options = CreateCountOptions();
            return _collection.Count(_filter, options, cancellationToken);
        }

        public override Task<long> CountAsync(CancellationToken cancellationToken)
        {
            var options = CreateCountOptions();
            return _collection.CountAsync(_filter, options, cancellationToken);
        }

        public override IFindFluent<TDocument, TProjection> Limit(int? limit)
        {
            _options.Limit = limit;
            return this;
        }

        public override IFindFluent<TDocument, TNewProjection> Project<TNewProjection>(ProjectionDefinition<TDocument, TNewProjection> projection)
        {
            var newOptions = new FindOptions<TDocument, TNewProjection>
            {
                AllowPartialResults = _options.AllowPartialResults,
                BatchSize = _options.BatchSize,
                Collation = _options.Collation,
                Comment = _options.Comment,
                CursorType = _options.CursorType,
                Limit = _options.Limit,
                MaxAwaitTime = _options.MaxAwaitTime,
                MaxTime = _options.MaxTime,
                Modifiers = _options.Modifiers,
                NoCursorTimeout = _options.NoCursorTimeout,
                OplogReplay = _options.OplogReplay,
                Projection = projection,
                Skip = _options.Skip,
                Sort = _options.Sort,
            };
            return new FindFluent<TDocument, TNewProjection>(_collection, _filter, newOptions);
        }

        public override IFindFluent<TDocument, TProjection> Skip(int? skip)
        {
            _options.Skip = skip;
            return this;
        }

        public override IFindFluent<TDocument, TProjection> Sort(SortDefinition<TDocument> sort)
        {
            _options.Sort = sort;
            return this;
        }

        public override IAsyncCursor<TProjection> ToCursor(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindSync(_filter, _options, cancellationToken);
        }

        public override Task<IAsyncCursor<TProjection>> ToCursorAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindAsync(_filter, _options, cancellationToken);
        }

        public override string ToString()
        {
            var sb = new StringBuilder("find(");
            var renderedFilter = Render(_filter.Render);
            sb.Append(renderedFilter.ToString());

            if (_options.Projection != null)
            {
                var renderedProjection = Render(_options.Projection.Render);
                if (renderedProjection.Document != null)
                {
                    sb.Append(", " + renderedProjection.Document.ToString());
                }
            }
            sb.Append(")");

            if (_options.Collation != null)
            {
                sb.Append(".collation(" + _options.Collation.ToString() + ")");
            }

            if (_options.Sort != null)
            {
                var renderedSort = Render(_options.Sort.Render);
                sb.Append(".sort(" + renderedSort.ToString() + ")");
            }

            if (_options.Skip.HasValue)
            {
                sb.Append(".skip(" + _options.Skip.Value.ToString() + ")");
            }

            if (_options.Limit.HasValue)
            {
                sb.Append(".limit(" + _options.Limit.Value.ToString() + ")");
            }

            if (_options.MaxTime != null)
            {
                sb.Append(".maxTime(" + _options.MaxTime.Value.TotalMilliseconds + ")");
            }

            if (_options.Comment != null)
            {
                sb.Append("._addSpecial(\"$comment\", \"" + _options.Comment + "\")");
            }

            if (_options.Modifiers != null)
            {
                foreach (var modifier in _options.Modifiers)
                {
                    sb.Append("._addSpecial(\"" + modifier.Name + "\", ");
                    if (modifier.Value.BsonType == BsonType.String)
                    {
                        sb.Append("\"" + modifier.Value.ToString() + "\"");
                    }
                    else
                    {
                        sb.Append(modifier.Value.ToString());
                    }
                    sb.Append(")");
                }
            }

            return sb.ToString();
        }

        // private methods
        private CountOptions CreateCountOptions()
        {
            BsonValue hint = null;
            if (_options.Modifiers != null)
            {
                _options.Modifiers.TryGetValue("$hint", out hint);
            }
            return new CountOptions
            {
                Collation = _options.Collation,
                Hint = hint,
                Limit = _options.Limit,
                MaxTime = _options.MaxTime,
                Skip = _options.Skip
            };
        }

        private TRendered Render<TRendered>(Func<IBsonSerializer<TDocument>, IBsonSerializerRegistry, TRendered> renderer)
        {
            return renderer(_collection.DocumentSerializer, _collection.Settings.SerializerRegistry);
        }
    }
}