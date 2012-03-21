// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_SQLHELPER_H
#define ORM_SQLHELPER_H

#include <boost/scoped_ptr.hpp>
#include <boost/make_shared.hpp>
#include <cppconn/driver.h>
#include <cppconn/exception.h>
#include <cppconn/resultset.h>
#include <cppconn/statement.h>
#include <mysql_connection.h>
#include <glog/logging.h>
#include "Orm/DbResult.h"
#include "Orm/Typedef.h"
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
		std::string sql = select.ToString();
		VLOG(2) << "execute sql: " << sql;
		ResultSetPtr resultSet(statement->executeQuery(sql));
		auto dbResult = boost::make_shared<DbResult>(resultSet);
		return dbResult;
	}
};

} // namespace Egametang
#endif // ORM_SQLHELPER_H
