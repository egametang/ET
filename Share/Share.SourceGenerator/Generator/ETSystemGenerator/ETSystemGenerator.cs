using System;
using System.Collections.Generic;
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
        context.RegisterForSyntaxNotifications(()=> SyntaxContextReceiver.Create(this.templates));
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

            List<string> argsTypesList = new List<string>();
            List<string> argsTypeVarsList = new List<string>();
            List<string> argsVarsList = new List<string>();
            List<string> argsTypesWithout0List = new List<string>();
            List<string> argsTypeVarsWithout0List = new List<string>();
            List<string> argsVarsWithout0List = new List<string>();
            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                string type = methodSymbol.Parameters[i].Type.ToString();
                type = type.Trim();
                if (type == "")
                {
                    continue;
                }
                string name = $"{methodSymbol.Parameters[i].Name}";
                

                
                argsTypesList.Add(type);
                argsVarsList.Add(name);
                string typeName = $"{type} {name}";
                argsTypeVarsList.Add(typeName);

                if (i != 0)
                {
                    argsTypesWithout0List.Add(type);
                    argsTypeVarsWithout0List.Add(typeName);
                    argsVarsWithout0List.Add(name);
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

                string argsVars = string.Join(", ", argsVarsList);
                string argsTypes = string.Join(", ", argsTypesList);
                string argsTypesVars = string.Join(", ", argsTypeVarsList);
                string argsTypesUnderLine = string.Join("_", argsTypesList).Replace(", ", "_").Replace(".", "_").Replace("<", "_").Replace(">", "_");
                string argsTypesWithout0 = string.Join(", ", argsTypesWithout0List);
                string argsVarsWithout0 = string.Join(", ", argsVarsWithout0List);
                string argsTypesVarsWithout0 = string.Join(", ", argsTypeVarsWithout0List);

                SpeicalProcessForArgs();
                
                code = code.Replace("$attribute$", attributeString);
                code = code.Replace("$attributeType$", attributeType);
                code = code.Replace("$methodName$", methodName);
                code = code.Replace("$className$", className);
                code = code.Replace("$entityType$", componentName);
                code = code.Replace("$argsTypes$", argsTypes);
                code = code.Replace("$argsTypesUnderLine$", argsTypesUnderLine);
                code = code.Replace("$argsTypesVars$", argsTypesVars);
                code = code.Replace("$argsVars$", argsVars);
                code = code.Replace("$argsTypesWithout0$", argsTypesWithout0);
                code = code.Replace("$argsVarsWithout0$", argsVarsWithout0);
                code = code.Replace("$argsTypesVarsWithout0$", argsTypesVarsWithout0);

                for (int i = 0; i < argsTypesList.Count; ++i)
                {
                    code = code.Replace($"$argsTypes{i}$", argsTypesList[i]);
                    code = code.Replace($"$argsVars{i}$", argsVarsList[i]);
                }

                string fileName = $"{namespaceName}.{className}.{methodName}.{argsTypesUnderLine}.g.cs";
                
                context.AddSource(fileName, code);
                
                void SpeicalProcessForArgs()
                {
                    if ((attributeType=="EntitySystem" || attributeType=="LSEntitySystem")&&methodName==Definition.GetComponentMethod)
                    {
                        argsTypes = argsTypes.Split(',')[0];
                    }
                }
            }
        }
    }


    

    class SyntaxContextReceiver: ISyntaxContextReceiver
    {
        internal static ISyntaxContextReceiver Create(AttributeTemplate attributeTemplate)
        {
            return new SyntaxContextReceiver(attributeTemplate);
        }

        private AttributeTemplate attributeTemplate;

        SyntaxContextReceiver(AttributeTemplate attributeTemplate)
        {
            this.attributeTemplate = attributeTemplate;
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

            bool found = false;
            foreach (AttributeListSyntax attributeListSyntax in methodDeclarationSyntax.AttributeLists)
            {
                AttributeSyntax? attribute = attributeListSyntax.Attributes.FirstOrDefault();
                if (attribute == null)
                {
                    return;
                }

                string attributeName = attribute.Name.ToString();

                if (this.attributeTemplate.Contains(attributeName))
                {
                    found = true;
                }
            }

            if (!found)
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