// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_QUERY_H
#define ORM_QUERY_H

#include <string>
#include <vector>
#include <boost/lexical_cast.hpp>
#include <google/protobuf/descriptor.h>
#include "Base/Marcos.h"
#include "Orm/Expr.h"
#include "Orm/Column.h"

namespace Egametang {

template <typename Table>
class Select
{
private:
	Column select;
	bool distinct;
	Expr where;
	Column groupBy;
	Expr having;
	Column orderBy;
	bool desc;
	int limit;
	int offset;

public:
	explicit Select(Column columns):
		select(columns),
		distinct(false), groupBy(),
		orderBy(), desc(false),
		limit(0), offset(0)
	{
		columns.CheckAllColumns(Table::default_instance());
	}

	Select<Table>& Distinct()
	{
		distinct = true;
		return *this;
	}

	Select<Table>& Where(Expr expr)
	{
		expr.CheckAllColumns(Table::default_instance());
		where = expr;
		return *this;
	}

	Select<Table>& GroupBy(Column columns)
	{
		columns.CheckAllColumns(Table::default_instance());
		groupBy = columns;
		return *this;
	}

	Select<Table>& Having(Expr expr)
	{
		expr.CheckAllColumns(Table::default_instance());
		having = expr;
		return *this;
	}

	Select<Table>& OrderBy(Column columns)
	{
		columns.CheckAllColumns(Table::default_instance());
		orderBy = columns;
		return *this;
	}

	Select<Table>& Desc()
	{
		desc = true;
		return *this;
	}

	Select<Table>& Limit(int value)
	{
		limit = value;
		return *this;
	}

	Select<Table>& Offset(int value)
	{
		offset = value;
		return *this;
	}

	std::string ToString() const
	{
		std::string sql = "select ";
		if (!select.Empty())
		{
			sql += select.ToString();
		}
		if (distinct)
		{
			sql += " distinct";
		}
		sql += " from " + Table::descriptor()->full_name();
		if (!where.Empty())
		{
			sql += " where " + where.ToString();
		}
		if (!groupBy.Empty())
		{
			sql += " group by " + groupBy.ToString();

			if (!having.Empty())
			{
				sql += " having " + having.ToString();
			}
			if (!orderBy.Empty())
			{
				sql += " order by " + orderBy.ToString();
			}
			if (desc)
			{
				sql += " desc ";
			}
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


}

 // namespace Egametang
#endif // ORM_QUERY_H
