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

        public ClosedSourceRewriter(SemanticModel semanticModel)
        {
            this.SemanticModel = semanticModel;
        }

        public static bool HasClosedSourceAttribute(MethodDeclarationSyntax node)
        {
            if (node.AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == "ClosedSource")))
                return true;

            if (node.Parent is ClassDeclarationSyntax && (node.Parent as ClassDeclarationSyntax).AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == "ClosedSource")))
                return true;

            return false;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (!HasClosedSourceAttribute(node))
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