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

using System;
using System.Collections.Generic;

namespace MongoDB.Driver.Linq.Translators
{
    internal sealed class ProjectedObject
    {
        private Dictionary<string, object> _values;

        public ProjectedObject()
        {
            _values = new Dictionary<string, object>();
        }

        public void Add(string name, object value)
        {
            _values.Add(name, value);
        }

        public T GetValue<T>(string name, object valueIfNotPresent)
        {
            var result = GetValueAndRemainingKey(name, null, valueIfNotPresent);

            if (result != null)
            {
                var projectedObject = result.Item1 as ProjectedObject;
                if (projectedObject != null && result.Item2 != null)
                {
                    return projectedObject.GetValue<T>(result.Item2, valueIfNotPresent);
                }
                else if (result.Item2 == null)
                {
                    return (T)result.Item1;
                }
            }

            if (valueIfNotPresent is T)
            {
                return (T)valueIfNotPresent;
            }

            return default(T);
        }

        private Tuple<object, string> GetValueAndRemainingKey(string firstKey, string secondKey, object valueIfNotPresent)
        {
            object value;
            if (_values.TryGetValue(firstKey, out value))
            {
                return Tuple.Create(value, secondKey);
            }

            // split up the key by '.' and gradually check each section
            // to see if we have a flattened object stored.
            var parts = firstKey.Split('.');
            string currentKey = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                if (_values.TryGetValue(currentKey, out value))
                {
                    // we check the full thing first, so we'll never 
                    // end up with a string overflow here
                    return Tuple.Create(value, firstKey.Substring(currentKey.Length + 1));
                }

                currentKey += "." + parts[i];
            }

            return null;
        }
    }
}
