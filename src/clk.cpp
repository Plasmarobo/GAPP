#include "clk.h"

#include <ctime>

unsigned long GetUtc()
{
    //Visual Studio defines time(0) as UTC, but other compilers might not...
    unsigned long t = std::time(NULL);
    return t;
}

