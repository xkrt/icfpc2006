#include <fstream>
#include <iostream>
#include <fcntl.h>
#include <io.h>

#include "um-vm.h"

using namespace std;

int main(int argc, char** argv)
{
    if (argc != 2)
    {
        cout << "Universal Machine interpretator (ICFPC 2006)" << endl;
        cout << "Pavel Martynov aka xkrt, originally 2009-07-20, updated 2013-06-29" << endl;
        cout << "Usage: " << argv[0] << " <scroll_file_path>" << endl;
        return 1;
    }

    ifstream file(argv[1], ios::in|ios::binary|ios::ate);
    if (file.fail())
    {
        cerr << "Can't open '" << argv[1] << "'. Exit." << endl;
        return 1;
    }

    auto fileSize = file.tellg();
    file.seekg (0, ios::beg);
    char* memblock = new char[fileSize];
    file.read (memblock, fileSize);
    file.close();

    CVirtualMachine vm(memblock, fileSize);
    delete[] memblock;
    vm.run();

    return 0;
}

uint32_t swap_endian(uint32_t input)
{
    return (input >> 24)
        | ((input << 8) & 0x00FF0000)
        | ((input >> 8) & 0x0000FF00)
        | (input << 24);
}

CVirtualMachine::CVirtualMachine(const char* memblock, int memblockSize)
{
    uint32_t plattersCount = memblockSize / 4;

    auto zeroArray = new vector<platter_t>(plattersCount);
    char bytes[4];
    for (uint32_t plInd = 0; plInd < plattersCount; ++plInd)
    {
        auto platterFirstByteInd = plInd * 4;
        bytes[0] = memblock[platterFirstByteInd];
        bytes[1] = memblock[platterFirstByteInd+1];
        bytes[2] = memblock[platterFirstByteInd+2];
        bytes[3] = memblock[platterFirstByteInd+3];
        
        uint32_t number = *(uint32_t*)bytes;
        platter_t platter = swap_endian(number);
        zeroArray->at(plInd) = platter;
    }
    arrays.push_back(zeroArray);

    for (int i = 0; i < 8; ++i) regs[i] = 0;
    finger = 0;

    _setmode(_fileno(stdout), _O_BINARY);
}

void CVirtualMachine::run()
{
    auto a = [] (const platter_t& plat) { return (plat & 0x01C0) >> 6; };
    auto b = [] (const platter_t& plat) { return (plat & 0x38) >> 3; };
    auto c = [] (const platter_t& plat) { return plat & 0x07; };

    while(true)
    {
        platter_t plat = arrays[0]->at(finger);
        uint32_t opcode = plat >> 28;
        ++finger;

        switch(opcode)
        {
            case 0 :                        // Conditional Move
            {
                if(regs[c(plat)] != 0) regs[a(plat)] = regs[b(plat)];
                break;
            }
            case 1 :                        // Array Index
            {
                regs[a(plat)] = arrays[regs[b(plat)]]->at(regs[c(plat)]);
                break;
            }
            case 2 :                        // Array Amendment
            {
                arrays[regs[a(plat)]]->at(regs[b(plat)]) = regs[c(plat)];
                break;
            }
            case 3 :                        // Addition
            {
                regs[a(plat)] = regs[b(plat)] + regs[c(plat)];
                break;
            }
            case 4 :                        // Multiplication
            {
                regs[a(plat)] = regs[b(plat)] * regs[c(plat)];
                break;
            }
            case 5 :                        // Division
            {
                regs[a(plat)] = regs[b(plat)] / regs[c(plat)];
                break;
            }
            case 6 :                        // Not-And
            {
                regs[a(plat)] = ~(regs[b(plat)] & regs[c(plat)]);
                break;
            }
            case 7 :                        // Halt
            {
                cerr << "Halt" << endl;
                exit(0);
            }
            case 8 :                        // Allocation
            {
                arrays.push_back(new vector<platter_t>);
                uint32_t index = arrays.size()-1;
                arrays[index]->resize(regs[c(plat)], 0);
                regs[b(plat)] = index;
                break;
            }
            case 9 :                        // Abandonment
            {
                delete arrays[regs[c(plat)]];
                arrays[regs[c(plat)]] = 0;
                break;
            }
            case 10 :                       // Output
            {
                cout << (char)regs[c(plat)];
                break;
            }
            case 11 :                       // Input
            {
                char ch;
                cin.get(ch);
                if (ch == EOF)
                    regs[c(plat)] = ~0;
                else
                    regs[c(plat)] = ch;
                break;
            }
            case 12 :                       // Load Program
            {
                if (regs[b(plat)] != 0)
                {
                    delete arrays[0];
                    arrays[0] = new vector<platter_t>(*(arrays[regs[b(plat)]]));
                }
                finger = regs[c(plat)];
                break;
            }
            case 13 :                       // Orthography
            {
                auto a = (plat & 0xE000000) >> 25;
                uint32_t value = plat & 0x1FFFFFF;
                regs[a] = value;
                break;
            }
            default :
            {
                cerr <<  "Operator code out of range" << endl;
                exit(1);
            }
        }
    }

    return;
}
