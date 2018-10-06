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
using System.IO;
using System.Text;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// A static class that represents a JSON scanner.
    /// </summary>
    internal static class JsonScanner
    {
        // public static methods
        /// <summary>
        /// Gets the next JsonToken from a JsonBuffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The next token.</returns>
        public static JsonToken GetNextToken(JsonBuffer buffer)
        {
            // skip leading whitespace
            var c = buffer.Read();
            while (c != -1 && char.IsWhiteSpace((char)c))
            {
                c = buffer.Read();
            }
            if (c == -1)
            {
                return new JsonToken(JsonTokenType.EndOfFile, "<eof>");
            }

            // leading character determines token type
            switch (c)
            {
                case '{': return new JsonToken(JsonTokenType.BeginObject, "{");
                case '}': return new JsonToken(JsonTokenType.EndObject, "}");
                case '[': return new JsonToken(JsonTokenType.BeginArray, "[");
                case ']': return new JsonToken(JsonTokenType.EndArray, "]");
                case '(': return new JsonToken(JsonTokenType.LeftParen, "(");
                case ')': return new JsonToken(JsonTokenType.RightParen, ")");
                case ':': return new JsonToken(JsonTokenType.Colon, ":");
                case ',': return new JsonToken(JsonTokenType.Comma, ",");
                case '\'':
                case '"':
                    return GetStringToken(buffer, (char)c);
                case '/': return GetRegularExpressionToken(buffer);
                default:
                    if (c == '-' || char.IsDigit((char)c))
                    {
                        return GetNumberToken(buffer, c);
                    }
                    else if (c == '$' || c == '_' || char.IsLetter((char)c))
                    {
                        return GetUnquotedStringToken(buffer);
                    }
                    else
                    {
                        buffer.UnRead(c);
                        throw new FormatException(FormatMessage("Invalid JSON input", buffer, buffer.Position));
                    }
            }
        }

        // private methods
        private static string FormatMessage(string message, JsonBuffer buffer, int start)
        {
            var maxLength = 20;
            var snippet = buffer.GetSnippet(start, maxLength);
            return string.Format("{0} '{1}'.", message, snippet);
        }

        private static JsonToken GetNumberToken(JsonBuffer buffer, int firstChar)
        {
            var c = firstChar;

            // leading digit or '-' has already been read
            var start = buffer.Position - 1;
            NumberState state;
            switch (c)
            {
                case '-': state = NumberState.SawLeadingMinus; break;
                case '0': state = NumberState.SawLeadingZero; break;
                default: state = NumberState.SawIntegerDigits; break;
            }
            var type = JsonTokenType.Int64; // assume integer until proved otherwise

            while (true)
            {
                c = buffer.Read();
                switch (state)
                {
                    case NumberState.SawLeadingMinus:
                        switch (c)
                        {
                            case '0':
                                state = NumberState.SawLeadingZero;
                                break;
                            case 'I':
                                state = NumberState.SawMinusI;
                                break;
                            default:
                                if (char.IsDigit((char)c))
                                {
                                    state = NumberState.SawIntegerDigits;
                                }
                                else
                                {
                                    state = NumberState.Invalid;
                                }
                                break;
                        }
                        break;
                    case NumberState.SawLeadingZero:
                        switch (c)
                        {
                            case '.':
                                state = NumberState.SawDecimalPoint;
                                break;
                            case 'e':
                            case 'E':
                                state = NumberState.SawExponentLetter;
                                break;
                            case ',':
                            case '}':
                            case ']':
                            case ')':
                            case -1:
                                state = NumberState.Done;
                                break;
                            default:
                                if (char.IsWhiteSpace((char)c))
                                {
                                    state = NumberState.Done;
                                }
                                else
                                {
                                    state = NumberState.Invalid;
                                }
                                break;
                        }
                        break;
                    case NumberState.SawIntegerDigits:
                        switch (c)
                        {
                            case '.':
                                state = NumberState.SawDecimalPoint;
                                break;
                            case 'e':
                            case 'E':
                                state = NumberState.SawExponentLetter;
                                break;
                            case ',':
                            case '}':
                            case ']':
                            case ')':
                            case -1:
                                state = NumberState.Done;
                                break;
                            default:
                                if (char.IsDigit((char)c))
                                {
                                    state = NumberState.SawIntegerDigits;
                                }
                                else if (char.IsWhiteSpace((char)c))
                                {
                                    state = NumberState.Done;
                                }
                                else
                                {
                                    state = NumberState.Invalid;
                                }
                                break;
                        }
                        break;
                    case NumberState.SawDecimalPoint:
                        type = JsonTokenType.Double;
                        if (char.IsDigit((char)c))
                        {
                            state = NumberState.SawFractionDigits;
                        }
                        else
                        {
                            state = NumberState.Invalid;
                        }
                        break;
                    case NumberState.SawFractionDigits:
                        switch (c)
                        {
                            case 'e':
                            case 'E':
                                state = NumberState.SawExponentLetter;
                                break;
                            case ',':
                            case '}':
                            case ']':
                            case ')':
                            case -1:
                                state = NumberState.Done;
                                break;
                            default:
                                if (char.IsDigit((char)c))
                                {
                                    state = NumberState.SawFractionDigits;
                                }
                                else if (char.IsWhiteSpace((char)c))
                                {
                                    state = NumberState.Done;
                                }
                                else
                                {
                                    state = NumberState.Invalid;
                                }
                                break;
                        }
                        break;
                    case NumberState.SawExponentLetter:
                        type = JsonTokenType.Double;
                        switch (c)
                        {
                            case '+':
                            case '-':
                                state = NumberState.SawExponentSign;
                                break;
                            default:
                                if (char.IsDigit((char)c))
                                {
                                    state = NumberState.SawExponentDigits;
                                }
                                else
                                {
                                    state = NumberState.Invalid;
                                }
                                break;
                        }
                        break;
                    case NumberState.SawExponentSign:
                        if (char.IsDigit((char)c))
                        {
                            state = NumberState.SawExponentDigits;
                        }
                        else
                        {
                            state = NumberState.Invalid;
                        }
                        break;
                    case NumberState.SawExponentDigits:
                        switch (c)
                        {
                            case ',':
                            case '}':
                            case ']':
                            case ')':
                            case -1:
                                state = NumberState.Done;
                                break;
                            default:
                                if (char.IsDigit((char)c))
                                {
                                    state = NumberState.SawExponentDigits;
                                }
                                else if (char.IsWhiteSpace((char)c))
                                {
                                    state = NumberState.Done;
                                }
                                else
                                {
                                    state = NumberState.Invalid;
                                }
                                break;
                        }
                        break;
                    case NumberState.SawMinusI:
                        var sawMinusInfinity = true;
                        var nfinity = new char[] { 'n', 'f', 'i', 'n', 'i', 't', 'y' };
                        for (var i = 0; i < nfinity.Length; i++)
                        {
                            if (c != nfinity[i])
                            {
                                sawMinusInfinity = false;
                                break;
                            }
                            c = buffer.Read();
                        }
                        if (sawMinusInfinity)
                        {
                            type = JsonTokenType.Double;
                            switch (c)
                            {
                                case ',':
                                case '}':
                                case ']':
                                case ')':
                                case -1:
                                    state = NumberState.Done;
                                    break;
                                default:
                                    if (char.IsWhiteSpace((char)c))
                                    {
                                        state = NumberState.Done;
                                    }
                                    else
                                    {
                                        state = NumberState.Invalid;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            state = NumberState.Invalid;
                        }
                        break;
                }

                switch (state)
                {
                    case NumberState.Done:
                        buffer.UnRead(c);
                        var lexeme = buffer.GetSubstring(start, buffer.Position - start);
                        if (type == JsonTokenType.Double)
                        {
                            var value = JsonConvert.ToDouble(lexeme);
                            return new DoubleJsonToken(lexeme, value);
                        }
                        else
                        {
                            var value = JsonConvert.ToInt64(lexeme);
                            if (value < int.MinValue || value > int.MaxValue)
                            {
                                return new Int64JsonToken(lexeme, value);
                            }
                            else
                            {
                                return new Int32JsonToken(lexeme, (int)value);
                            }
                        }
                    case NumberState.Invalid:
                        throw new FormatException(FormatMessage("Invalid JSON number", buffer, start));
                }
            }
        }

        private static JsonToken GetRegularExpressionToken(JsonBuffer buffer)
        {
            // opening slash has already been read
            var start = buffer.Position - 1;
            var state = RegularExpressionState.InPattern;
            while (true)
            {
                var c = buffer.Read();
                switch (state)
                {
                    case RegularExpressionState.InPattern:
                        switch (c)
                        {
                            case '/': state = RegularExpressionState.InOptions; break;
                            case '\\': state = RegularExpressionState.InEscapeSequence; break;
                            default: state = RegularExpressionState.InPattern; break;
                        }
                        break;
                    case RegularExpressionState.InEscapeSequence:
                        state = RegularExpressionState.InPattern;
                        break;
                    case RegularExpressionState.InOptions:
                        switch (c)
                        {
                            case 'i':
                            case 'm':
                            case 'x':
                            case 's':
                                state = RegularExpressionState.InOptions;
                                break;
                            case ',':
                            case '}':
                            case ']':
                            case ')':
                            case -1:
                                state = RegularExpressionState.Done;
                                break;
                            default:
                                if (char.IsWhiteSpace((char)c))
                                {
                                    state = RegularExpressionState.Done;
                                }
                                else
                                {
                                    state = RegularExpressionState.Invalid;
                                }
                                break;
                        }
                        break;
                }

                switch (state)
                {
                    case RegularExpressionState.Done:
                        buffer.UnRead(c);
                        var lexeme = buffer.GetSubstring(start, buffer.Position - start);
                        var regex = new BsonRegularExpression(lexeme);
                        return new RegularExpressionJsonToken(lexeme, regex);
                    case RegularExpressionState.Invalid:
                        throw new FormatException(FormatMessage("Invalid JSON regular expression", buffer, start));
                }
            }
        }

        private static JsonToken GetStringToken(JsonBuffer buffer, char quoteCharacter)
        {
            // opening quote has already been read
            var start = buffer.Position - 1;
            var sb = new StringBuilder();
            while (true)
            {
                var c = buffer.Read();
                switch (c)
                {
                    case '\\':
                        c = buffer.Read();
                        switch (c)
                        {
                            case '\'': sb.Append('\''); break;
                            case '"': sb.Append('"'); break;
                            case '\\': sb.Append('\\'); break;
                            case '/': sb.Append('/'); break;
                            case 'b': sb.Append('\b'); break;
                            case 'f': sb.Append('\f'); break;
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 't': sb.Append('\t'); break;
                            case 'u':
                                var u1 = buffer.Read();
                                var u2 = buffer.Read();
                                var u3 = buffer.Read();
                                var u4 = buffer.Read();
                                if (u4 != -1)
                                {
                                    var hex = new string(new char[] { (char)u1, (char)u2, (char)u3, (char)u4 });
                                    var n = Convert.ToInt32(hex, 16);
                                    sb.Append((char)n);
                                }
                                break;
                            default:
                                if (c != -1)
                                {
                                    var message = string.Format("Invalid escape sequence in JSON string '\\{0}'.", (char)c);
                                    throw new FormatException(message);
                                }
                                break;
                        }
                        break;
                    default:
                        if (c == quoteCharacter)
                        {
                            var lexeme = buffer.GetSubstring(start, buffer.Position - start);
                            return new StringJsonToken(JsonTokenType.String, lexeme, sb.ToString());
                        }
                        if (c != -1)
                        {
                            sb.Append((char)c);
                        }
                        break;
                }
                if (c == -1)
                {
                    throw new FormatException(FormatMessage("End of file in JSON string.", buffer, start));
                }
            }
        }

        private static JsonToken GetUnquotedStringToken(JsonBuffer buffer)
        {
            // opening letter or $ has already been read
            var start = buffer.Position - 1;
            var c = buffer.Read();
            while (c == '$' || c == '_' || char.IsLetterOrDigit((char)c))
            {
                c = buffer.Read();
            }
            buffer.UnRead(c);
            var lexeme = buffer.GetSubstring(start, buffer.Position - start);
            return new StringJsonToken(JsonTokenType.UnquotedString, lexeme, lexeme);
        }

        // nested types
        private enum NumberState
        {
            SawLeadingMinus,
            SawLeadingZero,
            SawIntegerDigits,
            SawDecimalPoint,
            SawFractionDigits,
            SawExponentLetter,
            SawExponentSign,
            SawExponentDigits,
            SawMinusI,
            Done,
            Invalid
        }

        private enum RegularExpressionState
        {
            InPattern,
            InEscapeSequence,
            InOptions,
            Done,
            Invalid
        }
    }
}
