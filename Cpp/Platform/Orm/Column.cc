#include "Orm/Column.h"

namespace Egametang {

Column::Column(const std::string name)
{
	columns += name;
}

Column::~Column()
{
}

Column& Column::operator()(std::string& column)
{
	columns = columns + ", " + column;
	return *this;
}

std::string Column::ToString() const
{
	return columns;
}


Gt Column::operator>(const std::string& value)
{
	value = "'" + value + "'";
	return Gt(*this, value);
}
Gt Column::operator>(int value)
{
	std::string strRight = boost::lexical_cast<std::string>(value);
	return Gt(*this, strRight);
}
Gt Column::operator>(double value)
{
	std::string strRight = boost::lexical_cast<std::string>(value);
	return Gt(*this, strRight);
}
Gt Column::operator>(const Column value)
{
	std::string strRight = value.ToString();
	return Gt(*this, strRight);
}


Ge Column::operator>=(const std::string& value)
{
	value = "'" + value + "'";
	return Ge(*this, value);
}
Ge Column::operator>=(int value)
{
	std::string strRight = boost::lexical_cast<std::string>(value);
	return Ge(*this, strRight);
}
Ge Column::operator>=(double value)
{
	std::string strRight = boost::lexical_cast<std::string>(value);
	return Ge(*this, strRight);
}
Ge Column::operator>=(const Column value)
{
	std::string strRight = value.ToString();
	return Ge(*this, strRight);
}


Lt Column::operator<(const std::string& value)
{
	value = "'" + value + "'";
	return Lt(*this, value);
}
Lt Column::operator<(int value)
{
	std::string strRight = boost::lexical_cast<std::string>(value);
	return Lt(*this, strRight);
}
Lt Column::operator<(double value)
{
	std::string strRight = boost::lexical_cast<std::string>(value);
	return Lt(*this, strRight);
}
Lt Column::operator<(const Column value)
{
	std::string strRight = value.ToString();
	return Lt(*this, strRight);
}


Le Column::operator<=(const std::string& value)
{
	value = "'" + value + "'";
	return Le(*this, value);
}
Le Column::operator<=(int value)
{
	std::string strRight = boost::lexical_cast<std::string>(value);
	return Le(*this, strRight);
}
Le Column::operator<=(double value)
{
	std::string strRight = boost::lexical_cast<std::string>(value);
	return Le(*this, strRight);
}
Le Column::operator<=(const Column value)
{
	std::string strRight = value.ToString();
	return Le(*this, strRight);
}


Ne Column::operator!=(const std::string& value)
{
	value = "'" + value + "'";
	return Ne(*this, value);
}
Ne Column::operator!=(int value)
{
	std::string strRight = boost::lexical_cast<std::string>(value);
	return Ne(*this, strRight);
}
Ne Column::operator!=(double value)
{
	std::string strRight = boost::lexical_cast<std::string>(value);
	return Ne(*this, strRight);
}
Ne Column::operator!=(const Column value)
{
	std::string strRight = value.ToString();
	return Ne(*this, strRight);
}

Like like(const std::string value)
{
	value = "'" + value + "'";
	return Like(*this, value);
}

} // namespace Egametang
