#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "LogicTree/OrNode.h"

namespace Egametang {

bool OrNode::Run()
{
	foreach(LogicNodeIf* node, nodes)
	{
		if (node->Run())
		{
			return true;
		}
	}
	return false;
}

} // namespace Egametang

