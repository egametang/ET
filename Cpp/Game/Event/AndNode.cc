#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "Event/AndNode.h"

namespace Egametang {

AndNode::~AndNode()
{
	foreach(NodeIf* node, nodes)
	{
		delete node;
	}
}

bool AndNode::Run(ContexIf* contex)
{
	foreach(NodeIf* node, nodes)
	{
		if (!node->Run(contex))
		{
			return false;
		}
	}
	return true;
}

void AndNode::AddChildNode(NodeIf *node)
{
	nodes.push_back(node);
}

std::string AndNode::ToString()
{
	std::string s;
	s += "AndNode: \n";
	foreach(NodeIf* node, nodes)
	{
		s += "    " + node->ToString() + "\n";
	}
	return s;
}

NodeIf* AndNodeFactory::GetInstance(const EventNode& conf)
{
	return new AndNode();
}

} // namespace Egametang

