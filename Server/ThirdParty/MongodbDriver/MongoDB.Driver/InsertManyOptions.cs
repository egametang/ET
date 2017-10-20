/* Copyright 2010-2015 MongoDB Inc.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Driver
{
    /// <summary>
    /// Options for inserting many documents.
    /// </summary>
    public sealed class InsertManyOptions
    {
        // fields
        private bool? _bypassDocumentValidation;
        private bool _isOrdered;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertManyOptions"/> class.
        /// </summary>
        public InsertManyOptions()
        {
            _isOrdered = true;
        }

        // properties
        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        public bool? BypassDocumentValidation
        {
            get { return _bypassDocumentValidation; }
            set { _bypassDocumentValidation = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the requests are fulfilled in order.
        /// </summary>
        public bool IsOrdered
        {
            get { return _isOrdered; }
            set { _isOrdered = value; }
        }
    }
}
