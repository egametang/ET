using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntime.Runtime.Debugger.Expressions
{
    public abstract class EvalExpression
    {
        public virtual bool Completed { get { return true; } }
        internal abstract bool Parse(Token curToken, Lexer lexer);

        public bool IsRoot { get; set; }
    }

    public class NameExpression : EvalExpression
    {
        string content;
        public string Content => content;

        public NameExpression(string content)
        {
            this.content = content;
        }

        internal override bool Parse(Token curToken, Lexer lexer)
        {
            return false;
        }
    }

    public class StringLiteralExpression : EvalExpression
    {
        string content;
        public string Content => content;

        public StringLiteralExpression(string content)
        {
            this.content = content;
        }

        internal override bool Parse(Token curToken, Lexer lexer)
        {
            return false;
        }
    }

    public class MemberAcessExpression : EvalExpression
    {
        EvalExpression body;
        string member;
        public EvalExpression Body { get { return body; } }
        public string Member { get { return member; } }
        public override bool Completed => member != null;

        public MemberAcessExpression(EvalExpression body)
        {
            if (body is NameExpression || body is MemberAcessExpression || body is IndexAccessExpression || body is InvocationExpression)
                this.body = body;
            else
                throw new NotSupportedException("Cannot retrive member for " + body);
        }

        internal override bool Parse(Token curToken, Lexer lexer)
        {
            if (member == null)
            {
                if (curToken.Type == TokenTypes.Name)
                {
                    member = ((NameToken)curToken).Content;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
    }

    public class IndexAccessExpression : EvalExpression
    {
        EvalExpression body;
        EvalExpression index;
        bool isCompleted = false;

        public EvalExpression Body => body;

        public EvalExpression Index => index;

        public override bool Completed => isCompleted;

        public IndexAccessExpression(EvalExpression body)
        {
            if (body is NameExpression || body is MemberAcessExpression || body is IndexAccessExpression || body is InvocationExpression)
                this.body = body;
            else
                throw new NotSupportedException("Cannot get index for " + body);
        }

        internal override bool Parse(Token curToken, Lexer lexer)
        {
            if (index == null)
            {
                switch (curToken.Type)
                {
                    case TokenTypes.Name:
                        index = new NameExpression(((NameToken)curToken).Content);
                        return true;
                    case TokenTypes.StringLiteral:
                        index = new StringLiteralExpression(((StringLiteralToken)curToken).Content);
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                if (!index.Completed)
                    return index.Parse(curToken, lexer);
                else
                {
                    if (!isCompleted)
                    {
                        switch(curToken.Type)
                        {
                            case TokenTypes.IndexEnd:
                                isCompleted = true;
                                return true;
                            case TokenTypes.MemberAccess:
                                index = new MemberAcessExpression(index);
                                return true;
                            case TokenTypes.IndexStart:
                                index = new IndexAccessExpression(index);
                                return true;
                            case TokenTypes.InvocationStart:
                                index = new InvocationExpression(index);
                                return true;
                            default:
                                return false;                                    
                        }
                    }
                    else
                        return false;
                }
            }
        }
    }

    public class InvocationExpression : EvalExpression
    {
        EvalExpression body;        
        List<EvalExpression> parameters = new List<EvalExpression>();
        EvalExpression curParam;
        bool isCompleted = false;

        public EvalExpression Body => body;

        public List<EvalExpression> Parameters => parameters;

        public override bool Completed => isCompleted;

        public InvocationExpression(EvalExpression body)
        {
            if (body is NameExpression || body is MemberAcessExpression || body is IndexAccessExpression || body is InvocationExpression)
                this.body = body;
            else
                throw new NotSupportedException("Cannot make invocation  for " + body);
        }

        internal override bool Parse(Token curToken, Lexer lexer)
        {
            if (curParam == null)
            {
                switch (curToken.Type)
                {
                    case TokenTypes.Name:
                        curParam = new NameExpression(((NameToken)curToken).Content);
                        return true;
                    case TokenTypes.StringLiteral:
                        curParam = new StringLiteralExpression(((StringLiteralToken)curToken).Content);
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                if (!curParam.Completed)
                    return curParam.Parse(curToken, lexer);
                else
                {
                    if (!isCompleted)
                    {
                        switch (curToken.Type)
                        {
                            case TokenTypes.InvocationEnd:
                                if (curParam != null)
                                    parameters.Add(curParam);
                                isCompleted = true;
                                return true;
                            case TokenTypes.MemberAccess:
                                curParam = new MemberAcessExpression(curParam);
                                return true;
                            case TokenTypes.IndexStart:
                                curParam = new IndexAccessExpression(curParam);
                                return true;
                            case TokenTypes.InvocationStart:
                                curParam = new InvocationExpression(curParam);
                                return true;
                            case TokenTypes.Comma:
                                if (curParam != null)
                                {
                                    parameters.Add(curParam);
                                    return true;
                                }
                                else
                                    return false;
                            default:
                                return false;
                        }
                    }
                    else
                        return false;
                }
            }
        }
    }
}
