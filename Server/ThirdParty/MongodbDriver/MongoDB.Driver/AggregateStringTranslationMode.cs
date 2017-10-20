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
* 
*/

namespace MongoDB.Driver
{
    /// <summary>
    /// Option for which expression to generate for certain string operations.
    /// </summary>
    public enum AggregateStringTranslationMode
    {
        /// <summary>
        /// Translate to the byte variation.
        /// </summary>
        Bytes,
        /// <summary>
        /// Translate to the code points variation. This is only supported in >= MongoDB 3.4.
        /// </summary>
        CodePoints
    }
}
