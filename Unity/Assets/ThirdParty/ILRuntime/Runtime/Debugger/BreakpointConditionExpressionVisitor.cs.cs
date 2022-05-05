#if ILRUNTIME_ENABLE_ROSYLN
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Reflection;
using ILRuntime.Runtime.Debugger.Protocol;
using ILRuntime.Runtime.Intepreter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ILRuntime.Runtime.Debugger
{
    internal class BreakpointConditionExpressionVisitor : CSharpSyntaxVisitor<TempComputeResult>
    {
        public DebugService DebugService { get; private set; }
        public ILIntepreter Intepreter { get; private set; }
        public IDictionary<string, VariableInfo> LocalVariables { get; private set; }
        private int tempVariableIndex = 0;
        private static Dictionary<string, string> dic_UnaryOperator_MethodName = new Dictionary<string, string>();
        private static Dictionary<string, string> dic_BinaryOperator_MethodName = new Dictionary<string, string>();

        static BreakpointConditionExpressionVisitor()
        {
            dic_UnaryOperator_MethodName["+"] = "op_UnaryPlus";
            dic_UnaryOperator_MethodName["-"] = "op_UnaryNegation";
            dic_UnaryOperator_MethodName["!"] = "op_LogicalNot";
            dic_UnaryOperator_MethodName["~"] = "op_OnesComplement";
            dic_UnaryOperator_MethodName["++"] = "op_Increment";
            dic_UnaryOperator_MethodName["--"] = "op_Decrement";
            dic_UnaryOperator_MethodName["true"] = "op_True";
            dic_UnaryOperator_MethodName["false"] = "op_False";

            dic_BinaryOperator_MethodName["+"] = "op_Addition";
            dic_BinaryOperator_MethodName["-"] = "op_Subtraction";
            dic_BinaryOperator_MethodName["*"] = "op_Multiply";
            dic_BinaryOperator_MethodName["/"] = "op_Division";
            dic_BinaryOperator_MethodName["%"] = "op_Modulus";
            dic_BinaryOperator_MethodName["&"] = "op_BitwiseAnd";
            dic_BinaryOperator_MethodName["|"] = "op_BitwiseOr";
            dic_BinaryOperator_MethodName["^"] = "op_ExclusiveOr";
            dic_BinaryOperator_MethodName["<<"] = "op_LeftShift";
            dic_BinaryOperator_MethodName[">>"] = "op_RightShift";
            dic_BinaryOperator_MethodName["<"] = "op_LessThan";
            dic_BinaryOperator_MethodName[">"] = "op_GreaterThan";
            dic_BinaryOperator_MethodName["<="] = "op_LessThanOrEqual";
            dic_BinaryOperator_MethodName[">="] = "op_GreaterThanOrEqual";
            dic_BinaryOperator_MethodName["=="] = "op_Equality";
            dic_BinaryOperator_MethodName["!="] = "op_Inequality";
        }

        public BreakpointConditionExpressionVisitor(DebugService debugService, ILIntepreter intp, VariableInfo[] localVariables)
        {
            DebugService = debugService;
            Intepreter = intp;
            if (localVariables == null)
                LocalVariables = new Dictionary<string, VariableInfo>();
            else
                LocalVariables = localVariables.ToDictionary(i => i.Name);
        }

        private TempComputeResult CreateComputeResult(object value, Type type)
        {
            return new TempComputeResult("tempVariable" + (++tempVariableIndex), value, type);
        }

        public override TempComputeResult DefaultVisit(SyntaxNode node)
        {
            throw new NotSupportedException("Unsupport Expression:" + node.GetText());
        }

        public override TempComputeResult VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            var value = node.Token.Value;
            return CreateComputeResult(value, (value == null) ? null : value.GetType());
        }

        public override TempComputeResult VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            //ConditionalAccessExpressionSyntax
            //MemberBindingExpressionSyntax
            PassVariableReference(node, node.Expression);
            return Visit(node.Expression);
        }

        public override TempComputeResult VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            var tuple = GetOrCreateVariableReference(node, "");
            tuple.Top.Type = VariableTypes.Value;

            bool conditionValue;
            var conditionResult = Visit(node.Condition);
            if (conditionResult.Value is bool)
                conditionValue = (bool)conditionResult.Value;
            else
            {
                // true运算符重载
                var overloadTrueMethod = DebugService.GetMethod(conditionResult.Type, "op_True", BindingFlags.Public | BindingFlags.Static, methodInfo => CheckConditionalOperatorUnaryMethodParameters(conditionResult.Type, methodInfo));
                if (overloadTrueMethod != null)
                    conditionValue = (bool)overloadTrueMethod.Invoke(null, new object[1] { conditionResult.Value });
                else
                {
                    throw new InvalidOperationException(string.Format("\"{0}\" is not conditional expression", node.Condition.ToString()));
                }
            }

            TempComputeResult result;
            if (conditionValue)
                result = Visit(node.WhenTrue);
            else
                result = Visit(node.WhenFalse);

            tuple.Top.Value = result.Value;
            tuple.Top.ValueType = result.Type;

            return ResolveVariable(tuple.Bottom);
        }

        public override TempComputeResult VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            return VisitUnaryExpression(node.Operand, node.OperatorToken.Text, true, node.ToString());
        }

        public override TempComputeResult VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            return VisitUnaryExpression(node.Operand, node.OperatorToken.Text, false, node.ToString());
        }

        public TempComputeResult VisitUnaryExpression(ExpressionSyntax operand, string operatorText, bool isPrefix, string nodeText)
        {
            var operandResult = Visit(operand);
            if (operandResult.Type == null)
                return CreateComputeResult(null, null);

            string methodName;
            if (dic_UnaryOperator_MethodName.TryGetValue(operatorText, out methodName))
            {
                var bindingFlags = BindingFlags.Public | BindingFlags.Static;
                var overloadOperatorMethod = DebugService.GetMethod(operandResult.Type, methodName, bindingFlags, true, operandResult.Type);
                if (overloadOperatorMethod != null) // 有运算符重载
                {
                    var result = overloadOperatorMethod.Invoke(null, new object[1] { operandResult.Value });
                    return CreateComputeResult(result, overloadOperatorMethod.ReturnType);
                }
            }

            try
            {
                var expressionText = isPrefix ? "{0}x" : "x{0}";
                var func = new DynamicExpresso.Interpreter().Parse(string.Format(expressionText, operatorText), new DynamicExpresso.Parameter("x", operandResult.Type.UnWrapper()));
                var result = func.Invoke(operandResult.Value);
                return CreateComputeResult(result, result == null ? null : result.GetType());
            }
            catch
            {
                throw new Exception(string.Format("Fail to calculate '{0}'", nodeText));
            }
        }

        public override TempComputeResult VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);
            var operatorText = node.OperatorToken.Text;
            if (left.Type == null && right.Type == null)
            {
                if (operatorText == "==")
                    return CreateComputeResult(true, typeof(bool));
                else
                    return CreateComputeResult(null, null);
            }

            var leftType = left.Type;
            var rightType = right.Type;
            string methodName;
            if (dic_BinaryOperator_MethodName.TryGetValue(operatorText, out methodName))
            {
                var bindingFlags = BindingFlags.Public | BindingFlags.Static;
                var overloadOperatorMethod = DebugService.GetMethod(leftType, methodName, bindingFlags, true, leftType, rightType);
                if (overloadOperatorMethod == null)
                    overloadOperatorMethod = DebugService.GetMethod(rightType, methodName, bindingFlags, true, leftType, rightType);
                if (overloadOperatorMethod != null) // 有运算符重载
                {
                    var result = overloadOperatorMethod.Invoke(null, new object[2] { left.Value, right.Value });
                    return CreateComputeResult(result, overloadOperatorMethod.ReturnType);
                }
                else
                {
                    return ComputeBinaryNative(leftType, rightType, left.Value, right.Value, operatorText, node);
                }
            }
            else
            {
                var result = ComputeCustomConditionalLogicOperator(leftType, rightType, left.Value, right.Value, operatorText); // 用户定义的条件逻辑运算符，规则为https://docs.microsoft.com/zh-cn/dotnet/csharp/language-reference/language-specification/expressions#conditional-logical-operators
                if (result != null)
                    return result;
                return ComputeBinaryNative(leftType, rightType, left.Value, right.Value, operatorText, node);
                //throw new NotSupportedException("Unknown Binary Operator:" + operatorText);
            }
        }

        private TempComputeResult ComputeCustomConditionalLogicOperator(Type leftType, Type rightType, object leftValue, object rightValue, string operatorText)
        {
            if (operatorText != "&&" && operatorText != "||")
                return null;
            if (leftType != rightType)
                return null;
            if (operatorText == "&&") // T.false(x) ? x : T.& (x, y)
            {
                return ComputeCustomConditionalLogic(leftType, "op_False", "op_BitwiseAnd", leftValue, rightValue);
            }
            else if (operatorText == "||") // T.true(x) ? x : T.| (x, y)
            {
                return ComputeCustomConditionalLogic(leftType, "op_True", "op_BitwiseOr", leftValue, rightValue);
            }
            return null;
        }

        private TempComputeResult ComputeCustomConditionalLogic(Type type, string methodXName, string methodYName, object leftValue, object rightValue)
        {
            var methodX = DebugService.GetMethod(type, methodXName, BindingFlags.Public | BindingFlags.Static, methodInfo => CheckConditionalOperatorUnaryMethodParameters(type, methodInfo));
            if (methodX == null)
                return null;
            var methodY = DebugService.GetMethod(type, methodYName, BindingFlags.Public | BindingFlags.Static, methodInfo => CheckConditionalOperatorBinaryMethodParameters(type, methodInfo));
            if (methodY == null)
                return null;

            var condition = (bool)methodX.Invoke(null, new object[1] { leftValue });
            object obj = leftValue;
            if (!condition)
                obj = methodY.Invoke(null, new object[2] { leftValue, rightValue });
            return CreateComputeResult(obj, type);
        }

        private bool CheckConditionalOperatorUnaryMethodParameters(Type type, MethodInfo methodInfo)
        {
            if (methodInfo.ReturnType.UnWrapper() != typeof(bool))
                return false;
            var parameterInfos = methodInfo.GetParameters();
            return parameterInfos.Length == 1 && parameterInfos[0].ParameterType == type;
        }

        private bool CheckConditionalOperatorBinaryMethodParameters(Type type, MethodInfo methodInfo)
        {
            if (methodInfo.ReturnType.UnWrapper() != type)
                return false;
            var parameterInfos = methodInfo.GetParameters();
            return parameterInfos.Length == 2 && parameterInfos[0].ParameterType == type && parameterInfos[1].ParameterType == type;
        }

        private TempComputeResult ComputeBinaryNative(Type leftType, Type rightType, object leftValue, object rightValue, string operatorText, ExpressionSyntax node) // 数学运算, 依赖动态编译lambda表达式，只能JIT
        {
            try
            {
                var func = new DynamicExpresso.Interpreter().Parse(string.Format("x{0}y", operatorText), new DynamicExpresso.Parameter("x", leftType.UnWrapper()), new DynamicExpresso.Parameter("y", rightType.UnWrapper()));
                var result = func.Invoke(leftValue, rightValue);
                return CreateComputeResult(result, result == null ? null : result.GetType());
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Fail to calculate '{0}'", node.ToString()), e);
            }
        }

        public override TempComputeResult VisitThisExpression(ThisExpressionSyntax node)
        {
            VariableReferenceTuple tuple;
            if (dic_Expression_Variable.TryGetValue(node, out tuple))
                return ResolveVariable(tuple.Bottom);

            ILMethod currentMethod;
            var v = DebugService.GetThis(Intepreter, 0, out currentMethod);
            return CreateComputeResult(v, currentMethod.DeclearingType.ReflectionType);
        }

        public override TempComputeResult VisitIdentifierName(IdentifierNameSyntax node)
        {
            var identifierName = node.ToString();
            var tuple = GetOrCreateVariableReference(node, identifierName);
            if (node.Parent is InvocationExpressionSyntax)
                HandleInvocationExpressionSyntax(tuple, node.Parent as InvocationExpressionSyntax);
            else
            {
                var top = tuple.Top;
                VariableInfo localVariable;
                if (LocalVariables.TryGetValue(top.Name, out localVariable))
                {
                    top.Type = VariableTypes.Normal;
                    top.Address = localVariable.Address;
                    top.ValueType = localVariable.ValueObjType;
                }
                else
                    top.Type = VariableTypes.FieldReference;
            }

            return ResolveVariable(tuple.Bottom);
        }

        private TempComputeResult ResolveVariable(VariableReference variableReference)
        {
            object variableValue;
            var variableInfo = DebugService.ResolveVariable(Intepreter.GetHashCode(), 0, variableReference, out variableValue);
            HandleResolveVariableError(variableInfo);
            return CreateComputeResult(variableValue, variableInfo.ValueObjType);
        }

        private Dictionary<ExpressionSyntax, object> dic_Expression_Conditional = new Dictionary<ExpressionSyntax, object>();
        public override TempComputeResult VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            if (node.WhenNotNull is MemberBindingExpressionSyntax) // x?.y
            {
                var memberBindingExpressionSyntax = node.WhenNotNull as MemberBindingExpressionSyntax;
                var memberAccessExpressionSyntax = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Expression, memberBindingExpressionSyntax.OperatorToken, memberBindingExpressionSyntax.Name);
                PassVariableReference(node, memberAccessExpressionSyntax);
                dic_Expression_Conditional[memberAccessExpressionSyntax] = null;
                return VisitMemberAccessExpression(memberAccessExpressionSyntax);
            }
            else if (node.WhenNotNull is InvocationExpressionSyntax) // x?.y()
            {
                var invocationExpressionSyntax = node.WhenNotNull as InvocationExpressionSyntax;
                if (invocationExpressionSyntax.Expression is MemberBindingExpressionSyntax)
                {
                    var memberBindingExpressionSyntax = invocationExpressionSyntax.Expression as MemberBindingExpressionSyntax;
                    var memberAccessExpressionSyntax = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Expression, memberBindingExpressionSyntax.OperatorToken, memberBindingExpressionSyntax.Name);
                    invocationExpressionSyntax = invocationExpressionSyntax.WithExpression(memberAccessExpressionSyntax);
                    PassVariableReference(node, invocationExpressionSyntax);
                    dic_Expression_Conditional[invocationExpressionSyntax] = null;
                    return VisitInvocationExpression(invocationExpressionSyntax);
                }
            }
            else if (node.WhenNotNull is ElementBindingExpressionSyntax) // x?[y]
            {
                var elementBindingExpressionSyntax = node.WhenNotNull as ElementBindingExpressionSyntax;
                var elementAccessExpressionSyntax = SyntaxFactory.ElementAccessExpression(node.Expression, elementBindingExpressionSyntax.ArgumentList);
                PassVariableReference(node, elementAccessExpressionSyntax);
                dic_Expression_Conditional[elementAccessExpressionSyntax] = null;
                return VisitElementAccessExpression(elementAccessExpressionSyntax);
            }
            else
                throw new NotSupportedException(string.Format("unknown expression \"{0}\" in Conditional Access Expression", node.WhenNotNull.ToString()));

            return base.VisitConditionalAccessExpression(node);
        }

        public override TempComputeResult VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            PassVariableReference(node, node.Expression);
            if (dic_Expression_Conditional.ContainsKey(node))
            {
                dic_Expression_Conditional.Remove(node);
                dic_Expression_Conditional[node.Expression] = null;
            }
            return Visit(node.Expression);
        }

        public override TempComputeResult VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var tuple = GetOrCreateVariableReference(node, node.Name.ToString());
            if (dic_Expression_Conditional.ContainsKey(node))
            {
                tuple.Top.Conditional = true;
                dic_Expression_Conditional.Remove(node);
            }
            if (node.Parent is InvocationExpressionSyntax)
                HandleInvocationExpressionSyntax(tuple, node.Parent as InvocationExpressionSyntax);
            else
                tuple.Top.Type = VariableTypes.FieldReference;
            dic_Expression_Variable.Add(node.Expression, tuple);
            return Visit(node.Expression);
        }

        public override TempComputeResult VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var tuple = GetOrCreateVariableReference(node, null);
            if (dic_Expression_Conditional.ContainsKey(node))
            {
                tuple.Top.Conditional = true;
                dic_Expression_Conditional.Remove(node);
            }
            tuple.Top.Type = VariableTypes.IndexAccess;
            tuple.Top.Parameters = HandleParameters(node.ArgumentList);
            dic_Expression_Variable.Add(node.Expression, tuple);
            return Visit(node.Expression);
        }

        private void HandleInvocationExpressionSyntax(VariableReferenceTuple tuple, InvocationExpressionSyntax invocationExpressionSyntax)
        {
            tuple.Top.Type = VariableTypes.Invocation;
            tuple.Top.Parameters = HandleParameters(invocationExpressionSyntax.ArgumentList);
        }

        private VariableReference[] HandleParameters(BaseArgumentListSyntax argumentListSyntax)
        {
            var variableReferenceList = new List<VariableReference>();
            foreach (var argument in argumentListSyntax.Arguments)
            {
                var argumentResult = Visit(argument.Expression);
                variableReferenceList.Add(new VariableReference
                {
                    Type = VariableTypes.Value,
                    Value = argumentResult.Value,
                    ValueType = argumentResult.Type,
                });
            }
            return variableReferenceList.ToArray();
        }

        private void HandleResolveVariableError(VariableInfo variableInfo)
        {
            if (variableInfo.Type == VariableTypes.Null)
                throw new NullReferenceException();
            if (variableInfo.Type == VariableTypes.Error || variableInfo.Type == VariableTypes.NotFound)
                throw new Exception(variableInfo.Value);
        }

        private Dictionary<ExpressionSyntax, VariableReferenceTuple> dic_Expression_Variable = new Dictionary<ExpressionSyntax, VariableReferenceTuple>();
        private VariableReferenceTuple GetOrCreateVariableReference(ExpressionSyntax expressionSyntax, string name, Func<bool> createParentCondition = null)
        {
            VariableReferenceTuple tuple;
            if (!dic_Expression_Variable.TryGetValue(expressionSyntax, out tuple))
            {
                var variableReference = new VariableReference { Name = name };
                tuple = new VariableReferenceTuple { Bottom = variableReference, Top = variableReference };
            }
            else
            {
                dic_Expression_Variable.Remove(expressionSyntax);
                if (createParentCondition == null || createParentCondition())
                {
                    tuple.Top.Parent = new VariableReference { Name = name };
                    tuple.Top = tuple.Top.Parent;
                }
            }
            return tuple;
        }

        private void PassVariableReference(ExpressionSyntax from, ExpressionSyntax to)
        {
            VariableReferenceTuple tuple;
            if (dic_Expression_Variable.TryGetValue(from, out tuple))
            {
                dic_Expression_Variable.Remove(from);
                dic_Expression_Variable.Add(to, tuple);
            }
        }
    }

    internal class VariableReferenceTuple
    {
        public VariableReference Bottom { get; set; }
        public VariableReference Top { get; set; }
    }

    internal class TempComputeResult
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public Type Type { get; set; }

        public TempComputeResult(string name, object value, Type type)
        {
            Name = name;
            Value = value;
            Type = type;
        }
    }
}
#endif