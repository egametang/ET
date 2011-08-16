#ifndef BEHAVIORTREE_BEHAVIORTREE_H
#define BEHAVIORTREE_BEHAVIORTREE_H

#include "BehaviorTree/NodeIf.h"

namespace Egametang {

class BehaviorTreeConf;
class BehaviorNodeConf;
class NodeFactories;

class BehaviorTree
{
private:
	int type;
	NodeIf* node;

	void BuildTree(NodeFactories& factories, const BehaviorNodeConf& node_conf,
			NodeIf*& node);

public:
	BehaviorTree(NodeFactories& factories, const BehaviorTreeConf& tree_conf);

	~BehaviorTree();

	void Run(ContexIf* contex);

	std::string ToString();
};

} // namespace Egametang


#endif // BEHAVIORTREE_BEHAVIORTREE_H
