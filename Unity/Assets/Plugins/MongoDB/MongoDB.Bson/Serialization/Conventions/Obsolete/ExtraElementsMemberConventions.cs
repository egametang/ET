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

using System;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Represents an extra elements member convention.
    /// </summary>
    [Obsolete("Use IClassMapConvention instead.")]
    public interface IExtraElementsMemberConvention
    {
        /// <summary>
        /// Finds the extra elements member of a class.
        /// </summary>
        /// <param name="type">The class.</param>
        /// <returns>The extra elements member.</returns>
        string FindExtraElementsMember(Type type);
    }
}