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

        object add()
        {
            return null;
        }


        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {

            if (!node.AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == "ClosedSource")))
                return node;

            var returntype = node.ReturnType.ToString();

            ExpressionSyntax nullexpression = null;

            if (Char.IsUpper(returntype.First()) || returntype == "string")
            {
                nullexpression = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            }
            else if(returntype == "int")
            {
                nullexpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));
            }
            else if(returntype == "double")
            {
                nullexpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));
            }

            var returns = SyntaxFactory.ReturnStatement(nullexpression).NormalizeWhitespace().WithTrailingTrivia(SyntaxFactory.EndOfLine("\r\n")).WithLeadingTrivia(node.Body.OpenBraceToken.LeadingTrivia.AddRange(fourSpaces));
            var newBody = SyntaxFactory.Block(returns).WithTriviaFrom(node.Body).WithOpenBraceToken(node.Body.OpenBraceToken).WithCloseBraceToken(node.Body.CloseBraceToken);

            return SyntaxFactory.MethodDeclaration(node.AttributeLists, node.Modifiers,
                    node.ReturnType, node.ExplicitInterfaceSpecifier, node.Identifier,
                    node.TypeParameterList, node.ParameterList, node.ConstraintClauses, newBody, null, node.SemicolonToken);
        }
    }
}