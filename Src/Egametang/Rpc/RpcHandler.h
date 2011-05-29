#ifndef RPC_RPC_HANDLER_H
#define RPC_RPC_HANDLER_H

namespace Egametang {

class google::protobuf::RpcController;
class google::protobuf::Message;
class google::protobuf::Closure;

class RpcHandler
{
private:
	google::protobuf::RpcController* controller_;
	google::protobuf::Message* response_;
	google::protobuf::Closure* done_;
public:
	RpcHandler(google::protobuf::RpcController* controller,
			google::protobuf::Message* response,
			google::protobuf::Closure* done);
    google::protobuf::RpcController *GetController() const;
    google::protobuf::Closure *GetDone() const;
    google::protobuf::Message *GetResponse() const;
};

} // namespace Egametang

#endif // RPC_RPC_HANDLER_H
