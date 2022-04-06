using Microsoft.CodeAnalysis;

namespace ET.Analyzer
{
    public static class AnalyzerHelper
    {
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
    }
}

