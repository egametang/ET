using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntime.Runtime.Debugger.Expressions
{
    enum TokenTypes
    {
        Unknown,
        Name,
        StringLiteral,
        MemberAccess,
        IndexStart,
        IndexEnd,
        MethodStart,
        MethodEnd,
        InvocationStart,
        InvocationEnd,
        Comma,
        EOF,
    }
    abstract class Token
    {
        public abstract TokenTypes Type { get; }

        public virtual void Parse(char c)
        {

        }
    }

    class StringLiteralToken : Token
    {
        StringBuilder sb = new StringBuilder();
        public override TokenTypes Type =>  TokenTypes.StringLiteral;

        public string Content
        {
            get
            {
                return sb.ToString();
            }
        }

        public override void Parse(char c)
        {
            sb.Append(c);
        }
    }

    class NameToken : StringLiteralToken
    {
        public override TokenTypes Type => TokenTypes.Name;
    }

    class MemberAccessToken : Token
    {
        public override TokenTypes Type => TokenTypes.MemberAccess;
    }

    class IndexStartToken : Token
    {
        public override TokenTypes Type => TokenTypes.IndexStart;
    }

    class IndexEndToken : Token
    {
        public override TokenTypes Type => TokenTypes.IndexEnd;
    }

    class InvocationStartToken : Token
    {
        public override TokenTypes Type => TokenTypes.InvocationStart;
    }

    class InvocationEndToken : Token
    {
        public override TokenTypes Type => TokenTypes.InvocationEnd;
    }

    class CommaToken : Token
    {
        public override TokenTypes Type => TokenTypes.Comma;
    }

    class EOFToken : Token
    {
        public override TokenTypes Type => TokenTypes.EOF;
    }
}
