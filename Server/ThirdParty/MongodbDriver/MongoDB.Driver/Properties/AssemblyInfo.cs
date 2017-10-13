/* Copyright 2010-2016 MongoDB Inc.
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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MongoDB.Driver")]
[assembly: AssemblyDescription("Official MongoDB supported driver for MongoDB.  See http://www.mongodb.org/display/DOCS/CSharp+Language+Center for more details.")]
[assembly: AssemblyProduct("MongoDB.Driver")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("09134819-2a03-409d-8f38-462bb20e23aa")]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]

[assembly: InternalsVisibleTo("MongoDB.Driver.Legacy")]
[assembly: InternalsVisibleTo("MongoDB.Driver.Legacy.Dotnet")]
[assembly: InternalsVisibleTo("MongoDB.Driver.Legacy.TestHelpers")]
[assembly: InternalsVisibleTo("MongoDB.Driver.Legacy.Tests")]
[assembly: InternalsVisibleTo("MongoDB.Driver.Legacy.Tests.Dotnet")]
[assembly: InternalsVisibleTo("MongoDB.Driver.Tests")]
[assembly: InternalsVisibleTo("MongoDB.Driver.Tests.Dotnet")]
[assembly: InternalsVisibleTo("MongoDB.Driver.TestHelpers")]
