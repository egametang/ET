#include <boost/format.hpp>
#include <glog/logging.h>
#include "Base/Typedef.h"
#include "Event/Event.h"
#include "Event/NodeFactories.h"
#include "Event/EventConf.pb.h"

namespace Egametang {

Event::Event(NodeFactories& factories, EventConf& conf):
		action(NULL), condition(NULL)
{
	type = conf.type();

	const ConditionConf& condition_conf = conf.condition();
	if (condition_conf.has_node())
	{
		const EventNode& node_conf = condition_conf.node();
		BuildTree(factories, node_conf, condition);
	}

	const ActionConf& action_conf = conf.action();
	if (action_conf.has_node())
	{
		const EventNode& node_conf = action_conf.node();
		BuildTree(factories, node_conf, action);
	}
}

void Event::BuildTree(
		NodeFactories& factories, const EventNode& conf,
		NodeIf*& node)
{
	int32 type = conf.type();
	node = factories.GetInstance(conf);
	for (int i = 0; i < conf.nodes_size(); ++i)
	{
		const EventNode& logic_node_conf = conf.nodes(i);
		NodeIf* logic_node = NULL;
		BuildTree(factories, logic_node_conf, logic_node);
		node->AddChildNode(logic_node);
	}
}

Event::~Event()
{
	delete condition;
	delete action;
}

void Event::Run(ContexIf* contex)
{
	if(condition->Run(contex))
	{
		// 执行动作
		CHECK(action);
		action->Run(contex);
	}
}

std::string Event::ToString()
{
	boost::format format("type: %1%\ncondition: %2%\naction: %3%");
	format % type % condition->ToString() % action->ToString();
	return format.str();
}

} // namespace Egametang

