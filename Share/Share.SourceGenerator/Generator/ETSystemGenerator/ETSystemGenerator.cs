using System.Collections.Generic;
using System.Linq;
using System.Text;
using ET.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace ET.Generator;

[Generator(LanguageNames.CSharp)]
public class ETSystemGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(SyntaxContextReceiver.Create);
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SyntaxContextReceiver receiver || receiver.MethodDeclarations.Count==0)
        {
            return;
        }

        foreach (var kv in receiver.MethodDeclarations)
        {
            this.GenerateCSFiles(kv.Key,kv.Value,context);
        }
    }

    /// <summary>
    /// 每个静态类生成一个cs文件
    /// </summary>
    private void GenerateCSFiles(ClassDeclarationSyntax classDeclarationSyntax, HashSet<MethodDeclarationSyntax>methodDeclarationSyntaxes,GeneratorExecutionContext context)
    {
        var className = classDeclarationSyntax.Identifier.Text;
        var semanticModel = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
        var classTypeSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
        if (classTypeSymbol==null)
        {
            context.AddSource($"{className}.g.cs", "classTypeSymbol==null");
            return;
        }
        var namespaceSymbol = classTypeSymbol?.ContainingNamespace;
        var namespaceName = namespaceSymbol?.Name;
        while (namespaceSymbol?.ContainingNamespace != null)
        {
            namespaceSymbol = namespaceSymbol.ContainingNamespace;
            if (string.IsNullOrEmpty(namespaceSymbol.Name))
            {
                break;
            }
            namespaceName = $"{namespaceSymbol.Name}.{namespaceName}";
        }
        
        var inClassSb = new StringBuilder();

        this.GenerateSystemCodeByTemplate(inClassSb, classDeclarationSyntax, methodDeclarationSyntaxes, context,semanticModel);

        string code = $$"""
namespace {{namespaceName}}{
    public static partial class {{className}}
    {
{{inClassSb}}
    }
}
""";
        context.AddSource($"{className}.g.cs",code);
    }

    /// <summary>
    /// 根据模板生成System代码
    /// </summary>
    private void GenerateSystemCodeByTemplate(StringBuilder inClassSb,ClassDeclarationSyntax classDeclarationSyntax, HashSet<MethodDeclarationSyntax>methodDeclarationSyntaxes,GeneratorExecutionContext context, SemanticModel semanticModel)
    {
        foreach (var methodDeclarationSyntax in methodDeclarationSyntaxes)
        {
            var componentParam = methodDeclarationSyntax.ParameterList.Parameters.FirstOrDefault();
            if (componentParam==null)
            {
                continue;
            }
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;
            if (methodSymbol==null)
            {
                continue;
            }
            var methodName = methodDeclarationSyntax.Identifier.Text;
            var componentName = componentParam.Type?.ToString();
            
            var argsTypes = new StringBuilder();
            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                argsTypes.Append(i == 0? $"{methodSymbol.Parameters[i].Type}" : $",{methodSymbol.Parameters[i].Type}");
            }

            var argsTypeVars = new StringBuilder();
            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                argsTypeVars.Append(i == 0? $"{methodSymbol.Parameters[i].Type} self" : $",{methodSymbol.Parameters[i].Type} args{i}");
            }

            var argsVars = new StringBuilder();
            if (methodSymbol.Parameters.Length>1)
            {
                for (int i = 1; i < methodSymbol.Parameters.Length; i++)
                {
                    argsVars.Append(i == 1? $"args1" : $",args{i}");
                }
            }
            
            inClassSb.AppendLine($$"""
        public class {{componentName}}{{methodName}}System: {{methodName}}System<{{argsTypes}}>
        {   
            protected override void {{methodName}}({{argsTypeVars}})
            {
                self.{{methodName}}({{argsVars}});
            }
        }
""");
        }
    }
    
    class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        internal static ISyntaxContextReceiver Create()
        {
            return new SyntaxContextReceiver();
        }

        public Dictionary<ClassDeclarationSyntax, HashSet<MethodDeclarationSyntax>> MethodDeclarations { get; } = new ();
        

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var node = context.Node;
            if (node is not MethodDeclarationSyntax methodDeclarationSyntax)
            {
                return;
            }
            if (methodDeclarationSyntax.AttributeLists.Count == 0)
            {
                return;
            }
            
            var attr = methodDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes)
                    .FirstOrDefault(x=>x.Name.ToString()=="EntitySystem");
            if (attr == null)
            {
                return;
            }

            var parentClass = methodDeclarationSyntax.GetParentClassDeclaration();
            if (parentClass==null)
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