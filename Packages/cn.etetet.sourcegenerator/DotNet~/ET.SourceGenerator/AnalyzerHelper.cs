using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Exception = System.Exception;

namespace ET
{
    public static class AnalyzerHelper
    {
        /// <summary>
        ///     获取语法树节点的子节点中第一个指定类型节点
        /// </summary>
        /// <param name="syntaxNode">语法树节点</param>
        /// <typeparam name="T">指定语法节点类型</typeparam>
        /// <returns>第一个指定类型节点</returns>
        public static T? GetFirstChild<T>(this SyntaxNode syntaxNode) where T : SyntaxNode
        {
            foreach (SyntaxNode? childNode in syntaxNode.ChildNodes())
            {
                if (childNode.GetType() == typeof (T))
                {
                    return childNode as T;
                }
            }

            return null;
        }

        public static SyntaxNode? GetFirstChild(this SyntaxNode syntaxNode)
        {
            var childNodes = syntaxNode.ChildNodes();
            if (childNodes.Count() > 0)
            {
                return childNodes.First();
            }

            return null;
        }

        /// <summary>
        ///     获取语法树节点的子节点中最后一个指定类型节点
        /// </summary>
        /// <param name="syntaxNode">语法树节点</param>
        /// <typeparam name="T">指定语法节点类型</typeparam>
        /// <returns>最后一个指定类型节点</returns>
        public static T? GetLastChild<T>(this SyntaxNode syntaxNode) where T : SyntaxNode
        {
            foreach (SyntaxNode? childNode in syntaxNode.ChildNodes().Reverse())
            {
                if (childNode.GetType() == typeof (T))
                {
                    return childNode as T;
                }
            }

            return null;
        }

        /// <summary>
        ///     获取语法节点所属的ClassDeclarationSyntax
        /// </summary>
        public static ClassDeclarationSyntax? GetParentClassDeclaration(this SyntaxNode syntaxNode)
        {
            SyntaxNode? parentNode = syntaxNode.Parent;
            while (parentNode != null)
            {
                if (parentNode is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    return classDeclarationSyntax;
                }

                parentNode = parentNode.Parent;
            }

            return null;
        }

        /// <summary>
        ///     INamedTypeSymbol 是否有指定的Attribute
        /// </summary>
        public static bool HasAttribute(this ITypeSymbol typeSymbol, string AttributeName)
        {
            foreach (AttributeData? attributeData in typeSymbol.GetAttributes())
            {
                if (attributeData.AttributeClass?.ToString() == AttributeName)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasAttributeInTypeAndBaseTyes(this ITypeSymbol typeSymbol, string AttributeName)
        {
            if (typeSymbol.HasAttribute(AttributeName))
            {
                return true;
            }

            foreach (var baseType in typeSymbol.BaseTypes())
            {
                if (baseType.HasAttribute(AttributeName))
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<ITypeSymbol> BaseTypes(this ITypeSymbol typeSymbol)
        {
            ITypeSymbol? baseType = typeSymbol.BaseType;
            while (baseType!=null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }

        /// <summary>
        ///     INamedTypeSymbol 是否有指定的基类Attribute
        /// </summary>
        public static bool HasAttribute(this INamedTypeSymbol namedTypeSymbol, string AttributeName)
        {
            foreach (AttributeData? attributeData in namedTypeSymbol.GetAttributes())
            {
                INamedTypeSymbol? attributeType = attributeData.AttributeClass;
                
                while (attributeType != null)
                {
                    if (attributeType.ToString() == AttributeName)
                    {
                        return true;
                    }

                    attributeType = attributeType.BaseType;
                }
            }

            return false;
        }

        /// <summary>
        ///     INamedTypeSymbol 获取指定类型的第一个Attribute
        /// </summary>
        public static AttributeData? GetFirstAttribute(this INamedTypeSymbol namedTypeSymbol, string AttributeName)
        {
            foreach (AttributeData? attributeData in namedTypeSymbol.GetAttributes())
            {
                if (attributeData.AttributeClass?.ToString() == AttributeName)
                {
                    return attributeData;
                }
            }

            return null;
        }

        /// <summary>
        ///     INamedTypeSymbol 是否含有指定接口
        /// </summary>
        public static bool HasInterface(this INamedTypeSymbol namedTypeSymbol, string InterfaceName)
        {
            foreach (INamedTypeSymbol? iInterface in namedTypeSymbol.AllInterfaces)
            {
                if (iInterface.IsInterface(InterfaceName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 某个接口symbol 是否是指定的接口
        /// </summary>

        public static bool IsInterface(this INamedTypeSymbol namedTypeSymbol, string InterfaceName)
        {
            return $"{namedTypeSymbol.GetNameSpace()}.{namedTypeSymbol.Name}" == InterfaceName;
        }

        /// <summary>
        ///     判断指定的程序集是否需要分析
        /// </summary>
        public static bool IsAssemblyNeedAnalyze(string? assemblyName, params string[] analyzeAssemblyNames)
        {
            if (assemblyName == null)
            {
                return false;
            }

            foreach (string analyzeAssemblyName in analyzeAssemblyNames)
            {
                if (assemblyName == analyzeAssemblyName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取 成员访问语法节点的父级类型
        /// </summary>
        public static ITypeSymbol? GetMemberAccessSyntaxParentType(this MemberAccessExpressionSyntax memberAccessExpressionSyntax,
        SemanticModel semanticModel)
        {
            SyntaxNode? firstChildSyntaxNode = memberAccessExpressionSyntax.GetFirstChild();
            if (firstChildSyntaxNode == null)
            {
                return null;
            }

            ISymbol? firstChildSymbol = semanticModel.GetSymbolInfo(firstChildSyntaxNode).Symbol;
            if (firstChildSymbol == null)
            {
                return null;
            }

            if (firstChildSymbol is ILocalSymbol localSymbol)
            {
                return localSymbol.Type;
            }

            if (firstChildSymbol is IParameterSymbol parameterSymbol)
            {
                return parameterSymbol.Type;
            }

            if (firstChildSymbol is IPropertySymbol propertySymbol)
            {
                return propertySymbol.Type;
            }

            if (firstChildSymbol is IMethodSymbol methodSymbol)
            {
                return methodSymbol.ReturnType;
            }

            if (firstChildSymbol is IFieldSymbol fieldSymbol)
            {
                return fieldSymbol.Type;
            }

            if (firstChildSymbol is IEventSymbol eventSymbol)
            {
                return eventSymbol.Type;
            }

            return null;
        }
        
        /// <summary>
        /// 获取最近的指定类型祖先节点
        /// </summary>
        public static T? GetNeareastAncestor<T>(this SyntaxNode syntaxNode) where T:SyntaxNode
        {
            
            foreach (var ancestorNode in syntaxNode.Ancestors())
            {
                if (ancestorNode is T Tancestor)
                {
                    return Tancestor;
                }
            }
            return null ;
        }
        
        /// <summary>
        /// 判断函数是否是否含有指定类型的参数
        /// </summary>
        public static bool HasParameterType(this IMethodSymbol methodSymbol, string parameterType, out IParameterSymbol? cencelTokenSymbol)
        {
            foreach (var parameterSymbol in methodSymbol.Parameters)
            {
                if (parameterSymbol.Type.ToString() == parameterType)
                {
                    cencelTokenSymbol = parameterSymbol;
                    return true;
                }
            }
            cencelTokenSymbol = null;
            return false;
        }

        /// <summary>
        /// 获取所有指定类型的子节点
        /// </summary>
        public static IEnumerable<T> DescendantNodes<T>(this SyntaxNode syntaxNode) where T : SyntaxNode
        {
            foreach (var descendantNode in syntaxNode.DescendantNodes())
            {
                if (descendantNode is T node)
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// 获取与该语法节点同层级的上一个节点
        /// </summary>
        public static SyntaxNode? PreviousNode(this SyntaxNode syntaxNode)
        {
            if (syntaxNode.Parent==null)
            {
                return null;
            }
            
            int index = 0;
            foreach (var childNode in syntaxNode.Parent.ChildNodes())
            {
                if (childNode == syntaxNode)
                {
                    break;
                }
                index++;
            }

            if (index==0)
            {
                return null;
            }
            
            return syntaxNode.Parent.ChildNodes().ElementAt(index-1);
        }

        /// <summary>
        /// 获取与该语法节点同层级的下一个节点
        /// </summary>
        public static SyntaxNode? NextNode(this SyntaxNode syntaxNode)
        {
            if (syntaxNode.Parent==null)
            {
                return null;
            }
            
            int index = 0;
            
            foreach (var childNode in syntaxNode.Parent.ChildNodes())
            {
                if (childNode == syntaxNode)
                {
                    break;
                }
                index++;
            }

            if (index == syntaxNode.Parent.ChildNodes().Count()-1)
            {
                return null;
            }
            
            return syntaxNode.Parent.ChildNodes().ElementAt(index+1);
        }
        
        /// <summary>
        /// 获取await表达式所在的控制流block
        /// </summary>
        public static BasicBlock? GetAwaitStatementControlFlowBlock(StatementSyntax statementSyntax,AwaitExpressionSyntax awaitExpressionSyntax ,SemanticModel semanticModel)
        {
            // 跳过 return 表达式
            if (statementSyntax.IsKind(SyntaxKind.ReturnStatement))
            {
                return null;
            }
            
            var methodSyntax = statementSyntax.GetNeareastAncestor<MethodDeclarationSyntax>();
            if (methodSyntax==null)
            {
                return null;
            }

            // 构建表达式所在函数的控制流图
            var controlFlowGraph = ControlFlowGraph.Create(methodSyntax, semanticModel);

            if (controlFlowGraph==null)
            {
                return null;
            }

            if (statementSyntax is LocalDeclarationStatementSyntax)
            {
                return null;
            }
            
            BasicBlock? block = controlFlowGraph.Blocks.FirstOrDefault(x => x.Operations.Any(y => y.Syntax.Contains(statementSyntax)));
            return block;
        }
        
        /// <summary>
        /// 判断类是否为partial类
        /// </summary>
        public static bool IsPartial(this ClassDeclarationSyntax classDeclaration)
        {
            foreach (var modifier in classDeclaration.Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.PartialKeyword))
                {
                    return true;
                }
            }

            return false;
        }

        public static string? GetNameSpace(this INamedTypeSymbol namedTypeSymbol)
        {
            INamespaceSymbol? namespaceSymbol = namedTypeSymbol.ContainingNamespace;
            string? namespaceName = namespaceSymbol?.Name;
            while (namespaceSymbol?.ContainingNamespace != null)
            {
                namespaceSymbol = namespaceSymbol.ContainingNamespace;
                if (string.IsNullOrEmpty(namespaceSymbol.Name))
                {
                    break;
                }

                namespaceName = $"{namespaceSymbol.Name}.{namespaceName}";
            }

            if (string.IsNullOrEmpty(namespaceName))
            {
                return null;
            }

            return namespaceName;
        }

        /// <summary>
        /// 根据语义模型的文件路径 判断是否需要分析
        /// </summary>
        public static bool IsSemanticModelNeedAnalyze(SemanticModel semanticModel, params string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                if (semanticModel.SyntaxTree.FilePath.Contains(filePath))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 类型symbol是否有指定名字 指定参数的方法
        /// </summary>
        public static bool HasMethodWithParams(this INamedTypeSymbol namedTypeSymbol, string methodName, params ITypeSymbol[] typeSymbols)
        {
            
            foreach (var member in namedTypeSymbol.GetMembers())
            {
                if (member is not IMethodSymbol methodSymbol)
                {
                    continue;
                }

                if (methodSymbol.Name!=methodName)
                {
                    continue;
                }
                
                if (typeSymbols.Length!=methodSymbol.Parameters.Length)
                {
                    continue;
                }
                
                if (typeSymbols.Length==0)
                {
                    return true;
                }

                bool isEqual = true;
                
                for (int i = 0; i < typeSymbols.Length; i++)
                {
                    if (typeSymbols[i].ToString()!=methodSymbol.Parameters[i].Type.ToString())
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (isEqual)
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// 类型symbol是否有指定名字 指定参数的方法
        /// </summary>
        public static bool HasMethodWithParams(this INamedTypeSymbol namedTypeSymbol, string methodName, params string[] typeSymbols)
        {
            
            foreach (var member in namedTypeSymbol.GetMembers())
            {
                if (member is not IMethodSymbol methodSymbol)
                {
                    continue;
                }

                if (methodSymbol.Name!=methodName)
                {
                    continue;
                }
                
                if (typeSymbols.Length!=methodSymbol.Parameters.Length)
                {
                    continue;
                }
                
                if (typeSymbols.Length==0)
                {
                    return true;
                }

                bool isEqual = true;
                
                for (int i = 0; i < typeSymbols.Length; i++)
                {
                    if (typeSymbols[i]!=methodSymbol.Parameters[i].Type.ToString())
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (isEqual)
                {
                    return true;
                }
            }

            return false;
        }
        
        


        /// <summary>
        /// 方法symbol 是否有指定的attribute
        /// </summary>
        public static bool HasAttribute(this IMethodSymbol methodSymbol, string AttributeName)
        {
            foreach (AttributeData? attributeData in methodSymbol.GetAttributes())
            {
                if (attributeData?.AttributeClass?.ToString()==AttributeName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 类型symbol是否是实体类 包含 enity及其子类 lsentity及其子类
        /// </summary>
        public static bool IsETEntity(this ITypeSymbol typeSymbol)
        {
            string typeName = typeSymbol.ToString();
            string? baseType = typeSymbol.BaseType?.ToString();
            return typeName == Definition.EntityType || baseType == Definition.EntityType || baseType == Definition.LSEntityType;
        }

        /// <summary>
        /// 类型symbol是否是EntiyRef 或EntityWeakRef类
        /// </summary>
        public static bool IsEntityRefOrEntityWeakRef(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            {
                return false;
            }
            

            if (!namedTypeSymbol.IsGenericType)
            {
                return false;
            }

            string typeName = namedTypeSymbol.Name;
            return typeName is Definition.EntityRefType or Definition.EntityWeakRefType;
        }
    }
}