using nebulae.dotBlake3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nebulae.dotBlake3.TestVectors;

public static class TbbHashBenchmark
{
    public static void Run()
    {
        const int dataSize = 1 << 30; // 1 GB
        var data = new byte[dataSize];

        Console.WriteLine("Filling 1GB of data...");
        for (int i = 0; i < dataSize; i++)
            data[i] = (byte)(i % 251);

        Console.WriteLine("Running standard (serial) hash...");
        var sw = Stopwatch.StartNew();
        byte[] serialHash;
        using (var h1 = new Blake3())
        {
            h1.Update(data);
            serialHash = h1.Finalize();
        }
        sw.Stop();
        Console.WriteLine($"Standard hash time: {sw.Elapsed.TotalMilliseconds:F2} ms");

        Console.WriteLine("Running parallel hash...");
        sw.Restart();
        byte[] parallelHash;
        using (var h2 = new Blake3())
        {
            h2.UpdateParallel(data);
            parallelHash = h2.Finalize();
        }
        sw.Stop();
        Console.WriteLine($"Parallel hash time: {sw.Elapsed.TotalMilliseconds:F2} ms");

        bool match = serialHash.AsSpan().SequenceEqual(parallelHash);
        Console.WriteLine($"Digest match: {(match ? "YES" : "NO")}");

        if (!match)
        {
            Console.WriteLine("Serial:   " + Convert.ToHexString(serialHash).ToLowerInvariant());
            Console.WriteLine("Parallel: " + Convert.ToHexString(parallelHash).ToLowerInvariant());
        }
    }
}
