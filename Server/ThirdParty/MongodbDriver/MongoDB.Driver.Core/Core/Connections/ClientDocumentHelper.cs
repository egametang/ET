/* Copyright 2016-2017 MongoDB Inc.
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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MongoDB.Bson;

namespace MongoDB.Driver.Core.Connections
{
    internal class ClientDocumentHelper
    {
        #region static
        // private static fields
        private static readonly Lazy<BsonDocument> __driverDocument = new Lazy<BsonDocument>(CreateDriverDocument);
        private static readonly Lazy<BsonDocument> __osDocument = new Lazy<BsonDocument>(CreateOSDocument);
        private static readonly Lazy<string> __platformString = new Lazy<string>(GetPlatformString);

        // private static methods
        internal static BsonDocument CreateClientDocument(string applicationName)
        {
            return CreateClientDocument(applicationName, __driverDocument.Value, __osDocument.Value, __platformString.Value);
        }

        internal static BsonDocument CreateClientDocument(string applicationName, BsonDocument driverDocument, BsonDocument osDocument, string platformString)
        {
            var clientDocument = new BsonDocument
            {
                { "application", () => new BsonDocument("name", applicationName), applicationName != null },
                { "driver", driverDocument },
                { "os", osDocument.Clone() }, // clone because we might be removing optional fields from this particular clientDocument
                { "platform", platformString }
            };

            return RemoveOptionalFieldsUntilDocumentIsLessThan512Bytes(clientDocument);
        }

        internal static BsonDocument CreateDriverDocument()
        {
            var assembly = typeof(ConnectionInitializer).GetTypeInfo().Assembly;
            var fileVersionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            var driverVersion = fileVersionAttribute.Version;

            return CreateDriverDocument(driverVersion);
        }

        internal static BsonDocument CreateDriverDocument(string driverVersion)
        {
            return new BsonDocument
            {
                { "name", "mongo-csharp-driver" },
                { "version", driverVersion }
            };
        }

        internal static BsonDocument CreateOSDocument()
        {
            string osType;
            string osName;
            string architecture;
            string osVersion;

#if NET45
            if (Type.GetType("Mono.Runtime") != null)
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        osType = "Windows";
                        break;

                    case PlatformID.Unix:
                        osType = "Linux";
                        break;

                    case PlatformID.Xbox:
                        osType = "XBox";
                        break;

                    case PlatformID.MacOSX:
                        osType = "macOS";
                        break;

                    default:
                        osType = "unknown";
                        break;
                }

                osName = Environment.OSVersion.VersionString;

                PortableExecutableKinds peKind;
                ImageFileMachine machine;
                typeof(object).Module.GetPEKind(out peKind, out machine);
                switch (machine)
                {
                    case ImageFileMachine.I386:
                        architecture = "x86_32";
                        break;
                    case ImageFileMachine.IA64:
                    case ImageFileMachine.AMD64:
                        architecture = "x86_64";
                        break;
                    case ImageFileMachine.ARM:
                        architecture = "arm" + (Environment.Is64BitProcess ? "64" : "");
                        break;
                    default:
                        architecture = null;
                        break;
                }

                osVersion = Environment.OSVersion.Version.ToString();

                return CreateOSDocument(osType, osName, architecture, osVersion);
            }
#endif

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                osType = "Windows";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                osType = "Linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                osType = "macOS";
            }
            else
            {
                osType = "unknown";
            }

            osName = RuntimeInformation.OSDescription.Trim();

            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm: architecture = "arm"; break;
                case Architecture.Arm64: architecture = "arm64"; break;
                case Architecture.X64: architecture = "x86_64"; break;
                case Architecture.X86: architecture = "x86_32"; break;
                default: architecture = null; break;
            }

            var match = Regex.Match(osName, @" (?<version>\d+\.\d[^ ]*)");
            if (match.Success)
            {
                osVersion = match.Groups["version"].Value;
            }
            else
            {
                osVersion = null;
            }

            return CreateOSDocument(osType, osName, architecture, osVersion);
        }

        internal static BsonDocument CreateOSDocument(string osType, string osName, string architecture, string osVersion)
        {
            return new BsonDocument
            {
                { "type", osType },
                { "name", osName },
                { "architecture", architecture, architecture != null },
                { "version", osVersion, osVersion != null }
            };
        }

        internal static string GetPlatformString()
        {
            return RuntimeInformation.FrameworkDescription;
        }

        internal static BsonDocument RemoveOneOptionalField(BsonDocument clientDocument)
        {
            if (clientDocument.Contains("application"))
            {
                clientDocument.Remove("application");
                return clientDocument;
            }

            var os = clientDocument["os"].AsBsonDocument;
            if (os.Contains("version"))
            {
                os.Remove("version");
                return clientDocument;
            }
            if (os.Contains("architecture"))
            {
                os.Remove("architecture");
                return clientDocument;
            }
            if (os.Contains("name"))
            {
                os.Remove("name");
                return clientDocument;
            }

            if (clientDocument.Contains("platform"))
            {
                clientDocument.Remove("platform");
                return clientDocument;
            }

            // no optional fields left to remove
            return null;
        }

        internal static BsonDocument RemoveOptionalFieldsUntilDocumentIsLessThan512Bytes(BsonDocument clientDocument)
        {
            while (clientDocument != null && clientDocument.ToBson().Length > 512)
            {
                clientDocument = RemoveOneOptionalField(clientDocument);
            }

            return clientDocument;
        }
        #endregion

    }
}
