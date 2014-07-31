using System;
using System.Collections.Generic;

namespace um_vm.csharp
{
    class VirtualMachine
    {
        readonly uint[] _registers = new uint[8];
        readonly List<Platter[]> _platters;
        uint _pointer;

        public VirtualMachine(Platter[] platters)
        {
            _platters = new List<Platter[]> { platters };
        }

        public void Run()
        {
            var stdoutStream = Console.OpenStandardOutput();
            while (true)
            {
                var platter = _platters[0][_pointer];
                _pointer++;

                switch (platter.Op())
                {
                    case Operation.ConditionalMove:
                        if (_registers[platter.C()] != 0)
                            _registers[platter.A()] = _registers[platter.B()];
                        break;
                    case Operation.ArrayIndex:
                        _registers[platter.A()] = _platters[(int)_registers[platter.B()]][_registers[platter.C()]];
                        break;
                    case Operation.ArrayAmendment:
                        _platters[(int)_registers[platter.A()]][_registers[platter.B()]] = new Platter(_registers[platter.C()]);
                        break;
                    case Operation.Addition:
                        _registers[platter.A()] = _registers[platter.B()] + _registers[platter.C()];
                        break;
                    case Operation.Multiplication:
                        _registers[platter.A()] = _registers[platter.B()] * _registers[platter.C()];
                        break;
                    case Operation.Division:
                        _registers[platter.A()] = _registers[platter.B()] / _registers[platter.C()];
                        break;
                    case Operation.NotAnd:
                        _registers[platter.A()] = ~(_registers[platter.B()] & _registers[platter.C()]);
                        break;
                    case Operation.Halt:
                        Console.Error.WriteLine("halt");
                        Environment.Exit(0);
                        break;
                    case Operation.Allocation:
                        var newArraySize = (int)_registers[platter.C()];
                        _platters.Add(new Platter[newArraySize]);
                        _registers[platter.B()] = (uint)_platters.Count - 1;
                        break;
                    case Operation.Abandonment:
                        _platters[(int)_registers[platter.C()]] = null;
                        break;
                    case Operation.Output:
                        stdoutStream.WriteByte((byte)_registers[platter.C()]);
                        break;
                    case Operation.Input:
                        int key = Console.Read();
                        _registers[platter.C()] = key == -1 ? ~0u : Convert.ToUInt32(key);
                        break;
                    case Operation.LoadProgram:
                        var arrayToLoadInd = (int)_registers[platter.B()];
                        if (arrayToLoadInd != 0)
                        {
                            var arrayToLoad = _platters[arrayToLoadInd];
                            var copyArray = new Platter[arrayToLoad.Length];
                            Array.Copy(arrayToLoad, copyArray, arrayToLoad.Length);
                            _platters[0] = copyArray;
                        }
                        _pointer = _registers[platter.C()];
                        break;
                    case Operation.Orthography:
                        _registers[platter.OrtoA()] = platter.OrtoValue();
                        break;
                    default:
                        throw new NotImplementedException(string.Format("Operation {0} is not implemented", platter.Op()));
                }
            }
        }
    }
}
