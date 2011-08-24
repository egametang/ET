#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "BehaviorTree/SelectorNode.h"
#include "BehaviorTree/BehaviorTreeConf.pb.h"

namespace Egametang {

SelectorNode::SelectorNode(int32 type): BehaviorNode(type)
{
}

SelectorNode::~SelectorNode()
{
	foreach (BehaviorNode* node, nodes)
	{
		delete node;
	}
}

bool SelectorNode::Run(ContexIf* contex)
{
	foreach (BehaviorNode* node, nodes)
	{
		if (node->Run(contex))
		{
			return true;
		}
	}
	return false;
}

void SelectorNode::AddChildNode(BehaviorNode *node)
{
	nodes.push_back(node);
}

std::string SelectorNode::ToString()
{
	std::string s;
	s += "SelectorNode: \n";
	foreach (BehaviorNode* node, nodes)
	{
		s += "    " + node->ToString() + "\n";
	}
	return s;
}

BehaviorNode* SelectorNodeFactory::GetInstance(const BehaviorNodeConf& conf)
{
	return new SelectorNode(conf.type());
}

} // namespace Egametang

