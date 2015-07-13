using System;

namespace DeBruijnSequenceGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("usage: genBitScan 1 .. {0}", 1 << 26);
            else
                (new CGenBitScan(int.Parse(args[0]))).Run();
        }
    }
}
