using System.Diagnostics;

namespace um_vm.csharp
{
    struct Platter
    {
        readonly uint _bits;

        public Platter(uint bits)
        {
            _bits = bits;
        }

        [DebuggerStepThrough]
        public static implicit operator uint(Platter platter)
        {
            return platter._bits;
        }

        [DebuggerStepThrough]
        public Operation Op()
        {
            return (Operation)(_bits >> 28);
        }

        [DebuggerStepThrough]
        public uint A()
        {
            return (_bits & 0x1c0u) >> 6;
        }

        [DebuggerStepThrough]
        public uint B()
        {
            return (_bits & 0x038u) >> 3;
        }

        [DebuggerStepThrough]
        public uint C()
        {
            return _bits & 0x007u;
        }

        [DebuggerStepThrough]
        public uint OrtoA()
        {
            return (_bits & 0xe000000u) >> 25;
        }

        [DebuggerStepThrough]
        public uint OrtoValue()
        {
            return _bits & 0x1ffffffu;
        }
    }
}