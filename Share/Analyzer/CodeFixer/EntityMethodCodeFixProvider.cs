using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace ET.Analyzer;
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityMethodCodeFixProvider)), Shared]
public class EntityMethodCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.EntityPartialMethodAnalyzerRuleId);
    
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
    
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            
        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
        
        ClassDeclarationSyntax? classDeclaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();
        
        CodeAction codeAction = CodeAction.Create(
            "Generate Entity Method",
            cancelToken => GenerateEntitySystemAsync(context.Document,classDeclaration,diagnostic,cancelToken), 
            equivalenceKey: nameof(EntityMethodCodeFixProvider));
        context.RegisterCodeFix(codeAction,diagnostic);
    }

    private static async Task<Document> GenerateEntitySystemAsync(Document document, ClassDeclarationSyntax? classDeclaration, Diagnostic diagnostic,
    CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        ImmutableDictionary<string, string?> properties = diagnostic.Properties;

        
        
        if (classDeclaration==null || root==null || properties.Count==0)
        {
            throw new Exception("classDeclaration==null || root==null || properties.Count==0!");
        }
        
        var newMembers = new SyntaxList<MemberDeclarationSyntax>();
        
        foreach (var kv in properties)
        {
            var methodDeclaration = GenerateEntityMethodSyntax(kv.Key,kv.Value);
            if (methodDeclaration!=null)
            {
                newMembers = newMembers.Add(methodDeclaration);
            }
            
        }
        
        if (newMembers.Count==0)
        {
            throw new Exception("newMembers.Count==0");
        }
        var newClassDeclaration = classDeclaration.WithMembers(classDeclaration.Members.AddRange(newMembers)).WithAdditionalAnnotations(Formatter.Annotation);
        document = document.WithSyntaxRoot(root.ReplaceNode(classDeclaration, newClassDeclaration));
        document = await CleanupDocumentAsync(document, cancellationToken);
        return document;
    }

    private static MethodDeclarationSyntax? GenerateEntityMethodSyntax(string methodName, string? valueString)
    {
        if (valueString==null)
        {
            throw new Exception("valueString==null");
        }

        var args = valueString.Split('/');
        
        string returnType = args[0];
        string methodArgs = args[1];
        string fullMethodName = args[2];
        string code = 
$$"""
[{{Definition.EntityMethodOfAttribute}}(nameof({{fullMethodName}}))]
public static {{returnType}} {{methodName}}({{methodArgs}})
{
    throw new System.NotImplementedException();
}

""";
        var syntax = SyntaxFactory.ParseMemberDeclaration(code);
        if (syntax==null)
        {
            throw new Exception("SyntaxFactory.ParseMemberDeclaration(code)==null");
        }

        if (syntax is not MethodDeclarationSyntax memberDeclarationSyntax)
        {
            throw new Exception("syntax is not MethodDeclarationSyntax");
        }
        return memberDeclarationSyntax;
    }
    
    internal static async Task<Document> CleanupDocumentAsync(
    Document document, CancellationToken cancellationToken)
    {
        if (document.SupportsSyntaxTree)
        {
            document = await ImportAdder.AddImportsAsync(
                document, Simplifier.AddImportsAnnotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            document = await Simplifier.ReduceAsync(document, Simplifier.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            // format any node with explicit formatter annotation
            document = await Formatter.FormatAsync(document, Formatter.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            // format any elastic whitespace
            document = await Formatter.FormatAsync(document, SyntaxAnnotation.ElasticAnnotation, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        return document;
    }
    
    
}