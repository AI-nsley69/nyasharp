namespace nyasharp.AST;

public interface Visitor<out T>
{
    T VisitExpressionBinary(Expr.Binary binary);
    T VisitExpressionGrouping(Expr.Grouping grouping);
    T VisitExpressionLiteral(Expr.Literal literal);
    T VisitExpressionUnary(Expr.Unary unary);
}