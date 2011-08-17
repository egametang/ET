#include <glog/logging.h>
#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "BehaviorTree/SequenceNode.h"
#include "BehaviorTree/BehaviorTreeConf.pb.h"

namespace Egametang {

SequenceNode::SequenceNode(int32 type): BehaviorNodeIf(type)
{
}

SequenceNode::~SequenceNode()
{
	foreach(BehaviorNodeIf* node, nodes)
	{
		delete node;
	}
}

bool SequenceNode::Run(ContexIf* contex)
{
	foreach(BehaviorNodeIf* node, nodes)
	{
		if (!node->Run(contex))
		{
			return false;
		}
	}
	return true;
}

void SequenceNode::AddChildNode(BehaviorNodeIf *node)
{
	nodes.push_back(node);
}

std::string SequenceNode::ToString()
{
	std::string s;
	s += "SequenceNode: \n";
	foreach(BehaviorNodeIf* node, nodes)
	{
		s += "    " + node->ToString() + "\n";
	}
	return s;
}

BehaviorNodeIf* SequenceNodeFactory::GetInstance(const BehaviorNodeConf& conf)
{
	return new SequenceNode(conf.type());
}

} // namespace Egametang

