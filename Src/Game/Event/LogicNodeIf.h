#ifndef EVENT_LOGICNODEIF_H
#define EVENT_LOGICNODEIF_H

namespace Egametang {

class Spell
{

};

class Buff
{
public:
	int type;
};

class ContexIf
{
};

class LogicNodeIf
{
public:
	virtual ~LogicNodeIf();
	virtual bool Check(ContexIf* contex) = 0;
};

} // namespace Egametang

#endif // EVENT_LOGICNODEIF_H
