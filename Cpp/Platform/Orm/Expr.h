// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_EXPRESSION_H
#define ORM_EXPRESSION_H

#include <list>
#include <string>
#include <boost/unordered_set.hpp>
#include <google/protobuf/message.h>

namespace Egametang {

class Column;

class Expr
{
protected:
	// 记录表达式中用到的列名
	std::list<Column> columns;
	std::string exprStr;

public:
	virtual ~Expr();
	Expr& operator=(const Expr& expr);
	std::string ToString() const;
	bool Empty() const;
	void SaveColumn(const Expr& expr);
	void SaveColumn(const Column& column);
	void CheckAllColumns(const google::protobuf::Message& message) const;
};

class Not: public Expr
{
public:
	explicit Not(const Expr& expr);
};

class And: public Expr
{
public:
	explicit And(const Expr& left, const Expr& right);
};

class Or: public Expr
{
public:
	explicit Or(const Expr& left, const Expr& right);
};

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
