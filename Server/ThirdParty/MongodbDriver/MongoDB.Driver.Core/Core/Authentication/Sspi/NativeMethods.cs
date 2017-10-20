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
#if NET45
using System.Runtime.ConstrainedExecution;
#endif
using System.Runtime.InteropServices;

namespace MongoDB.Driver.Core.Authentication.Sspi
{
    internal static class NativeMethods
    {
        // public constants
        public const int MAX_TOKEN_SIZE = 12288;

        public const long SEC_E_OK = 0x0;
        public const long SEC_E_INSUFFICENT_MEMORY = 0x80090300;
        public const long SEC_E_INVALID_HANDLE = 0x80090301;
        public const long SEC_E_TARGET_UNKNOWN = 0x80090303;
        public const long SEC_E_INTERNAL_ERROR = 0x80090304;
        public const long SEC_E_SECPKG_NOT_FOUND = 0x80090305;
        public const long SEC_E_INVALID_TOKEN = 0x80090308;
        public const long SEC_E_QOP_NOT_SUPPORTED = 0x8009030A;
        public const long SEC_E_LOGON_DENIED = 0x8009030C;
        public const long SEC_E_UNKNOWN_CREDENTIALS = 0x8009030D;
        public const long SEC_E_NO_CREDENTIALS = 0x8009030E;
        public const long SEC_E_MESSAGE_ALTERED = 0x8009030F;
        public const long SEC_E_OUT_OF_SEQUENCE = 0x80090310;
        public const long SEC_E_NO_AUTHENTICATING_AUTHORITY = 0x80090311;
        public const long SEC_E_CONTEXT_EXPIRED = 0x80090317;
        public const long SEC_E_INCOMPLETE_MESSAGE = 0x80090318;
        public const long SEC_E_BUFFER_TOO_SMALL = 0x80090321;
        public const long SEC_E_CRYPTO_SYSTEM_INVALID = 0x80090337;
        
        public const long SEC_I_CONTINUE_NEEDED = 0x00090312;
        public const long SEC_I_CONTEXT_EXPIRED = 0x00090317;
        public const long SEC_I_RENEGOTIATE = 0x00090321;

        /// <summary>
        /// Creates an exception for the specified error code.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="defaultMessage">The default message.</param>
        /// <returns></returns>
        public static Win32Exception CreateException(long errorCode, string defaultMessage)
        {
            string message = defaultMessage;
            switch (errorCode)
            {
                case SEC_E_BUFFER_TOO_SMALL:
                    message = "The message buffer is too small. Used with the Digest SSP.";
                    break;
                case SEC_E_CONTEXT_EXPIRED:
                    message = "The application is referencing a context that has already been closed.";
                    break;
                case SEC_E_CRYPTO_SYSTEM_INVALID:
                    message = "The cipher chosen for the security context is not supported. Used with the Digest SSP.";
                    break;
                case SEC_E_INCOMPLETE_MESSAGE:
                    message = "The data in the input buffer is incomplete.";
                    break;
                case SEC_E_INSUFFICENT_MEMORY:
                    message = "There is not enough memory available to complete the requested action.";
                    break;
                case SEC_E_INTERNAL_ERROR:
                    message = "An error occurred that did not map to an SSPI error code.";
                    break;
                case SEC_E_INVALID_HANDLE:
                    message = "The handle passed to the function is not valid.";
                    break;
                case SEC_E_INVALID_TOKEN:
                    message = "The input token is malformed . Possible causes include a token corrupted in transit, a token of incorrect size, and a token passed into the wrong security package. This last condition can happen if the client and server did not negotiate the proper security package.";
                    break;
                case SEC_E_LOGON_DENIED:
                    message = "The logon failed.";
                    break;
                case SEC_E_MESSAGE_ALTERED:
                    message = "The message has been altered. Used with the Digest and Schannel SSPs.";
                    break;
                case SEC_E_NO_AUTHENTICATING_AUTHORITY:
                    message = "No authority could be contacted for authentication. The domain name of the authenticating party could be wrong, the domain could be unreachable, or there might have been a trust relationship failure.";
                    break;
                case SEC_E_NO_CREDENTIALS:
                    message = "No credentials are available in the security package.";
                    break;
                case SEC_E_OUT_OF_SEQUENCE:
                    message = "The message was not received in the correct sequence.";
                    break;
                case SEC_E_QOP_NOT_SUPPORTED:
                    message = "Neither confidentiality nor integrity are supported by the security context. Used with the Digest SSP.";
                    break;
                case SEC_E_SECPKG_NOT_FOUND:
                    message = "The requested security package does not exist.";
                    break;
                case SEC_E_TARGET_UNKNOWN:
                    message = "The target was not recognized.";
                    break;
                case SEC_E_UNKNOWN_CREDENTIALS:
                    message = "The credentials supplied to the package were not recognized.";
                    break;
                case SEC_I_CONTEXT_EXPIRED:
                    message = "The message sender has finished using the connection and has initiated a shutdown.";
                    break;
                case SEC_I_RENEGOTIATE:
                    message = "The remote party requires a new handshake sequence or the application has just initiated a shutdown.";
                    break;
            }

            return new Win32Exception(errorCode, message);
        }

        // public static methods
        /// <summary>
        /// Acquires the credentials handle.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <param name="package">The package.</param>
        /// <param name="credentialUsage">The credential usage.</param>
        /// <param name="logonId">The logon id.</param>
        /// <param name="identity">The identity.</param>
        /// <param name="keyCallback">The key callback.</param>
        /// <param name="keyArgument">The key argument.</param>
        /// <param name="credentialHandle">The credential handle.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>A result code.</returns>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa374712(v=vs.85).aspx
        /// </remarks>
        [DllImport("security.dll", CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern uint AcquireCredentialsHandle(
            string principal,
            string package,
            SecurityCredentialUse credentialUsage,
            IntPtr logonId,
            AuthIdentity identity,
            int keyCallback,
            IntPtr keyArgument,
            ref SspiHandle credentialHandle,
            out long timestamp);

        /// <summary>
        /// Acquires the credentials handle.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <param name="package">The package.</param>
        /// <param name="credentialUsage">The credential usage.</param>
        /// <param name="logonId">The logon id.</param>
        /// <param name="identity">The identity.</param>
        /// <param name="keyCallback">The key callback.</param>
        /// <param name="keyArgument">The key argument.</param>
        /// <param name="credentialHandle">The credential handle.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>A result code.</returns>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa374712(v=vs.85).aspx
        /// </remarks>
        [DllImport("security.dll", CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern uint AcquireCredentialsHandle(
            string principal,
            string package,
            SecurityCredentialUse credentialUsage,
            IntPtr logonId,
            IntPtr identity,
            int keyCallback,
            IntPtr keyArgument,
            ref SspiHandle credentialHandle,
            out long timestamp);

        /// <summary>
        /// Deletes the security context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A result code.</returns>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa375354(v=vs.85).aspx
        /// </remarks>
        [DllImport("security.dll", CharSet = CharSet.Unicode, SetLastError = false)]
#if NET45
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
        public static extern uint DeleteSecurityContext(ref SspiHandle context);

        /// <summary>
        /// Decrypts the message.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="pMessage">The p message.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        /// <param name="quality">The quality.</param>
        /// <returns>A result code.</returns>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa375211(v=vs.85).aspx
        /// </remarks>
        [DllImport("security.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern uint DecryptMessage(ref SspiHandle context,
            ref SecurityBufferDescriptor pMessage,
            uint sequenceNumber,
            out uint quality);

        /// <summary>
        /// Encrypts the message.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="quality">The quality.</param>
        /// <param name="pMessage">The p message.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        /// <returns>A result code.</returns>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa375378(v=vs.85).aspx
        /// </remarks>
        [DllImport("security.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern uint EncryptMessage(ref SspiHandle context,
            EncryptQualityOfProtection quality,
            ref SecurityBufferDescriptor pMessage,
            uint sequenceNumber);

        /// <summary>
        /// Enumerates the security packages.
        /// </summary>
        /// <param name="numPackages">The pc packages.</param>
        /// <param name="securityPackageInfoArray">The pp package information.</param>
        /// <returns>A result code.</returns>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/aa375397%28v=VS.85%29.aspx
        /// </remarks>
        [DllImport("security.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern uint EnumerateSecurityPackages(ref uint numPackages, ref IntPtr securityPackageInfoArray);

        /// <summary>
        /// Frees the context buffer.
        /// </summary>
        /// <param name="contextBuffer">The context buffer.</param>
        /// <returns>A result code.</returns>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/aa375416(v=vs.85).aspx
        /// </remarks>
        [DllImport("security.dll")]
#if NET45
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
        public static extern uint FreeContextBuffer(IntPtr contextBuffer);

        /// <summary>
        /// Frees the credentials handle.
        /// </summary>
        /// <param name="sspiHandle">The sspi handle.</param>
        /// <returns>A result code.</returns>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa375417(v=vs.85).aspx
        /// </remarks>
        [DllImport("security.dll")]
#if NET45
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
        public static extern int FreeCredentialsHandle(ref SspiHandle sspiHandle);

        /// <summary>
        /// Initializes the security context.
        /// </summary>
        /// <param name="credentialHandle">The credential handle.</param>
        /// <param name="inContextPtr">The in context PTR.</param>
        /// <param name="targetName">Name of the target.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="reserved1">The reserved1.</param>
        /// <param name="dataRepresentation">The data representation.</param>
        /// <param name="inputBuffer">The input buffer.</param>
        /// <param name="reserved2">The reserved2.</param>
        /// <param name="outContextHandle">The out context handle.</param>
        /// <param name="outputBuffer">The output buffer.</param>
        /// <param name="outAttributes">The out attributes.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>A result code.</returns>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa375506(v=vs.85).aspx
        /// </remarks>
        [DllImport("security.dll", CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern uint InitializeSecurityContext(
            ref SspiHandle credentialHandle,
            IntPtr inContextPtr,
            string targetName,
            SspiContextFlags flags,
            int reserved1,
            DataRepresentation dataRepresentation,
            IntPtr inputBuffer,
            int reserved2,
            ref SspiHandle outContextHandle,
            ref SecurityBufferDescriptor outputBuffer,
            out SspiContextFlags outAttributes,
            out long timestamp);

        /// <summary>
        /// Initializes the security context.
        /// </summary>
        /// <param name="credentialHandle">The credential handle.</param>
        /// <param name="inContextHandle">The in context handle.</param>
        /// <param name="targetName">Name of the target.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="reserved1">The reserved1.</param>
        /// <param name="dataRepresentation">The data representation.</param>
        /// <param name="inputBuffer">The input buffer.</param>
        /// <param name="reserved2">The reserved2.</param>
        /// <param name="outContext">The out context.</param>
        /// <param name="outputBuffer">The output buffer.</param>
        /// <param name="outAttributes">The out attributes.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>A result code.</returns>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa375506(v=vs.85).aspx
        /// </remarks>
        [DllImport("security.dll", CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern uint InitializeSecurityContext(
            ref SspiHandle credentialHandle,
            ref SspiHandle inContextHandle,
            string targetName,
            SspiContextFlags flags,
            int reserved1,
            DataRepresentation dataRepresentation,
            ref SecurityBufferDescriptor inputBuffer,
            int reserved2,
            ref SspiHandle outContext,
            ref SecurityBufferDescriptor outputBuffer,
            out SspiContextFlags outAttributes,
            out long timestamp);

        /// <summary>
        /// Queries the context attributes.
        /// </summary>
        /// <param name="inContextHandle">The in context handle.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="sizes">The sizes.</param>
        /// <returns>A result code.</returns>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa379326(v=vs.85).aspx
        /// </remarks>
        [DllImport("security.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern uint QueryContextAttributes(
            ref SspiHandle inContextHandle,
            QueryContextAttributes attribute,
            out SecurityPackageContextSizes sizes);
    }
}
