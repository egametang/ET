#include "Net/RpcController.h"

namespace Hainan {

Hainan::RpcController::RpcController()
{
}

Hainan::RpcController::~RpcController()
{
}

void Hainan::RpcController::Reset()
{
	failed = false;
	reason = "";
	canceled = false;
}

bool Hainan::RpcController::Failed() const
{
	return failed;
}

std::string Hainan::RpcController::ErrorText() const
{
	return reason;
}

void Hainan::RpcController::StartCancel()
{
}

void Hainan::RpcController::SetFailed(const string & reason)
{
}

bool Hainan::RpcController::IsCanceled() const
{
}

void Hainan::RpcController::NotifyOnCancel(Closure *callback)
{
}

} // namespace Hainan
