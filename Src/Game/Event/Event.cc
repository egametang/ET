#include "Event/Event.h"

namespace Egametang {

Event::Event(EventConf& conf)
{
	BuildCondition(conf);
	BuildAction(conf);
}

Event::~Event()
{
	delete condition;
	delete action;
}

void Event::BuildCondition(EventConf& conf)
{

}

void Event::BuildAction(EventConf& conf)
{

}

void Event::Excute(ContexIf* contex)
{
	if(condition->Check(contex))
	{
		// 执行动作
		action->Excute(contex);
	}
}

int Event::Type() const
{
	return type;
}

} // namespace Egametang

