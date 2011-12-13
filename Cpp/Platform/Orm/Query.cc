#include <google/protobuf/descriptor.h>
#include <glog/logging.h>
#include "Orm/Query.h"

namespace Egametang {

Query::Query(): distinct(false), where("true"), limit(0), offset(0)
{
}

Query::~Query()
{
}

Query& Query::Select(Column column)
{
	select = column.ToString();
	return *this;
}

Query& Query::Distinct(bool value)
{
	distinct = value;
	return *this;
}

Query& Query::Where(const Expr& expr)
{
	where = expr.ToString();
	return *this;
}

Query& Query::GroupBy(Column column)
{
	groupBy = column.ToString();
	return *this;
}

Query& Query::Having(const Expr& having)
{
	having = having.ToString();
	return *this;
}

Query& Query::OrderBy(Column column, bool ascending)
{
	std::string value = column.ToString();
	if (!ascending)
	{
		value += " desc";
	}
	orderBy = value;
	return *this;
}

Query& Query::Limit(int value)
{
	limit = value;
	return *this;
}

Query& Query::Offset(int value)
{
	offset = value;
	return *this;
}

std::string Query::ToString() const
{
	std::string sql = "select ";
	if (!select.empty())
	{
		sql += select;
	}
	if (distinct)
	{
		sql += " distinct";
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
	if (!orderBy.empty())
	{
		sql += " order by " + orderBy;
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
