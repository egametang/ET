#include <glog/logging.h>
#include "Event/NotNode.h"

namespace Egametang {
NotNode::~NotNode()
{
	delete node;
}

bool NotNode::Check(ContexIf* contex)
{
	return !node->Check(contex);
}

void NotNode::AddChildNode(NodeIf *node, int type)
{
	CHECK_EQ(1, type);
	this->node = node;
}

} // namespace Egametang

