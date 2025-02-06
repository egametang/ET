#!/bin/bash

clang -shared -undefined dynamic_lookup -o libMonoHookUtils_OSX.dylib Utils.cpp

