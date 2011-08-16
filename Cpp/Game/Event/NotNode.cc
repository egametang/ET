#include <stddef.h>
#include "Event/NotNode.h"

namespace Egametang {

NotNode::NotNode(): node(NULL)
{
}

NotNode::~NotNode()
{
	delete node;
}

bool NotNode::Run(ContexIf* contex)
{
	return !node->Run(contex);
}

void NotNode::AddChildNode(NodeIf *node)
{
	this->node = node;
}

std::string NotNode::ToString()
{
	std::string s;
	s += "NotNode: \n";
	s += "    " + node->ToString() + "\n";
	return s;
}

NodeIf* NotNodeFactory::GetInstance(const EventNode& conf)
{
	return new NotNode();
}

} // namespace Egametang

