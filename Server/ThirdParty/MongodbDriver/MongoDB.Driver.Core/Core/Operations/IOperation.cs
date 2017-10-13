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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Bindings;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a database read operation.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IReadOperation<TResult>
    {
        // methods
        /// <summary>
        /// Executes the operation.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        TResult Execute(IReadBinding binding, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the operation.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the result of the operation.</returns>
        Task<TResult> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents a database write operation.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IWriteOperation<TResult>
    {
        // methods
        /// <summary>
        /// Executes the operation.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        TResult Execute(IWriteBinding binding, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the operation.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the result of the operation.</returns>
        Task<TResult> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken);
    }
}
