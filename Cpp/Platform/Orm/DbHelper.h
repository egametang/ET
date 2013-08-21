// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_SQLHELPER_H
#define ORM_SQLHELPER_H

#include <cppconn/driver.h>
#include <cppconn/exception.h>
#include <cppconn/resultset.h>
#include <cppconn/statement.h>
#include <mysql_connection.h>
#include "Orm/DbResult.h"
#include "Orm/Typedef.h"
#include "Orm/Select.h"

namespace Egametang {

class DbHelper
{
private:
	std::unique_ptr<sql::Connection> connection;
	std::unique_ptr<sql::Statement> statement;

public:
	DbHelper(std::string url, std::string username, std::string password);
	virtual ~DbHelper();

	template <typename Table>
	DbResultPtr Execute(Select<Table> select)
	{
		std::string sql = select.ToString();
		ResultSetPtr resultSet(statement->executeQuery(sql));
		auto dbResult = boost::make_shared<DbResult>(resultSet);
		return dbResult;
	}
};

} // namespace Egametang
#endif // ORM_SQLHELPER_H
