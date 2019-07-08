/* Copyright 2016-present MongoDB Inc.
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

namespace MongoDB.Driver.Core.Misc
{
    /// <summary>
    /// Represents a feature that is not supported by all versions of the server.
    /// </summary>
    public class Feature
    {
        #region static
        private static readonly Feature __aggregate = new Feature("Aggregate", new SemanticVersion(2, 2, 0));
        private static readonly Feature __aggregateAddFields = new Feature("AggregateAddFields", new SemanticVersion(3, 4, 0));
        private static readonly Feature __aggregateAllowDiskUse = new Feature("AggregateAllowDiskUse", new SemanticVersion(2, 6, 0));
        private static readonly Feature __aggregateBucketStage = new Feature("AggregateBucketStage", new SemanticVersion(3, 3, 11));
        private static readonly Feature __aggregateComment = new Feature("AggregateComment", new SemanticVersion(3, 6, 0, "rc0"));
        private static readonly Feature __aggregateCountStage = new Feature("AggregateCountStage", new SemanticVersion(3, 3, 11));
        private static readonly Feature __aggregateCursorResult = new Feature("AggregateCursorResult", new SemanticVersion(2, 6, 0));
        private static readonly Feature __aggregateExplain = new Feature("AggregateExplain", new SemanticVersion(2, 6, 0));
        private static readonly Feature __aggregateFacetStage = new Feature("AggregateFacetStage", new SemanticVersion(3, 4, 0, "rc0"));
        private static readonly Feature __aggregateGraphLookupStage = new Feature("AggregateGraphLookupStage", new SemanticVersion(3, 4, 0, "rc0"));
        private static readonly Feature __aggregateHint = new Feature("AggregateHint", new SemanticVersion(3, 6, 0, "rc0"));
        private static readonly Feature __aggregateLet = new Feature("AggregateLet", new SemanticVersion(3, 6, 0));
        private static readonly Feature __aggregateOut = new Feature("Aggregate", new SemanticVersion(2, 6, 0));
        private static readonly ArrayFiltersFeature __arrayFilters = new ArrayFiltersFeature("ArrayFilters", new SemanticVersion(3, 5, 11));
        private static readonly Feature __bypassDocumentValidation = new Feature("BypassDocumentValidation", new SemanticVersion(3, 2, 0));
        private static readonly Feature __changeStreamStage = new Feature("ChangeStreamStage", new SemanticVersion(3, 5, 11));
        private static readonly CollationFeature __collation = new CollationFeature("Collation", new SemanticVersion(3, 3, 11));
        private static readonly Feature __commandMessage = new Feature("CommandMessage", new SemanticVersion(3, 6, 0));
        private static readonly CommandsThatWriteAcceptWriteConcernFeature __commandsThatWriteAcceptWriteConcern = new CommandsThatWriteAcceptWriteConcernFeature("CommandsThatWriteAcceptWriteConcern", new SemanticVersion(3, 3, 11));
        private static readonly Feature __createIndexesCommand = new Feature("CreateIndexesCommand", new SemanticVersion(3, 0, 0));
        private static readonly Feature __createIndexesUsingInsertOperations = new Feature("CreateIndexesUsingInsertOperations", new SemanticVersion(1, 0, 0), new SemanticVersion(4, 1, 1, ""));
        private static readonly Feature __currentOpCommand = new Feature("CurrentOpCommand", new SemanticVersion(3, 2, 0));
        private static readonly Feature __documentValidation = new Feature("DocumentValidation", new SemanticVersion(3, 2, 0));
        private static readonly Feature __eval = new Feature("Eval", new SemanticVersion(0, 0, 0), new SemanticVersion(4, 1, 0, ""));
        private static readonly Feature __explainCommand = new Feature("ExplainCommand", new SemanticVersion(3, 0, 0));
        private static readonly Feature __failPoints = new Feature("FailPoints", new SemanticVersion(2, 4, 0));
        private static readonly Feature __findAndModifyWriteConcern = new Feature("FindAndModifyWriteConcern", new SemanticVersion(3, 2, 0));
        private static readonly Feature __findCommand = new Feature("FindCommand", new SemanticVersion(3, 2, 0));
        private static readonly Feature __geoNearCommand = new Feature("GeoNearCommand", new SemanticVersion(1, 0, 0), new SemanticVersion(4, 1, 0, ""));
        private static readonly Feature __groupCommand = new Feature("GroupCommand", new SemanticVersion(1, 0, 0), new SemanticVersion(4, 0, 0, "rc1"));
        private static readonly Feature __killCursorsCommand = new Feature("KillCursorsCommand", new SemanticVersion(3, 2, 0));
        private static readonly Feature __listCollectionsCommand = new Feature("ListCollectionsCommand", new SemanticVersion(3, 0, 0));
        private static readonly Feature __listDatabasesFilter = new Feature("ListDatabasesFilter", new SemanticVersion(3, 4, 2));
        private static readonly Feature __listDatabasesNameOnlyOption = new Feature("ListDatabasesNameOnlyOption", new SemanticVersion(3, 4, 3));
        private static readonly Feature __listIndexesCommand = new Feature("ListIndexesCommand", new SemanticVersion(3, 0, 0));
        private static readonly Feature __indexOptionsDefaults = new Feature("IndexOptionsDefaults", new SemanticVersion(3, 2, 0));
        private static readonly Feature __maxStaleness = new Feature("MaxStaleness", new SemanticVersion(3, 3, 12));
        private static readonly Feature __maxTime = new Feature("MaxTime", new SemanticVersion(2, 6, 0));
        private static readonly Feature __mmapV1StorageEngine = new Feature("MmapV1StorageEngine", new SemanticVersion(0, 0, 0), new SemanticVersion(4, 1, 0, ""));
        private static readonly Feature __partialIndexes = new Feature("PartialIndexes", new SemanticVersion(3, 2, 0));
        private static readonly ReadConcernFeature __readConcern = new ReadConcernFeature("ReadConcern", new SemanticVersion(3, 2, 0));
        private static readonly Feature __scramSha1Authentication = new Feature("ScramSha1Authentication", new SemanticVersion(3, 0, 0));
        private static readonly Feature __scramSha256Authentication = new Feature("ScramSha256Authentication", new SemanticVersion(4, 0, 0, ""));
        private static readonly Feature __serverExtractsUsernameFromX509Certificate = new Feature("ServerExtractsUsernameFromX509Certificate", new SemanticVersion(3, 3, 12));
        private static readonly Feature __transactions = new Feature("Transactions", new SemanticVersion(3, 7, 0));
        private static readonly Feature __userManagementCommands = new Feature("UserManagementCommands", new SemanticVersion(2, 6, 0));
        private static readonly Feature __views = new Feature("Views", new SemanticVersion(3, 3, 11));
        private static readonly Feature __writeCommands = new Feature("WriteCommands", new SemanticVersion(2, 6, 0));

        /// <summary>
        /// Gets the aggregate feature.
        /// </summary>
        public static Feature Aggregate => __aggregate;

        /// <summary>
        /// Gets the aggregate AddFields feature.
        /// </summary>
        public static Feature AggregateAddFields => __aggregateAddFields;

        /// <summary>
        /// Gets the aggregate allow disk use feature.
        /// </summary>
        public static Feature AggregateAllowDiskUse => __aggregateAllowDiskUse;

        /// <summary>
        /// Gets the aggregate bucket stage feature.
        /// </summary>
        public static Feature AggregateBucketStage => __aggregateBucketStage;

        /// <summary>
        /// Gets the aggregate comment feature.
        /// </summary>
        public static Feature AggregateComment => __aggregateComment;

        /// <summary>
        /// Gets the aggregate count stage feature.
        /// </summary>
        public static Feature AggregateCountStage => __aggregateCountStage;

        /// <summary>
        /// Gets the aggregate cursor result feature.
        /// </summary>
        public static Feature AggregateCursorResult => __aggregateCursorResult;

        /// <summary>
        /// Gets the aggregate explain feature.
        /// </summary>
        public static Feature AggregateExplain => __aggregateExplain;

        /// <summary>
        /// Gets the aggregate $facet stage feature.
        /// </summary>
        public static Feature AggregateFacetStage => __aggregateFacetStage;

        /// <summary>
        /// Gets the aggregate $graphLookup stage feature.
        /// </summary>
        public static Feature AggregateGraphLookupStage => __aggregateGraphLookupStage;

        /// <summary>
        /// Gets the aggregate hint feature.
        /// </summary>
        public static Feature AggregateHint => __aggregateHint;

        /// <summary>
        /// Gets the aggregate let feature.
        /// </summary>
        public static Feature AggregateLet => __aggregateLet;

        /// <summary>
        /// Gets the aggregate out feature.
        /// </summary>
        public static Feature AggregateOut => __aggregateOut;

        /// <summary>
        /// Gets the arrayFilters feature.
        /// </summary>
        public static ArrayFiltersFeature ArrayFilters => __arrayFilters;

        /// <summary>
        /// Gets the bypass document validation feature.
        /// </summary>
        public static Feature BypassDocumentValidation => __bypassDocumentValidation;

        /// <summary>
        /// Gets the aggregate $changeStream stage feature.
        /// </summary>
        public static Feature ChangeStreamStage => __changeStreamStage;

        /// <summary>
        /// Gets the collation feature.
        /// </summary>
        public static CollationFeature Collation => __collation;

        /// <summary>
        /// Gets the command message feature.
        /// </summary>
        public static Feature CommandMessage => __commandMessage;

        /// <summary>
        /// Gets the commands that write accept write concern feature.
        /// </summary>
        public static CommandsThatWriteAcceptWriteConcernFeature CommandsThatWriteAcceptWriteConcern => __commandsThatWriteAcceptWriteConcern;

        /// <summary>
        /// Gets the create indexes command feature.
        /// </summary>
        public static Feature CreateIndexesCommand => __createIndexesCommand;

        /// <summary>
        /// Gets the create indexes using insert operations feature.
        /// </summary>
        public static Feature CreateIndexesUsingInsertOperations => __createIndexesUsingInsertOperations;

        /// <summary>
        /// Gets the current op command feature.
        /// </summary>
        public static Feature CurrentOpCommand => __currentOpCommand;

        /// <summary>
        /// Gets the document validation feature.
        /// </summary>
        public static Feature DocumentValidation => __documentValidation;

        /// <summary>
        /// Gets the eval feature.
        /// </summary>
        public static Feature Eval => __eval;

        /// <summary>
        /// Gets the explain command feature.
        /// </summary>
        public static Feature ExplainCommand => __explainCommand;

        /// <summary>
        /// Gets the fail points feature.
        /// </summary>
        public static Feature FailPoints => __failPoints;

        /// <summary>
        /// Gets the find and modify write concern feature.
        /// </summary>
        public static Feature FindAndModifyWriteConcern => __findAndModifyWriteConcern;

        /// <summary>
        /// Gets the find command feature.
        /// </summary>
        public static Feature FindCommand => __findCommand;

        /// <summary>
        /// Gets the geoNear command feature.
        /// </summary>
        public static Feature GeoNearCommand => __geoNearCommand;

        /// <summary>
        /// Gets the group command feature.
        /// </summary>
        public static Feature GroupCommand => __groupCommand;

        /// <summary>
        /// Get the killCursors command feature.
        /// </summary>
        public static Feature KillCursorsCommand => __killCursorsCommand;

        /// <summary>
        /// Gets the index options defaults feature.
        /// </summary>
        public static Feature IndexOptionsDefaults => __indexOptionsDefaults;

        /// <summary>
        /// Gets the list databases filter feature.
        /// </summary>
        public static Feature ListDatabasesFilter => __listDatabasesFilter;

        /// <summary>
        /// Get the list databases nameOnly feature.
        /// </summary>
        public static Feature ListDatabasesNameOnlyOption => __listDatabasesNameOnlyOption;

        /// <summary>
        /// Gets the list collections command feature.
        /// </summary>
        public static Feature ListCollectionsCommand => __listCollectionsCommand;

        /// <summary>
        /// Gets the list indexes command feature.
        /// </summary>
        public static Feature ListIndexesCommand => __listIndexesCommand;

        /// <summary>
        /// Gets the maximum staleness feature.
        /// </summary>
        public static Feature MaxStaleness => __maxStaleness;

        /// <summary>
        /// Gets the maximum time feature.
        /// </summary>
        public static Feature MaxTime => __maxTime;

        /// <summary>
        /// Gets the mmapv1 storage engine feature.
        /// </summary>
        public static Feature MmapV1StorageEngine => __mmapV1StorageEngine;

        /// <summary>
        /// Gets the partial indexes feature.
        /// </summary>
        public static Feature PartialIndexes => __partialIndexes;

        /// <summary>
        /// Gets the read concern feature.
        /// </summary>
        public static ReadConcernFeature ReadConcern => __readConcern;

        /// <summary>
        /// Gets the scram sha1 authentication feature.
        /// </summary>
        public static Feature ScramSha1Authentication => __scramSha1Authentication;

        /// <summary>
        /// Gets the scram sha256 authentication feature.
        /// </summary>
        public static Feature ScramSha256Authentication => __scramSha256Authentication;

        /// <summary>
        /// Gets the server extracts username from X509 certificate feature.
        /// </summary>
        public static Feature ServerExtractsUsernameFromX509Certificate => __serverExtractsUsernameFromX509Certificate;

        /// <summary>
        /// Gets the transactions feature.
        /// </summary>
        public static Feature Transactions => __transactions;

        /// <summary>
        /// Gets the user management commands feature.
        /// </summary>
        public static Feature UserManagementCommands => __userManagementCommands;

        /// <summary>
        /// Gets the views feature.
        /// </summary>
        public static Feature Views => __views;

        /// <summary>
        /// Gets the write commands feature.
        /// </summary>
        public static Feature WriteCommands => __writeCommands;
        #endregion

        private readonly string _name;
        private readonly SemanticVersion _firstSupportedVersion;
        private readonly SemanticVersion _supportRemovedVersion;


        /// <summary>
        /// Initializes a new instance of the <see cref="Feature"/> class.
        /// </summary>
        /// <param name="name">The name of the feature.</param>
        /// <param name="firstSupportedVersion">The first server version that supports the feature.</param>
        /// /// <param name="supportRemovedVersion">The server version that stops support the feature.</param>
        public Feature(string name, SemanticVersion firstSupportedVersion, SemanticVersion supportRemovedVersion = null)
        {
            _name = name;
            _firstSupportedVersion = firstSupportedVersion;
            _supportRemovedVersion = supportRemovedVersion;
        }

        /// <summary>
        /// Gets the name of the feature.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets the first server version that supports the feature.
        /// </summary>
        public SemanticVersion FirstSupportedVersion => _firstSupportedVersion;

        /// <summary>
        /// Gets the last server version that does not support the feature.
        /// </summary>
        public SemanticVersion LastNotSupportedVersion => VersionBefore(_firstSupportedVersion);

        /// <summary>
        /// Determines whether a feature is supported by a version of the server.
        /// </summary>
        /// <param name="serverVersion">The server version.</param>
        /// <returns>Whether a feature is supported by a version of the server.</returns>
        public bool IsSupported(SemanticVersion serverVersion)
        {
            return _supportRemovedVersion != null
                   ? serverVersion >= _firstSupportedVersion && serverVersion < _supportRemovedVersion
                   : serverVersion >= _firstSupportedVersion;
        }

        /// <summary>
        /// Returns a version of the server where the feature is or is not supported.
        /// </summary>
        /// <param name="isSupported">Whether the feature is supported or not.</param>
        /// <returns>A version of the server where the feature is or is not supported.</returns>
        public SemanticVersion SupportedOrNotSupportedVersion(bool isSupported)
        {
            return isSupported ? _firstSupportedVersion : VersionBefore(_firstSupportedVersion);
        }

        /// <summary>
        /// Throws if the feature is not supported by a version of the server.
        /// </summary>
        /// <param name="serverVersion">The server version.</param>
        public void ThrowIfNotSupported(SemanticVersion serverVersion)
        {
            if (!IsSupported(serverVersion))
            {
                throw new NotSupportedException($"Server version {serverVersion} does not support the {_name} feature.");
            }
        }

        private SemanticVersion VersionBefore(SemanticVersion version)
        {
            if (version.Patch > 0)
            {
                return new SemanticVersion(version.Major, version.Minor, version.Patch - 1);
            }
            else if (version.Minor > 0)
            {
                return new SemanticVersion(version.Major, version.Minor - 1, 99);
            }
            else if (version.Major > 0)
            {
                return new SemanticVersion(version.Major - 1, 99, 99);
            }
            else
            {
                throw new ArgumentException("There is no version before 0.0.0.", nameof(version));
            }
        }
    }
}
