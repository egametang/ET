//
// coroutine.hpp
// ~~~~~~~~~~~~~
//
// Copyright (c) 2003-2011 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#ifndef THREAD_COROUTINE_H
#define THREAD_COROUTINE_H

class Coroutine
{
public:
	Coroutine() : value(0)
	{
	}
	bool isChild() const
	{
		return value < 0;
	}
	bool isParent() const
	{
		return !isChild();
	}
	bool isComplete() const
	{
		return value == -1;
	}
private:
	friend class CoroutineRef;
	int value;
};

class CoroutineRef
{
private:
	void operator=(const CoroutineRef&);
	int& value;
	bool modified;

public:
	CoroutineRef(Coroutine& c) : value(c.value), modified(false)
	{
	}
	CoroutineRef(Coroutine* c) : value(c->value), modified(false)
	{
	}
	~CoroutineRef()
	{
		if (!modified)
		{
			value = -1;
		}
	}
	operator int() const
	{
		return value;
	}
	int& operator=(int v)
	{
		modified = true;
		return value = v;
	}
};

#define CORO_REENTER(c) \
	switch (CoroutineRef coroValue = c) \
		case -1: if (coroValue) \
		{ \
			goto terminateCoroutine; \
			terminateCoroutine: \
			coroValue = -1; \
			goto bailOutOfCoroutine; \
			bailOutOfCoroutine: \
			break; \
		} \
		else case 0:

#define CORO_YIELD_IMPL(n) \
	for (coroValue = (n);;) \
		if (coroValue == 0) \
		{ \
			case (n): ; \
			break; \
		} \
		else \
			switch (coroValue ? 0 : 1) \
				for (;;) \
					case -1: if (coroValue) \
						goto terminateCoroutine; \
					else for (;;) \
						case 1: if (coroValue) \
							goto bailOutOfCoroutine; \
						else case 0:

#define CORO_FORK_IMPL(n) \
	for (coroValue = -(n);; coroValue = (n)) \
		if (coroValue == (n)) \
		{ \
			case -(n): ; \
			break; \
		} \
		else

#if defined(_MSC_VER)
# define CORO_YIELD CORO_YIELD_IMPL(__COUNTER__ + 1)
# define CORO_FORK CORO_FORK_IMPL(__COUNTER__ + 1)
#else // defined(_MSC_VER)
# define CORO_YIELD CORO_YIELD_IMPL(__LINE__)
# define CORO_FORK CORO_FORK_IMPL(__LINE__)
#endif // defined(_MSC_VER)
#endif // THREAD_COROUTINE_H
