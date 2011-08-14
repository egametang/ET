#include "Event/AndNode.h"

namespace Egametang {

AndNode::~AndNode()
{
	delete left;
	delete right;
}

bool AndNode::Check(ContexIf* contex)
{
	if (!left->Check(contex))
	{
		return false;
	}
	if (!right->Check(contex))
	{
		return false;
	}
	return true;
}

void AndNode::AddChildNode(NodeIf *node, int type)
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

