using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ET.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ET.Generator;

[Generator(LanguageNames.CSharp)]
public class ETSystemGenerator: ISourceGenerator
{
    private AttributeTemplate? templates;

    public void Initialize(GeneratorInitializationContext context)
    {
        this.templates = new AttributeTemplate();
        context.RegisterForSyntaxNotifications(SyntaxContextReceiver.Create);
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SyntaxContextReceiver receiver || receiver.MethodDeclarations.Count == 0)
        {
            return;
        }

        foreach (var kv in receiver.MethodDeclarations)
        {
            this.GenerateCSFiles(kv.Key, kv.Value, context);
        }
    }

    /// <summary>
    /// 每个静态类生成一个cs文件
    /// </summary>
    private void GenerateCSFiles(ClassDeclarationSyntax classDeclarationSyntax, HashSet<MethodDeclarationSyntax> methodDeclarationSyntaxes,
    GeneratorExecutionContext context)
    {
        string className = classDeclarationSyntax.Identifier.Text;
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
        INamedTypeSymbol? classTypeSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
        if (classTypeSymbol == null)
        {
            return;
        }

        if (!classTypeSymbol.IsStatic || !classDeclarationSyntax.IsPartial())
        {
            Diagnostic diagnostic = Diagnostic.Create(ETSystemMethodIsInStaticPartialClassRule.Rule, classDeclarationSyntax.GetLocation(),
                classDeclarationSyntax.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        INamespaceSymbol? namespaceSymbol = classTypeSymbol?.ContainingNamespace;
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

        if (namespaceName == null)
        {
            throw new Exception($"{className} namespace is null");
        }

        this.GenerateSystemCodeByTemplate(namespaceName, className, classDeclarationSyntax, methodDeclarationSyntaxes, context, semanticModel);
    }

    /// <summary>
    /// 根据模板生成System代码
    /// </summary>
    private void GenerateSystemCodeByTemplate(string namespaceName, string className, ClassDeclarationSyntax classDeclarationSyntax,
    HashSet<MethodDeclarationSyntax> methodDeclarationSyntaxes, GeneratorExecutionContext context, SemanticModel semanticModel)
    {
        if (this.templates == null)
        {
            throw new Exception("attribute template is null");
        }
        
        foreach (MethodDeclarationSyntax? methodDeclarationSyntax in methodDeclarationSyntaxes)
        {
            IMethodSymbol? methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;
            if (methodSymbol == null)
            {
                continue;
            }

            ParameterSyntax? componentParam = methodDeclarationSyntax.ParameterList.Parameters.FirstOrDefault();
            if (componentParam == null)
            {
                continue;
            }

            string methodName = methodDeclarationSyntax.Identifier.Text;
            string? componentName = componentParam.Type?.ToString();

            StringBuilder argsTypes = new StringBuilder();
            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                argsTypes.Append(i == 0? $"{methodSymbol.Parameters[i].Type}" : $",{methodSymbol.Parameters[i].Type}");
            }

            StringBuilder argsTypeVars = new StringBuilder();
            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                argsTypeVars.Append(i == 0? $"{methodSymbol.Parameters[i].Type} self" : $",{methodSymbol.Parameters[i].Type} args{i}");
            }

            StringBuilder argsVars = new StringBuilder();
            if (methodSymbol.Parameters.Length > 1)
            {
                for (int i = 1; i < methodSymbol.Parameters.Length; i++)
                {
                    argsVars.Append(i == 1? $"args1" : $",args{i}");
                }
            }

            foreach (AttributeListSyntax attributeListSyntax in methodDeclarationSyntax.AttributeLists)
            {
                AttributeSyntax? attribute = attributeListSyntax.Attributes.FirstOrDefault();
                if (attribute == null)
                {
                    continue;
                }

                string attributeType = attribute.Name.ToString();
                string attributeString = $"[{attribute.ToString()}]";
                    
                string template = this.templates.Get(attributeType);
                    
                string code = $$"""
namespace {{namespaceName}}
{
    public static partial class {{className}}
    {
        {{template}}
    }
}
""";

                string argsTypesString = argsTypes.ToString();
                string argsTypesUnderLine = argsTypesString.Replace(",", "_").Replace(".", "_");
                code = code.Replace("$attribute$", attributeString);
                code = code.Replace("$attributeType$", attributeType);
                code = code.Replace("$methodName$", methodName);
                code = code.Replace("entityType", componentName);
                code = code.Replace("$argsTypes$", argsTypesString);
                code = code.Replace("$argsTypesUnderLine$", argsTypesUnderLine);
                code = code.Replace("$argsVars$", argsVars.ToString());
                code = code.Replace("$argsTypeVars$", argsTypeVars.ToString());
                
                string fileName = $"{namespaceName}.{className}.{methodName}.{argsTypesUnderLine}.g.cs";
                
                context.AddSource(fileName, code);
            }
        }
    }

    class SyntaxContextReceiver: ISyntaxContextReceiver
    {
        internal static ISyntaxContextReceiver Create()
        {
            return new SyntaxContextReceiver();
        }

        public Dictionary<ClassDeclarationSyntax, HashSet<MethodDeclarationSyntax>> MethodDeclarations { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            SyntaxNode node = context.Node;
            if (node is not MethodDeclarationSyntax methodDeclarationSyntax)
            {
                return;
            }

            if (methodDeclarationSyntax.AttributeLists.Count == 0)
            {
                return;
            }

            AttributeSyntax? attr = methodDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes)
                    .FirstOrDefault(x => x.Name.ToString() == "EntitySystem");
            if (attr == null)
            {
                return;
            }

            ClassDeclarationSyntax? parentClass = methodDeclarationSyntax.GetParentClassDeclaration();
            if (parentClass == null)
            {
                return;
            }

            if (!MethodDeclarations.ContainsKey(parentClass))
            {
                MethodDeclarations[parentClass] = new HashSet<MethodDeclarationSyntax>();
            }

            MethodDeclarations[parentClass].Add(methodDeclarationSyntax);
        }
    }
}