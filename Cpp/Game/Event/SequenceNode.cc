#include <glog/logging.h>
#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "Event/SequenceNode.h"

namespace Egametang {

SequenceNode::~SequenceNode()
{
	foreach(NodeIf* node, nodes)
	{
		delete node;
	}
}

bool SequenceNode::Run(ContexIf* contex)
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

void SequenceNode::AddChildNode(NodeIf *node)
{
	nodes.push_back(node);
}

std::string SequenceNode::ToString()
{
	std::string s;
	s += "SequenceNode: \n";
	foreach(NodeIf* node, nodes)
	{
		s += "    " + node->ToString() + "\n";
	}
	return s;
}

NodeIf* SequenceNodeFactory::GetInstance(const EventNode& conf)
{
	return new SequenceNode();
}

} // namespace Egametang

