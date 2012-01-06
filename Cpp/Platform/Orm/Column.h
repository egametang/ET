#ifndef ORM_COLUMN_H
#define ORM_COLUMN_H

#include <string>
#include "Orm/Expr.h"

namespace Egametang {

class Column
{
private:
	std::string columnStr;

public:
	Column(const std::string name);
	~Column();
	Column& operator()(std::string& name);
	bool Empty();
	std::string ToString() const;

	template <typename T>
	Expr operator>(const T& value)
	{
		return Oper(*this,">", value);
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
		return Oper(*this, "<>", value);
	}

	template <typename T>
	Expr operator==(const T& value)
	{
		return Oper(*this, "=", value);
	}

	Expr like(const std::string value)
	{
		return Oper(*this, "like", value);
	}
};

} // namespace Egametang
#endif // ORM_COLUMN_H
