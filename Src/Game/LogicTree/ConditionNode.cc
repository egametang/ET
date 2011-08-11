#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "LogicTree/ConditionNode.h"

namespace Egametang {

bool BuffType::Run(LogicContex* contex)
{
	Buff* buff = contex->buff;
	if (buff->type == type)
	{
		return true;
	}
	return false;
}

} // namespace Egametang

