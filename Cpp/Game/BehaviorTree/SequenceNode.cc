#include <glog/logging.h>
#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "BehaviorTree/SequenceNode.h"
#include "BehaviorTree/BehaviorTreeConf.pb.h"

namespace Egametang {

SequenceNode::SequenceNode(int32 type): BehaviorNode(type)
{
}

SequenceNode::~SequenceNode()
{
	foreach (BehaviorNode* node, nodes)
	{
		delete node;
	}
}

bool SequenceNode::Run(ContexIf* contex)
{
	foreach (BehaviorNode* node, nodes)
	{
		if (!node->Run(contex))
		{
			return false;
		}
	}
	return true;
}

void SequenceNode::AddChildNode(BehaviorNode *node)
{
	nodes.push_back(node);
}

std::string SequenceNode::ToString()
{
	std::string s;
	s += "SequenceNode: \n";
	foreach (BehaviorNode* node, nodes)
	{
		s += "    " + node->ToString() + "\n";
	}
	return s;
}

BehaviorNode* SequenceNodeFactory::GetInstance(const BehaviorNodeConf& conf)
{
	return new SequenceNode(conf.type());
}

} // namespace Egametang

