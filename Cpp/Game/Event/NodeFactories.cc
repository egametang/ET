#include <glog/logging.h>
#include "Base/Typedef.h"
#include "Event/AndNode.h"
#include "Event/OrNode.h"
#include "Event/NotNode.h"
#include "Event/BuffType.h"
#include "Event/ChangeHealth.h"
#include "Event/NodeFactories.h"
#include "Event/EventConf.pb.h"

namespace Egametang {

NodeFactories::NodeFactories(): factories(2000, NULL)
{
	factories[0] = new AndNodeFactory();
	factories[1] = new OrNodeFactory();
	factories[2] = new NotNodeFactory();

	// 条件节点
	factories[101] = new BuffTypeFactory();

	// 行为节点
	factories[1001] = new ChangeHealthFactory();
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


