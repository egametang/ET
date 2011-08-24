#ifndef BEHAVIORTREE_BEHAVIORTREE_H
#define BEHAVIORTREE_BEHAVIORTREE_H

#include "BehaviorTree/BehaviorNode.h"

namespace Egametang {

class BehaviorTreeConf;
class BehaviorNodeConf;
class NodeFactories;

class BehaviorTree
{
private:
	int type;
	BehaviorNode* node;

	void BuildTree(NodeFactories& factories, const BehaviorNodeConf& node_conf,
			BehaviorNode*& node);

public:
	BehaviorTree(NodeFactories& factories, const BehaviorTreeConf& tree_conf);

	~BehaviorTree();

	void Run(ContexIf* contex);

	std::string ToString();
};

} // namespace Egametang


#endif // BEHAVIORTREE_BEHAVIORTREE_H
