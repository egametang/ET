#ifndef EVENT_EVENTDEFINE_H
#define EVENT_EVENTDEFINE_H

enum EventType
{
	ON_SPELL_START     = 0,
	ON_SPELL_FINISH    = 1,
	ON_ADD_BUFF        = 2,
	ON_REMOVE_BUFF     = 3,
	ON_HITTED          = 4,
	ON_HIT             = 5,
};

enum NodeType
{
	AND                = 0,
	OR                 = 1,
	NOT                = 2,

	SEQUENCE           = 6,
	SELECTOR           = 7,

	// 条件子节点100 - 1001
	BUFF_TYPE          = 101,

	// 动作子节点
	CHANGE_HEALTH      = 1001,
};

enum SpellUnit
{
	CASTER = 0,
	VICTIM = 1,
};

#endif // EVENT_EVENTDEFINE_H
