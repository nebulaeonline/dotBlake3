# dotBlake3

A high-performance, minimal, cross-platform wrapper for the optimized reference Blake3 hash function implementation. Supports parallel hashing for ultra-fast performance (~50ms per gig on Ryzen 9 3950X under Windows & Linux and ~130ms per gig on M1 Mac Mini).

The native backend is compiled straight from the official C/C++ implementation of Blake3, along with oneTBB for parallel hashing. 

Passes all reference test vectors and is fully compatible with the official Blake3 specification.

Tests and benchmarks are included and available in the Github repo.

This one has been beaten up pretty good, with cross-platform testing on Windows, Linux, and macOS.

[![NuGet](https://img.shields.io/nuget/v/nebulae.dotBlake3.svg)](https://www.nuget.org/packages/nebulae.dotBlake3)

---

## Features

- **Cross-platform**: Works on Windows, Linux, and macOS (x64 & Apple Silicon).
- **High performance**: Optimized for speed, leveraging native SIMD-enabled code (neon, SSE2, SSE4.1 AVX2 & AVX512). Has scalar fallbacks for unsupported platforms.
- **Easy to use**: Simple API for generating hashes and keyed hashes.
- **Secure**: Uses the reference implementation, which is widely trusted in the industry.
- **Minimal dependencies**: No external dependencies required (all are included), making it lightweight and easy to integrate.

---

## Requirements

- .NET 8.0 or later
- Windows x64, Linux x64, or macOS (x64 & Apple Silicon)

---

## Usage

One-Shot Hashing:

```csharp

using nebulae.dotBlake3;

byte[] data = File.ReadAllBytes("bigfile.bin");
byte[] digest = Blake3.ComputeHash(data); // 32-byte digest

```

With Custom Output Length:

```csharp

byte[] digest = Blake3.ComputeHash(data, outputLength: 64); // 64-byte XOF

```

Keyed Hashing (MAC-like use case):

```csharp

byte[] key = Encoding.ASCII.GetBytes("whats the Elvish word for friend");
byte[] digest = Blake3.ComputeKeyedHash(data, key); // 32-byte keyed digest

```

Derived Keying (KDF-like use case):

```csharp

string context = "BLAKE3 example usage context";
byte[] digest = Blake3.ComputeDerivedKey(data, context); // 32-byte derived key

```

Incremental Hashing (streaming):

```csharp

using nebulae.dotBlake3;

using var hasher = new Blake3();

foreach (var chunk in ReadChunks("bigfile.bin", 64 * 1024))
    hasher.Update(chunk);

byte[] digest = hasher.Finalize(); // Default: 32 bytes

```

Parallel Hashing (TBB backend) Ideal for >256KB buffers - Super Fast (5-8x speedup):

```csharp

using var hasher = new Blake3();

hasher.UpdateParallel(data); // Automatically splits into chunks and hashes in parallel
byte[] digest = hasher.Finalize();

```

---

## Installation

You can install the package via NuGet:

```bash

$ dotnet add package nebulae.dotBlake3

```

Or via git:

```bash

$ git clone https://github.com/nebulaeonline/dotBlake3.git
$ cd dotBlake3
$ dotnet build

```

---

## License

MIT

## Roadmap

Unless there are vulnerabilities found, there are no plans to add any new features.