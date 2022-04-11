using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ET.Analyzer
{
    public static class AnalyzerHelper
    {
        /// <summary>
        /// 获取语法树节点的子节点中第一个指定类型节点
        /// </summary>
        /// <param name="syntaxNode">语法树节点</param>
        /// <typeparam name="T">指定语法节点类型</typeparam>
        /// <returns>第一个指定类型节点</returns>
        public static T? GetFirstChild<T>(this SyntaxNode syntaxNode) where T: SyntaxNode
        {
            foreach (var childNode in syntaxNode.ChildNodes())
            {
                if (childNode.GetType()==typeof(T))
                {
                    return childNode as T;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 获取语法树节点的子节点中最后一个指定类型节点
        /// </summary>
        /// <param name="syntaxNode">语法树节点</param>
        /// <typeparam name="T">指定语法节点类型</typeparam>
        /// <returns>最后一个指定类型节点</returns>
        public static T? GetLastChild<T>(this SyntaxNode syntaxNode) where T : SyntaxNode
        {
            foreach (var childNode in syntaxNode.ChildNodes().Reverse())
            {
                if (childNode.GetType() == typeof(T))
                {
                    return childNode as T;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 获取语法节点所属的ClassDeclarationSyntax
        /// </summary>
        public static ClassDeclarationSyntax? GetParentClassDeclaration(this SyntaxNode syntaxNode)
        {
            var parentNode = syntaxNode.Parent;
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
        /// INamedTypeSymbol 是否含有指定类型的Attribute
        /// </summary>
        public static bool HasAttribute(this INamedTypeSymbol namedTypeSymbol, string AttributeName)
        {
            foreach (var attributeData in namedTypeSymbol.GetAttributes())
            {
                if (attributeData.AttributeClass?.ToString() == AttributeName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// INamedTypeSymbol 获取指定类型的第一个Attribute
        /// </summary>
        public static AttributeData? GetFirstAttribute(this INamedTypeSymbol namedTypeSymbol, string AttributeName)
        {
            foreach (var attributeData in namedTypeSymbol.GetAttributes())
            {
                if (attributeData.AttributeClass?.ToString() == AttributeName)
                {
                    return attributeData;
                }
            }
            return null;
        }

        /// <summary>
        /// INamedTypeSymbol 是否含有指定接口
        /// </summary>
        public static bool HasInterface(this INamedTypeSymbol namedTypeSymbol, string InterfaceName)
        {
            foreach (var iInterface in namedTypeSymbol.AllInterfaces)
            {
                if (iInterface.ToString()==InterfaceName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断指定的程序集是否需要分析
        /// </summary>
        public static bool IsAssemblyNeedAnalyze(string? assemblyName,  params string[] analyzeAssemblyNames)
        {
            if (assemblyName==null)
            {
                return false;
            }
            foreach (var analyzeAssemblyName in analyzeAssemblyNames)
            {
                if (assemblyName==analyzeAssemblyName)
                {
                    return true;
                }
            }
            return false;
        }
        
        
    }
}

