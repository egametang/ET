#ifndef NET_RPC_SESSION_H
#define NET_RPC_SESSION_H

#include <boost/asio.hpp>
#include <boost/array.hpp>
#include <boost/noncopyable.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>

namespace Hainan {

class RpcSession:
		public boost::enable_shared_from_this<RpcSession>,
		private boost::noncopyable
{
private:
	struct RpcSessionFeild;
	boost::scoped_ptr<RpcSessionFeild> rpc_session;

public:
	RpcSession();
	~RpcSession();
	boost::asio::ip::tcp::socket& Socket();
	void Start();
	void Stop();
};

typedef boost::shared_ptr<RpcSession> RpcSessionPtr;

}

#endif // NET_RPC_SESSION_H
