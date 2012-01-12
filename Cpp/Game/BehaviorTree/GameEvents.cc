#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "Base/Typedef.h"
#include "BehaviorTree/GameEvents.h"
#include "BehaviorTree/BehaviorTreeConf.pb.h"

namespace Egametang {

GameEvents::GameEvents(NodeFactories& factories):
		factories(factories), events(100)
{
}

GameEvents::~GameEvents()
{
	foreach (std::list<BehaviorTree*> list, events)
	{
		foreach (BehaviorTree* tree, list)
		{
			delete tree;
		}
	}
}

void GameEvents::AddEvent(const BehaviorTreeConf& conf)
{
	int32 type = conf.type();
	BehaviorTree* event = new BehaviorTree(factories, conf);
	events[type].push_back(event);
}

void GameEvents::Excute(int type, ContexIf* contex)
{
	std::list<BehaviorTree*>& es = events[type];

	for (std::list<BehaviorTree*>::iterator iter = es.begin(); iter != es.end(); ++iter)
	{
		(*iter)->Run(contex);
	}
}

} // namespace Egametang
