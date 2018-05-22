using System.Linq.Expressions;
using System.Text;
using ExpressionVisitor = MongoDB.Bson.Serialization.ExpressionVisitor;

namespace ETModel
{
    public sealed class ExpressionVistor: ExpressionVisitor
    {
        private readonly StringBuilder Builder = new StringBuilder("{");

        public string Output
        {
            get
            {
                return this.Builder.ToString();
            }
        }

        private string         Variable;
        private ExpressionType NodeType;

        public ExpressionVistor(Expression node)
        {
            Visit(node);
        }
        
        protected override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected override Expression VisitLambda(LambdaExpression node)
        {
            var lambda = base.VisitLambda(node);
            this.Builder.Remove(this.Builder.Length - 1, 1); //Remove the Last Comma
            this.Builder.Append("}");
            return lambda;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            this.NodeType = node.NodeType;
            return base.VisitBinary(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            this.Variable = node.Member.Name;
            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            this.Builder.Append(this.Variable);
            bool flag = false;
            if (this.NodeType == ExpressionType.Equal)
            {
                this.Builder.Append(":");
            }
            else if (this.NodeType == ExpressionType.GreaterThan)
            {
                this.Builder.Append(":{");
                this.Builder.Append("$gt:");
                flag = true;
            }
            else if (this.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                this.Builder.Append(":{");
                this.Builder.Append("$gte:");
                flag = true;
            }
            else if (this.NodeType == ExpressionType.LessThan)
            {
                this.Builder.Append(":{");
                this.Builder.Append("$lt:");
                flag = true;
            }
            else if (this.NodeType == ExpressionType.LessThanOrEqual)
            {
                this.Builder.Append(":{");
                this.Builder.Append("lte:");
                flag = true;
            }

            this.Builder.Append(node.Value);
            if (flag)
                this.Builder.Append("}");
            this.Builder.Append(",");
            return base.VisitConstant(node);
        }
    }
}