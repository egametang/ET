#ifndef ORM_QUERY_H
#define ORM_QUERY_H

#include <vector>
#include <boost/shared_ptr.hpp>
#include <google/protobuf/message.h>
#include "Base/Typedef.h"

namespace Egametang {

class SelectQuery
{
	bool distinct;
	int limit;
	int offset;
	std::string where;
	std::string groupBy;
	std::string having;
	std::string orderBy;
public:
	SelectQuery(): distinct(false), limit(0), offset(0), where("true")
	{
	}
	SelectQuery& Distinct(bool distinct);
	SelectQuery& Limit(int limit);
	SelectQuery& Offset(int offset);
	SelectQuery& Result(std::string result);
	SelectQuery& ClearResults();
	SelectQuery& Where(const Expr& where);
	SelectQuery& GroupBy(std::string groupby);
	SelectQuery& Having(const Expr& having);
	SelectQuery& OrderBy(Column column, bool ascending = true);
	std::string ToString() const
	{
	}
};

} // namespace Egametang
#endif // ORM_QUERY_H
