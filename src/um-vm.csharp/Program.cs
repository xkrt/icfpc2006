using System;
using System.Linq;

namespace um_vm.csharp
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                PrintUsage();
                Environment.Exit(1);
            }

            var platters = PlatterLoader.Load(args.Single());
            var vm = new VirtualMachine(platters);
            vm.Run();
        }

        static void PrintUsage()
        {
            Console.WriteLine("Universal Machine interpretator (C# version) for ICFPC 2006");
            Console.WriteLine("Pavel Martynov aka xkrt, 2013-06-29");
            Console.WriteLine("usage: um-vm.csharp <scroll file path>");
        }
    }
}
