#pragma once

#include "il2cpp-config.h"

bool poll_init(int wakeup_pipe_fd);

void poll_register_fd(int fd, int events, bool is_new);

int poll_event_wait(void(*callback)(int fd, int events, void* user_data), void* user_data);

void poll_remove_fd(int fd);

/* Keep in sync with System.IOOperation in mcs/class/System/System/IOSelector.cs */
enum Il2CppIOOperation
{
    EVENT_IN = 1 << 0,
    EVENT_OUT = 1 << 1,
    EVENT_ERR = 1 << 2, /* not in managed */
};
