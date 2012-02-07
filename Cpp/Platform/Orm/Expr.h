// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_EXPRESSION_H
#define ORM_EXPRESSION_H

#include <string>

namespace Egametang {

class Expr
{
protected:
	std::string exprStr;

public:
	virtual ~Expr();
	Expr& operator=(const Expr& expr);
	std::string ToString() const;
	bool Empty() const;
};

class Not: public Expr
{
public:
	Not(const Expr& expr);
};

class And: public Expr
{
public:
	And(const Expr& left, const Expr& right);
};

class Or: public Expr
{
public:
	Or(const Expr& left, const Expr& right);
};

class Column;

// > < >= <= != like
class Oper: public Expr
{
public:
	Oper(const Column& left, const std::string& op, const std::string& right);
	Oper(const Column& left, const std::string& op, const Column& right);
	Oper(const Column& left, const std::string& op, int right);
	Oper(const Column& left, const std::string& op, double right);
};

} // namespace Egametang
#endif // ORM_EXPRESSION_H
