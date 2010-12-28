#ifndef NET_RPC_CONTROLLER_H
#define NET_RPC_CONTROLLER_H

#include <google/protobuf/service.h>

namespace Hainan {

class RpcController: public google::protobuf::RpcController
{
private:
	bool failed;
	std::string reason;
	bool canceled;

public:
	RpcController();
	~RpcController();

	// client
	virtual void Reset();
	virtual bool Failed() const;
	virtual std::string ErrorText() const;
	virtual void StartCancel();

	// server
	virtual void SetFailed(const string& reason);
	virtual bool IsCanceled() const;
	virtual void NotifyOnCancel(Closure* callback);
};

} // namespace Hainan

#endif // NET_RPC_CONTROLLER_H
