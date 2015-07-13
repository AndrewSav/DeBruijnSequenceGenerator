using System;
using System.Diagnostics;

namespace DeBruijnSequenceGenerator
{
    // https://chessprogramming.wikispaces.com/De+Bruijn+Sequence+Generator
    // https://chessprogramming.wikispaces.com/De+Bruijn+sequence

    /*
     * With findDeBruijn(0, 64-6, 0, 6), starting with six leading zeros on its most significant top, 
     * the routine has 58 decrements to reach depth zero claiming a found De Bruijn sequence. The algorithm 
     * does not explicitly prove the uniqueness of six remaining indices which result from the up to five 
     * trailing hidden zeros with 100000B = 32 as least significant subsequence. However, the constraints 
     * of the odd De Bruin sequence seem strong enough to make that test redundant, that is proving 
     * 64-6 unique keys seems sufficient. 
     * 
     * As demonstrated <elsewhere>, there are two branch-less series {32, 0, 1} and {31, 63, 62} respectively. 
     * The recursive search routine performs some kind of pruning and reductions to take advantage of that. 
     * 63 to follow 31 must not be locked and skipped for the 62 with an extra depth reduction. 
     * 32 can not appear as index inside the 58 recursive tests in one path, and is therefor locked 
     * initially before the findDeBruijn(0, 64-6, 0, 6) call.
     * 
     * A small improvement was introducing an additional parameter for the number of binary zeros to check 
     * it not to become greater 32. This makes also the depth > 0 condition for the even successors 
     * no longer necessary, to enumerate all 2^26 De Bruijn sequences.

     */
    public class CGenBitScan
    {
        public class StopException : Exception {}
        private readonly ulong[] _pow2 = new ulong[64]; // single bits
        private ulong _lock; // locks each bit used
        private int _dbCount; // counter
        private readonly int _match4Nth; // to match

        public CGenBitScan(int match4Nth)
        {
            _dbCount = 0;
            _match4Nth = match4Nth;
            InitPow2();
        }

        public void Run()
        {
            Stopwatch clock = new Stopwatch();
            clock.Start();
            _lock = _pow2[32]; // optimization to exclude 32, see remarks 
            try
            {
                FindDeBruijn(0, 64 - 6, 0, 6);
            }
            catch (StopException){}
            clock.Stop();
            Console.WriteLine("{0} Seconds for {1} De Bruijn sequences found", clock.ElapsedMilliseconds / 1000, _dbCount);            
        }

        //==========================================
        // on the fly initialization of pow2
        //==========================================
        private void InitPow2()
        {
            _pow2[0] = 1;
            for (int i = 1; i < 64; i++)
                _pow2[i] = 2*_pow2[i - 1];
        }

        //==========================================
        // print the bitscan routine and throw
        //==========================================
        private void BitScanRoutineFound(ulong deBruijn)
        {
            int[] index = new int[64];
            int i;
            for (i = 0; i < 64; i++) // init magic array
                index[(deBruijn << i) >> (64 - 6)] = i;
            Console.WriteLine("\nprivate const ulong Magic = 0x{0:X}; // the {1}.",
                (deBruijn), _dbCount);
            Console.Write("private static readonly int[] MagicTable = {");
            for (i = 0; i < 64; i++)
            {
                if ((i & 7) == 0) Console.WriteLine();
                Console.Write(" {0},", index[i]);
            }
            Console.WriteLine("\n};\n\npublic static int BitScanForward (ulong b) {");
            Console.WriteLine(" return MagicTable[((ulong)((long)b&-(long)b)*Magic) >> 58];\n}");
            Console.WriteLine("\n\npublic static int BitScanReverse (ulong b) {");
            Console.WriteLine(" b |= b >> 1;");
            Console.WriteLine(" b |= b >> 2;");
            Console.WriteLine(" b |= b >> 4;");
            Console.WriteLine(" b |= b >> 8;");
            Console.WriteLine(" b |= b >> 16;");
            Console.WriteLine(" b |= b >> 32;");
            Console.WriteLine(" b = b & ~(b >> 1);");
            Console.WriteLine(" return MagicTable[b*Magic >> 58];\n}");
            throw new StopException(); // unwind the stack until catched
        }

        //============================================
        // recursive search
        //============================================
        private void FindDeBruijn(ulong seq, int depth, int vtx, int nz)
        {
            if ((_lock & _pow2[vtx]) == 0 && nz <= 32)
            {
                // only if vertex is not locked
                if (depth == 0)
                {
                    // depth zero, De Bruijn sequence found, see remarks
                    if (++_dbCount == _match4Nth)
                        BitScanRoutineFound(seq);
                }
                else
                {
                    _lock ^= _pow2[vtx]; // set bit, lock the vertex to don't appear multiple
                    if (vtx == 31 && depth > 2)
                    {
                        // optimization, see remarks
                        FindDeBruijn(seq | _pow2[depth - 1], depth - 2, 62, nz + 1);
                    }
                    else
                    {
                        FindDeBruijn(seq, depth - 1, (2*vtx) & 63, nz + 1); // even successor
                        FindDeBruijn(seq | _pow2[depth - 1], depth - 1, (2*vtx + 1) & 63, nz); // odd successor
                    }
                    _lock ^= _pow2[vtx]; // reset bit, unlock
                }
            }
        }
    }
}
