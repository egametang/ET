/* Copyright 2018-present MongoDB Inc.
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
using MongoDB.Bson;

namespace MongoDB.Driver.Core.WireProtocol.Messages
{
    internal sealed class CommandMessageDisposer : IDisposable
    {
        // private fields
        private CommandMessage _message;

        // constructors
        public CommandMessageDisposer(CommandMessage message)
        {
            _message = message;
        }

        // public methods
        public void Dispose()
        {
            foreach (var section in _message.Sections)
            {
                DisposeSection(section);
            }
        }

        // private methods
        private void DisposeSection(CommandMessageSection section)
        {
            switch (section.PayloadType)
            {
                case PayloadType.Type0:
                    DisposeType0Section((Type0CommandMessageSection<RawBsonDocument>)section);
                    break;

                case PayloadType.Type1:
                    DisposeType1Section((Type1CommandMessageSection<RawBsonDocument>)section);
                    break;
            }
        }

        private void DisposeType0Section(Type0CommandMessageSection<RawBsonDocument> section)
        {
            section.Document.Dispose();
        }

        private void DisposeType1Section(Type1CommandMessageSection<RawBsonDocument> section)
        {
            foreach (var document in section.Documents.GetBatchItems())
            {
                document.Dispose();
            }
        }
    }
}
