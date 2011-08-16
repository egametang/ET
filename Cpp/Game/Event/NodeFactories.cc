#include <glog/logging.h>
#include "Base/Typedef.h"
#include "Event/AndNode.h"
#include "Event/OrNode.h"
#include "Event/NotNode.h"
#include "Event/SequenceNode.h"
#include "Event/SelectorNode.h"
#include "Event/BuffType.h"
#include "Event/ChangeHealth.h"
#include "Event/NodeFactories.h"
#include "Event/EventConf.pb.h"
#include "Event/EventDefine.h"

namespace Egametang {

NodeFactories::NodeFactories(): factories(2000, NULL)
{
	// 条件节点
	factories[AND] = new AndNodeFactory();
	factories[OR] = new OrNodeFactory();
	factories[NOT] = new NotNodeFactory();

	// 动作节点
	factories[SEQUENCE] = new SequenceNodeFactory();
	factories[SELECTOR] = new SelectorNodeFactory();

	// 条件叶子节点
	factories[BUFF_TYPE] = new BuffTypeFactory();

	// 动作叶子节点
	factories[CHANGE_HEALTH] = new ChangeHealthFactory();
}

NodeFactories::~NodeFactories()
{
	for (std::size_t i = 0; i < factories.size(); ++i)
	{
		if (factories[i] == NULL)
		{
			continue;
		}
		delete factories[i];
	}
}

NodeIf* NodeFactories::GetInstance(const EventNode& conf)
{
	int32 type = conf.type();
	return factories[type]->GetInstance(conf);
}

}


