/* Copyright 2013-2015 MongoDB Inc.
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.Misc
{
    internal static class TimeSpanParser
    {
        // methods
        public static string ToString(TimeSpan value)
        {
            const int msInOneSecond = 1000;
            const int msInOneMinute = 60 * msInOneSecond;
            const int msInOneHour = 60 * msInOneMinute;

            var ms = (long)value.TotalMilliseconds;
            if ((ms % msInOneHour) == 0)
            {
                return string.Format("{0}h", ms / msInOneHour);
            }
            else if ((ms % msInOneMinute) == 0 && ms < msInOneHour)
            {
                return string.Format("{0}m", ms / msInOneMinute);
            }
            else if ((ms % msInOneSecond) == 0 && ms < msInOneMinute)
            {
                return string.Format("{0}s", ms / msInOneSecond);
            }
            else if (ms < 1000)
            {
                return string.Format("{0}ms", ms);
            }
            else
            {
                return value.ToString();
            }
        }

        public static bool TryParse(string value, out TimeSpan result)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.ToLowerInvariant();
                var end = value.Length - 1;

                var multiplier = 1000; // default units are seconds
                if (value[end] == 's')
                {
                    if (value[end - 1] == 'm')
                    {
                        value = value.Substring(0, value.Length - 2);
                        multiplier = 1;
                    }
                    else
                    {
                        value = value.Substring(0, value.Length - 1);
                        multiplier = 1000;
                    }
                }
                else if (value[end] == 'm')
                {
                    value = value.Substring(0, value.Length - 1);
                    multiplier = 60 * 1000;
                }
                else if (value[end] == 'h')
                {
                    value = value.Substring(0, value.Length - 1);
                    multiplier = 60 * 60 * 1000;
                }
                else if (value.IndexOf(':') != -1)
                {
                    return TimeSpan.TryParse(value, out result);
                }

                double multiplicand;
                var numberStyles = NumberStyles.None;
                if (double.TryParse(value, numberStyles, CultureInfo.InvariantCulture, out multiplicand))
                {
                    result = TimeSpan.FromMilliseconds(multiplicand * multiplier);
                    return true;
                }
            }

            result = default(TimeSpan);
            return false;
        }
    }
}
