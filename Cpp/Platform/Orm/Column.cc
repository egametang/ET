// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include "Orm/Column.h"

namespace Egametang {

Column::Column()
{
}

Column::Column(const std::string& name): columnStr(name)
{
}

Column::Column(const Column& column)
{
	columnStr = column.columnStr;
}

Column::~Column()
{
}

Column& Column::operator()(std::string& name)
{
	columnStr = columnStr + ", " + name;
	return *this;
}

bool Column::Empty() const
{
	return columnStr.empty();
}

std::string Column::ToString() const
{
	return columnStr;
}

} // namespace Egametang
