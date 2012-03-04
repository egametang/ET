// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <boost/format.hpp>
#include "Orm/DbHelper.h"
#include "Orm/Exception.h"

namespace Egametang {

DbHelper::DbHelper(std::string url, std::string username, std::string password)
{
	try
	{
		sql::Driver* driver = get_driver_instance();
		connection.reset(driver->connect(url, username, password));
	}
	catch (sql::SQLException &e)
	{
		boost::format format("can't connect to %1%, username: %2%, password: %3%, error code: %4%");
		format % url % username % password % e.getErrorCode();
		throw ConnectionException() << ConnectionErrStr(format.str());
	}
	statement.reset(connection->createStatement());
}

DbHelper::~DbHelper()
{
}

} // namespace Egametang
