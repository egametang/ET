#ifndef NET_RPC_SERVER_H
#define NET_RPC_SERVER_H

namespace Hainan {

class RpcServer
{
private:
	struct RpcServerFeild;
	boost::scoped_ptr<RpcServerFeild> rpc_server;
public:
	RpcServer();
	~RpcServer();
	void Start();
	void Stop();
};

} // namespace Hainan

#endif // NET_RPC_SERVER_H
