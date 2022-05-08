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
using Microsoft.CodeAnalysis.Formatting;

namespace ET.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityFiledAccessCodeFixProvider)), Shared]
    public class EntityFiledAccessCodeFixProvider: CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.EntityFiledAccessAnalyzerRuleId);
        
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
        
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            
            Diagnostic diagnostic = context.Diagnostics.First();

            Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
            // 获取diagnostic 传递来的 FriendClassType 值
            diagnostic.Properties.TryGetValue("FriendClassType", out string? frienClassType);
            if (frienClassType==null)
            {
                return;
            }
            
            ClassDeclarationSyntax? classDeclaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();
            // 构造Code Action
            CodeAction action = CodeAction.Create(
                "Add FriendClass Attribute",
                c => AddFriendClassAttributeAsync(context.Document, classDeclaration,frienClassType, c),
                equivalenceKey: nameof(EntityFiledAccessCodeFixProvider));

            // 注册codeFix Code Action
            context.RegisterCodeFix(action, diagnostic);
        }


        private static async Task<Document> AddFriendClassAttributeAsync(Document document, ClassDeclarationSyntax? classDeclaration, string friendClassType, CancellationToken cancellationToken)
        {
            // 构造FriendClassAttribute 语法节点
            AttributeArgumentSyntax attributeArgument = SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(friendClassType)));
            AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FriendClassAttribute"))
                    .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(attributeArgument)));
            // 构造添加构造FriendClassAttribute 得AttributeList语法节点
            SyntaxList<AttributeListSyntax>? attributes = classDeclaration?.AttributeLists.Add(
                SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attributeSyntax)).NormalizeWhitespace());

            if (attributes == null)
            {
                return document;
            }
            // 构造替换AttributeList的 ClassDeclaration语法节点
            ClassDeclarationSyntax? newClassDeclaration =  classDeclaration?.WithAttributeLists(attributes.Value).WithAdditionalAnnotations(Formatter.Annotation);

            SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            if (root==null ||classDeclaration==null || newClassDeclaration==null )
            {
                return document;
            }
            // 构造替换classDeclaration的root语法节点
            var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);
            
            // 替换root语法节点
            return document.WithSyntaxRoot(newRoot);
        }
    }
}