/* Copyright 2013-2016 MongoDB Inc.
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
using System.Diagnostics;
using System.Threading;

namespace MongoDB.Driver.Core.Misc
{
    /// <summary>
    /// Represents methods that can be used to ensure that parameter values meet expected conditions.
    /// </summary>
    [DebuggerStepThrough]
    public static class Ensure
    {
        /// <summary>
        /// Ensures that the value of a parameter is between a minimum and a maximum value.
        /// </summary>
        /// <typeparam name="T">Type type of the value.</typeparam>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static T IsBetween<T>(T value, T min, T max, string paramName) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            {
                var message = string.Format("Value is not between {1} and {2}: {0}.", value, min, max);
                throw new ArgumentOutOfRangeException(paramName, message);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is equal to a comparand.
        /// </summary>
        /// <typeparam name="T">Type type of the value.</typeparam>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="comparand">The comparand.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static T IsEqualTo<T>(T value, T comparand, string paramName)
        {
            if (!value.Equals(comparand))
            {
                var message = string.Format("Value is not equal to {1}: {0}.", value, comparand);
                throw new ArgumentException(message, paramName);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is greater than or equal to a comparand.
        /// </summary>
        /// <typeparam name="T">Type type of the value.</typeparam>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="comparand">The comparand.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static T IsGreaterThanOrEqualTo<T>(T value, T comparand, string paramName) where T : IComparable<T>
        {
            if (value.CompareTo(comparand) < 0)
            {
                var message = string.Format("Value is not greater than or equal to {1}: {0}.", value, comparand);
                throw new ArgumentOutOfRangeException(paramName, message);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is greater than or equal to zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static int IsGreaterThanOrEqualToZero(int value, string paramName)
        {
            if (value < 0)
            {
                var message = string.Format("Value is not greater than or equal to 0: {0}.", value);
                throw new ArgumentOutOfRangeException(paramName, message);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is greater than or equal to zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static long IsGreaterThanOrEqualToZero(long value, string paramName)
        {
            if (value < 0)
            {
                var message = string.Format("Value is not greater than or equal to 0: {0}.", value);
                throw new ArgumentOutOfRangeException(paramName, message);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is greater than or equal to zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static TimeSpan IsGreaterThanOrEqualToZero(TimeSpan value, string paramName)
        {
            if (value < TimeSpan.Zero)
            {
                var message = string.Format("Value is not greater than or equal to zero: {0}.", TimeSpanParser.ToString(value));
                throw new ArgumentOutOfRangeException(paramName, message);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is greater than zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static int IsGreaterThanZero(int value, string paramName)
        {
            if (value <= 0)
            {
                var message = string.Format("Value is not greater than zero: {0}.", value);
                throw new ArgumentOutOfRangeException(paramName, message);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is greater than zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static long IsGreaterThanZero(long value, string paramName)
        {
            if (value <= 0)
            {
                var message = string.Format("Value is not greater than zero: {0}.", value);
                throw new ArgumentOutOfRangeException(paramName, message);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is greater than zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static TimeSpan IsGreaterThanZero(TimeSpan value, string paramName)
        {
            if (value <= TimeSpan.Zero)
            {
                var message = string.Format("Value is not greater than zero: {0}.", value);
                throw new ArgumentOutOfRangeException(paramName, message);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is infinite or greater than or equal to zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static TimeSpan IsInfiniteOrGreaterThanOrEqualToZero(TimeSpan value, string paramName)
        {
            if (value < TimeSpan.Zero && value != Timeout.InfiniteTimeSpan)
            {
                var message = string.Format("Value is not infinite or greater than or equal to zero: {0}.", TimeSpanParser.ToString(value));
                throw new ArgumentOutOfRangeException(paramName, message);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is infinite or greater than zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static TimeSpan IsInfiniteOrGreaterThanZero(TimeSpan value, string paramName)
        {
            if (value == Timeout.InfiniteTimeSpan || value > TimeSpan.Zero)
            {
                return value;
            }
            var message = string.Format("Value is not infinite or greater than zero: {0}.", TimeSpanParser.ToString(value));
            throw new ArgumentOutOfRangeException(paramName, message);
        }

        /// <summary>
        /// Ensures that the value of a parameter is not null.
        /// </summary>
        /// <typeparam name="T">Type type of the value.</typeparam>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static T IsNotNull<T>(T value, string paramName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName, "Value cannot be null.");
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is not null or empty.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static string IsNotNullOrEmpty(string value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (value.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty.", paramName);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is null.
        /// </summary>
        /// <typeparam name="T">Type type of the value.</typeparam>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static T IsNull<T>(T value, string paramName) where T : class
        {
            if (value != null)
            {
                throw new ArgumentNullException(paramName, "Value must be null.");
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is null or greater than or equal to zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static int? IsNullOrGreaterThanOrEqualToZero(int? value, string paramName)
        {
            if (value != null)
            {
                IsGreaterThanOrEqualToZero(value.Value, paramName);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is null or greater than or equal to zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static long? IsNullOrGreaterThanOrEqualToZero(long? value, string paramName)
        {
            if (value != null)
            {
                IsGreaterThanOrEqualToZero(value.Value, paramName);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is null or greater than zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static int? IsNullOrGreaterThanZero(int? value, string paramName)
        {
            if (value != null)
            {
                IsGreaterThanZero(value.Value, paramName);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is null or greater than zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static long? IsNullOrGreaterThanZero(long? value, string paramName)
        {
            if (value != null)
            {
                IsGreaterThanZero(value.Value, paramName);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is null or greater than zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static TimeSpan? IsNullOrGreaterThanZero(TimeSpan? value, string paramName)
        {
            if (value != null)
            {
                IsGreaterThanZero(value.Value, paramName);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is null, or infinite, or greater than or equal to zero.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static TimeSpan? IsNullOrInfiniteOrGreaterThanOrEqualToZero(TimeSpan? value, string paramName)
        {
            if (value.HasValue && value.Value < TimeSpan.Zero && value.Value != Timeout.InfiniteTimeSpan)
            {
                var message = string.Format("Value is not null or infinite or greater than or equal to zero: {0}.", TimeSpanParser.ToString(value.Value));
                throw new ArgumentOutOfRangeException(paramName, message);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is null or not empty.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static string IsNullOrNotEmpty(string value, string paramName)
        {
            if (value != null && value == "")
            {
                throw new ArgumentException("Value cannot be empty.", paramName);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is null or a valid timeout.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static TimeSpan? IsNullOrValidTimeout(TimeSpan? value, string paramName)
        {
            if (value != null)
            {
                IsValidTimeout(value.Value, paramName);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the value of a parameter is a valid timeout.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static TimeSpan IsValidTimeout(TimeSpan value, string paramName)
        {
            if (value < TimeSpan.Zero && value != Timeout.InfiniteTimeSpan)
            {
                var message = string.Format("Invalid timeout: {0}.", value);
                throw new ArgumentException(message, paramName);
            }
            return value;
        }

        /// <summary>
        /// Ensures that an assertion is true.
        /// </summary>
        /// <param name="assertion">The assertion.</param>
        /// <param name="message">The message to use with the exception that is thrown if the assertion is false.</param>
        public static void That(bool assertion, string message)
        {
            if (!assertion)
            {
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Ensures that an assertion is true.
        /// </summary>
        /// <param name="assertion">The assertion.</param>
        /// <param name="message">The message to use with the exception that is thrown if the assertion is false.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void That(bool assertion, string message, string paramName)
        {
            if (!assertion)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        /// <summary>
        /// Ensures that the value of a parameter meets an assertion.
        /// </summary>
        /// <typeparam name="T">Type type of the value.</typeparam>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="assertion">The assertion.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="message">The message to use with the exception that is thrown if the assertion is false.</param>
        /// <returns>The value of the parameter.</returns>
        public static T That<T>(T value, Func<T, bool> assertion, string paramName, string message)
        {
            if (!assertion(value))
            {
                throw new ArgumentException(message, paramName);
            }

            return value;
        }
    }
}
