#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "LogicTree/AndNode.h"

namespace Egametang {

bool AndNode::Run()
{
	foreach(LogicNodeIf* node, nodes)
	{
		if (!node->Run())
		{
			return false;
		}
	}
	return true;
}

} // namespace Egametang

