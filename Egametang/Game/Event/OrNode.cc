#include "Event/OrNode.h"

namespace Egametang {
OrNode::~OrNode()
{
	delete left;
	delete right;
}

bool OrNode::Check(ContexIf* contex)
{
	if (left->Check(contex))
	{
		return true;
	}
	if (right->Check(contex))
	{
		return true;
	}
	return false;
}

void OrNode::AddChildNode(NodeIf *node, int type)
{
	if (type == 0)
	{
		left = node;
	}
	else
	{
		right = node;
	}
}

} // namespace Egametang

