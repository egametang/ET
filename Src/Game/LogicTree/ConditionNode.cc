#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "LogicTree/ConditionNode.h"

namespace Egametang {

bool ConditionNode::Run()
{
	return condition();
}

} // namespace Egametang

