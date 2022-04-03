using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AddChildTypeAnalyzer: DiagnosticAnalyzer
{
    private const string Title = "AddChild方法类型约束错误";

    public const string MessageFormat = "Type: {0} 不允许作为实体: {1} 的AddChild函数参数类型! 若要允许该类型作为参数,请使用ChildTypeAttribute对实体类进行标记";

    private const string Description = "请使用被允许的ChildType 或添加该类型至ChildType";

    private static readonly string[] AddChildMethods = new[] {"AddChild","AddChildWithId"};

    internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticIds.AddChildTypeAnalyzerRuleId,
                Title,
                MessageFormat,
                DiagnosticCategories.Stateless,
                DiagnosticSeverity.Error,
                true,
                Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(this.AnalyzeMemberAccessExpression, SyntaxKind.SimpleMemberAccessExpression);
    }

    private void AnalyzeMemberAccessExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            return;
        }
        // 筛选出 AddChild函数syntax
        var methodName = memberAccessExpressionSyntax.Name.Identifier.Text;
        if (!AddChildMethods.Contains(methodName))
        {
            return;
        }
        if (memberAccessExpressionSyntax.Parent is not InvocationExpressionSyntax invocationExpressionSyntax ||
            context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is not IMethodSymbol addChildMethodSymbol)
        {
            return;
        }

        ISymbol? identifierSymbol = context.SemanticModel
                .GetSymbolInfo(memberAccessExpressionSyntax.ChildNodes().OfType<IdentifierNameSyntax>().First()).Symbol;
        if (identifierSymbol == null)
        {
            throw new Exception("identifierSymbol == null");
        }

        // 获取父级实体类型
        ITypeSymbol parentTypeSymbol = null;

        if (identifierSymbol is ILocalSymbol localSymbol)
        {
            parentTypeSymbol = localSymbol.Type;
        }
        else if (identifierSymbol is IParameterSymbol parameterSymbol)
        {
            parentTypeSymbol = parameterSymbol.Type;
        }
        else
        {
            throw new Exception("componentTypeSymbol==null");
        }

        // 获取实体类 ChildType标签的约束类型
        List<INamedTypeSymbol> availableChildTypeSymbols = null;
        foreach (AttributeData? attributeData in parentTypeSymbol.GetAttributes())
        {
            if (attributeData.AttributeClass?.Name == "ChildTypeAttribute")
            {
                if (attributeData.ConstructorArguments[0].Value is not INamedTypeSymbol availableChildTypeSymbol)
                {
                    continue;
                }

                if (availableChildTypeSymbols == null)
                {
                    availableChildTypeSymbols = new List<INamedTypeSymbol>();
                }

                availableChildTypeSymbols.Add(availableChildTypeSymbol);
            }
        }

        // 没有ChildType标签的不做约束
        if (availableChildTypeSymbols == null)
        {
            return;
        }

        // 获取 child实体类型
        ISymbol? childTypeSymbol = null;
        // addChild为泛型调用
        if (addChildMethodSymbol.IsGenericMethod)
        {
            GenericNameSyntax? genericNameSyntax = memberAccessExpressionSyntax?.ChildNodes()?.OfType<GenericNameSyntax>()?.First();

            TypeArgumentListSyntax? typeArgumentList = genericNameSyntax?.ChildNodes().OfType<TypeArgumentListSyntax>().First();

            IdentifierNameSyntax? childTypeSyntax = typeArgumentList?.ChildNodes().OfType<IdentifierNameSyntax>().First();

            if (childTypeSyntax == null)
            {
                throw new Exception("childTypeSyntax==null");
            }

            childTypeSymbol = context.SemanticModel.GetSymbolInfo(childTypeSyntax).Symbol;
        }
        else
        {
            // addChild为非泛型调用
            IdentifierNameSyntax? identifierSyntax = invocationExpressionSyntax.ChildNodes().OfType<ArgumentListSyntax>().First().ChildNodes()
                    .OfType<ArgumentSyntax>().First().ChildNodes().OfType<IdentifierNameSyntax>().First();
            if (identifierSyntax == null)
            {
                throw new Exception("identifierSyntax==null");
            }

            ISymbol? childSymbol = context.SemanticModel.GetSymbolInfo(identifierSyntax).Symbol;

            if (childSymbol is ILocalSymbol childLocalSymbol)
            {
                childTypeSymbol = childLocalSymbol.Type;
            }
            else if (childSymbol is IParameterSymbol childParamaterSymbol)
            {
                childTypeSymbol = childParamaterSymbol.Type;
            }
        }

        if (childTypeSymbol == null)
        {
            throw new Exception("childTypeSymbol==null");
        }

        // 判断child类型是否属于约束类型
        foreach (INamedTypeSymbol? availableChildTypeSymbol in availableChildTypeSymbols)
        {
            if (availableChildTypeSymbol.ToString() == childTypeSymbol.ToString())
            {
                return;
            }
        }
        Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(), childTypeSymbol.Name, parentTypeSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}