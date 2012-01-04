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
#include "Orm/OrmTypedef.h"
#include "Orm/Query.h"

namespace Egametang {

class DbHelper
{
private:
	sql::Driver* driver;
	boost::scoped_ptr<sql::Connection> connection;

public:
	DbHelper(std::string url, std::string username, std::string password);
	virtual ~DbHelper();

	template <typename Table>
	ResultSetPtr ExecuteQuery(Query<Table>& query)
	{
		StatementPtr statemet(connection->createStatement());
		ResultSetPtr resultSet(statemet->executeQuery(query.ToString()));
		return resultSet;
	}
};

} // namespace Egametang
#endif // ORM_SQLHELPER_H
