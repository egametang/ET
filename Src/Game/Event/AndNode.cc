#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "Event/AndNode.h"

namespace Egametang {
AndNode::~AndNode()
{
	foreach(LogicNodeIf* node, nodes)
	{
		delete node;
	}
}

bool AndNode::Check(ContexIf* contex)
{
	foreach(LogicNodeIf* node, nodes)
	{
		if (!node->Check(contex))
		{
			return false;
		}
	}
	return true;
}

} // namespace Egametang

