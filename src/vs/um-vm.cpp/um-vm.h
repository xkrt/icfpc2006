#ifndef __UM_VM__
#define __UM_VM__

#include <vector>

typedef unsigned int uint32_t;
typedef unsigned int platter_t;

class CVirtualMachine
{
    platter_t regs[8];
    uint32_t finger;
    std::vector<std::vector<platter_t>* > arrays;

public:
    explicit CVirtualMachine(const char* memblock, int memblockSize);
    void run();
};

#endif
