/* Copyright 2016 MongoDB Inc.
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

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents the granularity value for a $bucketAuto stage.
    /// </summary>
    public struct AggregateBucketAutoGranularity
    {
        #region static
        /// <summary>
        /// Gets the E6 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity E6 => new AggregateBucketAutoGranularity("E6");

        /// <summary>
        /// Gets the E12 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity E12 => new AggregateBucketAutoGranularity("E12");

        /// <summary>
        /// Gets the E24 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity E24 => new AggregateBucketAutoGranularity("E24");

        /// <summary>
        /// Gets the E48 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity E48 => new AggregateBucketAutoGranularity("E48");

        /// <summary>
        /// Gets the E96 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity E96 => new AggregateBucketAutoGranularity("E96");

        /// <summary>
        /// Gets the E192 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity E192 => new AggregateBucketAutoGranularity("E192");

        /// <summary>
        /// Gets the POWERSOF2 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity PowersOf2 => new AggregateBucketAutoGranularity("POWERSOF2");

        /// <summary>
        /// Gets the R5 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity R5 => new AggregateBucketAutoGranularity("R5");

        /// <summary>
        /// Gets the R10 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity R10 => new AggregateBucketAutoGranularity("R10");

        /// <summary>
        /// Gets the R20 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity R20 => new AggregateBucketAutoGranularity("R20");

        /// <summary>
        /// Gets the R40 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity R40 => new AggregateBucketAutoGranularity("R40");

        /// <summary>
        /// Gets the R80 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity R80 => new AggregateBucketAutoGranularity("R80");

        /// <summary>
        /// Gets the 1-2-5 granularity.
        /// </summary>
        public static AggregateBucketAutoGranularity S1_2_5 => new AggregateBucketAutoGranularity("1-2-5");

        #endregion

        private readonly string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateBucketAutoGranularity"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public AggregateBucketAutoGranularity(string value)
        {
            _value = Ensure.IsNotNull(value, nameof(value));
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value => _value;
    }
}
