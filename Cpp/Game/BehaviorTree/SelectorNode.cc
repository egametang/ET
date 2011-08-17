#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "BehaviorTree/SelectorNode.h"
#include "BehaviorTree/BehaviorTreeConf.pb.h"

namespace Egametang {

SelectorNode::SelectorNode(int32 type): BehaviorNodeIf(type)
{
}

SelectorNode::~SelectorNode()
{
	foreach(BehaviorNodeIf* node, nodes)
	{
		delete node;
	}
}

bool SelectorNode::Run(ContexIf* contex)
{
	foreach(BehaviorNodeIf* node, nodes)
	{
		if (node->Run(contex))
		{
			return true;
		}
	}
	return false;
}

void SelectorNode::AddChildNode(BehaviorNodeIf *node)
{
	nodes.push_back(node);
}

std::string SelectorNode::ToString()
{
	std::string s;
	s += "SelectorNode: \n";
	foreach(BehaviorNodeIf* node, nodes)
	{
		s += "    " + node->ToString() + "\n";
	}
	return s;
}

BehaviorNodeIf* SelectorNodeFactory::GetInstance(const BehaviorNodeConf& conf)
{
	return new SelectorNode(conf.type());
}

} // namespace Egametang

