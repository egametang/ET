#include <google/protobuf/descriptor.h>
#include <glog/logging.h>
#include "Orm/Query.h"

namespace Egametang {

SelectQuery& SelectQuery::Distinct(bool d)
{
	distinct = d;
	return *this;
}
SelectQuery& SelectQuery::Limit(int value)
{
	limit = value;
	return *this;
}
SelectQuery& SelectQuery::Offset(int value)
{
	offset = value;
	return *this;
}
SelectQuery& SelectQuery::Result(std::string r)
{
	results.push_back(r);
	return *this;
}
SelectQuery& SelectQuery::ClearResults()
{
	results.clear();
	return *this;
}
SelectQuery& SelectQuery::Source(std::string s, std::string alias)
{
	if (!alias.empty())
		s += " as " + alias;
	sources.push_back(s);
	return *this;
}
SelectQuery& SelectQuery::Where(const Expr& w)
{
	where = (RawExpr(where) && w).asString();
	return *this;
}
SelectQuery& SelectQuery::Where(std::string w)
{
	where = (RawExpr(where) && RawExpr(w)).asString();
	return *this;
}
SelectQuery& SelectQuery::GroupBy(std::string gb)
{
	groupBy.push_back(gb);
	return *this;
}
SelectQuery& SelectQuery::Having(const Expr & h)
{
	having = h.asString();
	return *this;
}
SelectQuery& SelectQuery::Having(std::string h)
{
	having = h;
	return *this;
}
SelectQuery& SelectQuery::OrderBy(std::string ob, bool ascending)
{
	std::string value = ob;
	if (!ascending)
	{
		value += " desc";
	}
	orderBy.push_back(value);
	return *this;
}
SelectQuery::operator std::string() const
{
	std::string sql = "select ";
	if (distinct)
	{
		sql += "distinct ";
	}
	if (where != "true")
	{
		sql += " where " + where;
	}
	if (!groupBy.empty())
	{
		sql += " group by " + groupBy;
	}
	if (!having.empty())
	{
		sql += " having " + having;
	}
	if (orderBy.size() > 0)
	{
		sql += " order by " + orderBy.join(",");
	}
	if (limit)
	{
		sql += " limit " + boost::lexical_cast<std::string>(limit);
	}
	if (offset)
	{
		sql += " offset " + boost::lexical_cast<std::string>(offset);
	}
	return sql;
}

} // namespace Egametang
