using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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

        private string Variable;
        private ExpressionType NodeType;

        public ExpressionVistor(Expression node)
        {
            Visit(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Expression lambda = base.VisitLambda(node);
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
                    ConstantExpression cleanNode = GetMemberConstant(node);
                    return VisitConstant(cleanNode);
                }
            }

            if (node.Member.Name != nameof(ComponentWithId.Id))
            {
                this.Variable = node.Member.Name;
            }
            else
            {
                this.Variable = "_id";
            }

            return base.VisitMember(node);
        }

        private static ConstantExpression GetMemberConstant(MemberExpression node)
        {
            object value;
            switch (node.Member.MemberType)
            {
                case MemberTypes.Field:
                    value = GetFieldValue(node);
                    break;
                case MemberTypes.Property:
                    value = GetPropertyValue(node);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return Expression.Constant(value, node.Type);
        }
        
        protected override Expression VisitConstant(ConstantExpression node)
        {
            this.Builder.Append(this.Variable);
            bool flag = false;
            switch (this.NodeType)
            {
                case ExpressionType.Equal:
                    this.Builder.Append(":");
                    break;
                case ExpressionType.GreaterThan:
                    this.Builder.Append(":{");
                    this.Builder.Append("$gt:");
                    flag = true;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    this.Builder.Append(":{");
                    this.Builder.Append("$gte:");
                    flag = true;
                    break;
                case ExpressionType.LessThan:
                    this.Builder.Append(":{");
                    this.Builder.Append("$lt:");
                    flag = true;
                    break;
                case ExpressionType.LessThanOrEqual:
                    this.Builder.Append(":{");
                    this.Builder.Append("lte:");
                    flag = true;
                    break;
            }

            this.Builder.Append(node.Value);
            if (flag)
            {
                this.Builder.Append("}");
            }

            this.Builder.Append(",");
            return base.VisitConstant(node);
        }
        
        private static object GetFieldValue(MemberExpression node)
        {
            FieldInfo fieldInfo = (FieldInfo)node.Member;

            object instance = node.Expression == null ? null : TryEvaluate(node.Expression).Value;

            return fieldInfo.GetValue(instance);
        }

        private static object GetPropertyValue(MemberExpression node)
        {
            PropertyInfo propertyInfo = (PropertyInfo)node.Member;

            object instance = node.Expression == null ? null : TryEvaluate(node.Expression).Value;

            return propertyInfo.GetValue(instance, null);
        }
        
        private static ConstantExpression TryEvaluate(Expression expression)
        {
            if (expression.NodeType != ExpressionType.Constant)
            {
                throw new NotSupportedException();
            }

            return (ConstantExpression)expression;
        }
    }
}