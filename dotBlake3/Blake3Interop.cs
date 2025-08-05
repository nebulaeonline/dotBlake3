using System.Runtime.InteropServices;

namespace nebulae.dotBlake3;

internal static class Blake3Interop
{
    static Blake3Interop()
    {
        // Ensure the native library is loaded
        Blake3Library.Init();
    }

    [DllImport("blake3", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void blake3_hasher_init(IntPtr state);

    [DllImport("blake3")]
    internal static extern void blake3_hasher_init_keyed(IntPtr state, byte[] key);

    [DllImport("blake3")]
    internal static extern void blake3_hasher_init_derive_key(IntPtr state, string context);

    [DllImport("blake3")]
    internal static extern void blake3_hasher_update(IntPtr state, IntPtr input, UIntPtr len);

    [DllImport("blake3")]
    internal static extern void blake3_hasher_update_tbb(IntPtr state, IntPtr input, UIntPtr len);

    [DllImport("blake3")]
    internal static extern void blake3_hasher_finalize(IntPtr state, byte[] outBuf, UIntPtr outLen);
}
