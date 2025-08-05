using System.Runtime.InteropServices;

namespace nebulae.dotBlake3
{
    internal static class Blake3Library
    {
        private static bool _isLoaded;

        internal static void Init()
        {
            if (_isLoaded)
                return;

            var libName = GetPlatformLibraryName();
            var assemblyDir = Path.GetDirectoryName(typeof(Blake3Library).Assembly.Location)!;
            var fullPath = Path.Combine(assemblyDir, libName);

            if (!File.Exists(fullPath))
                throw new DllNotFoundException($"Could not find native Blake3 library at {fullPath}");

            NativeLibrary.Load(fullPath);
            _isLoaded = true;
        }

        private static string GetPlatformLibraryName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Path.Combine("runtimes", "win-x64", "native", "blake3.dll");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Path.Combine("runtimes", "linux-x64", "native", "libblake3.so");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    return Path.Combine("runtimes", "osx-arm64", "native", "libblake3.dylib");

                return Path.Combine("runtimes", "osx-x64", "native", "libblake3.dylib");
            }

            throw new PlatformNotSupportedException("Unsupported platform");
        }
    }
}
