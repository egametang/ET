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

#if NET45
using System;
using System.Diagnostics;

namespace MongoDB.Driver.Core.Events.Diagnostics.PerformanceCounters
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class PerformanceCounterAttribute : Attribute
    {
        // private fields
        private readonly string _name;
        private readonly string _help;
        private readonly PerformanceCounterType _type;

        // constructors
        public PerformanceCounterAttribute(string name, string help, PerformanceCounterType type)
        {
            _name = name;
            _help = help;
            _type = type;
        }

        // public properties
        public string Name
        {
            get { return _name; }
        }

        public string Help
        {
            get { return _help; }
        }

        public PerformanceCounterType Type
        {
            get { return _type; }
        }
    }
}
#endif
