using System.Text.Json;

using nebulae.dotBlake3;

namespace nebulae.dotBlake3.TestVectors;

internal static class Program
{
    public static void Main()
    {
        var path = "test_vectors.json";
        if (!File.Exists(path))
        {
            Console.WriteLine($"Missing {path}");
            return;
        }

        var testFile = JsonSerializer.Deserialize<B3TestVectorFile>(File.ReadAllText(path))!;
        byte[] key = System.Text.Encoding.ASCII.GetBytes(testFile.key);
        string context = testFile.context_string;

        int total = 0, passed = 0;

        Console.WriteLine($"Running {testFile.cases.Count} tests from {path}");
        Console.Out.Flush();

        foreach (var vec in testFile.cases)
        {
            var input = GenerateInput(vec.input_len);

            bool ok = true;

            byte[] out1 = Blake3.ComputeHash(input, vec.hash.Length / 2);
            byte[] out2 = Blake3.ComputeKeyedHash(input, key, vec.keyed_hash.Length / 2);
            byte[] out3 = Blake3.ComputeDerivedKey(input, context, vec.derive_key.Length / 2);

            string got1 = Convert.ToHexString(out1).ToLowerInvariant();
            string got2 = Convert.ToHexString(out2).ToLowerInvariant();
            string got3 = Convert.ToHexString(out3).ToLowerInvariant();

            if (got1 != vec.hash.ToLowerInvariant())
            {
                Console.WriteLine($"FAIL hash @ len={vec.input_len}");
                ok = false;
            }

            if (got2 != vec.keyed_hash.ToLowerInvariant())
            {
                Console.WriteLine($"FAIL keyed_hash @ len={vec.input_len}");
                ok = false;
            }

            if (got3 != vec.derive_key.ToLowerInvariant())
            {
                Console.WriteLine($"FAIL derive_key @ len={vec.input_len}");
                ok = false;
            }

            if (ok)
                passed++;

            total++;
        }

        Console.WriteLine($"Passed {passed} out of {total} tests.");
        Console.Out.Flush();
        TbbHashBenchmark.Run();
        Environment.Exit(0);
    }

    private static byte[] GenerateInput(int length)
    {
        byte[] input = new byte[length];
        for (int i = 0; i < length; i++)
            input[i] = (byte)(i % 251);
        return input;
    }

    private class B3TestVectorFile
    {
        public string _comment { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;
        public string context_string { get; set; } = string.Empty;
        public List<B3TestVector> cases { get; set; } = new();
    }
    private class B3TestVector
    {
        public int input_len { get; set; }
        public string hash { get; set; } = default!;
        public string keyed_hash { get; set; } = default!;
        public string derive_key { get; set; } = default!;
    }
}
