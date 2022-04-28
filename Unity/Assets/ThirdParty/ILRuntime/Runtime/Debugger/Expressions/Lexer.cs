using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntime.Runtime.Debugger.Expressions
{
    class Lexer
    {
        char[] content;
        int idx;

        public Token LastToken { get; set; }

        public Lexer(string exp)
        {
            content = exp.ToCharArray();
        }

        public Token PeekNextToken()
        {
            int oldIdx = idx;
            var oldToken = LastToken;
            Token res = GetNextToken();
            idx = oldIdx;
            LastToken = oldToken;
            return res;
        }

        public Token GetNextToken()
        {
            var len = content.Length;
            Token res = null;
            while(idx < len)
            {
                char c = content[idx++];

                switch(c)
                {
                    case '"':
                        if (res != null)
                        {
                            if (res.Type == TokenTypes.StringLiteral)
                            {
                                LastToken = res;
                                return res;
                            }
                            else
                                throw new NotSupportedException();
                        }
                        res = new StringLiteralToken();
                        break;
                    case '[':
                        if(res != null)
                        {
                            if(res.Type == TokenTypes.StringLiteral)
                            {
                                res.Parse(c);
                            }
                            else
                            {
                                idx--;
                                LastToken = res;
                                return res;
                            }
                        }
                        else
                        {
                            LastToken = new IndexStartToken();
                            return LastToken;
                        }
                        break;
                    case ']':
                        if (res != null)
                        {
                            if (res.Type == TokenTypes.StringLiteral)
                            {
                                res.Parse(c);
                            }
                            else
                            {
                                idx--;
                                LastToken = res;
                                return res;
                            }
                        }
                        else
                        {
                            LastToken = new IndexEndToken();
                            return LastToken;
                        }
                        break;
                    case '(':
                        if (res != null)
                        {
                            if (res.Type == TokenTypes.StringLiteral)
                            {
                                res.Parse(c);
                            }
                            else
                            {
                                idx--;
                                LastToken = res;
                                return res;
                            }
                        }
                        else
                        {
                            LastToken = new InvocationStartToken();
                            return LastToken;
                        }
                        break;
                    case ')':
                        if (res != null)
                        {
                            if (res.Type == TokenTypes.StringLiteral)
                            {
                                res.Parse(c);
                            }
                            else
                            {
                                idx--;
                                LastToken = res;
                                return res;
                            }
                        }
                        else
                        {
                            LastToken = new InvocationEndToken();
                            return LastToken;
                        }
                        break;
                    case ',':
                        if (res != null)
                        {
                            if (res.Type == TokenTypes.StringLiteral)
                            {
                                res.Parse(c);
                            }
                            else
                            {
                                idx--;
                                LastToken = res;
                                return res;
                            }
                        }
                        else
                        {
                            LastToken = new CommaToken();
                            return LastToken;
                        }
                        break;
                    case ' ':
                        if (res != null)
                        {
                            if (res.Type == TokenTypes.StringLiteral)
                            {
                                res.Parse(c);
                            }
                            else
                            {
                                idx--;
                                LastToken = res;
                                return res;
                            }
                        }
                        break;
                    case '.':
                        if (res != null)
                        {
                            if (res.Type == TokenTypes.StringLiteral)
                            {
                                res.Parse(c);
                            }
                            else
                            {
                                idx--;
                                LastToken = res;
                                return res;
                            }
                        }
                        else
                        {
                            LastToken = new MemberAccessToken();
                            return LastToken;
                        }
                        break;
                    default:
                        if(res == null)
                        {
                            res = new NameToken();
                        }
                        res.Parse(c);
                        break;
                }
            }
            if(res == null)
            {
                res = new EOFToken();
            }
            LastToken = res;
            return res;
        }
    }
}
