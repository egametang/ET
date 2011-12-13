#ifndef ORM_QUERY_H
#define ORM_QUERY_H

#include <string>
#include <vector>
#include "Orm/Expr.h"

namespace Egametang {

class Query
{
	std::string select;
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
	Query& Select(Column column);
	Query& Distinct(bool distinct);
	Query& Where(const Expr& where);
	Query& GroupBy(Column column);
	Query& Having(const Expr& having);
	Query& OrderBy(Column column, bool ascending = true);
	Query& Limit(int limit);
	Query& Offset(int offset);

	std::string ToString() const;
};

} // namespace Egametang
#endif // ORM_QUERY_H
