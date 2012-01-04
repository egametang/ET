#include "Orm/Column.h"

namespace Egametang {

Column::Column(const std::string name): columnStr(name)
{
}

Column::~Column()
{
}

Column& Column::operator()(std::string& name)
{
	columnStr = columnStr + ", " + name;
	return *this;
}

std::string Column::ToString() const
{
	return columnStr;
}

} // namespace Egametang
