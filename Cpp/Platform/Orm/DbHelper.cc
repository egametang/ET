// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include "Orm/DbHelper.h"

namespace Egametang {

DbHelper::DbHelper(std::string url, std::string username, std::string password):
		driver(NULL)
{
	driver = get_driver_instance();
	connection.reset(driver->connect(url, username, password));
}

DbHelper::~DbHelper()
{
}

} // namespace Egametang
