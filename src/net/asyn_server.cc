#include <boost/foreach.hpp>
#include "base/base.h"
#include "net/asyn_server.h"
#include "net/connection.h"

namespace hainan {

explicit asyn_server::asyn_server(
		const string & address, const string & port):
		io_service(), acceptor(io_service),
		new_connection(new connection())
{
}

void asyn_server::handle_accept(const system::error_code & e)
{
}

void asyn_server::handle_stop()
{
	acceptor.close();
	foreach(connection_ptr connection, all_connections)
	{
		connection.stop();
	}
}

void asyn_server::start()
{
	io_service.run();
}

void asyn_server::stop()
{
	io_service.post(bind(&asyn_server::handle_stop(), this));
}

} // namespace hainan
