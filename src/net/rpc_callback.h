#ifndef NET_RPC_CALLBACK_H
#define NET_RPC_CALLBACK_H

namespace hainan {

class google::protobuf::RpcController;
class google::protobuf::Message;
class google::protobuf::Closure;

class rpc_callback
{
private:
	google::protobuf::RpcController* controller_;
	google::protobuf::Message* response_;
	google::protobuf::Closure* done_;

public:
	rpc_callback(google::protobuf::RpcController* controller,
			google::protobuf::Message* response,
			google::protobuf::Closure* done);
	~rpc_callback();
    google::protobuf::RpcController*controller() const;
    google::protobuf::Closure* done() const;
    google::protobuf::Message* response() const;
};

} // hainan

#endif // NET_RPC_CALLBACK_H
