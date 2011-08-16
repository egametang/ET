#include <boost/format.hpp>
#include <glog/logging.h>
#include "Base/Typedef.h"
#include "BehaviorTree/BehaviorTree.h"
#include "BehaviorTree/NodeFactories.h"
#include "BehaviorTree/BehaviorTreeConf.pb.h"

namespace Egametang {

BehaviorTree::BehaviorTree(NodeFactories& factories, const BehaviorTreeConf& tree_conf):
		node(NULL)
{
	type = tree_conf.type();

	const BehaviorNodeConf& node_conf = tree_conf.node();
	if (tree_conf.has_node())
	{
		const BehaviorNodeConf& node_conf = tree_conf.node();
		BuildTree(factories, node_conf, node);
	}
}

void BehaviorTree::BuildTree(
		NodeFactories& factories, const BehaviorNodeConf& node_conf,
		NodeIf*& node)
{
	int32 type = node_conf.type();
	node = factories.GetInstance(node_conf);
	for (int i = 0; i < node_conf.node_size(); ++i)
	{
		const BehaviorNodeConf& logic_node_conf = node_conf.node(i);
		NodeIf* logic_node = NULL;
		BuildTree(factories, logic_node_conf, logic_node);
		node->AddChildNode(logic_node);
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

