// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_EXPRESSION_H
#define ORM_EXPRESSION_H

namespace Egametang {

class Expr
{
public:
	virtual ~Expr()
	{
	}
	virtual std::string ToString()
	{
		return "true";
	}
};

class Oper: public Expr
{
protected:
	std::string left;
	std::string op;
	std::string right;

	Oper(const std::string& left, const std::string& o, const std::string& right):
			left(left), op(o), right(right)
	{
	}

public:
	std::string ToString() const
	{
		return left + " " + op + " " + right;
	}
};

class Eq: public Oper
{
public:
	Eq(const std::string& left, const std::string& right):
			Oper(left, "=", right)
	{
	}
};

class Ne: public Oper
{
public:
	Ne(const std::string& left, const std::string& right):
			Oper(left, "<>", right)
	{
	}
};

class Gt: public Oper
{
public:
	Gt(const std::string& left, const std::string& right):
			Oper(left, ">", right)
	{
	}
};

class Ge: public Oper
{
public:
	Ge(const std::string& left, const std::string& right):
			Oper(left, ">=", right)
	{
	}
};

class Lt: public Oper
{
public:
	Lt(const std::string& left, const std::string& right):
			Oper(left, "<", right)
	{
	}
};

class Le: public Oper
{
public:
	Le(const std::string& left, const std::string& right):
			Oper(left, "<=", right)
	{
	}
};

class Like: public Oper
{
public:
	Like(const std::string& left, const std::string& right):
			Oper(left, "like", right)
	{
	}
};

} // namespace Egametang
#endif // ORM_EXPRESSION_H
