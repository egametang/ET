/* Copyright 2015 MongoDB Inc.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MongoDB.Driver.Linq.Expressions
{
    /// <summary>
    /// Compare two expressions to determine if they are equivalent.
    /// </summary>
    internal sealed class ExpressionComparer
    {
        // private fields
        private ScopedDictionary<ParameterExpression, ParameterExpression> _parameterScope;

        // public methods
        public bool Compare(Expression a, Expression b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            if (a.NodeType != b.NodeType)
            {
                return false;
            }
            if (a.Type != b.Type)
            {
                return false;
            }
            if (a.NodeType == ExpressionType.Extension && a is ExtensionExpression && !(b is ExtensionExpression))
            {
                return false;
            }
            switch (a.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                case ExpressionType.UnaryPlus:
                    return CompareUnary((UnaryExpression)a, (UnaryExpression)b);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.Power:
                    return CompareBinary((BinaryExpression)a, (BinaryExpression)b);
                case ExpressionType.TypeIs:
                    return CompareTypeIs((TypeBinaryExpression)a, (TypeBinaryExpression)b);
                case ExpressionType.Conditional:
                    return CompareConditional((ConditionalExpression)a, (ConditionalExpression)b);
                case ExpressionType.Constant:
                    return CompareConstant((ConstantExpression)a, (ConstantExpression)b);
                case ExpressionType.Parameter:
                    return CompareParameter((ParameterExpression)a, (ParameterExpression)b);
                case ExpressionType.MemberAccess:
                    return CompareMemberAccess((MemberExpression)a, (MemberExpression)b);
                case ExpressionType.Call:
                    return CompareMethodCall((MethodCallExpression)a, (MethodCallExpression)b);
                case ExpressionType.Lambda:
                    return CompareLambda((LambdaExpression)a, (LambdaExpression)b);
                case ExpressionType.New:
                    return CompareNew((NewExpression)a, (NewExpression)b);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return CompareNewArray((NewArrayExpression)a, (NewArrayExpression)b);
                case ExpressionType.Invoke:
                    return CompareInvocation((InvocationExpression)a, (InvocationExpression)b);
                case ExpressionType.MemberInit:
                    return CompareMemberInit((MemberInitExpression)a, (MemberInitExpression)b);
                case ExpressionType.ListInit:
                    return CompareListInit((ListInitExpression)a, (ListInitExpression)b);
                case ExpressionType.Extension:
                    var extensionA = (ExtensionExpression)a;
                    var extensionB = (ExtensionExpression)b;
                    if (extensionA.ExtensionType != extensionB.ExtensionType)
                    {
                        return false;
                    }
                    switch (extensionA.ExtensionType)
                    {
                        case ExtensionExpressionType.Accumulator:
                            return CompareAccumulator((AccumulatorExpression)extensionA, (AccumulatorExpression)extensionB);
                        case ExtensionExpressionType.Document:
                            return CompareDocument((DocumentExpression)a, (DocumentExpression)b);
                        case ExtensionExpressionType.FieldAsDocument:
                            return CompareDocumentWrappedField((FieldAsDocumentExpression)a, (FieldAsDocumentExpression)b);
                        case ExtensionExpressionType.Field:
                            return CompareField((FieldExpression)a, (FieldExpression)b);
                        case ExtensionExpressionType.SerializedConstant:
                            return CompareSerializedConstant((SerializedConstantExpression)a, (SerializedConstantExpression)b);
                        default:
                            throw new MongoInternalException(string.Format("Unhandled mongo expression type: '{0}'", extensionA.ExtensionType));
                    }
                default:
                    throw new MongoInternalException(string.Format("Unhandled expression type: '{0}'", a.NodeType));
            }
        }

        // private methods
        private bool CompareAccumulator(AccumulatorExpression a, AccumulatorExpression b)
        {
            return a.AccumulatorType == b.AccumulatorType
                && Compare(a.Argument, b.Argument);
        }

        private bool CompareDocument(DocumentExpression a, DocumentExpression b)
        {
            // not exact...
            return a.Serializer.GetType() == b.Serializer.GetType();
        }

        private bool CompareDocumentWrappedField(FieldAsDocumentExpression a, FieldAsDocumentExpression b)
        {
            return a.FieldName == b.FieldName
                && Compare(a.Expression, b.Expression);
        }

        private bool CompareField(FieldExpression a, FieldExpression b)
        {
            return a.FieldName == b.FieldName
                && a.Serializer.GetType() == b.Serializer.GetType()
                && Compare(a.Original, b.Original);
        }

        private bool CompareSerializedConstant(SerializedConstantExpression a, SerializedConstantExpression b)
        {
            return CompareConstantValues(a.Value, b.Value);
        }

        private bool CompareUnary(UnaryExpression a, UnaryExpression b)
        {
            return a.NodeType == b.NodeType
                && a.Method == b.Method
                && a.IsLifted == b.IsLifted
                && a.IsLiftedToNull == b.IsLiftedToNull
                && Compare(a.Operand, b.Operand);
        }

        private bool CompareBinary(BinaryExpression a, BinaryExpression b)
        {
            return a.NodeType == b.NodeType
                && a.Method == b.Method
                && a.IsLifted == b.IsLifted
                && a.IsLiftedToNull == b.IsLiftedToNull
                && Compare(a.Left, b.Left)
                && Compare(a.Right, b.Right);
        }

        private bool CompareTypeIs(TypeBinaryExpression a, TypeBinaryExpression b)
        {
            return a.TypeOperand == b.TypeOperand
                && Compare(a.Expression, b.Expression);
        }

        private bool CompareConditional(ConditionalExpression a, ConditionalExpression b)
        {
            return Compare(a.Test, b.Test)
                && Compare(a.IfTrue, b.IfTrue)
                && Compare(a.IfFalse, b.IfFalse);
        }

        private bool CompareConstant(ConstantExpression a, ConstantExpression b)
        {
            return CompareConstantValues(a.Value, b.Value);
        }

        private bool CompareConstantValues(object a, object b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            if (a is IQueryable && b is IQueryable && a.GetType() == b.GetType())
            {
                return true;
            }

            return object.Equals(a, b);
        }

        private bool CompareParameter(ParameterExpression a, ParameterExpression b)
        {
            if (_parameterScope != null)
            {
                ParameterExpression mapped;
                if (_parameterScope.TryGetValue(a, out mapped))
                {
                    return mapped == b;
                }
            }

            return a.Type == b.Type && a.Name == b.Name;
        }

        private bool CompareMemberAccess(MemberExpression a, MemberExpression b)
        {
            return a.Member == b.Member
                && Compare(a.Expression, b.Expression);
        }

        private bool CompareMethodCall(MethodCallExpression a, MethodCallExpression b)
        {
            return a.Method == b.Method
                && Compare(a.Object, b.Object)
                && CompareExpressionList(a.Arguments, b.Arguments);
        }

        private bool CompareLambda(LambdaExpression a, LambdaExpression b)
        {
            int n = a.Parameters.Count;
            if (b.Parameters.Count != n)
            {
                return false;
            }
            // all must have same type
            for (int i = 0; i < n; i++)
            {
                if (a.Parameters[i].Type != b.Parameters[i].Type)
                {
                    return false;
                }
            }
            var save = _parameterScope;
            _parameterScope = new ScopedDictionary<ParameterExpression, ParameterExpression>(_parameterScope);
            try
            {
                for (int i = 0; i < n; i++)
                {
                    _parameterScope.Add(a.Parameters[i], b.Parameters[i]);
                }
                return Compare(a.Body, b.Body);
            }
            finally
            {
                _parameterScope = save;
            }
        }

        private bool CompareNew(NewExpression a, NewExpression b)
        {
            return a.Constructor == b.Constructor
                && CompareExpressionList(a.Arguments, b.Arguments)
                && CompareMemberList(a.Members, b.Members);
        }

        private bool CompareExpressionList(ReadOnlyCollection<Expression> a, ReadOnlyCollection<Expression> b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            if (a.Count != b.Count)
            {
                return false;
            }
            for (int i = 0, n = a.Count; i < n; i++)
            {
                if (!Compare(a[i], b[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CompareMemberList(ReadOnlyCollection<MemberInfo> a, ReadOnlyCollection<MemberInfo> b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            if (a.Count != b.Count)
            {
                return false;
            }
            for (int i = 0, n = a.Count; i < n; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        private bool CompareNewArray(NewArrayExpression a, NewArrayExpression b)
        {
            return CompareExpressionList(a.Expressions, b.Expressions);
        }

        private bool CompareInvocation(InvocationExpression a, InvocationExpression b)
        {
            return Compare(a.Expression, b.Expression)
                && CompareExpressionList(a.Arguments, b.Arguments);
        }

        private bool CompareMemberInit(MemberInitExpression a, MemberInitExpression b)
        {
            return Compare(a.NewExpression, b.NewExpression)
                && CompareBindingList(a.Bindings, b.Bindings);
        }

        private bool CompareBindingList(ReadOnlyCollection<MemberBinding> a, ReadOnlyCollection<MemberBinding> b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            if (a.Count != b.Count)
            {
                return false;
            }
            for (int i = 0, n = a.Count; i < n; i++)
            {
                if (!CompareBinding(a[i], b[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CompareBinding(MemberBinding a, MemberBinding b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            if (a.BindingType != b.BindingType)
            {
                return false;
            }
            if (a.Member != b.Member)
            {
                return false;
            }
            switch (a.BindingType)
            {
                case MemberBindingType.Assignment:
                    return CompareMemberAssignment((MemberAssignment)a, (MemberAssignment)b);
                case MemberBindingType.ListBinding:
                    return CompareMemberListBinding((MemberListBinding)a, (MemberListBinding)b);
                case MemberBindingType.MemberBinding:
                    return CompareMemberMemberBinding((MemberMemberBinding)a, (MemberMemberBinding)b);
                default:
                    throw new Exception(string.Format("Unhandled binding type: '{0}'", a.BindingType));
            }
        }

        private bool CompareMemberAssignment(MemberAssignment a, MemberAssignment b)
        {
            return a.Member == b.Member
                && Compare(a.Expression, b.Expression);
        }

        private bool CompareMemberListBinding(MemberListBinding a, MemberListBinding b)
        {
            return a.Member == b.Member
                && CompareElementInitList(a.Initializers, b.Initializers);
        }

        private bool CompareMemberMemberBinding(MemberMemberBinding a, MemberMemberBinding b)
        {
            return a.Member == b.Member
                && CompareBindingList(a.Bindings, b.Bindings);
        }

        private bool CompareListInit(ListInitExpression a, ListInitExpression b)
        {
            return Compare(a.NewExpression, b.NewExpression)
                && CompareElementInitList(a.Initializers, b.Initializers);
        }

        private bool CompareElementInitList(ReadOnlyCollection<ElementInit> a, ReadOnlyCollection<ElementInit> b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            if (a.Count != b.Count)
            {
                return false;
            }
            for (int i = 0, n = a.Count; i < n; i++)
            {
                if (!CompareElementInit(a[i], b[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CompareElementInit(ElementInit a, ElementInit b)
        {
            return a.AddMethod == b.AddMethod
                && CompareExpressionList(a.Arguments, b.Arguments);
        }

        private class ScopedDictionary<TKey, TValue>
        {
            private readonly Dictionary<TKey, TValue> _map;
            private readonly ScopedDictionary<TKey, TValue> _previous;

            public ScopedDictionary(ScopedDictionary<TKey, TValue> previous)
            {
                _previous = previous;
                _map = new Dictionary<TKey, TValue>();
            }

            public ScopedDictionary(ScopedDictionary<TKey, TValue> previous, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
                : this(previous)
            {
                foreach (var p in pairs)
                {
                    _map.Add(p.Key, p.Value);
                }
            }

            public void Add(TKey key, TValue value)
            {
                _map.Add(key, value);
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                var current = this;
                while (current != null)
                {
                    if (current._map.TryGetValue(key, out value))
                    {
                        return true;
                    }
                    current = current._previous;
                }
                value = default(TValue);
                return false;
            }

            public bool ContainsKey(TKey key)
            {
                var current = this;
                while (current != null)
                {
                    if (current._map.ContainsKey(key))
                    {
                        return true;
                    }
                    current = current._previous;
                }
                return false;
            }
        }
    }
}