#pragma once

#include "il2cpp-config.h"

bool poll_init(int wakeup_pipe_fd);

void poll_register_fd(int fd, int events, bool is_new);

int poll_event_wait(void(*callback)(int fd, int events, void* user_data), void* user_data);

void poll_remove_fd(int fd);
