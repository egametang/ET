// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include "Orm/DbHelper.h"

namespace Egametang {

DbHelper::DbHelper(std::string url, std::string username, std::string password)
{
	sql::Driver* driver = get_driver_instance();
	connection.reset(driver->connect(url, username, password));
	statement.reset(connection->createStatement());
}

DbHelper::~DbHelper()
{
}

} // namespace Egametang
