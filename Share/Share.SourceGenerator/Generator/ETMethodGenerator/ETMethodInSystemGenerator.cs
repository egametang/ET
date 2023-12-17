using System.Collections.Generic;
using System.Text;
using ET.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ET.Generator;

[Generator(LanguageNames.CSharp)]
public class ETMethodInSystemGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(()=>ETMethodInSystemGeneratorSyntaxContextReceiver.Create());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not ETMethodInSystemGeneratorSyntaxContextReceiver receiver)
        {
            return;
        }

        if (receiver.Methods.Count==0)
        {
            return;
        }

        string className = $"EntityMethodRegister_{context.Compilation.AssemblyName}".Replace('.','_');
        string registeMethodContent = GenerateRegisteMethodContent(context,receiver.Methods);
        
        
        
        string code  = 
$$"""
namespace ET
{
    [Code]
    public class {{className}} : Singleton<{{className}}>, ISingletonAwake
    {
        public void Awake()
        {
            unsafe
            {
{{registeMethodContent}}
            }
        }
    }
}
""";
        
        context.AddSource($"{className}_ETMethodInSystemGenerator.g.cs",code);
    }

    string GenerateRegisteMethodContent(GeneratorExecutionContext context,Dictionary<INamedTypeSymbol,HashSet<IMethodSymbol>> methodsDic)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var methods in methodsDic)
        {
            string entityClassName = methods.Key.ToString();

            foreach (var method in methods.Value)
            {
                string content = 
$$"""
                {{entityClassName}}.FuncPointer.{{method.Name}} = &{{method.ContainingType}}.{{method.Name}};
""";
                sb.AppendLine(content);
            }
        }

        return sb.ToString();
    }

    class ETMethodInSystemGeneratorSyntaxContextReceiver: ISyntaxContextReceiver
    {
        internal static ISyntaxContextReceiver Create()
        {
            return new ETMethodInSystemGeneratorSyntaxContextReceiver();
        }

        public Dictionary<INamedTypeSymbol,HashSet<IMethodSymbol>> Methods = new();
        
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
            {
                return;
            }

            var systemTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            if (systemTypeSymbol==null)
            {
                return;
            }

            
            if (!IsEntitySystem(systemTypeSymbol, out AttributeData? systemOfAttribute))
            {
                return;
            }
        
            // 获取所属的实体类symbol
            if (systemOfAttribute?.ConstructorArguments[0].Value is not INamedTypeSymbol entityTypeSymbol)
            {
                return;
            }

            var baseType = entityTypeSymbol.BaseType?.ToString();
        
            // 筛选出实体类
            if (baseType!= Definition.EntityType && baseType != Definition.LSEntityType)
            {
                return;
            }

            foreach (var member in systemTypeSymbol.GetMembers())
            {
                if (member is not IMethodSymbol methodSymbol)
                {
                    continue;
                }

                if (!methodSymbol.HasAttribute(Definition.EntityMethodOfAttributeMetaName))
                {
                    continue;
                }

                if (!this.Methods.ContainsKey(entityTypeSymbol))
                {
                    Methods.Add(entityTypeSymbol, new());
                }

                Methods[entityTypeSymbol].Add(methodSymbol);
            }
        }
        
        /// <summary>
        /// 是否是enitysystem类
        /// </summary>
        private bool IsEntitySystem(INamedTypeSymbol namedTypeSymbol, out AttributeData? attributeData)
        {
            attributeData = namedTypeSymbol.GetFirstAttribute(Definition.EntitySystemOfAttribute);
            if (attributeData!=null)
            {
                return true;
            }

            attributeData = namedTypeSymbol.GetFirstAttribute(Definition.LSEntitySystemOfAttribute);
            if (attributeData!=null)
            {
                return true;
            }

            return false;
        }
    }
}