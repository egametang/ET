using System;
using System.Linq.Expressions;
using System.Reflection;
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
            switch (node.Expression.NodeType)
            {
                case ExpressionType.Constant:
                case ExpressionType.MemberAccess:
                {
                    var cleanNode = GetMemberConstant(node);
                    return VisitConstant(cleanNode);
                }
            }

            if (node.Member.Name != nameof(ComponentWithId.Id))
                this.Variable = node.Member.Name;
            else
                this.Variable = "_id";
            return base.VisitMember(node);
        }

        private static ConstantExpression GetMemberConstant(MemberExpression node)
        {
            object value;
            if (node.Member.MemberType == MemberTypes.Field)
            {
                value = GetFieldValue(node);
            }
            else if (node.Member.MemberType == MemberTypes.Property)
            {
                value = GetPropertyValue(node);
            }
            else
            {
                throw new NotSupportedException();
            }
            return Expression.Constant(value, node.Type);
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
        
        private static object GetFieldValue(MemberExpression node)
        {
            var fieldInfo = (FieldInfo)node.Member;

            var instance = (node.Expression == null) ? null : TryEvaluate(node.Expression).Value;

            return fieldInfo.GetValue(instance);
        }

        private static object GetPropertyValue(MemberExpression node)
        {
            var propertyInfo = (PropertyInfo)node.Member;

            var instance = (node.Expression == null) ? null : TryEvaluate(node.Expression).Value;

            return propertyInfo.GetValue(instance, null);
        }
        
        private static ConstantExpression TryEvaluate(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return (ConstantExpression)expression;
            }
            throw new NotSupportedException();
        }
    }
}