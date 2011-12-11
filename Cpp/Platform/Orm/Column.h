#ifndef ORM_COLUMN_H
#define ORM_COLUMN_H

#include <string>
#include "Orm/Expr.h"

namespace Egametang {

class Column
{
private:
	std::string columns;

public:
	Column(const std::string name);
	~Column();
	Column& operator()(std::string& column);
	std::string ToString() const;

	Gt operator>(const std::string& value);
	Gt operator>(int value);
	Gt operator>(double value);
	Gt operator>(const Column value);

	Ge operator>=(const std::string& value);
	Ge operator>=(int value);
	Ge operator>=(double value);
	Ge operator>=(const Column value);

	Lt operator<(const std::string& value);
	Lt operator<(int value);
	Lt operator<(double value);
	Lt operator<(const Column value);

	Le operator<=(const std::string& value);
	Le operator<=(int value);
	Le operator<=(double value);
	Le operator<=(const Column value);

	Ne operator!=(const std::string& value);
	Ne operator!=(int value);
	Ne operator!=(double value);
	Ne operator!=(const Column value);

	Like like(const std::string value);
};

} // namespace Egametang
#endif // ORM_COLUMN_H
