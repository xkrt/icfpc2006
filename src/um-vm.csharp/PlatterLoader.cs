using System;
using System.IO;
using System.Linq;

namespace um_vm.csharp
{
    static class PlatterLoader
    {
        public static Platter[] Load(string scrollFilePath)
        {
            var bytes = File.ReadAllBytes(scrollFilePath);
            var groupedBytes = GroupByFour(bytes);
            return groupedBytes
                .Select(ToBigEndianUint)
                .Select(ui => new Platter(ui))
                .ToArray();
        }

        static byte[][] GroupByFour(byte[] bytes)
        {
            return bytes
                .Select((b, ind) => new { b, ind })
                .GroupBy(pair => pair.ind/4, pair => pair.b, (_, b) => b.ToArray())
                .ToArray();
        }

        static uint ToBigEndianUint(byte[] fourBytes)
        {
            Array.Reverse(fourBytes);
            return BitConverter.ToUInt32(fourBytes, 0);
        }
    }
}