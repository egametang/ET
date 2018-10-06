/* Copyright 2010-2014 MongoDB Inc.
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
    /// A static class containing methods to convert to and from Guids and byte arrays in various byte orders.
    /// </summary>
    public static class GuidConverter
    {
        /// <summary>
        /// Converts a byte array to a Guid.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="representation">The representation of the Guid in the byte array.</param>
        /// <returns>A Guid.</returns>
        public static Guid FromBytes(byte[] bytes, GuidRepresentation representation)
        {
            if (bytes.Length != 16)
            {
                var message = string.Format("Length of byte array must be 16, not {0}.", bytes.Length);
                throw new ArgumentException(message);
            }
            bytes = (byte[])bytes.Clone();
            switch (representation)
            {
                case GuidRepresentation.CSharpLegacy:
                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes, 0, 4);
                        Array.Reverse(bytes, 4, 2);
                        Array.Reverse(bytes, 6, 2);
                    }
                    break;
                case GuidRepresentation.JavaLegacy:
                    Array.Reverse(bytes, 0, 8);
                    Array.Reverse(bytes, 8, 8);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes, 0, 4);
                        Array.Reverse(bytes, 4, 2);
                        Array.Reverse(bytes, 6, 2);
                    }
                    break;
                case GuidRepresentation.PythonLegacy:
                case GuidRepresentation.Standard:
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes, 0, 4);
                        Array.Reverse(bytes, 4, 2);
                        Array.Reverse(bytes, 6, 2);
                    }
                    break;
                case GuidRepresentation.Unspecified:
                    throw new InvalidOperationException("Unable to convert byte array to Guid because GuidRepresentation is Unspecified.");
                default:
                    throw new BsonInternalException("Unexpected GuidRepresentation.");
            }
            return new Guid(bytes);
        }

        /// <summary>
        /// Converts a Guid to a byte array.
        /// </summary>
        /// <param name="guid">The Guid.</param>
        /// <param name="representation">The representation of the Guid in the byte array.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToBytes(Guid guid, GuidRepresentation representation)
        {
            var bytes = (byte[])guid.ToByteArray().Clone();
            switch (representation)
            {
                case GuidRepresentation.CSharpLegacy:
                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes, 0, 4);
                        Array.Reverse(bytes, 4, 2);
                        Array.Reverse(bytes, 6, 2);
                    }
                    break;
                case GuidRepresentation.JavaLegacy:
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes, 0, 4);
                        Array.Reverse(bytes, 4, 2);
                        Array.Reverse(bytes, 6, 2);
                    }
                    Array.Reverse(bytes, 0, 8);
                    Array.Reverse(bytes, 8, 8);
                    break;
                case GuidRepresentation.PythonLegacy:
                case GuidRepresentation.Standard:
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes, 0, 4);
                        Array.Reverse(bytes, 4, 2);
                        Array.Reverse(bytes, 6, 2);
                    }
                    break;
                case GuidRepresentation.Unspecified:
                    throw new InvalidOperationException("Unable to convert Guid to byte array because GuidRepresentation is Unspecified.");
                default:
                    throw new BsonInternalException("Unexpected GuidRepresentation.");
            }
            return bytes;
        }
    }
}
