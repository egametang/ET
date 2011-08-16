#ifndef BEHAVIORTREE_SPELLBUFF_H
#define BEHAVIORTREE_SPELLBUFF_H

namespace Egametang {

class Unit
{
public:
	int health;
};

class Spell
{
public:
	Unit* caster;
	Unit* victim;
};

class Buff
{
public:
	int buff_type;

public:
	Buff(): buff_type(0)
	{
	}
};

} // namespace Egametang


#endif // BEHAVIORTREE_SPELLBUFF_H
