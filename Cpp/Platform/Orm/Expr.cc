// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <boost/lexical_cast.hpp>
#include "Orm/Column.h"
#include "Orm/Expr.h"

namespace Egametang {

Expr::~Expr()
{
}

Expr& Expr::operator=(const Expr& expr)
{
	exprStr = expr.exprStr;
	return *this;
}

bool Expr::Empty()
{
	return exprStr.empty();
}

std::string Expr::ToString() const
{
	return exprStr;
}

Not::Not(const Expr& expr)
{
	exprStr = "not (" + expr.ToString() + ") ";
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
