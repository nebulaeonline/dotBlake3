using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nebulae.dotBlake3;

public sealed class Blake3 : IDisposable
{
    private enum InitMode { Default, Keyed, Derived }

    private const int StateSize = 1912; // sizeof(blake3_hasher); fixed size
    private IntPtr _state;

    private InitMode _mode;
    private byte[]? _key;
    private string? _context;

    private bool _disposed;

    public Blake3()
    {
        _state = Marshal.AllocHGlobal(StateSize);
        _mode = InitMode.Default;
        Blake3Interop.blake3_hasher_init(_state);
    }

    public Blake3(ReadOnlySpan<byte> key)
    {
        if (key.Length != 32)
            throw new ArgumentException("Key must be exactly 32 bytes", nameof(key));

        _state = Marshal.AllocHGlobal(StateSize);
        _mode = InitMode.Keyed;
        _key = key.ToArray();
        var tmp = new byte[32];
        key.CopyTo(tmp);
        Blake3Interop.blake3_hasher_init_keyed(_state, tmp);
    }

    public Blake3(string context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        _state = Marshal.AllocHGlobal(StateSize);
        _mode = InitMode.Derived;
        _context = context ?? throw new ArgumentNullException(nameof(context));
        Blake3Interop.blake3_hasher_init_derive_key(_state, context);
    }

    public void Update(ReadOnlySpan<byte> data)
    {
        EnsureNotDisposed();
        if (data.IsEmpty) return;

        unsafe
        {
            fixed (byte* ptr = data)
            {
                Blake3Interop.blake3_hasher_update(_state, (IntPtr)ptr, (UIntPtr)data.Length);
            }
        }
    }

    public void UpdateParallel(ReadOnlySpan<byte> data)
    {
        EnsureNotDisposed();
        if (data.IsEmpty) return;

        unsafe
        {
            fixed (byte* ptr = data)
            {
                Blake3Interop.blake3_hasher_update_tbb(_state, (IntPtr)ptr, (UIntPtr)data.Length);
            }
        }
    }

    public byte[] Finalize(int outputLength = 32)
    {
        EnsureNotDisposed();
        if (outputLength <= 0 || outputLength > 65536)
            throw new ArgumentOutOfRangeException(nameof(outputLength));

        var output = new byte[outputLength];
        Blake3Interop.blake3_hasher_finalize(_state, output, (UIntPtr)outputLength);
        return output;
    }

    public void Reset()
    {
        EnsureNotDisposed();

        switch (_mode)
        {
            case InitMode.Default:
                Blake3Interop.blake3_hasher_init(_state);
                break;
            case InitMode.Keyed:
                if (_key is null)
                    throw new InvalidOperationException("Keyed mode but key is missing.");
                Blake3Interop.blake3_hasher_init_keyed(_state, _key);
                break;
            case InitMode.Derived:
                if (_context is null)
                    throw new InvalidOperationException("Derive-key mode but context is missing.");
                Blake3Interop.blake3_hasher_init_derive_key(_state, _context);
                break;
            default:
                throw new InvalidOperationException("Unknown hasher mode.");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Marshal.FreeHGlobal(_state);
            _state = IntPtr.Zero;
            _disposed = true;
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Blake3));
    }

    /// <summary>
    /// Computes the BLAKE3 hash of the input buffer.
    /// </summary>
    /// <param name="input">Input data</param>
    /// <param name="outputLength">Length of output in bytes (default: 32)</param>
    public static byte[] ComputeHash(ReadOnlySpan<byte> input, int outputLength = 32)
    {
        if (outputLength <= 0 || outputLength > 65536)
            throw new ArgumentOutOfRangeException(nameof(outputLength), "Must be 1 to 65536 bytes");

        using var hasher = new Blake3();
        hasher.Update(input);
        return hasher.Finalize(outputLength);
    }

    /// <summary>
    /// Computes the keyed BLAKE3 hash of the input using a 32-byte key.
    /// </summary>
    /// <param name="input">Input data</param>
    /// <param name="key">32-byte key</param>
    /// <param name="outputLength">Length of output in bytes (default: 32)</param>
    public static byte[] ComputeKeyedHash(ReadOnlySpan<byte> input, ReadOnlySpan<byte> key, int outputLength = 32)
    {
        if (key.Length != 32)
            throw new ArgumentException("Key must be exactly 32 bytes", nameof(key));
        if (outputLength <= 0 || outputLength > 65536)
            throw new ArgumentOutOfRangeException(nameof(outputLength));

        using var hasher = new Blake3(key);
        hasher.Update(input);
        return hasher.Finalize(outputLength);
    }

    /// <summary>
    /// Computes a derived BLAKE3 key from input using a context string.
    /// </summary>
    /// <param name="input">Input data</param>
    /// <param name="context">Derivation context string</param>
    /// <param name="outputLength">Length of output in bytes (default: 32)</param>
    public static byte[] ComputeDerivedKey(ReadOnlySpan<byte> input, string context, int outputLength = 32)
    {
        if (string.IsNullOrEmpty(context))
            throw new ArgumentNullException(nameof(context));
        if (outputLength <= 0 || outputLength > 65536)
            throw new ArgumentOutOfRangeException(nameof(outputLength));

        using var hasher = new Blake3(context);
        hasher.Update(input);
        return hasher.Finalize(outputLength);
    }
}
