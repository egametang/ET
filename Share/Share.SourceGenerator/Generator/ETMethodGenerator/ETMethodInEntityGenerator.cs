using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ET.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ET.Generator;

[Generator(LanguageNames.CSharp)]
public class ETMethodInEntityGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications (() => ETMethodInEntityGeneratorSyntaxContextReceiver.Create());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not ETMethodInEntityGeneratorSyntaxContextReceiver receiver)
        {
            return;
        }

        foreach (var kv in receiver.ETMethods)
        {
            GenerateCSFiles(kv.Key,kv.Value,context);
        }

    }

    private void GenerateCSFiles(ClassDeclarationSyntax classDeclarationSyntax, HashSet<MethodDeclarationSyntax> methodDeclarationSyntaxes,
    GeneratorExecutionContext context)
    {
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
        INamedTypeSymbol? namedTypeSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (namedTypeSymbol==null)
        {
            return;
        }
        
        string? nameSpace = namedTypeSymbol.GetNameSpace();
        string className = namedTypeSymbol.Name;
        string FuncPointerContent = GenerateFuncPointerContent(classDeclarationSyntax,methodDeclarationSyntaxes,context,semanticModel);
        string partialMethodImplContent = GeneratePartialMethodImplContent(classDeclarationSyntax,methodDeclarationSyntaxes,context,semanticModel);
        string code =
$$"""
using System.Runtime.CompilerServices;
namespace {{nameSpace}}
{
    public partial class {{className}}
    {
        public partial class FuncPointer
        {
{{FuncPointerContent}}
        }
        
{{partialMethodImplContent}}

    }
}
""";
        
        context.AddSource($"{nameSpace}_{className}_ETMethodInEntityGenerator.g.cs",code);
    }

    private string GenerateFuncPointerContent(ClassDeclarationSyntax classDeclarationSyntax, HashSet<MethodDeclarationSyntax> methodDeclarationSyntaxes,
    GeneratorExecutionContext context,SemanticModel semanticModel)
    {
        StringBuilder sb = new StringBuilder();
        INamedTypeSymbol? namedTypeSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (namedTypeSymbol==null)
        {
            return string.Empty;
        }
        
        foreach (var methodDeclarationSyntax in methodDeclarationSyntaxes)
        {
            IMethodSymbol? methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
            if (methodSymbol==null)
            {
                continue;
            }

            string typeArgs = $"{namedTypeSymbol},";
            
            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                string type = methodSymbol.Parameters[i].Type.ToString();
                typeArgs = $"{typeArgs}{type},";
            }
            
            typeArgs = $"{typeArgs}{methodSymbol.ReturnType}";

            string name = methodSymbol.Name;
            
            string code = 
$$"""

            public static unsafe delegate*<{{typeArgs}}> {{name}};
""";
            sb.AppendLine(code);
        }
        return sb.ToString();
    }

    private string GeneratePartialMethodImplContent(ClassDeclarationSyntax classDeclarationSyntax, HashSet<MethodDeclarationSyntax> methodDeclarationSyntaxes,
    GeneratorExecutionContext context,SemanticModel semanticModel)
    {
        StringBuilder sb = new StringBuilder();
        
        foreach (var methodDeclarationSyntax in methodDeclarationSyntaxes)
        {
            IMethodSymbol? methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
            if (methodSymbol==null)
            {
                continue;
            }
            
            string content = string.Empty;
            string argsList = string.Empty;
            foreach (var parameterSymbol in methodSymbol.Parameters)
            {
                argsList = $"{argsList},{parameterSymbol.Name}";
            }
            content = $"FuncPointer.{methodSymbol.Name}(this{argsList});";

            if (!methodSymbol.ReturnsVoid)
            {
                content = $"return {content}";
            }

            var newMethodSyntax = methodDeclarationSyntax.WithAttributeLists(new SyntaxList<AttributeListSyntax>());
            string code = 
$$"""
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        {{newMethodSyntax.ToString().Replace(";", "")}}
        {
            unsafe
            {
                {{content}}
            }
        }

""";
            sb.AppendLine(code);
        }
        return sb.ToString();
    }
    
    
    class ETMethodInEntityGeneratorSyntaxContextReceiver: ISyntaxContextReceiver
    {
        internal static ISyntaxContextReceiver Create()
        {
            return new ETMethodInEntityGeneratorSyntaxContextReceiver();
        }
        
        public Dictionary<ClassDeclarationSyntax, HashSet<MethodDeclarationSyntax>> ETMethods =
                new ();
        
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
            {
                return;
            }
            
            var classTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            if (classTypeSymbol==null)
            {
                return;
            }

            var baseType = classTypeSymbol.BaseType?.ToString();
            
            // 筛选出实体类
            if (baseType!= Definition.EntityType && baseType != Definition.LSEntityType)
            {
                return;
            }

            // 筛选出 partial 实体类
            if (!classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return;
            }

            var methodDeclarationSyntaxes = classDeclarationSyntax.ChildNodes().OfType<MethodDeclarationSyntax>();

            if (!methodDeclarationSyntaxes.Any())
            {
                return;
            }

            // 筛选出非静态 partial空方法
            foreach (var methodDeclarationSyntax in methodDeclarationSyntaxes)
            {
                if (methodDeclarationSyntax.IsPartialEmptyMethod()&&!methodDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
                {
                    if (!ETMethods.ContainsKey(classDeclarationSyntax))
                    {
                        ETMethods.Add(classDeclarationSyntax, new HashSet<MethodDeclarationSyntax>());
                    }

                    ETMethods[classDeclarationSyntax].Add(methodDeclarationSyntax);
                }
            }
        }
    }
}