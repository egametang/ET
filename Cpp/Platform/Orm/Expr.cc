// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <boost/foreach.hpp>
#include <boost/lexical_cast.hpp>
#include <boost/format.hpp>
#include <glog/logging.h>
#include <google/protobuf/descriptor.h>
#include "Base/Marcos.h"
#include "Orm/Column.h"
#include "Orm/Expr.h"
#include "Orm/Exception.h"

namespace Egametang {

Expr::~Expr()
{
}

Expr& Expr::operator=(const Expr& expr)
{
	exprStr = expr.exprStr;
	return *this;
}

bool Expr::Empty() const
{
	return exprStr.empty();
}

void Expr::SaveColumn(const Expr& expr)
{
	foreach (const Column& column, expr.columns)
	{
		columns.push_back(column);
	}
}

void Expr::SaveColumn(const Column& column)
{
	columns.push_back(column);
}

void Expr::CheckAllColumns(const google::protobuf::Message& message) const
{
	const google::protobuf::Descriptor* descriptor = message.GetDescriptor();
	foreach (const Column& column, columns)
	{
		column.CheckAllColumns(message);
	}
}

std::string Expr::ToString() const
{
	return exprStr;
}

Not::Not(const Expr& expr)
{
	exprStr = "not (" + expr.ToString() + ") ";
	this->SaveColumn(expr);
}

And::And(const Expr& left, const Expr& right)
{
	exprStr = "(" + left.ToString() + ") and" + " (" + right.ToString() + ") ";
	SaveColumn(left);
	SaveColumn(right);
}

Or::Or(const Expr& left, const Expr& right)
{
	exprStr = "(" + left.ToString() + ") or" + " (" + right.ToString() + ") ";
	SaveColumn(left);
	SaveColumn(right);
}

Oper::Oper(const Column& left, const std::string& op, const std::string& right)
{
	exprStr = left.ToString() + " " + op + " '" + right + "'";
	SaveColumn(left);
}

Oper::Oper(const Column& left, const std::string& op, const Column& right)
{
	exprStr = left.ToString() + " " + op + " " + right.ToString();
	SaveColumn(left);
	SaveColumn(right);
}

Oper::Oper(const Column& left, const std::string& op, int right)
{
	exprStr = left.ToString() + " " + op + " " + boost::lexical_cast<std::string>(right);
	SaveColumn(left);
}

Oper::Oper(const Column& left, const std::string& op, double right)
{
	exprStr = left.ToString() + " " + op + " " + boost::lexical_cast<std::string>(right);
	SaveColumn(left);
}

} // namespace Egametang
