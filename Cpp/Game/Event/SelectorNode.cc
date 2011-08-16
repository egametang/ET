#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "Event/SelectorNode.h"

namespace Egametang {

SelectorNode::~SelectorNode()
{
	foreach(NodeIf* node, nodes)
	{
		delete node;
	}
}

bool SelectorNode::Run(ContexIf* contex)
{
	foreach(NodeIf* node, nodes)
	{
		if (node->Run(contex))
		{
			return true;
		}
	}
	return false;
}

void SelectorNode::AddChildNode(NodeIf *node)
{
	nodes.push_back(node);
}

std::string SelectorNode::ToString()
{
	std::string s;
	s += "SelectorNode: \n";
	foreach(NodeIf* node, nodes)
	{
		s += "    " + node->ToString() + "\n";
	}
	return s;
}

NodeIf* SelectorNodeFactory::GetInstance(const EventNode& conf)
{
	return new SelectorNode();
}

} // namespace Egametang

