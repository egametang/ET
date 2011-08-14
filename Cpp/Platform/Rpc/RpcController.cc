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
	failed = false;
	reason = "";
	canceled = false;
}

bool RpcController::Failed() const
{
	return failed;
}

std::string RpcController::ErrorText() const
{
	return reason;
}

void RpcController::StartCancel()
{
}

void RpcController::SetFailed(const std::string& reason)
{
}

bool RpcController::IsCanceled() const
{
	return canceled;
}

void RpcController::NotifyOnCancel(google::protobuf::Closure *callback)
{
}

} // namespace Egametang
