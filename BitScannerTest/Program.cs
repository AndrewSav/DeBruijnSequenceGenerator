using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace BitScannerTest
{
    public static class BitScanner
    {
        private const ulong Magic = 0x37E84A99DAE458F;

        private static readonly int[] MagicTable =
        {
            0, 1, 17, 2, 18, 50, 3, 57,
            47, 19, 22, 51, 29, 4, 33, 58,
            15, 48, 20, 27, 25, 23, 52, 41,
            54, 30, 38, 5, 43, 34, 59, 8,
            63, 16, 49, 56, 46, 21, 28, 32,
            14, 26, 24, 40, 53, 37, 42, 7,
            62, 55, 45, 31, 13, 39, 36, 6,
            61, 44, 12, 35, 60, 11, 10, 9,
        };

        public static int BitScanForward(ulong b)
        {
            return MagicTable[((ulong) ((long) b & -(long) b)*Magic) >> 58];
        }

        public static int BitScanReverse(ulong b)
        {
            b |= b >> 1;
            b |= b >> 2;
            b |= b >> 4;
            b |= b >> 8;
            b |= b >> 16;
            b |= b >> 32;
            b = b & ~(b >> 1);
            return MagicTable[b*Magic >> 58];
        }
    }

    class Program
    {
        private static int Obvious(ulong v)
        {
            int r = 0;
            while ((v >>= 1) != 0) 
            {
                r++;
            }
            return r;
        }

        //http://stackoverflow.com/questions/31374628/fast-way-of-finding-most-and-least-significant-bit-set-in-a-64-bit-integer/31377558#31377558
        public static UInt64 CountLeadingZeros(UInt64 input)
        {
            if (input == 0) return 64;

            UInt64 n = 1;

            if ((input >> 32) == 0) { n = n + 32; input = input << 32; }
            if ((input >> 48) == 0) { n = n + 16; input = input << 16; }
            if ((input >> 56) == 0) { n = n + 8; input = input << 8; }
            if ((input >> 60) == 0) { n = n + 4; input = input << 4; }
            if ((input >> 62) == 0) { n = n + 2; input = input << 2; }
            n = n - (input >> 63);

            return n;
        }
        
        static void Main()
        {
            PrintMagicNumberBits();
            PrintTestSequence();
            PrintTestRandom();
            CompareSpeed();
        }

        private static void PrintMagicNumberBits()
        {
            Console.WriteLine("0000000000111111111122222222223333333333444444444455555555556666");
            Console.WriteLine("0123456789012345678901234567890123456789012345678901234567890123");
            Console.WriteLine(Convert.ToString((long) 0x37E84A99DAE458F, 2).PadLeft(64, '0'));
        }

        private static void PrintTestSequence()
        {
            for (int i = 0; i < 100; i++)
            {
                PrintNumberAndScanResult((ulong)i);
            }
        }

        private static void PrintTestRandom()
        {
            PrintNumberBitsAndScanResult(0xF1001000F1001000);
            PrintNumberBitsAndScanResult(987654321);
            PrintNumberBitsAndScanResult(123456789);

            Random r = new Random();
            PrintNumberBitsAndScanResult((ulong) r.Next());
            PrintNumberBitsAndScanResult((ulong) r.Next());
            PrintNumberBitsAndScanResult((ulong) r.Next());
            PrintNumberBitsAndScanResult((ulong) r.Next());
            PrintNumberBitsAndScanResult((ulong) r.Next());
        }

        private static void CompareSpeed()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buf = new byte[8];
            rng.GetBytes(buf);
            ulong start = BitConverter.ToUInt64(buf, 0);
            Stopwatch sw1 = new Stopwatch();
            Stopwatch sw2 = new Stopwatch();
            Stopwatch sw3 = new Stopwatch();
            Stopwatch sw4 = new Stopwatch();
            ulong total = 10000000;
            ulong matches = 0;
            //start = start | 0x8000000000000000;
            Console.WriteLine("Starting {0} iterations at random {1}", total, start);
            for (ulong i = start; i < start + total; i++)
            {
                sw1.Start();
                int r1 = BitScanner.BitScanReverse(i);
                sw1.Stop();
                sw2.Start();
                int r2 = Obvious(i);
                sw2.Stop();
                sw3.Start();
                int r3 = (int)(Math.Log(i, 2));
                sw3.Stop();
                sw4.Start();
                int r4 = 63 - (int)CountLeadingZeros(i);
                sw4.Stop();
                if (r1 == r2 && r2 == r3 && r3 == r4)
                {
                    matches++;
                }
            }
            Console.WriteLine("DeBruijn time: {0}", sw1.Elapsed);
            Console.WriteLine("Obvious time: {0}", sw2.Elapsed);
            Console.WriteLine("Log2 time: {0}", sw3.Elapsed);
            Console.WriteLine("SO time: {0}", sw4.Elapsed);
            Console.WriteLine("{0} matches out of {1}", matches, total);
        }


        private static void PrintNumberAndScanResult(ulong number)
        {
            Console.WriteLine("{0:D2} {1:D2}", number, BitScanner.BitScanReverse(number));
        }
        private static void PrintNumberBitsAndScanResult(ulong number)
        {
            Console.WriteLine("6666555555555544444444443333333333222222222211111111110000000000");
            Console.WriteLine("3210987654321098765432109876543210987654321098765432109876543210");
            Console.WriteLine(Convert.ToString((long) number, 2).PadLeft(64, '0'));
            Console.WriteLine(BitScanner.BitScanReverse(number));
            Console.WriteLine();
        }
    }
}
