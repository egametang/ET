#region Header
/**
 * Lexer.cs
 *   JSON lexer implementation based on a finite state machine.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/
#endregion


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace LitJson
{
    internal class FsmContext
    {
        public bool  Return;
        public int   NextState;
        public Lexer L;
        public int   StateStack;
    }


    internal class Lexer
    {
        #region Fields
        private delegate bool StateHandler (FsmContext ctx);

        private static readonly int[]          fsm_return_table;
        private static readonly StateHandler[] fsm_handler_table;

        private bool          allow_comments;
        private bool          allow_single_quoted_strings;
        private bool          end_of_input;
        private FsmContext    fsm_context;
        private int           input_buffer;
        private int           input_char;
        private TextReader    reader;
        private int           state;
        private StringBuilder string_buffer;
        private string        string_value;
        private int           token;
        private int           unichar;
        #endregion


        #region Properties
        public bool AllowComments {
            get { return allow_comments; }
            set { allow_comments = value; }
        }

        public bool AllowSingleQuotedStrings {
            get { return allow_single_quoted_strings; }
            set { allow_single_quoted_strings = value; }
        }

        public bool EndOfInput {
            get { return end_of_input; }
        }

        public int Token {
            get { return token; }
        }

        public string StringValue {
            get { return string_value; }
        }
        #endregion


        #region Constructors
        static Lexer ()
        {
            PopulateFsmTables (out fsm_handler_table, out fsm_return_table);
        }

        public Lexer (TextReader reader)
        {
            allow_comments = true;
            allow_single_quoted_strings = true;

            input_buffer = 0;
            string_buffer = new StringBuilder (128);
            state = 1;
            end_of_input = false;
            this.reader = reader;

            fsm_context = new FsmContext ();
            fsm_context.L = this;
        }
        #endregion


        #region Static Methods
        private static int HexValue (int digit)
        {
            switch (digit) {
            case 'a':
            case 'A':
                return 10;

            case 'b':
            case 'B':
                return 11;

            case 'c':
            case 'C':
                return 12;

            case 'd':
            case 'D':
                return 13;

            case 'e':
            case 'E':
                return 14;

            case 'f':
            case 'F':
                return 15;

            default:
                return digit - '0';
            }
        }

        private static void PopulateFsmTables (out StateHandler[] fsm_handler_table, out int[] fsm_return_table)
        {
            // See section A.1. of the manual for details of the finite
            // state machine.
            fsm_handler_table = new StateHandler[28] {
                State1,
                State2,
                State3,
                State4,
                State5,
                State6,
                State7,
                State8,
                State9,
                State10,
                State11,
                State12,
                State13,
                State14,
                State15,
                State16,
                State17,
                State18,
                State19,
                State20,
                State21,
                State22,
                State23,
                State24,
                State25,
                State26,
                State27,
                State28
            };

            fsm_return_table = new int[28] {
                (int) ParserToken.Char,
                0,
                (int) ParserToken.Number,
                (int) ParserToken.Number,
                0,
                (int) ParserToken.Number,
                0,
                (int) ParserToken.Number,
                0,
                0,
                (int) ParserToken.True,
                0,
                0,
                0,
                (int) ParserToken.False,
                0,
                0,
                (int) ParserToken.Null,
                (int) ParserToken.CharSeq,
                (int) ParserToken.Char,
                0,
                0,
                (int) ParserToken.CharSeq,
                (int) ParserToken.Char,
                0,
                0,
                0,
                0
            };
        }

        private static char ProcessEscChar (int esc_char)
        {
            switch (esc_char) {
            case '"':
            case '\'':
            case '\\':
            case '/':
                return Convert.ToChar (esc_char);

            case 'n':
                return '\n';

            case 't':
                return '\t';

            case 'r':
                return '\r';

            case 'b':
                return '\b';

            case 'f':
                return '\f';

            default:
                // Unreachable
                return '?';
            }
        }

        private static bool State1 (FsmContext ctx)
        {
            while (ctx.L.GetChar ()) {
                if (ctx.L.input_char == ' ' ||
                    ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r')
                    continue;

                if (ctx.L.input_char >= '1' && ctx.L.input_char <= '9') {
                    ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                    ctx.NextState = 3;
                    return true;
                }

                switch (ctx.L.input_char) {
                case '"':
                    ctx.NextState = 19;
                    ctx.Return = true;
                    return true;

                case ',':
                case ':':
                case '[':
                case ']':
                case '{':
                case '}':
                    ctx.NextState = 1;
                    ctx.Return = true;
                    return true;

                case '-':
                    ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                    ctx.NextState = 2;
                    return true;

                case '0':
                    ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                    ctx.NextState = 4;
                    return true;

                case 'f':
                    ctx.NextState = 12;
                    return true;

                case 'n':
                    ctx.NextState = 16;
                    return true;

                case 't':
                    ctx.NextState = 9;
                    return true;

                case '\'':
                    if (! ctx.L.allow_single_quoted_strings)
                        return false;

                    ctx.L.input_char = '"';
                    ctx.NextState = 23;
                    ctx.Return = true;
                    return true;

                case '/':
                    if (! ctx.L.allow_comments)
                        return false;

                    ctx.NextState = 25;
                    return true;

                default:
                    return false;
                }
            }

            return true;
        }

        private static bool State2 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            if (ctx.L.input_char >= '1' && ctx.L.input_char<= '9') {
                ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                ctx.NextState = 3;
                return true;
            }

            switch (ctx.L.input_char) {
            case '0':
                ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                ctx.NextState = 4;
                return true;

            default:
                return false;
            }
        }

        private static bool State3 (FsmContext ctx)
        {
            while (ctx.L.GetChar ()) {
                if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9') {
                    ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                    continue;
                }

                if (ctx.L.input_char == ' ' ||
                    ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r') {
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                }

                switch (ctx.L.input_char) {
                case ',':
                case ']':
                case '}':
                    ctx.L.UngetChar ();
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;

                case '.':
                    ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                    ctx.NextState = 5;
                    return true;

                case 'e':
                case 'E':
                    ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                    ctx.NextState = 7;
                    return true;

                default:
                    return false;
                }
            }
            return true;
        }

        private static bool State4 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            if (ctx.L.input_char == ' ' ||
                ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r') {
                ctx.Return = true;
                ctx.NextState = 1;
                return true;
            }

            switch (ctx.L.input_char) {
            case ',':
            case ']':
            case '}':
                ctx.L.UngetChar ();
                ctx.Return = true;
                ctx.NextState = 1;
                return true;

            case '.':
                ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                ctx.NextState = 5;
                return true;

            case 'e':
            case 'E':
                ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                ctx.NextState = 7;
                return true;

            default:
                return false;
            }
        }

        private static bool State5 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9') {
                ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                ctx.NextState = 6;
                return true;
            }

            return false;
        }

        private static bool State6 (FsmContext ctx)
        {
            while (ctx.L.GetChar ()) {
                if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9') {
                    ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                    continue;
                }

                if (ctx.L.input_char == ' ' ||
                    ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r') {
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                }

                switch (ctx.L.input_char) {
                case ',':
                case ']':
                case '}':
                    ctx.L.UngetChar ();
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;

                case 'e':
                case 'E':
                    ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                    ctx.NextState = 7;
                    return true;

                default:
                    return false;
                }
            }

            return true;
        }

        private static bool State7 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            if (ctx.L.input_char >= '0' && ctx.L.input_char<= '9') {
                ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                ctx.NextState = 8;
                return true;
            }

            switch (ctx.L.input_char) {
            case '+':
            case '-':
                ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                ctx.NextState = 8;
                return true;

            default:
                return false;
            }
        }

        private static bool State8 (FsmContext ctx)
        {
            while (ctx.L.GetChar ()) {
                if (ctx.L.input_char >= '0' && ctx.L.input_char<= '9') {
                    ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                    continue;
                }

                if (ctx.L.input_char == ' ' ||
                    ctx.L.input_char >= '\t' && ctx.L.input_char<= '\r') {
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                }

                switch (ctx.L.input_char) {
                case ',':
                case ']':
                case '}':
                    ctx.L.UngetChar ();
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;

                default:
                    return false;
                }
            }

            return true;
        }

        private static bool State9 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case 'r':
                ctx.NextState = 10;
                return true;

            default:
                return false;
            }
        }

        private static bool State10 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case 'u':
                ctx.NextState = 11;
                return true;

            default:
                return false;
            }
        }

        private static bool State11 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case 'e':
                ctx.Return = true;
                ctx.NextState = 1;
                return true;

            default:
                return false;
            }
        }

        private static bool State12 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case 'a':
                ctx.NextState = 13;
                return true;

            default:
                return false;
            }
        }

        private static bool State13 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case 'l':
                ctx.NextState = 14;
                return true;

            default:
                return false;
            }
        }

        private static bool State14 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case 's':
                ctx.NextState = 15;
                return true;

            default:
                return false;
            }
        }

        private static bool State15 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case 'e':
                ctx.Return = true;
                ctx.NextState = 1;
                return true;

            default:
                return false;
            }
        }

        private static bool State16 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case 'u':
                ctx.NextState = 17;
                return true;

            default:
                return false;
            }
        }

        private static bool State17 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case 'l':
                ctx.NextState = 18;
                return true;

            default:
                return false;
            }
        }

        private static bool State18 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case 'l':
                ctx.Return = true;
                ctx.NextState = 1;
                return true;

            default:
                return false;
            }
        }

        private static bool State19 (FsmContext ctx)
        {
            while (ctx.L.GetChar ()) {
                switch (ctx.L.input_char) {
                case '"':
                    ctx.L.UngetChar ();
                    ctx.Return = true;
                    ctx.NextState = 20;
                    return true;

                case '\\':
                    ctx.StateStack = 19;
                    ctx.NextState = 21;
                    return true;

                default:
                    ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                    continue;
                }
            }

            return true;
        }

        private static bool State20 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case '"':
                ctx.Return = true;
                ctx.NextState = 1;
                return true;

            default:
                return false;
            }
        }

        private static bool State21 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case 'u':
                ctx.NextState = 22;
                return true;

            case '"':
            case '\'':
            case '/':
            case '\\':
            case 'b':
            case 'f':
            case 'n':
            case 'r':
            case 't':
                ctx.L.string_buffer.Append (
                    ProcessEscChar (ctx.L.input_char));
                ctx.NextState = ctx.StateStack;
                return true;

            default:
                return false;
            }
        }

        private static bool State22 (FsmContext ctx)
        {
            int counter = 0;
            int mult    = 4096;

            ctx.L.unichar = 0;

            while (ctx.L.GetChar ()) {

                if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9' ||
                    ctx.L.input_char >= 'A' && ctx.L.input_char <= 'F' ||
                    ctx.L.input_char >= 'a' && ctx.L.input_char <= 'f') {

                    ctx.L.unichar += HexValue (ctx.L.input_char) * mult;

                    counter++;
                    mult /= 16;

                    if (counter == 4) {
                        ctx.L.string_buffer.Append (
                            Convert.ToChar (ctx.L.unichar));
                        ctx.NextState = ctx.StateStack;
                        return true;
                    }

                    continue;
                }

                return false;
            }

            return true;
        }

        private static bool State23 (FsmContext ctx)
        {
            while (ctx.L.GetChar ()) {
                switch (ctx.L.input_char) {
                case '\'':
                    ctx.L.UngetChar ();
                    ctx.Return = true;
                    ctx.NextState = 24;
                    return true;

                case '\\':
                    ctx.StateStack = 23;
                    ctx.NextState = 21;
                    return true;

                default:
                    ctx.L.string_buffer.Append ((char) ctx.L.input_char);
                    continue;
                }
            }

            return true;
        }

        private static bool State24 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case '\'':
                ctx.L.input_char = '"';
                ctx.Return = true;
                ctx.NextState = 1;
                return true;

            default:
                return false;
            }
        }

        private static bool State25 (FsmContext ctx)
        {
            ctx.L.GetChar ();

            switch (ctx.L.input_char) {
            case '*':
                ctx.NextState = 27;
                return true;

            case '/':
                ctx.NextState = 26;
                return true;

            default:
                return false;
            }
        }

        private static bool State26 (FsmContext ctx)
        {
            while (ctx.L.GetChar ()) {
                if (ctx.L.input_char == '\n') {
                    ctx.NextState = 1;
                    return true;
                }
            }

            return true;
        }

        private static bool State27 (FsmContext ctx)
        {
            while (ctx.L.GetChar ()) {
                if (ctx.L.input_char == '*') {
                    ctx.NextState = 28;
                    return true;
                }
            }

            return true;
        }

        private static bool State28 (FsmContext ctx)
        {
            while (ctx.L.GetChar ()) {
                if (ctx.L.input_char == '*')
                    continue;

                if (ctx.L.input_char == '/') {
                    ctx.NextState = 1;
                    return true;
                }

                ctx.NextState = 27;
                return true;
            }

            return true;
        }
        #endregion


        private bool GetChar ()
        {
            if ((input_char = NextChar ()) != -1)
                return true;

            end_of_input = true;
            return false;
        }

        private int NextChar ()
        {
            if (input_buffer != 0) {
                int tmp = input_buffer;
                input_buffer = 0;

                return tmp;
            }

            return reader.Read ();
        }

        public bool NextToken ()
        {
            StateHandler handler;
            fsm_context.Return = false;

            while (true) {
                handler = fsm_handler_table[state - 1];

                if (! handler (fsm_context))
                    throw new JsonException (input_char);

                if (end_of_input)
                    return false;

                if (fsm_context.Return) {
                    string_value = string_buffer.ToString ();
                    string_buffer.Remove (0, string_buffer.Length);
                    token = fsm_return_table[state - 1];

                    if (token == (int) ParserToken.Char)
                        token = input_char;

                    state = fsm_context.NextState;

                    return true;
                }

                state = fsm_context.NextState;
            }
        }

        private void UngetChar ()
        {
            input_buffer = input_char;
        }
    }
}
