#include "..\src\cpu.h"


int main(int argc, void* argv)
{
    GBCPU cpu;
    cpu.Start();
    cpu.RunGBFile("test_cpu.gb");
    return 0;
}