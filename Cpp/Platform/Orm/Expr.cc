// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include "Orm/Expr.h"

namespace Egametang {

Expr::~Expr()
{
}
std::string Expr::ToString()
{
	return exprStr;
}

Not::Not(const Expr& expr)
{
	exprStr = "not (" + expr + ") ";
}

And::And(const Expr& left, const Expr& right)
{
	exprStr = "(" + left.ToString() + ") and" + " (" + right.ToString() + ") ";
}

Or::Or(const Expr& left, const Expr& right)
{
	exprStr = "(" + left.ToString() + ") or" + " (" + right.ToString() + ") ";
}

Oper::Oper(const Column& left, const std::string& op, const std::string& right)
{
	exprStr = left.ToString() + " " + op + " '" + right + "'";
}

Oper::Oper(const Column& left, const std::string& op, const Column& right)
{
	exprStr = left.ToString() + " " + op + " " + right.ToString();
}

Oper::Oper(const Column& left, const std::string& op, int right)
{
	exprStr = left.ToString() + " " + op + " " + boost::lexical_cast<std::string>(right);
}

Oper::Oper(const Column& left, const std::string& op, double right)
{
	exprStr = left.ToString() + " " + op + " " + boost::lexical_cast<std::string>(right);
}

} // namespace Egametang
