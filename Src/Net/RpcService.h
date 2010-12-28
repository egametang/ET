#ifndef NET_RPC_SERVICE_H
#define NET_RPC_SERVICE_H

#include <google/protobuf/service.h>

namespace Hainan {

class RpcService
{
private:
	typedef boost::unordered_map<std::string, RpcHandlerPtr> RpcServiceMap;

	RpcServiceMap services;


public:
	RpcServer(google::protobuf::Service* service);
	~RpcServer();
	void Start();
	void Stop();
};

} // namespace Hainan

#endif // NET_RPC_SERVICE_H
