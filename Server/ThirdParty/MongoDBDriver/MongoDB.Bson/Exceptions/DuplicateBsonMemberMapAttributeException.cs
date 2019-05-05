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

using System;

namespace MongoDB.Bson
{
    /// <summary>
    /// Indicates that an attribute restricted to one member has been applied to multiple members.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class DuplicateBsonMemberMapAttributeException : BsonException
    {
        // constructors 
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateBsonMemberMapAttributeException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public DuplicateBsonMemberMapAttributeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateBsonMemberMapAttributeException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public DuplicateBsonMemberMapAttributeException(string message, Exception inner)
            : base(message, inner)
        {
        }

#if NET452
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateBsonMemberMapAttributeException" /> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected DuplicateBsonMemberMapAttributeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
