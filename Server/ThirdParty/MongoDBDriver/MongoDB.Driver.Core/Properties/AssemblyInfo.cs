/* Copyright 2010-present MongoDB Inc.
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]

[assembly: InternalsVisibleTo("MongoDB.Driver.Core.FunctionalTests")]
[assembly: InternalsVisibleTo("MongoDB.Driver.Core.TestHelpers")]
[assembly: InternalsVisibleTo("MongoDB.Driver.Core.Tests")]
[assembly: InternalsVisibleTo("MongoDB.Driver.Legacy.Tests")]
[assembly: InternalsVisibleTo("MongoDB.Driver.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]