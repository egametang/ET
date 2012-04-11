#include <stddef.h>
#include "BehaviorTree/NotNode.h"
#include "BehaviorTree/BehaviorTreeConf.pb.h"

namespace Egametang {

NotNode::NotNode(int32 type): BehaviorNode(type), node(nullptr)
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

void NotNode::AddChildNode(BehaviorNode *node)
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

BehaviorNode* NotNodeFactory::GetInstance(const BehaviorNodeConf& conf)
{
	return new NotNode(conf.type());
}

} // namespace Egametang

