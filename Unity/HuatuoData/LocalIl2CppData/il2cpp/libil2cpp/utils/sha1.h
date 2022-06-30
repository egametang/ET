#pragma once

#include <stdint.h>

void sha1_get_digest(const uint8_t* buffer, int buffer_size, uint8_t digest[20]);
