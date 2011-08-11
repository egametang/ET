#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "LogicTree/AndNode.h"

namespace Egametang {

bool AndNode::Run(LogicContex* contex)
{
	foreach(LogicNodeIf* node, nodes)
	{
		if (!node->Run(contex))
		{
			return false;
		}
	}
	return true;
}

} // namespace Egametang

