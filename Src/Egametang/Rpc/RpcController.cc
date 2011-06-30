#include "Rpc/RpcController.h"

namespace Egametang {

RpcController::RpcController()
{
}

RpcController::~RpcController()
{
}

void RpcController::Reset()
{
	failed_ = false;
	reason_ = "";
	canceled_ = false;
}

bool RpcController::Failed() const
{
	return failed_;
}

std::string RpcController::ErrorText() const
{
	return reason_;
}

void RpcController::StartCancel()
{
}

void RpcController::SetFailed(const std::string& reason)
{
}

bool RpcController::IsCanceled() const
{
}

void RpcController::NotifyOnCancel(google::protobuf::Closure *callback)
{
}

} // namespace Egametang
