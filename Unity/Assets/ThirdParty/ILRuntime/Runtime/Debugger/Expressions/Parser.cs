using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntime.Runtime.Debugger.Expressions
{
    public class Parser
    {
        Lexer lexer;
        public Parser(string exp)
        {
            this.lexer = new Lexer(exp);
        }

        public EvalExpression Parse()
        {
            var curToken = lexer.GetNextToken();
            EvalExpression res = null;
            while(curToken.Type != TokenTypes.EOF)
            {
                if (res == null)
                {
                    if (curToken.Type == TokenTypes.Name)
                    {
                        res = new NameExpression(((NameToken)curToken).Content);
                        res.IsRoot = true;
                    }
                    else
                        throw new NotSupportedException("Unexpected token:" + curToken.Type);
                }
                else
                {
                    if (!res.Parse(curToken, lexer))
                    {
                        switch (curToken.Type)
                        {
                            case TokenTypes.MemberAccess:
                                res.IsRoot = false;
                                res = new MemberAcessExpression(res);
                                res.IsRoot = true;
                                break;
                            case TokenTypes.IndexStart:
                                res.IsRoot = false;
                                res = new IndexAccessExpression(res);
                                res.IsRoot = true;
                                break;
                            case TokenTypes.InvocationStart:
                                res.IsRoot = false;
                                res = new InvocationExpression(res);
                                res.IsRoot = true;
                                break;
                            default:
                                throw new NotSupportedException("Unexpected token:" + curToken.Type);
                        }
                    }
                }
                curToken = lexer.GetNextToken();
            }
            if(res != null && !res.Completed)
            {
                throw new NotSupportedException("Unexpected token: EOF");
            }
            return res;
        }
    }
}
