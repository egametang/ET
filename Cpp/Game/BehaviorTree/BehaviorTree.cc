#include <boost/format.hpp>
#include <glog/logging.h>
#include "Base/Typedef.h"
#include "BehaviorTree/BehaviorTree.h"
#include "BehaviorTree/NodeFactories.h"
#include "BehaviorTree/BehaviorTreeConf.pb.h"

namespace Egametang {

BehaviorTree::BehaviorTree(NodeFactories& factories, const BehaviorTreeConf& treeConf):
		node(NULL)
{
	type = treeConf.type();

	if (treeConf.has_node())
	{
		const BehaviorNodeConf& nodeConf = treeConf.node();
		BuildTree(factories, nodeConf, node);
	}
}

void BehaviorTree::BuildTree(
		NodeFactories& factories, const BehaviorNodeConf& nodeConf,
		BehaviorNode*& node)
{
	int32 type = nodeConf.type();
	node = factories.GetInstance(nodeConf);
	for (int i = 0; i < nodeConf.node_size(); ++i)
	{
		const BehaviorNodeConf& logicNodeConf = nodeConf.node(i);
		BehaviorNode* logicNode = NULL;
		BuildTree(factories, logicNodeConf, logicNode);
		node->AddChildNode(logicNode);
	}
}

BehaviorTree::~BehaviorTree()
{
	delete node;
}

void BehaviorTree::Run(ContexIf* contex)
{
	node->Run(contex);
}

std::string BehaviorTree::ToString()
{
	boost::format format("type: %1%\node: %2%");
	format % type % node->ToString();
	return format.str();
}

} // namespace Egametang

