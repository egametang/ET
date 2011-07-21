#include "Rpc/RpcSession.h"

namespace Egametang {

RpcSession::RpcSession(RpcServer& rpc_server):
		rpc_server_(rpc_server), RpcCommunicator(rpc_server_.io_service_)
{
}

void RpcSession::OnRecvMessage(RpcMetaPtr meta, StringPtr message)
{
	RpcMetaPtr send_meta(new RpcMeta());
	StringPtr send_message(new std::string);
	send_meta->id = meta->id;

	google::protobuf::Message* request;
	request->ParseFromString(*message);

	google::protobuf::Closure* done = google::protobuf::NewCallback(
			this, &RpcServer::Callback, shared_from_this(), handler);
	const google::protobuf::MethodDescriptor* method = NULL;



	rpc_server_.thread_pool_.Schedule(
			boost::bind(&RpcSession::SendResponse, shared_from_this(),
					send_meta, send_message));

	rpc_server_.RunService(meta, message,
			boost::bind(&RpcSession::SendResponse, shared_from_this(), _1, _2));
	// read size
	RpcMetaPtr recv_meta(new RpcMeta());
	StringPtr recv_message(new std::string);
	RecvMeta(recv_meta, recv_message);
}

void RpcSession::OnSendMessage(RpcMetaPtr meta, StringPtr message)
{
}

void RpcSession::SendResponse(RpcMetaPtr meta, StringPtr message)
{
	SendMeta(meta, message);
}

void RpcSession::Start()
{
	RpcMetaPtr meta(new RpcMeta());
	StringPtr message(new std::string);
	RecvMeta(meta, message);
}

void RpcSession::Stop()
{
	socket_.close();
	rpc_server_.RemoveSession(shared_from_this());
}

} // namespace Egametang
