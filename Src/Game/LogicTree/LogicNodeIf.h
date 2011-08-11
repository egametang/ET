#ifndef LOGICTREE_LOGICNODEIF_H
#define LOGICTREE_LOGICNODEIF_H

namespace Egametang {

class Spell;
class Buff;

class Spell
{

};

class Buff
{
public:
	int type;
};

class LogicContex
{
public:
	Spell* spell;
	Buff* buff;
};

class LogicNodeIf
{
public:
	virtual bool Run(LogicContex* contex) = 0;
};

} // namespace Egametang

#endif // LOGICTREE_LOGICNODEIF_H
