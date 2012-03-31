// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <boost/foreach.hpp>
#include <boost/format.hpp>
#include <google/protobuf/descriptor.h>
#include "Base/Marcos.h"
#include "Orm/Column.h"
#include "Orm/Exception.h"

namespace Egametang {

Column::Column()
{
}

Column::Column(const std::string& name)
{
	columns.push_back(name);
}

Column::Column(const Column& column)
{
	columns = column.columns;
}

Column::~Column()
{
}

Column& Column::operator()(const std::string& name)
{
	columns.push_back(name);
	return *this;
}

bool Column::Empty() const
{
	return !columns.size();
}

std::string Column::ToString() const
{
	std::string columnStr;
	foreach (const std::string& column, columns)
	{
		columnStr += column + ", ";
	}
	columnStr.resize(columnStr.size() - 2);
	return columnStr;
}

void Column::CheckAllColumns(const google::protobuf::Message& message) const
{
	const google::protobuf::Descriptor* descriptor = message.GetDescriptor();
	foreach (const std::string& column, columns)
	{
		if (column == "*")
		{
			continue;
		}
		if (!descriptor->FindFieldByName(column))
		{
			boost::format format("message: %1%, field: %2%");
			format % message.GetTypeName() % column;
			throw MessageHasNoSuchFeildException() << MessageHasNoSuchFeildErrStr(format.str());
		}
	}
}

} // namespace Egametang
