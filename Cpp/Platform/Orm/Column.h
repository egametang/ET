// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_COLUMN_H
#define ORM_COLUMN_H

#include <list>
#include <string>
#include "Orm/Expr.h"

namespace Egametang {

class Column
{
private:
	std::list<std::string> columns;

public:
	Column();
	Column(const std::string& name);
	Column(const Column& column);
	~Column();
	bool Empty() const;
	Column& operator()(const std::string& name);
	std::string ToString() const;
	void CheckAllColumns(const google::protobuf::Message& message) const;

	template <typename T>
	Expr operator>(const T& value)
	{
		return Oper(*this, ">", value);
	}

	template <typename T>
	Expr operator>=(const T& value)
	{
		return Oper(*this, ">=", value);
	}

	template <typename T>
	Expr operator<(const T& value)
	{
		return Oper(*this, "<", value);
	}

	template <typename T>
	Expr operator<=(const T& value)
	{
		return Oper(*this, "<=", value);
	}

	template <typename T>
	Expr operator!=(const T& value)
	{
		return Oper(*this, "!=", value);
	}

	template <typename T>
	Expr operator==(const T& value)
	{
		return Oper(*this, "=", value);
	}

	Expr like(const std::string& value)
	{
		return Oper(*this, "like", value);
	}
};

} // namespace Egametang
#endif // ORM_COLUMN_H
