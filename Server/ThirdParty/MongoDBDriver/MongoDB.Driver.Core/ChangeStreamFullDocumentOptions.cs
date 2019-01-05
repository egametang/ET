/* Copyright 2017-present MongoDB Inc.
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

namespace MongoDB.Driver
{
    /// <summary>
    /// Change stream FullDocument option.
    /// </summary>
    public enum ChangeStreamFullDocumentOption
    {
        /// <summary>
        /// Do not return the full document.
        /// </summary>
        Default = 0,
        /// <summary>
        /// The change stream for partial updates will include both a delta describing the
        /// changes to the document as well as a copy of the entire document that was
        /// changed from some time after the change occurred.
        /// </summary>
        UpdateLookup
    }
}
