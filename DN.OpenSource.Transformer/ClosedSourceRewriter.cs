using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DN.OpenSource.Transformer
{
    public class ClosedSourceRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel SemanticModel;

        IEnumerable<SyntaxTrivia> fourSpaces = new[] { SyntaxFactory.Space, SyntaxFactory.Space, SyntaxFactory.Space, SyntaxFactory.Space };
        SyntaxToken semicolonToken = SyntaxFactory.Token(SyntaxKind.SemicolonToken);

        public ClosedSourceRewriter(SemanticModel semanticModel)
        {
            this.SemanticModel = semanticModel;
        }

        public static bool HasClosedSourceAttribute(SyntaxNode node, SyntaxList<AttributeListSyntax> attributelist)
        {
            if (attributelist.Any(l => l.Attributes.Any(a => a.Name.ToString() == "ClosedSource")))
                return true;

            if (node.Parent is ClassDeclarationSyntax && (node.Parent as ClassDeclarationSyntax).AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == "ClosedSource")))
                return true;

            return false;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (!HasClosedSourceAttribute(node, node.AttributeLists))
                return node;

            return SyntaxFactory.PropertyDeclaration(node.AttributeLists, node.Modifiers, node.Type, node.ExplicitInterfaceSpecifier, node.Identifier.WithoutTrivia().WithTrailingTrivia(SyntaxFactory.Space),
                SyntaxFactory.AccessorList(SyntaxFactory.List<AccessorDeclarationSyntax>(new[] { SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(semicolonToken).WithLeadingTrivia(SyntaxFactory.Space).WithTrailingTrivia(SyntaxFactory.Space), SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(semicolonToken).WithTrailingTrivia(SyntaxFactory.Space) }))
                ).WithTrailingTrivia(SyntaxFactory.EndOfLine("\r\n"));
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            if (!HasClosedSourceAttribute(node, node.AttributeLists))
                return node;

            ExpressionSyntax expression = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(@"System.Linq.Expressions.Expression"),
                                SyntaxFactory.IdentifierName(@"Empty")).WithOperatorToken(SyntaxFactory.Token(SyntaxKind.DotToken)));
            
            ArrowExpressionClauseSyntax arrowExpressionClause = SyntaxFactory.ArrowExpressionClause(SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken).WithTrailingTrivia(SyntaxFactory.Space).WithLeadingTrivia(SyntaxFactory.Space), expression);

            return SyntaxFactory.ConstructorDeclaration(node.AttributeLists, node.Modifiers, node.Identifier, node.ParameterList.WithoutTrivia(), node.Initializer, arrowExpressionClause, SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithTrailingTrivia(SyntaxFactory.EndOfLine("\r\n"));
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (!HasClosedSourceAttribute(node, node.AttributeLists))
                return node;

            if (node.Modifiers.Any(m => m.ValueText == "const")) // for now
                return node;

            VariableDeclarationSyntax decl = SyntaxFactory.VariableDeclaration(node.Declaration.Type, SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(node.Declaration.Variables.Select(n => n.WithInitializer(null).WithoutTrailingTrivia()))).WithoutTrailingTrivia();
            return node.WithDeclaration(decl).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithTrailingTrivia(SyntaxFactory.EndOfLine("\r\n"));
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (!HasClosedSourceAttribute(node, node.AttributeLists))
                return node;

            if (node.Modifiers.Any(m => m.Text == "abstract"))
                return node;

            var returntype = node.ReturnType.ToString();

            ExpressionSyntax expression = null;

            if (returntype != "void")
            {
                expression = SyntaxFactory.DefaultExpression(node.ReturnType.WithoutTrivia());
            }
            else
            { 
                expression = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(@"System.Linq.Expressions.Expression"),
                                SyntaxFactory.IdentifierName(@"Empty")).WithOperatorToken(SyntaxFactory.Token(SyntaxKind.DotToken)));
            }

            ArrowExpressionClauseSyntax arrowExpressionClause = SyntaxFactory.ArrowExpressionClause(SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken).WithTrailingTrivia(SyntaxFactory.Space).WithLeadingTrivia(SyntaxFactory.Space), expression);

            return SyntaxFactory.MethodDeclaration(node.AttributeLists, node.Modifiers,
                    node.ReturnType, node.ExplicitInterfaceSpecifier, node.Identifier,
                    node.TypeParameterList, node.ParameterList.WithoutTrivia(), node.ConstraintClauses, null, arrowExpressionClause, SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithTrailingTrivia(SyntaxFactory.EndOfLine("\r\n"));
            
        }
    }
}