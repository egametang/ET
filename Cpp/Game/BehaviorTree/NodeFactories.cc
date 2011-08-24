#include <glog/logging.h>
#include "Base/Typedef.h"
#include "BehaviorTree/NotNode.h"
#include "BehaviorTree/SequenceNode.h"
#include "BehaviorTree/SelectorNode.h"
#include "BehaviorTree/BuffType.h"
#include "BehaviorTree/ChangeHealth.h"
#include "BehaviorTree/NodeFactories.h"
#include "BehaviorTree/BehaviorTreeConf.pb.h"
#include "BehaviorTree/EventDefine.h"

namespace Egametang {

NodeFactories::NodeFactories(): factories(2000, (BehaviorNodeFactoryIf*)(NULL))
{
	// 节点
	factories[SEQUENCE] = new SequenceNodeFactory();
	factories[SELECTOR] = new SelectorNodeFactory();
	factories[NOT] = new NotNodeFactory();

	// 叶子节点
	factories[BUFF_TYPE] = new BuffTypeFactory();
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

BehaviorNode* NodeFactories::GetInstance(const BehaviorNodeConf& conf)
{
	int32 type = conf.type();
	return factories[type]->GetInstance(conf);
}

}


