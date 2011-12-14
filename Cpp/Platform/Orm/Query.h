#ifndef ORM_QUERY_H
#define ORM_QUERY_H

#include <string>
#include <vector>
#include "Orm/Expr.h"
#include "Orm/Column.h"

namespace Egametang {

template <typename Table>
class Query
{
	std::string columns;
	bool distinct;
	std::string where;
	std::string groupBy;
	std::string having;
	std::string orderBy;
	int limit;
	int offset;

public:
	Query();
	~Query();

	Query(): distinct(false), where("true"), limit(0), offset(0)
	{
	}

	~Query()
	{
	}

	Query<Table>& Select(Column column)
	{
		columns = column.ToString();
		return *this;
	}

	Query<Table>& Distinct(bool value)
	{
		distinct = value;
		return *this;
	}

	Query<Table>& Where(const Expr& expr)
	{
		where = expr.ToString();
		return *this;
	}

	Query<Table>& GroupBy(Column column)
	{
		groupBy = column.ToString();
		return *this;
	}

	Query<Table>& Having(const Expr& having)
	{
		having = having.ToString();
		return *this;
	}

	Query<Table>& OrderBy(Column column, bool ascending)
	{
		std::string value = column.ToString();
		if (!ascending)
		{
			value += " desc";
		}
		orderBy = value;
		return *this;
	}

	Query<Table>& Limit(int value)
	{
		limit = value;
		return *this;
	}

	Query<Table>& Offset(int value)
	{
		offset = value;
		return *this;
	}

	std::string ToString() const
	{
		std::string sql = "select ";
		if (!columns.empty())
		{
			sql += columns;
		}
		if (distinct)
		{
			sql += " distinct";
		}
		sql += " from " + Table::descriptor()->full_name();
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
};

} // namespace Egametang
#endif // ORM_QUERY_H
