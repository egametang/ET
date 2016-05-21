/* Copyright 2010-2014 MongoDB Inc.
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

using System.Collections.Generic;

namespace MongoDB.Bson.Serialization
{
    internal class MostArgumentsCreatorSelector : ICreatorSelector
    {
        // public methods
        public BsonCreatorMap SelectCreator(BsonClassMap classMap, Dictionary<string, object> values)
        {
            MatchData bestMatch = null;

            foreach (var creatorMap in classMap.CreatorMaps)
            {
                var match = Match(creatorMap, values);
                if (match != null)
                {
                    if (bestMatch == null || IsBetterMatch(match, bestMatch))
                    {
                        bestMatch = match;
                    }
                }
            }

            return (bestMatch == null) ? null : bestMatch.CreatorMap;
        }

        // private methods
        private bool IsBetterMatch(MatchData lhs, MatchData rhs)
        {
            if (lhs.ArgumentCount < rhs.ArgumentCount)
            {
                return false;
            }
            else if (lhs.ArgumentCount > rhs.ArgumentCount)
            {
                return true;
            }
            else if (lhs.DefaultValueCount < rhs.DefaultValueCount)
            {
                return false;
            }
            else if (lhs.DefaultValueCount > rhs.DefaultValueCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private MatchData Match(BsonCreatorMap creatorMap, Dictionary<string, object> values)
        {
            var argumentCount = 0;
            var defaultValueCount = 0;

            // a creator is a match if we have a value for each parameter (either a deserialized value or a default value)
            foreach (var elementName in creatorMap.ElementNames)
            {
                if (values.ContainsKey(elementName))
                {
                    argumentCount++;
                }
                else if (creatorMap.HasDefaultValue(elementName))
                {
                    defaultValueCount++;
                }
                else
                {
                    return null;
                }
            }

            return new MatchData { CreatorMap = creatorMap, ArgumentCount = argumentCount, DefaultValueCount = defaultValueCount };
        }

        // nested classes
        private class MatchData
        {
            public BsonCreatorMap CreatorMap;
            public int ArgumentCount;
            public int DefaultValueCount;
        }
    }
}
