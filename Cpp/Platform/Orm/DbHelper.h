// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_SQLHELPER_H
#define ORM_SQLHELPER_H

#include <boost/scoped_ptr.hpp>
#include <cppconn/driver.h>
#include <cppconn/exception.h>
#include <cppconn/resultset.h>
#include <cppconn/statement.h>
#include <mysql_connection.h>
#include "Orm/DbResult.h"
#include "Orm/OrmTypedef.h"
#include "Orm/Select.h"

namespace Egametang {

class DbHelper
{
private:
	boost::scoped_ptr<sql::Connection> connection;
	boost::scoped_ptr<sql::Statement> statement;

public:
	DbHelper(std::string url, std::string username, std::string password);
	virtual ~DbHelper();

	template <typename Table>
	DbResultPtr Execute(Select<Table> select)
	{
		ResultSetPtr resultSet(statement->executeQuery(select.ToString()));
		DbResultPtr dbResult(new DbResult(resultSet));
		return dbResult;
	}
};

} // namespace Egametang
#endif // ORM_SQLHELPER_H
