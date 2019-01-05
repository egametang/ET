/* Original work:
 *   Copyright 2017 Tom Bentley
 * 
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 *   
 *   http://www.apache.org/licenses/LICENSE-2.0
 *   
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 * 
 * Modified work: 
 *   Copyright 2018–present MongoDB Inc.
 *
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 *   
 *   http://www.apache.org/licenses/LICENSE-2.0
 *   
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 */

using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// Utility class for Sasl string preparation.
    /// </summary>
    internal static class SaslPrepHelper
    {
        private const int MinCodepoint = 0x00;
        private const int MaxCodepoint = 0x10FFFF;
        private const int SurrogateMinCodepoint = 0x00d800;
        private const int SurrogateMaxCodePoint = 0x00dfff;
        
        /// <summary>
        /// Return the SASLPrep-canonicalised version of the given <paramref name="str"/> for use as a query string.                          
        /// This implements the {@code SASLPrep} algorithm defined in <a href="https://tools.ietf.org/html/rfc4013">RFC 4013</a>.          
        /// See <a href="https://tools.ietf.org/html/rfc3454#section-7">RFC 3454, Section 7</a> for discussion of what a
        /// query string is.
        /// String normalization step in the .NET Standard version of the driver is skipped due to a lack of a string
        /// normalization function.
        /// </summary>
        /// <param name="str">The string to canonicalise.</param>
        /// <returns>The canonicalised string.</returns>
        public static string SaslPrepQuery(string str)
        {
            return SaslPrep(str, true);
        }

        /// <summary>
        /// Return the SASLPrep-canonicalised version of the given <paramref name="str"/> for use as a stored string.                          
        /// This implements the SASLPrep algorithm defined in <a href="https://tools.ietf.org/html/rfc4013">RFC 4013</a>.
        /// See <a href="https://tools.ietf.org/html/rfc3454#section-7">RFC 3454, Section 7</a> for discussion of what a
        /// stored string is.
        /// String normalization step in the .NET Standard version of the driver is skipped due to a lack of a string
        /// normalization function.
        /// </summary>
        ///<param name="str">The string to canonicalise.</param>
        ///<returns>The canonicalised string.</returns>
        public static string SaslPrepStored(string str)
        {
            return SaslPrep(str, false);
        }

        private static string SaslPrep(string str, bool allowUnassigned) {
            var chars = str.ToCharArray();

            // 1. Map
            // non-ASCII space chars mapped to space
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                if (NonAsciiSpace(ch)) 
                {
                    chars[i] = ' ';
                }
            }

            var length = 0;
            for (var i = 0; i < str.Length; i++)
            {
                var ch = chars[i];
                if (!MappedToNothing(ch))
                {
                    chars[length++] = ch;
                }
            }

            var mappedString = new string(chars.Take(length).ToArray()); 
            // 2. Normalize
#if NET452
            var normalized = mappedString.Normalize(NormalizationForm.FormKC);
#else
            // String normalization step in the .NET Standard version of the driver is skipped due to a lack of a string
            // normalization function.
            var normalized = mappedString;
#endif
            var containsRandALCat = false;
            var containsLCat = false;
            var initialRandALCat = false;
            for (var i = 0; i < normalized.Length;)
            {
                int codepoint = char.ConvertToUtf32(normalized, i);
                // 3. Prohibit
                if (Prohibited(codepoint))
                {
                    throw new ArgumentException("Prohibited character at position " + i);
                }

                // 4. Check bidi
                var isRandALcat = IsRandALcat(codepoint);
                containsRandALCat |= isRandALcat;
                containsLCat |= IsLCat(codepoint);

                initialRandALCat |= i == 0 && isRandALcat;
                if (!allowUnassigned && !IsDefined(codepoint))
                {
                    throw new ArgumentException("Character at position " + i + " is unassigned");
                }   

                i += CharCount(codepoint);

                if (initialRandALCat && i >= normalized.Length && !isRandALcat)
                {
                    throw new ArgumentException("First character is RandALCat, but last character is not");
                }      
            }

            if (containsRandALCat && containsLCat)
            {
                throw new ArgumentException("Contains both RandALCat characters and LCat characters");
            }
                
            return normalized;
        }
        
        /// <summary>
        /// Return true if the given <paramref name="ch"/> is an ASCII control character as defined by
        /// <a href="https://tools.ietf.org/html/rfc3454#appendix-C.2.1">RFC 3454, Appendix C.2.1</a>. 
        /// </summary>
        /// <param name="ch">The character.</param>
        /// <returns>Whether the given character is an ASCII control character.</returns>
        private static bool AsciiControl(char ch)
        {
            return ch <= '\u001F' || ch == '\u007F';
        }
        
        /// <summary>
        /// Return true if the given <paramref name="codepoint"/> is a "change display properties" or a deprecated
        /// character as defined by <a href="https://tools.ietf.org/html/rfc3454#appendix-C.8">RFC 3454, Appendix C.8</a>.
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns>Whether the codepoint is a "change display properties" or a deprecated character.</returns>
        private static bool ChangeDisplayProperties(int codepoint)
        {
            return codepoint == 0x0340
                || codepoint == 0x0341
                || codepoint == 0x200E
                || codepoint == 0x200F
                || codepoint == 0x202A
                || codepoint == 0x202B
                || codepoint == 0x202C
                || codepoint == 0x202D
                || codepoint == 0x202E
                || codepoint == 0x206A
                || codepoint == 0x206B
                || codepoint == 0x206C
                || codepoint == 0x206D
                || codepoint == 0x206E
                || codepoint == 0x206F;
        }

        /// <summary>
        /// Returns the number of characters required to represent a specified Unicode character.
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns>Number of characters required to represent a specified Unicode character.</returns>
        private static int CharCount(int codepoint)
        {
            return codepoint >= 0x10000 ? 2 : 1;
        }
        
        /// <summary>
        /// Return true if the given <paramref name="codepoint"/> is inappropriate for canonical representation
        /// characters as defined by <a href="https://tools.ietf.org/html/rfc3454#appendix-C.7">RFC 3454, Appendix C.7</a>. 
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns></returns>
        private static bool InappropriateForCanonical(int codepoint)
        {
            return 0x2FF0 <= codepoint && codepoint <= 0x2FFB;
        }

        /// <summary>
        /// Return true if the given <paramref name="codepoint"/> is inappropriate for plain text characters as defined
        /// by <a href="https://tools.ietf.org/html/rfc3454#appendix-C.6">RFC 3454, Appendix C.6</a>.
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns></returns>
        private static bool InappropriateForPlainText(int codepoint)
        {
            return codepoint == 0xFFF9
                || codepoint == 0xFFFA
                || codepoint == 0xFFFB
                || codepoint == 0xFFFC
                || codepoint == 0xFFFD;
        }

        /// <summary>
        /// Returns whether or not a Unicode character represented by a codepoint is defined in Unicode.
        /// A character is considered to be defined if its Unicode designation is "Cn" (other, not assigned) OR if it is
        /// part of a surrogate pair.
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns>Whether or not the Unicode character represnted by codepoint is defined in Unicode.</returns>
        private static bool IsDefined(int codepoint)
        {   
            return IsSurrogateCodepoint(codepoint) || 
                  CharUnicodeInfo.GetUnicodeCategory(char.ConvertFromUtf32(codepoint), 0) != UnicodeCategory.OtherNotAssigned;
        }

        /// <summary>
        /// Returns whether or not a Unicode character represented by a codepoint is an "LCat" character.
        /// See <a href="https://tools.ietf.org/html/rfc3454#section-6">RFC 3454: Section 6</a> and
        /// <a href="https://tools.ietf.org/html/rfc3454#appendix-D.2">RFC 3454: Appendix D.2 </a> for more details.
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns>Whether or not the character is an "LCat" character.</returns>
        private static bool IsLCat(int codepoint)
        {
            return 0x0041 <= codepoint && codepoint <= 0x005A ||
                0x0061 <= codepoint && codepoint <= 0x007A ||
                codepoint == 0x00AA ||
                codepoint == 0x00B5 ||
                codepoint == 0x00BA ||
                0x00C0 <= codepoint && codepoint <= 0x00D6 ||
                0x00D8 <= codepoint && codepoint <= 0x00F6 ||
                0x00F8 <= codepoint && codepoint <= 0x0220 ||
                0x0222 <= codepoint && codepoint <= 0x0233 ||
                0x0250 <= codepoint && codepoint <= 0x02AD ||
                0x02B0 <= codepoint && codepoint <= 0x02B8 ||
                0x02BB <= codepoint && codepoint <= 0x02C1 ||
                0x02D0 <= codepoint && codepoint <= 0x02D1 ||
                0x02E0 <= codepoint && codepoint <= 0x02E4 ||
                codepoint == 0x02EE ||
                codepoint == 0x037A ||
                codepoint == 0x0386 ||
                0x0388 <= codepoint && codepoint <= 0x038A ||
                codepoint == 0x038C ||
                0x038E <= codepoint && codepoint <= 0x03A1 ||
                0x03A3 <= codepoint && codepoint <= 0x03CE ||
                0x03D0 <= codepoint && codepoint <= 0x03F5 ||
                0x0400 <= codepoint && codepoint <= 0x0482 ||
                0x048A <= codepoint && codepoint <= 0x04CE ||
                0x04D0 <= codepoint && codepoint <= 0x04F5 ||
                0x04F8 <= codepoint && codepoint <= 0x04F9 ||
                0x0500 <= codepoint && codepoint <= 0x050F ||
                0x0531 <= codepoint && codepoint <= 0x0556 ||
                0x0559 <= codepoint && codepoint <= 0x055F ||
                0x0561 <= codepoint && codepoint <= 0x0587 ||
                codepoint == 0x0589 ||
                codepoint == 0x0903 ||
                0x0905 <= codepoint && codepoint <= 0x0939 ||
                0x093D <= codepoint && codepoint <= 0x0940 ||
                0x0949 <= codepoint && codepoint <= 0x094C ||
                codepoint == 0x0950 ||
                0x0958 <= codepoint && codepoint <= 0x0961 ||
                0x0964 <= codepoint && codepoint <= 0x0970 ||
                0x0982 <= codepoint && codepoint <= 0x0983 ||
                0x0985 <= codepoint && codepoint <= 0x098C ||
                0x098F <= codepoint && codepoint <= 0x0990 ||
                0x0993 <= codepoint && codepoint <= 0x09A8 ||
                0x09AA <= codepoint && codepoint <= 0x09B0 ||
                codepoint == 0x09B2 ||
                0x09B6 <= codepoint && codepoint <= 0x09B9 ||
                0x09BE <= codepoint && codepoint <= 0x09C0 ||
                0x09C7 <= codepoint && codepoint <= 0x09C8 ||
                0x09CB <= codepoint && codepoint <= 0x09CC ||
                codepoint == 0x09D7 ||
                0x09DC <= codepoint && codepoint <= 0x09DD ||
                0x09DF <= codepoint && codepoint <= 0x09E1 ||
                0x09E6 <= codepoint && codepoint <= 0x09F1 ||
                0x09F4 <= codepoint && codepoint <= 0x09FA ||
                0x0A05 <= codepoint && codepoint <= 0x0A0A ||
                0x0A0F <= codepoint && codepoint <= 0x0A10 ||
                0x0A13 <= codepoint && codepoint <= 0x0A28 ||
                0x0A2A <= codepoint && codepoint <= 0x0A30 ||
                0x0A32 <= codepoint && codepoint <= 0x0A33 ||
                0x0A35 <= codepoint && codepoint <= 0x0A36 ||
                0x0A38 <= codepoint && codepoint <= 0x0A39 ||
                0x0A3E <= codepoint && codepoint <= 0x0A40 ||
                0x0A59 <= codepoint && codepoint <= 0x0A5C ||
                codepoint == 0x0A5E ||
                0x0A66 <= codepoint && codepoint <= 0x0A6F ||
                0x0A72 <= codepoint && codepoint <= 0x0A74 ||
                codepoint == 0x0A83 ||
                0x0A85 <= codepoint && codepoint <= 0x0A8B ||
                codepoint == 0x0A8D ||
                0x0A8F <= codepoint && codepoint <= 0x0A91 ||
                0x0A93 <= codepoint && codepoint <= 0x0AA8 ||
                0x0AAA <= codepoint && codepoint <= 0x0AB0 ||
                0x0AB2 <= codepoint && codepoint <= 0x0AB3 ||
                0x0AB5 <= codepoint && codepoint <= 0x0AB9 ||
                0x0ABD <= codepoint && codepoint <= 0x0AC0 ||
                codepoint == 0x0AC9 ||
                0x0ACB <= codepoint && codepoint <= 0x0ACC ||
                codepoint == 0x0AD0 ||
                codepoint == 0x0AE0 ||
                0x0AE6 <= codepoint && codepoint <= 0x0AEF ||
                0x0B02 <= codepoint && codepoint <= 0x0B03 ||
                0x0B05 <= codepoint && codepoint <= 0x0B0C ||
                0x0B0F <= codepoint && codepoint <= 0x0B10 ||
                0x0B13 <= codepoint && codepoint <= 0x0B28 ||
                0x0B2A <= codepoint && codepoint <= 0x0B30 ||
                0x0B32 <= codepoint && codepoint <= 0x0B33 ||
                0x0B36 <= codepoint && codepoint <= 0x0B39 ||
                0x0B3D <= codepoint && codepoint <= 0x0B3E ||
                codepoint == 0x0B40 ||
                0x0B47 <= codepoint && codepoint <= 0x0B48 ||
                0x0B4B <= codepoint && codepoint <= 0x0B4C ||
                codepoint == 0x0B57 ||
                0x0B5C <= codepoint && codepoint <= 0x0B5D ||
                0x0B5F <= codepoint && codepoint <= 0x0B61 ||
                0x0B66 <= codepoint && codepoint <= 0x0B70 ||
                codepoint == 0x0B83 ||
                0x0B85 <= codepoint && codepoint <= 0x0B8A ||
                0x0B8E <= codepoint && codepoint <= 0x0B90 ||
                0x0B92 <= codepoint && codepoint <= 0x0B95 ||
                0x0B99 <= codepoint && codepoint <= 0x0B9A ||
                codepoint == 0x0B9C ||
                0x0B9E <= codepoint && codepoint <= 0x0B9F ||
                0x0BA3 <= codepoint && codepoint <= 0x0BA4 ||
                0x0BA8 <= codepoint && codepoint <= 0x0BAA ||
                0x0BAE <= codepoint && codepoint <= 0x0BB5 ||
                0x0BB7 <= codepoint && codepoint <= 0x0BB9 ||
                0x0BBE <= codepoint && codepoint <= 0x0BBF ||
                0x0BC1 <= codepoint && codepoint <= 0x0BC2 ||
                0x0BC6 <= codepoint && codepoint <= 0x0BC8 ||
                0x0BCA <= codepoint && codepoint <= 0x0BCC ||
                codepoint == 0x0BD7 ||
                0x0BE7 <= codepoint && codepoint <= 0x0BF2 ||
                0x0C01 <= codepoint && codepoint <= 0x0C03 ||
                0x0C05 <= codepoint && codepoint <= 0x0C0C ||
                0x0C0E <= codepoint && codepoint <= 0x0C10 ||
                0x0C12 <= codepoint && codepoint <= 0x0C28 ||
                0x0C2A <= codepoint && codepoint <= 0x0C33 ||
                0x0C35 <= codepoint && codepoint <= 0x0C39 ||
                0x0C41 <= codepoint && codepoint <= 0x0C44 ||
                0x0C60 <= codepoint && codepoint <= 0x0C61 ||
                0x0C66 <= codepoint && codepoint <= 0x0C6F ||
                0x0C82 <= codepoint && codepoint <= 0x0C83 ||
                0x0C85 <= codepoint && codepoint <= 0x0C8C ||
                0x0C8E <= codepoint && codepoint <= 0x0C90 ||
                0x0C92 <= codepoint && codepoint <= 0x0CA8 ||
                0x0CAA <= codepoint && codepoint <= 0x0CB3 ||
                0x0CB5 <= codepoint && codepoint <= 0x0CB9 ||
                codepoint == 0x0CBE ||
                0x0CC0 <= codepoint && codepoint <= 0x0CC4 ||
                0x0CC7 <= codepoint && codepoint <= 0x0CC8 ||
                0x0CCA <= codepoint && codepoint <= 0x0CCB ||
                0x0CD5 <= codepoint && codepoint <= 0x0CD6 ||
                codepoint == 0x0CDE ||
                0x0CE0 <= codepoint && codepoint <= 0x0CE1 ||
                0x0CE6 <= codepoint && codepoint <= 0x0CEF ||
                0x0D02 <= codepoint && codepoint <= 0x0D03 ||
                0x0D05 <= codepoint && codepoint <= 0x0D0C ||
                0x0D0E <= codepoint && codepoint <= 0x0D10 ||
                0x0D12 <= codepoint && codepoint <= 0x0D28 ||
                0x0D2A <= codepoint && codepoint <= 0x0D39 ||
                0x0D3E <= codepoint && codepoint <= 0x0D40 ||
                0x0D46 <= codepoint && codepoint <= 0x0D48 ||
                0x0D4A <= codepoint && codepoint <= 0x0D4C ||
                codepoint == 0x0D57 ||
                0x0D60 <= codepoint && codepoint <= 0x0D61 ||
                0x0D66 <= codepoint && codepoint <= 0x0D6F ||
                0x0D82 <= codepoint && codepoint <= 0x0D83 ||
                0x0D85 <= codepoint && codepoint <= 0x0D96 ||
                0x0D9A <= codepoint && codepoint <= 0x0DB1 ||
                0x0DB3 <= codepoint && codepoint <= 0x0DBB ||
                codepoint == 0x0DBD ||
                0x0DC0 <= codepoint && codepoint <= 0x0DC6 ||
                0x0DCF <= codepoint && codepoint <= 0x0DD1 ||
                0x0DD8 <= codepoint && codepoint <= 0x0DDF ||
                0x0DF2 <= codepoint && codepoint <= 0x0DF4 ||
                0x0E01 <= codepoint && codepoint <= 0x0E30 ||
                0x0E32 <= codepoint && codepoint <= 0x0E33 ||
                0x0E40 <= codepoint && codepoint <= 0x0E46 ||
                0x0E4F <= codepoint && codepoint <= 0x0E5B ||
                0x0E81 <= codepoint && codepoint <= 0x0E82 ||
                codepoint == 0x0E84 ||
                0x0E87 <= codepoint && codepoint <= 0x0E88 ||
                codepoint == 0x0E8A ||
                codepoint == 0x0E8D ||
                0x0E94 <= codepoint && codepoint <= 0x0E97 ||
                0x0E99 <= codepoint && codepoint <= 0x0E9F ||
                0x0EA1 <= codepoint && codepoint <= 0x0EA3 ||
                codepoint == 0x0EA5 ||
                codepoint == 0x0EA7 ||
                0x0EAA <= codepoint && codepoint <= 0x0EAB ||
                0x0EAD <= codepoint && codepoint <= 0x0EB0 ||
                0x0EB2 <= codepoint && codepoint <= 0x0EB3 ||
                codepoint == 0x0EBD ||
                0x0EC0 <= codepoint && codepoint <= 0x0EC4 ||
                codepoint == 0x0EC6 ||
                0x0ED0 <= codepoint && codepoint <= 0x0ED9 ||
                0x0EDC <= codepoint && codepoint <= 0x0EDD ||
                0x0F00 <= codepoint && codepoint <= 0x0F17 ||
                0x0F1A <= codepoint && codepoint <= 0x0F34 ||
                codepoint == 0x0F36 ||
                codepoint == 0x0F38 ||
                0x0F3E <= codepoint && codepoint <= 0x0F47 ||
                0x0F49 <= codepoint && codepoint <= 0x0F6A ||
                codepoint == 0x0F7F ||
                codepoint == 0x0F85 ||
                0x0F88 <= codepoint && codepoint <= 0x0F8B ||
                0x0FBE <= codepoint && codepoint <= 0x0FC5 ||
                0x0FC7 <= codepoint && codepoint <= 0x0FCC ||
                codepoint == 0x0FCF ||
                0x1000 <= codepoint && codepoint <= 0x1021 ||
                0x1023 <= codepoint && codepoint <= 0x1027 ||
                0x1029 <= codepoint && codepoint <= 0x102A ||
                codepoint == 0x102C ||
                codepoint == 0x1031 ||
                codepoint == 0x1038 ||
                0x1040 <= codepoint && codepoint <= 0x1057 ||
                0x10A0 <= codepoint && codepoint <= 0x10C5 ||
                0x10D0 <= codepoint && codepoint <= 0x10F8 ||
                codepoint == 0x10FB ||
                0x1100 <= codepoint && codepoint <= 0x1159 ||
                0x115F <= codepoint && codepoint <= 0x11A2 ||
                0x11A8 <= codepoint && codepoint <= 0x11F9 ||
                0x1200 <= codepoint && codepoint <= 0x1206 ||
                0x1208 <= codepoint && codepoint <= 0x1246 ||
                codepoint == 0x1248 ||
                0x124A <= codepoint && codepoint <= 0x124D ||
                0x1250 <= codepoint && codepoint <= 0x1256 ||
                codepoint == 0x1258 ||
                0x125A <= codepoint && codepoint <= 0x125D ||
                0x1260 <= codepoint && codepoint <= 0x1286 ||
                codepoint == 0x1288 ||
                0x128A <= codepoint && codepoint <= 0x128D ||
                0x1290 <= codepoint && codepoint <= 0x12AE ||
                codepoint == 0x12B0 ||
                0x12B2 <= codepoint && codepoint <= 0x12B5 ||
                0x12B8 <= codepoint && codepoint <= 0x12BE ||
                codepoint == 0x12C0 ||
                0x12C2 <= codepoint && codepoint <= 0x12C5 ||
                0x12C8 <= codepoint && codepoint <= 0x12CE ||
                0x12D0 <= codepoint && codepoint <= 0x12D6 ||
                0x12D8 <= codepoint && codepoint <= 0x12EE ||
                0x12F0 <= codepoint && codepoint <= 0x130E ||
                codepoint == 0x1310 ||
                0x1312 <= codepoint && codepoint <= 0x1315 ||
                0x1318 <= codepoint && codepoint <= 0x131E ||
                0x1320 <= codepoint && codepoint <= 0x1346 ||
                0x1348 <= codepoint && codepoint <= 0x135A ||
                0x1361 <= codepoint && codepoint <= 0x137C ||
                0x13A0 <= codepoint && codepoint <= 0x13F4 ||
                0x1401 <= codepoint && codepoint <= 0x1676 ||
                0x1681 <= codepoint && codepoint <= 0x169A ||
                0x16A0 <= codepoint && codepoint <= 0x16F0 ||
                0x1700 <= codepoint && codepoint <= 0x170C ||
                0x170E <= codepoint && codepoint <= 0x1711 ||
                0x1720 <= codepoint && codepoint <= 0x1731 ||
                0x1735 <= codepoint && codepoint <= 0x1736 ||
                0x1740 <= codepoint && codepoint <= 0x1751 ||
                0x1760 <= codepoint && codepoint <= 0x176C ||
                0x176E <= codepoint && codepoint <= 0x1770 ||
                0x1780 <= codepoint && codepoint <= 0x17B6 ||
                0x17BE <= codepoint && codepoint <= 0x17C5 ||
                0x17C7 <= codepoint && codepoint <= 0x17C8 ||
                0x17D4 <= codepoint && codepoint <= 0x17DA ||
                codepoint == 0x17DC ||
                0x17E0 <= codepoint && codepoint <= 0x17E9 ||
                0x1810 <= codepoint && codepoint <= 0x1819 ||
                0x1820 <= codepoint && codepoint <= 0x1877 ||
                0x1880 <= codepoint && codepoint <= 0x18A8 ||
                0x1E00 <= codepoint && codepoint <= 0x1E9B ||
                0x1EA0 <= codepoint && codepoint <= 0x1EF9 ||
                0x1F00 <= codepoint && codepoint <= 0x1F15 ||
                0x1F18 <= codepoint && codepoint <= 0x1F1D ||
                0x1F20 <= codepoint && codepoint <= 0x1F45 ||
                0x1F48 <= codepoint && codepoint <= 0x1F4D ||
                0x1F50 <= codepoint && codepoint <= 0x1F57 ||
                codepoint == 0x1F59 ||
                codepoint == 0x1F5B ||
                codepoint == 0x1F5D ||
                0x1F5F <= codepoint && codepoint <= 0x1F7D ||
                0x1F80 <= codepoint && codepoint <= 0x1FB4 ||
                0x1FB6 <= codepoint && codepoint <= 0x1FBC ||
                codepoint == 0x1FBE ||
                0x1FC2 <= codepoint && codepoint <= 0x1FC4 ||
                0x1FC6 <= codepoint && codepoint <= 0x1FCC ||
                0x1FD0 <= codepoint && codepoint <= 0x1FD3 ||
                0x1FD6 <= codepoint && codepoint <= 0x1FDB ||
                0x1FE0 <= codepoint && codepoint <= 0x1FEC ||
                0x1FF2 <= codepoint && codepoint <= 0x1FF4 ||
                0x1FF6 <= codepoint && codepoint <= 0x1FFC ||
                codepoint == 0x200E ||
                codepoint == 0x2071 ||
                codepoint == 0x207F ||
                codepoint == 0x2102 ||
                codepoint == 0x2107 ||
                0x210A <= codepoint && codepoint <= 0x2113 ||
                codepoint == 0x2115 ||
                0x2119 <= codepoint && codepoint <= 0x211D ||
                codepoint == 0x2124 ||
                codepoint == 0x2126 ||
                codepoint == 0x2128 ||
                0x212A <= codepoint && codepoint <= 0x212D ||
                0x212F <= codepoint && codepoint <= 0x2131 ||
                0x2133 <= codepoint && codepoint <= 0x2139 ||
                0x213D <= codepoint && codepoint <= 0x213F ||
                0x2145 <= codepoint && codepoint <= 0x2149 ||
                0x2160 <= codepoint && codepoint <= 0x2183 ||
                0x2336 <= codepoint && codepoint <= 0x237A ||
                codepoint == 0x2395 ||
                0x249C <= codepoint && codepoint <= 0x24E9 ||
                0x3005 <= codepoint && codepoint <= 0x3007 ||
                0x3021 <= codepoint && codepoint <= 0x3029 ||
                0x3031 <= codepoint && codepoint <= 0x3035 ||
                0x3038 <= codepoint && codepoint <= 0x303C ||
                0x3041 <= codepoint && codepoint <= 0x3096 ||
                0x309D <= codepoint && codepoint <= 0x309F ||
                0x30A1 <= codepoint && codepoint <= 0x30FA ||
                0x30FC <= codepoint && codepoint <= 0x30FF ||
                0x3105 <= codepoint && codepoint <= 0x312C ||
                0x3131 <= codepoint && codepoint <= 0x318E ||
                0x3190 <= codepoint && codepoint <= 0x31B7 ||
                0x31F0 <= codepoint && codepoint <= 0x321C ||
                0x3220 <= codepoint && codepoint <= 0x3243 ||
                0x3260 <= codepoint && codepoint <= 0x327B ||
                0x327F <= codepoint && codepoint <= 0x32B0 ||
                0x32C0 <= codepoint && codepoint <= 0x32CB ||
                0x32D0 <= codepoint && codepoint <= 0x32FE ||
                0x3300 <= codepoint && codepoint <= 0x3376 ||
                0x337B <= codepoint && codepoint <= 0x33DD ||
                0x33E0 <= codepoint && codepoint <= 0x33FE ||
                0x3400 <= codepoint && codepoint <= 0x4DB5 ||
                0x4E00 <= codepoint && codepoint <= 0x9FA5 ||
                0xA000 <= codepoint && codepoint <= 0xA48C ||
                0xAC00 <= codepoint && codepoint <= 0xD7A3 ||
                0xD800 <= codepoint && codepoint <= 0xFA2D ||
                0xFA30 <= codepoint && codepoint <= 0xFA6A ||
                0xFB00 <= codepoint && codepoint <= 0xFB06 ||
                0xFB13 <= codepoint && codepoint <= 0xFB17 ||
                0xFF21 <= codepoint && codepoint <= 0xFF3A ||
                0xFF41 <= codepoint && codepoint <= 0xFF5A ||
                0xFF66 <= codepoint && codepoint <= 0xFFBE ||
                0xFFC2 <= codepoint && codepoint <= 0xFFC7 ||
                0xFFCA <= codepoint && codepoint <= 0xFFCF ||
                0xFFD2 <= codepoint && codepoint <= 0xFFD7 ||
                0xFFDA <= codepoint && codepoint <= 0xFFDC ||
                0x10300 <= codepoint && codepoint <= 0x1031E ||
                0x10320 <= codepoint && codepoint <= 0x10323 ||
                0x10330 <= codepoint && codepoint <= 0x1034A ||
                0x10400 <= codepoint && codepoint <= 0x10425 ||
                0x10428 <= codepoint && codepoint <= 0x1044D ||
                0x1D000 <= codepoint && codepoint <= 0x1D0F5 ||
                0x1D100 <= codepoint && codepoint <= 0x1D126 ||
                0x1D12A <= codepoint && codepoint <= 0x1D166 ||
                0x1D16A <= codepoint && codepoint <= 0x1D172 ||
                0x1D183 <= codepoint && codepoint <= 0x1D184 ||
                0x1D18C <= codepoint && codepoint <= 0x1D1A9 ||
                0x1D1AE <= codepoint && codepoint <= 0x1D1DD ||
                0x1D400 <= codepoint && codepoint <= 0x1D454 ||
                0x1D456 <= codepoint && codepoint <= 0x1D49C ||
                0x1D49E <= codepoint && codepoint <= 0x1D49F ||
                codepoint == 0x1D4A2 ||
                0x1D4A5 <= codepoint && codepoint <= 0x1D4A6 ||
                0x1D4A9 <= codepoint && codepoint <= 0x1D4AC ||
                0x1D4AE <= codepoint && codepoint <= 0x1D4B9 ||
                codepoint == 0x1D4BB ||
                0x1D4BD <= codepoint && codepoint <= 0x1D4C0 ||
                0x1D4C2 <= codepoint && codepoint <= 0x1D4C3 ||
                0x1D4C5 <= codepoint && codepoint <= 0x1D505 ||
                0x1D507 <= codepoint && codepoint <= 0x1D50A ||
                0x1D50D <= codepoint && codepoint <= 0x1D514 ||
                0x1D516 <= codepoint && codepoint <= 0x1D51C ||
                0x1D51E <= codepoint && codepoint <= 0x1D539 ||
                0x1D53B <= codepoint && codepoint <= 0x1D53E ||
                0x1D540 <= codepoint && codepoint <= 0x1D544 ||
                codepoint == 0x1D546 ||
                0x1D54A <= codepoint && codepoint <= 0x1D550 ||
                0x1D552 <= codepoint && codepoint <= 0x1D6A3 ||
                0x1D6A8 <= codepoint && codepoint <= 0x1D7C9 ||
                0x20000 <= codepoint && codepoint <= 0x2A6D6 ||
                0x2F800 <= codepoint && codepoint <= 0x2FA1D ||
                0xF0000 <= codepoint && codepoint <= 0xFFFFD ||
                0x100000 <= codepoint && codepoint <= 0x10FFFD;
        }
  
        /// <summary>
        /// Returns whether or not a Unicode character represented by a codepoint is an "RandALCat" character.
        /// See <a href="https://tools.ietf.org/html/rfc3454#section-6">RFC 3454: Section 6</a> and
        /// <a href="https://tools.ietf.org/html/rfc3454#appendix-D.1">RFC 3454: Appendix D.1 </a> for more details.
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns>Whether or not the character is an "RandALCat" character.</returns>
        private static bool IsRandALcat(int codepoint)
        {
            return codepoint == 0x05BE || 
                codepoint == 0x05C0 ||
                codepoint == 0x05C3 ||
                0x05D0 <= codepoint && codepoint <= 0x05EA ||
                0x05F0 <= codepoint && codepoint <= 0x05F4 ||
                codepoint == 0x061B ||
                codepoint == 0x061F ||
                0x0621 <= codepoint && codepoint <= 0x063A ||
                0x0640 <= codepoint && codepoint <= 0x064A ||
                0x066D <= codepoint && codepoint <= 0x066F ||
                0x0671 <= codepoint && codepoint <= 0x06D5 ||
                codepoint == 0x06DD ||
                0x06E5 <= codepoint && codepoint <= 0x06E6 ||
                0x06FA <= codepoint && codepoint <= 0x06FE ||
                0x0700 <= codepoint && codepoint <= 0x070D ||
                codepoint == 0x0710 ||
                0x0712 <= codepoint && codepoint <= 0x072C ||
                0x0780 <= codepoint && codepoint <= 0x07A5 ||
                codepoint == 0x07B1 ||
                codepoint == 0x200F ||
                codepoint == 0xFB1D ||
                0xFB1F <= codepoint && codepoint <= 0xFB28 ||
                0xFB2A <= codepoint && codepoint <= 0xFB36 ||
                0xFB38 <= codepoint && codepoint <= 0xFB3C ||
                codepoint == 0xFB3E ||
                0xFB40 <= codepoint && codepoint <= 0xFB41 ||
                0xFB43 <= codepoint && codepoint <= 0xFB44 ||
                0xFB46 <= codepoint && codepoint <= 0xFBB1 ||
                0xFBD3 <= codepoint && codepoint <= 0xFD3D ||
                0xFD50 <= codepoint && codepoint <= 0xFD8F ||
                0xFD92 <= codepoint && codepoint <= 0xFDC7 ||
                0xFDF0 <= codepoint && codepoint <= 0xFDFC ||
                0xFE70 <= codepoint && codepoint <= 0xFE74 || 
                0xFE76 <= codepoint && codepoint <= 0xFEFC;
        }

        private static bool IsSurrogateCodepoint(int codepoint)
        {
            return SurrogateMinCodepoint <= codepoint && codepoint <= SurrogateMaxCodePoint;
        }
        
        /// <summary>
        ///  Return true if the given <paramref name="ch"/> is a "commonly mapped to nothing" character as defined by
        ///  <a href="https://tools.ietf.org/html/rfc3454#appendix-B.1">RFC 3454, Appendix B.1</a>. 
        /// </summary>
        /// <param name="ch">The character.</param>
        /// <returns>Whether the given character is a "commonly mapped to nothing" character.</returns>
        private static bool MappedToNothing(char ch)
        {
            return ch == '\u00AD'
                || ch == '\u034F'
                || ch == '\u1806'
                || ch == '\u180B'
                || ch == '\u180C'
                || ch == '\u180D'
                || ch == '\u200B'
                || ch == '\u200C'
                || ch == '\u200D'
                || ch == '\u2060'
                || '\uFE00' <= ch && ch <= '\uFE0F'
                || ch == '\uFEFF';
        }
        
        /// <summary>
        /// Return true if the given <paramref name="codepoint"/> is a non-ASCII control character as defined by
        /// <a href="https://tools.ietf.org/html/rfc3454#appendix-C.2.2">RFC 3454, Appendix C.2.2</a>. 
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns>Whether the given character is a non-ASCII control character.</returns>
        private static bool NonAsciiControl(int codepoint)
        {
            return 0x0080 <= codepoint && codepoint <= 0x009F
                || codepoint == 0x06DD
                || codepoint == 0x070F
                || codepoint == 0x180E
                || codepoint == 0x200C
                || codepoint == 0x200D
                || codepoint == 0x2028
                || codepoint == 0x2029
                || codepoint == 0x2060
                || codepoint == 0x2061
                || codepoint == 0x2062
                || codepoint == 0x2063
                || 0x206A <= codepoint && codepoint <= 0x206F
                || codepoint == 0xFEFF
                || 0xFFF9 <= codepoint && codepoint <= 0xFFFC
                || 0x1D173 <= codepoint && codepoint <= 0x1D17A;
        }
        
        /// <summary>
        /// Return true if the given <paramref name="ch"/> is a non-ASCII space character as defined by
        /// <a href="https://tools.ietf.org/html/rfc3454#appendix-C.1.2">RFC 3454, Appendix C.1.2</a>. 
        /// </summary>
        /// <param name="ch">The character.</param>
        /// <returns>Whether the given character is a non-ASCII space character.</returns>
        private static bool NonAsciiSpace(char ch)
        {
            return ch == '\u00A0'
                || ch == '\u1680'
                || '\u2000' <= ch && ch <= '\u200B'
                || ch == '\u202F'
                || ch == '\u205F'
                || ch == '\u3000';
        }

        /// <summary>
        /// Return true if the given <paramref name="codepoint"/> is a non-character code point as defined by
        /// <a href="https://tools.ietf.org/html/rfc3454#appendix-C.4">RFC 3454, Appendix C.4</a>.
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns>Whether the given <paramref name="codepoint"/> is a non-character code point.</returns>
        private static bool NonCharacterCodepoint(int codepoint)
        {
            return 0xFDD0 <= codepoint && codepoint <= 0xFDEF
                || 0xFFFE <= codepoint && codepoint <= 0xFFFF
                || 0x1FFFE <= codepoint && codepoint <= 0x1FFFF
                || 0x2FFFE <= codepoint && codepoint <= 0x2FFFF
                || 0x3FFFE <= codepoint && codepoint <= 0x3FFFF
                || 0x4FFFE <= codepoint && codepoint <= 0x4FFFF
                || 0x5FFFE <= codepoint && codepoint <= 0x5FFFF
                || 0x6FFFE <= codepoint && codepoint <= 0x6FFFF
                || 0x7FFFE <= codepoint && codepoint <= 0x7FFFF
                || 0x8FFFE <= codepoint && codepoint <= 0x8FFFF
                || 0x9FFFE <= codepoint && codepoint <= 0x9FFFF
                || 0xAFFFE <= codepoint && codepoint <= 0xAFFFF
                || 0xBFFFE <= codepoint && codepoint <= 0xBFFFF
                || 0xCFFFE <= codepoint && codepoint <= 0xCFFFF
                || 0xDFFFE <= codepoint && codepoint <= 0xDFFFF
                || 0xEFFFE <= codepoint && codepoint <= 0xEFFFF
                || 0xFFFFE <= codepoint && codepoint <= 0xFFFFF
                || 0x10FFFE <= codepoint && codepoint <= 0x10FFFF;
        }
        
        /// <summary>
        /// Return true if the given <paramref name="codepoint"/> is a private use character as defined by
        /// <a href="https://tools.ietf.org/html/rfc3454#appendix-C.3">RFC 3454, Appendix C.3</a>.
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns>Whether if the given codepoint is a private use character.</returns>
        private static bool PrivateUse(int codepoint)
        {
            return 0xE000 <= codepoint && codepoint <= 0xF8FF
                || 0xF000 <= codepoint && codepoint <= 0xFFFFD
                || 0x100000 <= codepoint && codepoint <= 0x10FFFD;
        }
        
        /// <summary>
        /// Return true if the given <paramref name="codepoint"/> is a prohibited character as defined by
        ///<a href="https://tools.ietf.org/html/rfc4013#section-2.3">RFC 4013, Section 2.3</a>. 
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns>Whether the codepoint is a prohibited character.</returns>
        private static bool Prohibited(int codepoint)
        {
            return NonAsciiSpace((char) codepoint)
                || AsciiControl((char) codepoint)
                || NonAsciiControl(codepoint)
                || PrivateUse(codepoint)
                || NonCharacterCodepoint(codepoint)
                || Surrogatecodepoint(codepoint)
                || InappropriateForPlainText(codepoint)
                || InappropriateForCanonical(codepoint)
                || ChangeDisplayProperties(codepoint)
                || Tagging(codepoint);
        }

        /// <summary>
        /// Return true if the given <paramref name="codepoint"/> is a surrogate code point as defined by
        /// <a href="https://tools.ietf.org/html/rfc3454#appendix-C.5">RFC 3454, Appendix C.5</a>.
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns>Whether the given <paramref name="codepoint"/> is a surrogate code point.</returns>
        private static bool Surrogatecodepoint(int codepoint)
        {
            return 0xD800 <= codepoint && codepoint <= 0xDFFF;
        }
        
        /// <summary>
        /// Return true if the given <paramref name="codepoint"/> is a tagging character as defined by
        /// <a href="https://tools.ietf.org/html/rfc3454#appendix-C.9">RFC 3454, Appendix C.9</a>.
        /// </summary>
        /// <param name="codepoint">The Unicode character's codepoint.</param>
        /// <returns></returns>
        private static bool Tagging(int codepoint)
        {
            return codepoint == 0xE0001
                || 0xE0020 <= codepoint && codepoint <= 0xE007F;
        }
    }
}
