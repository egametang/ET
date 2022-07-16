﻿/* Copyright 2010-present MongoDB Inc.
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

namespace MongoDB.Bson.Serialization.Options
{
    /// <summary>
    /// Represents the units a TimeSpan is serialized in.
    /// </summary>
    public enum TimeSpanUnits
    {
        /// <summary>
        /// Use ticks as the units.
        /// </summary>
        Ticks = 0,
        /// <summary>
        /// Use days as the units.
        /// </summary>
        Days,
        /// <summary>
        /// Use hours as the units.
        /// </summary>
        Hours,
        /// <summary>
        /// Use minutes as the units.
        /// </summary>
        Minutes,
        /// <summary>
        /// Use seconds as the units.
        /// </summary>
        Seconds,
        /// <summary>
        /// Use milliseconds as the units.
        /// </summary>
        Milliseconds,
        /// <summary>
        /// Use microseconds as the units.
        /// </summary>
        Microseconds,
        /// <summary>
        /// Use nanoseconds as the units.
        /// </summary>
        Nanoseconds
    }
}
