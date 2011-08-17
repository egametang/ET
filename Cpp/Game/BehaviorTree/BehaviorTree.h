#ifndef BEHAVIORTREE_BEHAVIORTREE_H
#define BEHAVIORTREE_BEHAVIORTREE_H

#include "BehaviorTree/BehaviorNodeIf.h"

namespace Egametang {

class BehaviorTreeConf;
class BehaviorNodeConf;
class NodeFactories;

class BehaviorTree
{
private:
	int type;
	BehaviorNodeIf* node;

	void BuildTree(NodeFactories& factories, const BehaviorNodeConf& node_conf,
			BehaviorNodeIf*& node);

public:
	BehaviorTree(NodeFactories& factories, const BehaviorTreeConf& tree_conf);

	~BehaviorTree();

	void Run(ContexIf* contex);

	std::string ToString();
};

} // namespace Egametang


#endif // BEHAVIORTREE_BEHAVIORTREE_H
