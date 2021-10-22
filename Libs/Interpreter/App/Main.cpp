#include "Main.h"
#include "Interpreter/Interpreter.h"
#include <iostream>

int main(int argc, char* argv[])
{
    interpreter_init(argv[1], argv[2]);
    std::cout << "1111111111111111111" << std::endl;
    char name[50];
    std::cin >> name;
}