#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "Event/OrNode.h"

namespace Egametang {
OrNode::~OrNode()
{
	foreach(LogicNodeIf* node, nodes)
	{
		delete node;
	}
}

bool OrNode::Check(ContexIf* contex)
{
	foreach(LogicNodeIf* node, nodes)
	{
		if (node->Check(contex))
		{
			return true;
		}
	}
	return false;
}

} // namespace Egametang

