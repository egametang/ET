/* Copyright 2015-2016 MongoDB Inc.
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
using System.Linq.Expressions;
using System.Reflection;

namespace MongoDB.Driver.Linq.Processors
{
    internal sealed class ProjectionMapping
    {
        public ConstructorInfo Constructor;
        public Expression Expression;
        public List<ProjectionMemberMapping> Members;
    }

    internal sealed class ProjectionMemberMapping
    {
        public MemberInfo Member;
        public Expression Expression;
        public ParameterInfo Parameter;
    }

    internal sealed class ProjectionMapper
    {
        public static ProjectionMapping Map(Expression node)
        {
            var mapper = new ProjectionMapper();
            mapper.Visit(node);
            return new ProjectionMapping
            {
                Constructor = mapper._constructor,
                Expression = node,
                Members = mapper._mappings
            };
        }

        private ConstructorInfo _constructor;
        private List<ProjectionMemberMapping> _mappings;

        private ProjectionMapper()
        {
            _mappings = new List<ProjectionMemberMapping>();
        }

        public void Visit(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.MemberInit:
                    VisitMemberInit((MemberInitExpression)node);
                    break;
                case ExpressionType.New:
                    VisitNew((NewExpression)node);
                    break;
                default:
                    var message = string.Format("Only new expressions are supported in $project and $group, but found {0}.",
                        node.ToString());
                    throw new NotSupportedException(message);
            }
        }

        private void VisitMemberInit(MemberInitExpression node)
        {
            foreach (var memberBinding in node.Bindings)
            {
                var memberAssignment = memberBinding as MemberAssignment;
                if (memberAssignment != null)
                {
                    _mappings.Add(new ProjectionMemberMapping
                    {
                        Expression = memberAssignment.Expression,
                        Member = memberAssignment.Member
                    });
                }
                else
                {
                    var message = string.Format("Only member assignments are supported in a new expression in $project and $group, but found {0}.",
                        node.ToString());
                    throw new NotSupportedException(message);
                }
            }

            VisitNew(node.NewExpression);
        }

        private void VisitNew(NewExpression node)
        {
            _constructor = node.Constructor;

            var type = node.Type;
            foreach (var parameter in node.Constructor.GetParameters())
            {
                MemberInfo member;
                if (node.Members != null)
                {
                    // anonymous types will have this set...
                    member = node.Members[parameter.Position];
                }
                else
                {
                    var members = type.GetTypeInfo().GetMember(
                        parameter.Name,
                        MemberTypes.Field | MemberTypes.Property,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

                    if (members.Length != 1)
                    {
                        var message = string.Format("Could not find a member match for constructor parameter {0} on type {1} in the expression tree {2}.",
                            parameter.Name,
                            type.Name,
                            node.ToString());
                        throw new NotSupportedException(message);
                    }

                    member = members[0];
                }

                _mappings.Add(new ProjectionMemberMapping
                {
                    Expression = node.Arguments[parameter.Position],
                    Member = member,
                    Parameter = parameter
                });
            }
        }
    }
}