/// /////////////////////////////////////////////////////////////////////////////// ///
/// Copyright(c) 2024, Jared Gray                                                   ///
/// All rights reserved.                                                            ///
///                                                                                 ///
/// Redistribution and use in source and binary forms, with or without              ///
/// modification, are permitted provided that the following conditions are met:     ///
///                                                                                 ///
/// * Redistributions of source code must retain the above copyright notice, this   ///
///   list of conditions and the following disclaimer.                              ///
///                                                                                 ///
/// * Redistributions in binary form must reproduce the above copyright notice,     ///
///   this list of conditions and the following disclaimer in the documentation     ///
///   and/or other materials provided with the distribution.                        ///
///                                                                                 ///
/// * Neither the name of the copyright holder nor the names of its                 ///
///   contributors may be used to endorse or promote products derived from          ///
///   this software without specific prior written permission.                      ///
///                                                                                 ///
/// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"     ///
/// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE       ///
/// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE  ///
/// DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE     ///
/// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL      ///
/// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR      ///
/// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER      ///
/// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,   ///
/// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE   ///
/// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.            ///
/// /////////////////////////////////////////////////////////////////////////////// ///

///////////////////////
/// Required, pick one.
///
/// Pre-processor directive to block Unity dependent code for deployment outside a Unity environment.
//  #define PACKIT_NO_UNITY
///
/// Pre-processor directive to enable Unity dependent code.
//  #define PACKIT_UNITY
///////////////////////

///////////////////////
/// Optional
///
/// Pre-processor directive to instruct PackIt to use Unity IL2CPP optimizations via the Il2CppSetOptionAttribute.
/// WARNING: This will disable null, divide by zero, and array bound checks when using the classes' indexer and other functions.
/// You must write code in such a way as to avoid these wreaking havoc. (E.g. check against capacity when using the indexer property)
//  #define PACKIT_IL2cppOpt
///////////////////////

#if !(PACKIT_NO_UNITY || PACKIT_UNITY)
#error Please Read before use!
/// If you are getting this error, then you need to look above and uncomment the relevent defines for your project's configuration.
/// If these need to change dynamically with build targets, then place Pre-processor directives in the build settings.
/// At the time of writing in Unity 2021.3.X this setting is located in 'Project Settings'>'Player'>'Scripting Define Symbols'
#endif

/// Error when trying dumb things. <3
#if PACKIT_NO_UNITY && PACKIT_UNITY
#error Only one of PACKIT_UNITY or PACKIT_NO_UNITY may be used at a time.
#endif
/// Undefine optional directive(s) that don't make sense contextually.
#if PACKIT_NO_UNITY
#undef PACKIT_IL2cppOpt
#endif
/// Enable unmanaged functions and spans if .net version is applicable.
#if PACKIT_UNITY || NETCOREAPP2_0_OR_GREATER || NET5_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
#define PACKIT_UNMANAGED
#define PACKIT_SPAN
#endif
/// Enable bit converter functions for easy float bitting
#if PACKIT_UNITY || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
#define PACKIT_BITCONVERTER_SINGLEDOUBLE
#endif
/// pick struct types
#if PACKIT_NO_UNITY
using Vector2 = PackIt.Vector2;
using Vector3 = PackIt.Vector3;
using Quaternion = PackIt.Quaternion;
#else
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
#endif
/// define il2 attribute/option when we care about them
#if PACKIT_IL2cppOpt
using IL2CompilerOptionAttr = Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute;
using IL2CompilerOption = Unity.IL2CPP.CompilerServices.Option;
#endif

/// <summary>
/// PackIt is designed to pack data into a pre-designated buffer using built in methods for trimming and truncating values to make them smaller, often at the loss of prescision, and quickly.
/// Primarily designed for use with Unity, usefull for packing data for files or byte buffers to be sent over a network socket.
/// <br></br><br></br>
/// All pack and unpack functions syntactically follow this pattern. PackXY, where X is the data type to be packed, and Y is the number of bits that will be used to
/// represent X. In cases where Y is omitted, X is packed in full precision. E.g. <see cref="PackUInt(uint)"/> gets packed into 32 bits as you would expect from a 32
/// bit unsigned integer, where as <see cref="PackUInt24(int)"/> would pack the integer into 24 bits, throwing away the first 8 bits.
/// </summary>
public class PackIt
{
    // Structs for Unity like quats and vectors in non-Unity environments.
#if PACKIT_NO_UNITY
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Vector2
    {
        public static readonly Vector2 Zero = new Vector2(0, 0);

        public float x;
        public float y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }
        public static Vector2 operator *(Vector2 a, float b) { return new Vector2(a.x * b, a.y * b); }
        public static Vector2 operator /(Vector2 a, float b) { return new Vector2(a.x / b, a.y / b); }
        public float sqrMagnitude { get { return x * x + y * y; } }
        public float magnitude { get { return (float)System.Math.Sqrt(sqrMagnitude); } }
        public Vector2 normalized
        {
            get
            {
                float mag = magnitude;
                return mag != 0 ? this / mag : Zero;
            }
        }
#if UNITY_EDITOR // for testing in unity editor
        public static implicit operator UnityEngine.Vector2(Vector2 v) { return new UnityEngine.Vector2(v.x, v.y); }
        public static implicit operator Vector2(UnityEngine.Vector2 v) { return new Vector2(v.x, v.y); }
#endif
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Vector3
    {
        public static readonly Vector3 Zero = new Vector3(0, 0, 0);
        public float x;
        public float y;
        public float z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); }
        public static Vector3 operator *(Vector3 a, float b) { return new Vector3(a.x * b, a.y * b, a.z * b); }
        public static Vector3 operator /(Vector3 a, float b) { return new Vector3(a.x / b, a.y / b, a.z / b); }
        public float sqrMagnitude { get { return x * x + y * y + z * z; } }
        public float magnitude { get { return (float)System.Math.Sqrt(sqrMagnitude); } }
        public Vector3 normalized
        {
            get
            {
                float mag = magnitude;
                return mag != 0 ? this / mag : Zero;
            }
        }
#if UNITY_EDITOR // for testing in unity editor
        public static implicit operator UnityEngine.Vector3(Vector3 v) { return new UnityEngine.Vector3(v.x, v.y, v.z); }
        public static implicit operator Vector3(UnityEngine.Vector3 v) { return new Vector3(v.x, v.y, v.z); }
#endif
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Quaternion
    {
        public static readonly Quaternion identity = new Quaternion(0, 0, 0, 1);
        public float x;
        public float y;
        public float z;
        public float w;
        public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public float sqrMagnitude { get { return x * x + y * y + z * z + w * w; } }
        public float magnitude { get { return (float)System.Math.Sqrt(sqrMagnitude); } }
        public Quaternion normalized
        {
            get
            {
                float mag = magnitude;
                if (mag == 0) return identity;
                return new Quaternion(x / mag, y / mag, z / mag, w / mag);
            }
        }
#if UNITY_EDITOR // for testing in unity editor
        public static implicit operator UnityEngine.Quaternion(Quaternion v) { return new UnityEngine.Quaternion(v.x, v.y, v.z, v.w); }
        public static implicit operator Quaternion(UnityEngine.Quaternion v) { return new Quaternion(v.x, v.y, v.z, v.w); }
#endif
    }
#endif
    private static class Log
    {
        const string LOG_STAMP = "[Packit] ";
        private static string ToMessage(object o)
        {
            return LOG_STAMP + (o?.ToString() ?? "NULL");
        }
        public static void Info(object o)
        {
            string s = ToMessage(o);
#if PACKIT_UNITY
            UnityEngine.Debug.Log(s);
#else
            System.Console.WriteLine(s);
#endif
        }
        public static void Warning(object o)
        {
            string s = ToMessage(o);
#if PACKIT_UNITY
            UnityEngine.Debug.LogWarning(s);
#else
            System.Console.WriteLine(s);
#endif
        }
        public static void Error(object o)
        {
            string s = ToMessage(o);
#if PACKIT_UNITY
            UnityEngine.Debug.LogError(s);
#else
            System.Console.Error.WriteLine(s);
#endif
        }
    }
    /// <summary>
    /// Enum Type for the size/existance of a fingerprint. 
    /// Corresponding value when cast to an integral value must equal the number of bytes the finger print holds. See also <seealso cref="HasValidFingerprint"/>
    /// </summary>
    public enum EFingerprintType : byte
    {
        /// <summary>No finger print. <see cref="HasValidFingerprint"/> will always be true.</summary>
        None = 0,
        /// <summary>fingerprint one byte long. Only 256 possible values, expect to have collisions in uniqueness, unreliable.</summary>
        B8 = 1,
        /// <summary>fingerprint two bytes long. 65535 possible values, not bad.</summary>
        B16 = 2,
        /// <summary>fingerprint four bytes long. Generally good enough.</summary>
        B32 = 4,
        /// <summary>fingerprint eight bytes long.</summary>
        B64 = 8,
    }
    /// <summary>Raw data contained within this PackIt Object.</summary>
    private byte[] m_Data;
    /// <summary>The index that the PackIt object is operating at. This is where it will read or write to next.</summary>
    private uint m_Cursor;
    /// <summary>The size of m_Data</summary>
    private uint m_Capacity;
    /// <summary>Length of the fingerprint as a number of bytes. This is also an offset for all data since the fingerprint occupies the first X bytes in the data buffer.</summary>
    private uint m_FingerprintLen;

    #region Private Member Getters/Setters
    /// <summary>Data buffer. Your actual data will begin at Data[NumFingerprintBytes]. Use the indexer to avoid confusion... </summary>
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
#endif
    public byte[] Data { get { return m_Data; } }
    public uint Cursor { get { return m_Cursor; } }
    public int Length { get { return (int)(m_Capacity - m_FingerprintLen); } }
    public uint NumFingerprintBytes { get { return m_FingerprintLen; } }
    public EFingerprintType FingerprintType { get { return (EFingerprintType)m_FingerprintLen; } }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public byte this[int i]
    {
        get { return m_Data[m_FingerprintLen + i]; }
        set { m_Data[m_FingerprintLen + i] = value; }
    }
    #endregion

    private PackIt()
    {
        m_FingerprintLen = 0;
        m_Capacity = 0;
        m_Data = new byte[0];
        m_Cursor = 0;
    }
    /// <summary>
    /// Create a PackIt object with a buffer of 'capacity'.
    /// </summary>
    /// <param name="capacity">Number of bytes this PackIt holds at a maximum.</param>
    public PackIt(uint capacity)
    {
        m_FingerprintLen = (uint)EFingerprintType.None;
        m_Capacity = capacity + m_FingerprintLen;
        m_Data = new byte[m_Capacity];
        m_Cursor = m_FingerprintLen;
    }
    /// <summary>
    /// Create a PackIt object with a buffer of 'capacity'. Optionally, a fingerprint may be provided. (Capacity is increased to accomidate a fingerprint)
    /// </summary>
    /// <param name="capacity">Number of bytes this PackIt holds at a maximum.</param>
    /// <param name="fingerprintOption">The type of fingerprint to use.</param>
    public PackIt(uint capacity, EFingerprintType fingerprintOption)
    {
        m_FingerprintLen = (uint)fingerprintOption;
        m_Capacity = capacity + m_FingerprintLen;
        m_Data = new byte[m_Capacity];
        m_Cursor = m_FingerprintLen;
    }
    /// <summary>
    /// Create a PackIt object with an existing byte buffer. If the buffer does not contain a fingerprint and fingerprintOption != EFingerPrintType.None, 
    /// it will be copied into a new buffer with room for the fingerprint. (unless noCopy is specified, in which case values are shifted down in the 'buffer')
    /// </summary>
    /// <param name="buffer">Buffer this PackIt object will use or copy from.</param>
    /// <param name="containsFingerprint">True, this buffer already contains a fingerprint. False, the fingerprint will be created and the entire buffer will have data copied to accomidate it.</param>
    /// <param name="noCopy">
    /// In the event that the fingerprint increases the capacity of the buffer, you can instead opt to shift data down the buffer to make room
    /// for the fingerprint (True). This may result in data loss if the tail end of the buffer contains data you don't want to lose.
    /// </param>
    /// <param name="fingerprintOption">The type of fingerprint to use.</param>
    public PackIt(byte[] buffer, bool containsFingerprint, bool noCopy, EFingerprintType fingerprintOption)
    {
        m_FingerprintLen = (uint)fingerprintOption;
        m_Capacity = containsFingerprint ? (uint)buffer.Length : (uint)buffer.Length + m_FingerprintLen;
        m_Cursor = m_FingerprintLen;
        if (m_Capacity != (uint)buffer.Length)
        {
            if (noCopy)
            {
                m_Capacity = (uint)buffer.Length;
                m_Data = buffer;
                for (int i = buffer.Length - (int)m_FingerprintLen - 1; i >= 0; i--)
                {
                    m_Data[i + m_FingerprintLen] = m_Data[i];
                }
                for (uint i = 0; i < m_FingerprintLen; i++) m_Data[i] = 0;
            }
            else
            {
                m_Data = new byte[m_Capacity];
                System.Buffer.BlockCopy(buffer, 0, m_Data, (int)m_FingerprintLen, buffer.Length);
            }
        }
        else
        {
            m_Data = buffer;
        }
    }
    /// <summary>
    /// Extracts the FingerPrint from the data buffer. See <seealso cref="GenerateFingerPrint"/>
    /// </summary>
    /// <returns>fingerprint as a ulong. See <see cref="PackIt.FingerprintType"/> to get the fingerprint type.</returns>
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public ulong GetFingerPrint()
    {
        ulong value = 0;
        for (int i = 0; i < m_FingerprintLen; i++)
        {
            value <<= 8;
            value |= m_Data[i];
        }
        return value;
    }
    /// <summary>
    /// Generates the fingerprint for this PackIt object. This will become out of date whenever the data buffer is mutated.
    /// </summary>
    public void GenerateFingerPrint()
    {
        EFingerprintType type = FingerprintType;
        if (type == EFingerprintType.None) return;
        ulong fingerprint = GenerateFingerPrint(type, m_Data, m_FingerprintLen, m_Capacity - m_FingerprintLen);
        for (int i = (int)m_FingerprintLen - 1; i >= 0; i--)
        {
            m_Data[i] = (byte)(fingerprint & byte.MaxValue);
            fingerprint >>= 8;
        }
    }
    /// <summary>
    /// Given a byte array and a fingerprint type, creates a non-cryptographic good enough hash of the byte array to
    /// act as a finger print for the enclosed data.
    /// </summary>
    /// <returns>hash of the byte array.</returns>
    public static ulong GenerateFingerPrint(EFingerprintType type, byte[] barr, uint offset, uint length)
    {
        if (type == EFingerprintType.None) return 0;
        ulong fingerprint = 0;
        uint i = offset;
        for (; i < length - 7; i += 8)
        {
            ulong value =
                ((ulong)barr[i + 0] << 56) +
                ((ulong)barr[i + 1] << 48) +
                ((ulong)barr[i + 2] << 40) +
                ((ulong)barr[i + 3] << 32) +
                ((ulong)barr[i + 4] << 24) +
                ((ulong)barr[i + 5] << 16) +
                ((ulong)barr[i + 6] << 8) +
                barr[i + 7];
            unchecked
            {
                fingerprint += value << 3;
                fingerprint ^= fingerprint >> 3;
                fingerprint += fingerprint << 15;
            }
        }
        // XOR the remainder.
        ulong xorValue = 0;
        for (; i < length; i++)
        {
            xorValue += barr[i];
            xorValue <<= 8;
        }
        fingerprint ^= xorValue;

        switch (type)
        {
            default:
            case EFingerprintType.None:
                // This shouldn't occur, but its here.
                return 0;
            case EFingerprintType.B8:
                return fingerprint & byte.MaxValue;
            case EFingerprintType.B16: // Output print will be expected to fit in 16 bytes. So we will xor each short of the fingerprint.
                return (fingerprint & ushort.MaxValue);
            case EFingerprintType.B32:
                return (fingerprint & uint.MaxValue);
            case EFingerprintType.B64:
                return fingerprint;
        }
    }
    /// <summary>
    /// Checks the currently stored fingerprint against a newly generated one based off internal data. This will return false when
    /// the underlying buffer is changed in any way after calling <see cref="GenerateFingerPrint"/>. Great for checking integrity
    /// when sending PackIt data over a network.
    /// </summary>
    /// <returns>True if the integrity of the data buffer is upheld, false otherwise. See also <seealso cref="GenerateFingerPrint"/></returns>
    public bool HasValidFingerprint()
    {
        return GetFingerPrint() == GenerateFingerPrint(FingerprintType, m_Data, m_FingerprintLen, m_Capacity - m_FingerprintLen);
    }
    /// <summary>
    /// Seeks the internal cursor to the beginning of the PackIt so that you can start packing/unpacking data.
    /// </summary>
    public void SeekToStart()
    {
        m_Cursor = m_FingerprintLen;
    }
    /// <summary>
    /// Adds this value to the cursor.
    /// </summary>
    public void Advance(uint amount)
    {
        m_Cursor += amount;
    }
    /// <summary>
    /// Removes this value from the cursor.
    /// </summary>
    public void Reverse(uint amount)
    {
        m_Cursor -= amount;
    }
    /// <summary>
    /// Seeks the internal cursor to the specified index so that you can start packing/unpacking data at the cursors location.
    /// </summary>
    public void SeekTo(uint index)
    {
        m_Cursor = m_FingerprintLen + (index >= m_Capacity ? m_Capacity - 1 : index);
    }
#if PACKIT_UNMANAGED
    /// <summary>
    /// Unsafely packs an arbitrary unmanged object.
    /// </summary>
    public unsafe void Pack<T>(T* pValue) where T : unmanaged
    {
        Pack(pValue, (ushort)sizeof(T));
    }
#endif
    /// <summary>
    /// Packs a struct into the PackIt. Note: You must use specify a StructLayout attribute on the struct you are intending to use. This function uses
    /// pointers to memory to copy data to the buffer.
    /// 
    /// Ideally, you wouldn't use this for anything other than convienience.
    /// </summary>
    public void Pack<T>(T value) where T : struct
    {
        unsafe
        {
            System.Runtime.InteropServices.GCHandle h = System.Runtime.InteropServices.GCHandle.Alloc(value, System.Runtime.InteropServices.GCHandleType.Pinned);
            Pack((void*)h.AddrOfPinnedObject(), (ushort)System.Runtime.InteropServices.Marshal.SizeOf<T>());
            h.Free();
        }
    }
    /// <summary>
    /// Unsafely packs an arbitrary portion of memory. If you're using this, you'd best know what you're doing. Max size is that of a 16 bit unsigned short.
    /// </summary>
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public unsafe void Pack(void* pAddr, ushort len)
    {
        if (len + 2 + m_Cursor >= m_Capacity) return;
        PackUShort(len);
        fixed (void* pData = &m_Data[m_Cursor]) System.Buffer.MemoryCopy(pAddr, pData, m_Capacity - m_Cursor, len);
        m_Cursor += len;
    }
#if PACKIT_UNMANAGED
    /// <summary>
    /// Unpacks a struct from the PackIt into the referenced struct. Note: You must use specify a StructLayout attribute on the struct you are intending to use. This function uses
    /// pointers to memory to copy data from the buffer.
    /// 
    /// Ideally, you wouldn't use this for anything other than convienience.
    /// </summary>
    public void Unpack<T>(ref T value) where T : unmanaged
    {
        unsafe
        {
            fixed (T* ptr = &value)
            {
                Unpack(ptr);
            }
        }
    }
#endif
    /// <summary>
    /// Unsafely packs an arbitrary portion of memory. If you're using this, you'd best know what you're doing. Max size is that of a 16 bit unsigned short.
    /// </summary>
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public unsafe void Unpack(void* dest)
    {
        ushort len = UnpackUShort();
        if (len <= 0) return;
        //fixed (void* pData = &m_Data[m_Cursor]) System.Buffer.MemoryCopy(pData, dest, m_Capacity - m_Cursor, len);
        fixed (byte* pData = &m_Data[m_Cursor])
        {
            byte* pDest = (byte*)dest;
            for (int i = 0; i < len; i++)
            {
                pDest[i] = pData[i];
            }
        }
        m_Cursor += len;
    }
#if PACKIT_SPAN
    public void PackBytes(System.ReadOnlyMemory<byte> barr)
    {
        PackBytes(barr.Span);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackBytes(System.ReadOnlySpan<byte> barr)
    {
        if (m_Cursor + barr.Length + 2 > m_Capacity) return;
        ushort len = (ushort)barr.Length;
        PackUShort(len);
        for (int i = 0; i < len; i++) m_Data[m_Cursor++] = barr[i];
    }
#endif
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackBytes(byte[] barr)
    {
        if (m_Cursor + barr.Length + 2 > m_Capacity) return;
        ushort len = (ushort)barr.Length;
        PackUShort(len);
        System.Buffer.BlockCopy(barr, 0, m_Data, (int)m_Cursor, barr.Length);
        m_Cursor += len;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackBytes(byte[] barr, int offset)
    {
        if (m_Cursor + barr.Length + offset + 2 > m_Capacity) return;
        ushort len = (ushort)(barr.Length - offset);
        PackUShort(len);
        System.Buffer.BlockCopy(barr, offset, m_Data, (int)m_Cursor, len);
        m_Cursor += len;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackBytes(byte[] barr, int offset, ushort length)
    {
        if (m_Cursor + length + offset + 2 > m_Capacity) return;
        if (length >= barr.Length - offset) length = (ushort)(barr.Length - offset);
        PackUShort(length);
        System.Buffer.BlockCopy(barr, offset, m_Data, (int)m_Cursor, length);
        m_Cursor += length;
    }
    /// <summary>
    /// Unpacks bytes by allocating a new byte[]. Use only if you really can't keep a byte array around to cache your values. Prefer <see cref="UnpackBytes(byte[])"/>
    /// </summary>
    /// <returns>New byte[] of unpacked data.</returns>
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public byte[] UnpackBytes()
    {
        byte[] value = new byte[UnpackUShort()];
        System.Buffer.BlockCopy(m_Data, (int)m_Cursor, value, 0, value.Length);
        m_Cursor += (ushort)value.Length;
        return value;
    }
    /// <summary>
    /// Unpacks data from the PackIt to the destination buffer.
    /// </summary>
    /// <param name="destination">Destination buffer</param>
    /// <returns>number of bytes unpacked. If > then destination buffer, those values are lost.</returns>
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public ushort UnpackBytes(byte[] destination)
    {
        ushort len = UnpackUShort();
        System.Buffer.BlockCopy(m_Data, (int)m_Cursor, destination, 0, len);
        m_Cursor += len;
        return len;
    }

    #region Primitives
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackByte(byte value)
    {
        m_Data[m_Cursor++] = value;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public byte UnpackByte()
    {
        return m_Data[m_Cursor++];
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackSByte(sbyte value)
    {
        m_Data[m_Cursor++] = (byte)value;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public sbyte UnpackSByte()
    {
        return (sbyte)m_Data[m_Cursor++];
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackShort(short value)
    {
        if (m_Cursor + 2 > m_Capacity) return;
        m_Data[m_Cursor++] = (byte)(value >> 8);
        m_Data[m_Cursor++] = (byte)(value);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public short UnpackShort()
    {
        if (m_Cursor + 2 > m_Capacity) return 0;
        short value = m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        return value;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackUShort(ushort value)
    {
        if (m_Cursor + 2 > m_Capacity) return;
        m_Data[m_Cursor++] = (byte)(value >> 8);
        m_Data[m_Cursor++] = (byte)(value);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public ushort UnpackUShort()
    {
        if (m_Cursor + 2 > m_Capacity) return 0;
        ushort value = m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        return value;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackInt(int value)
    {
        if (m_Cursor + 4 > m_Capacity) return;
        m_Data[m_Cursor++] = (byte)(value >> 24);
        m_Data[m_Cursor++] = (byte)(value >> 16);
        m_Data[m_Cursor++] = (byte)(value >> 8);
        m_Data[m_Cursor++] = (byte)(value);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public int UnpackInt()
    {
        if (m_Cursor + 4 > m_Capacity) return 0;
        int value = m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        return value;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackUInt(uint value)
    {
        if (m_Cursor + 4 > m_Capacity) return;
        m_Data[m_Cursor++] = (byte)(value >> 24);
        m_Data[m_Cursor++] = (byte)(value >> 16);
        m_Data[m_Cursor++] = (byte)(value >> 8);
        m_Data[m_Cursor++] = (byte)(value);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public uint UnpackUInt()
    {
        if (m_Cursor + 4 > m_Capacity) return 0;
        uint value = m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        return value;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackLong(long value)
    {
        if (m_Cursor + 8 > m_Capacity) return;
        m_Data[m_Cursor++] = (byte)(value >> 56);
        m_Data[m_Cursor++] = (byte)(value >> 48);
        m_Data[m_Cursor++] = (byte)(value >> 40);
        m_Data[m_Cursor++] = (byte)(value >> 32);
        m_Data[m_Cursor++] = (byte)(value >> 24);
        m_Data[m_Cursor++] = (byte)(value >> 16);
        m_Data[m_Cursor++] = (byte)(value >> 8);
        m_Data[m_Cursor++] = (byte)(value);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public long UnpackLong()
    {
        if (m_Cursor + 8 > m_Capacity) return 0;
        long value = m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        return value;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackULong(ulong value)
    {
        if (m_Cursor + 8 > m_Capacity) return;
        m_Data[m_Cursor++] = (byte)(value >> 56);
        m_Data[m_Cursor++] = (byte)(value >> 48);
        m_Data[m_Cursor++] = (byte)(value >> 40);
        m_Data[m_Cursor++] = (byte)(value >> 32);
        m_Data[m_Cursor++] = (byte)(value >> 24);
        m_Data[m_Cursor++] = (byte)(value >> 16);
        m_Data[m_Cursor++] = (byte)(value >> 8);
        m_Data[m_Cursor++] = (byte)(value);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public ulong UnpackULong()
    {
        if (m_Cursor + 8 > m_Capacity) return 0;
        ulong value = m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        return value;
    }
    /// <summary>
    /// Floating point prescision is dependent on the client's machine. It is mostly standardized now but if you have a special case, you'll need to change
    /// the floating point functions...
    /// </summary>
    public void PackFloat(float value)
    {
#if PACKIT_BITCONVERTER_SINGLEDOUBLE
        PackInt(System.BitConverter.SingleToInt32Bits(value));
#else
        if (m_Cursor + 4 > m_Capacity) return;
        byte[] bits = System.BitConverter.GetBytes(value);
        m_Data[m_Cursor++] = bits[0];
        m_Data[m_Cursor++] = bits[1];
        m_Data[m_Cursor++] = bits[2];
        m_Data[m_Cursor++] = bits[3];
#endif
    }
    public float UnpackFloat()
    {
#if PACKIT_BITCONVERTER_SINGLEDOUBLE
        return System.BitConverter.Int32BitsToSingle(UnpackInt());
#else
        if (m_Cursor + 4 > m_Capacity) return 0;
        float f = System.BitConverter.ToSingle(m_Data, (int)m_Cursor);
        m_Cursor += 4;
        return f;
#endif
    }
    public void PackDouble(double value)
    {
#if PACKIT_BITCONVERTER_SINGLEDOUBLE
        PackLong(System.BitConverter.DoubleToInt64Bits(value));
#else
        if (m_Cursor + 8 > m_Capacity) return;
        byte[] bits = System.BitConverter.GetBytes(value);
        m_Data[m_Cursor++] = bits[0];
        m_Data[m_Cursor++] = bits[1];
        m_Data[m_Cursor++] = bits[2];
        m_Data[m_Cursor++] = bits[3];
        m_Data[m_Cursor++] = bits[4];
        m_Data[m_Cursor++] = bits[5];
        m_Data[m_Cursor++] = bits[6];
        m_Data[m_Cursor++] = bits[7];
#endif
    }
    public double UnpackDouble()
    {
#if PACKIT_BITCONVERTER_SINGLEDOUBLE
        return System.BitConverter.Int64BitsToDouble(UnpackLong());
#else
        if (m_Cursor + 8 > m_Capacity) return 0;
        double d = System.BitConverter.ToDouble(m_Data, (int)m_Cursor);
        m_Cursor += 8;
        return d;
#endif
    }

    #endregion // End Primitives
    #region Fractional

#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void PackUInt24(int value)
    {
        if (m_Cursor + 3 > m_Capacity) return;
        m_Data[m_Cursor++] = (byte)(value >> 16);
        m_Data[m_Cursor++] = (byte)(value >> 8);
        m_Data[m_Cursor++] = (byte)(value);

    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public int UnpackUInt24()
    {
        if (m_Cursor + 3 > m_Capacity) return 0;
        int value = m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        value <<= 8;
        value += m_Data[m_Cursor++];
        return value;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.DivideByZeroChecks, false)]
#endif
    public virtual void PackFloat24(float value, float min, float max)
    {
        if (m_Cursor + 3 > m_Capacity) return;
        float t = (value - min) / (max - min);
        int intValue = (int)(t * short.MaxValue * byte.MaxValue);
        m_Data[m_Cursor++] = (byte)(intValue & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 8) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 16) & 0xFF);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public float UnpackFloat24(float min, float max)
    {
        if (m_Cursor + 3 > m_Capacity) return 0;
        int intValue = m_Data[m_Cursor++] | m_Data[m_Cursor++] << 8 | m_Data[m_Cursor++] << 16;
        float t = (float)intValue / (float)(short.MaxValue * byte.MaxValue);
        return t * (max - min) + min;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.DivideByZeroChecks, false)]
#endif
    public virtual void PackFloat16(float value, float min, float max)
    {
        if (m_Cursor + 2 > m_Capacity) return;
        float t = (value - min) / (max - min);
        int intValue = (int)(t * short.MaxValue);
        m_Data[m_Cursor++] = (byte)(intValue & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 8) & 0xFF);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public float UnpackFloat16(float min, float max)
    {
        if (m_Cursor + 2 > m_Capacity) return 0f;
        int intValue = m_Data[m_Cursor++] | m_Data[m_Cursor++] << 8;
        float t = (float)intValue / (float)short.MaxValue;
        return t * (max - min) + min;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.DivideByZeroChecks, false)]
#endif
    public virtual void PackFloat8(float value, float min, float max)
    {
        if (m_Cursor >= m_Capacity) return;
        float factor = (value - min) / (max - min);
        m_Data[m_Cursor++] = (byte)(int)(factor * byte.MaxValue);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public float UnpackFloat8(float min, float max)
    {
        if (m_Cursor >= m_Capacity) return 0f;
        float factor = (float)m_Data[m_Cursor++] / (float)byte.MaxValue;
        return factor * (max - min) + min;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.DivideByZeroChecks, false)]
#endif
    public virtual void PackDouble56(double value, double min, double max)
    {
        if (m_Cursor + 5 > m_Capacity) return;
        double t = (value - min) / (max - min);
        long intValue = (long)(t * uint.MaxValue * ushort.MaxValue * byte.MaxValue);
        m_Data[m_Cursor++] = (byte)(intValue & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 8) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 16) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 24) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 32) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 40) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 48) & 0xFF);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public double UnpackDouble56(double min, double max)
    {
        if (m_Cursor + 5 > m_Capacity) return 0;
        long intValue = (long)(m_Data[m_Cursor++] | m_Data[m_Cursor++] << 8 | m_Data[m_Cursor++] << 16)
            | ((long)m_Data[m_Cursor++] << 24)
            | ((long)m_Data[m_Cursor++] << 32)
            | ((long)m_Data[m_Cursor++] << 40)
            | ((long)m_Data[m_Cursor++] << 48);
        double t = (double)intValue / (double)((long)uint.MaxValue * ushort.MaxValue * byte.MaxValue);
        return t * (max - min) + min;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.DivideByZeroChecks, false)]
#endif
    public virtual void PackDouble48(double value, double min, double max)
    {
        if (m_Cursor + 5 > m_Capacity) return;
        double t = (value - min) / (max - min);
        long intValue = (long)(t * uint.MaxValue * ushort.MaxValue);
        m_Data[m_Cursor++] = (byte)(intValue & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 8) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 16) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 24) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 32) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 40) & 0xFF);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public double UnpackDouble48(double min, double max)
    {
        if (m_Cursor + 5 > m_Capacity) return 0;
        long intValue = (long)(m_Data[m_Cursor++] | m_Data[m_Cursor++] << 8 | m_Data[m_Cursor++] << 16)
            | ((long)m_Data[m_Cursor++] << 24)
            | ((long)m_Data[m_Cursor++] << 32)
            | ((long)m_Data[m_Cursor++] << 40);
        double t = (double)intValue / (double)((long)uint.MaxValue * ushort.MaxValue);
        return t * (max - min) + min;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.DivideByZeroChecks, false)]
#endif
    public virtual void PackDouble40(double value, double min, double max)
    {
        if (m_Cursor + 5 > m_Capacity) return;
        double t = (value - min) / (max - min);
        long intValue = (long)(t * uint.MaxValue * byte.MaxValue);
        m_Data[m_Cursor++] = (byte)(intValue & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 8) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 16) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 24) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 32) & 0xFF);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public double UnpackDouble40(double min, double max)
    {
        if (m_Cursor + 5 > m_Capacity) return 0;
        long intValue = (long)(m_Data[m_Cursor++] | m_Data[m_Cursor++] << 8 | m_Data[m_Cursor++] << 16) | ((long)m_Data[m_Cursor++] << 24) | ((long)m_Data[m_Cursor++] << 32);
        double t = (double)intValue / (double)((long)uint.MaxValue * byte.MaxValue);
        return t * (max - min) + min;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.DivideByZeroChecks, false)]
#endif
    public virtual void PackDouble32(double value, double min, double max)
    {
        if (m_Cursor + 4 > m_Capacity) return;
        double t = (value - min) / (max - min);
        long intValue = (long)(t * uint.MaxValue);
        m_Data[m_Cursor++] = (byte)(intValue & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 8) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 16) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 24) & 0xFF);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public double UnpackDouble32(double min, double max)
    {
        if (m_Cursor + 4 > m_Capacity) return 0;
        long intValue = (long)(m_Data[m_Cursor++] | m_Data[m_Cursor++] << 8 | m_Data[m_Cursor++] << 16) | ((long)m_Data[m_Cursor++] << 24);
        double t = (double)intValue / (double)(uint.MaxValue);
        return t * (max - min) + min;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.DivideByZeroChecks, false)]
#endif
    public virtual void PackDouble24(double value, double min, double max)
    {
        if (m_Cursor + 3 > m_Capacity) return;
        double t = (value - min) / (max - min);
        int intValue = (int)(t * short.MaxValue * byte.MaxValue);
        m_Data[m_Cursor++] = (byte)(intValue & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 8) & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 16) & 0xFF);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public double UnpackDouble24(double min, double max)
    {
        if (m_Cursor + 3 > m_Capacity) return 0;
        int intValue = m_Data[m_Cursor++] | m_Data[m_Cursor++] << 8 | m_Data[m_Cursor++] << 16;
        double t = (double)intValue / (double)(short.MaxValue * byte.MaxValue);
        return t * (max - min) + min;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.DivideByZeroChecks, false)]
#endif
    public virtual void PackDouble16(double value, double min, double max)
    {
        if (m_Cursor + 2 > m_Capacity) return;
        double t = (value - min) / (max - min);
        int intValue = (int)(t * short.MaxValue);
        m_Data[m_Cursor++] = (byte)(intValue & 0xFF);
        m_Data[m_Cursor++] = (byte)((intValue >> 8) & 0xFF);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public double UnpackDouble16(double min, double max)
    {
        if (m_Cursor + 2 > m_Capacity) return 0f;
        int intValue = m_Data[m_Cursor++] | m_Data[m_Cursor++] << 8;
        double t = (double)intValue / (double)short.MaxValue;
        return t * (max - min) + min;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.DivideByZeroChecks, false)]
#endif
    public virtual void PackDouble8(double value, double min, double max)
    {
        if (m_Cursor >= m_Capacity) return;
        double factor = (value - min) / (max - min);
        m_Data[m_Cursor++] = (byte)(int)(factor * byte.MaxValue);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public double UnpackDouble8(double min, double max)
    {
        if (m_Cursor >= m_Capacity) return 0f;
        double factor = (double)m_Data[m_Cursor++] / (double)byte.MaxValue;
        return factor * (max - min) + min;
    }

    #endregion // End Fractional
    #region Mixed Precision
    /// If you happen to be browsing region by region, then take particular note of this region. This is where most of the use
    /// case specific functions are. Many times I've need either more or less precision with an odd number of bits or variables.


#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Pack7_1(byte v0, bool b0)
    {
        if (m_Cursor >= m_Capacity) return;
        m_Data[m_Cursor++] = (byte)(b0 ? (v0 << 1) + 1 : v0 << 1);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Unpack7_1(out byte v0, out bool b0)
    {
        v0 = m_Cursor >= m_Capacity ? (byte)0 : m_Data[m_Cursor++];
        b0 = (v0 & 1) != 0;
        v0 >>= 1;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Pack6_2(byte v0, byte v1)
    {
        if (m_Cursor >= m_Capacity) return;
        m_Data[m_Cursor++] = (byte)((v0 << 2) + (v1 & 0b_11));
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Unpack6_2(out byte v0, out byte v1)
    {
        v0 = m_Cursor >= m_Capacity ? (byte)0 : m_Data[m_Cursor++];
        v1 = (byte)(v0 & 0b_11);
        v0 >>= 2;
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Pack4_4(byte v0, byte v1)
    {
        if (m_Cursor >= m_Capacity) return;
        m_Data[m_Cursor++] = (byte)((v0 << 4) + (v1 & 0b_1111));
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Unpack4_4(out byte v0, out byte v1)
    {
        v0 = m_Cursor >= m_Capacity ? (byte)0 : m_Data[m_Cursor++];
        v1 = (byte)(v0 & 0b_1111);
        v0 >>= 4;
    }
    /// <summary>
    /// v0 = 6 bits <br></br>
    /// v1 = 5 bits <br></br>
    /// v2 = 5 bits <br></br>
    /// </summary>
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Pack3Float16(float v0, float v0Min, float v0Max, float v1, float v1Min, float v1Max, float v2, float v2Min, float v2Max)
    {
        if (m_Cursor + 2 > m_Capacity)
        {
            v0 = v1 = v2 = 0;
            return;
        }
        const int coef6 = 0b_11_1111;
        const int coef5 = 0b_1_1111;
        int bits = 0;
        //v0
        {
            float t = (v0 - v0Min) / (v0Max - v0Min);
            int intValue = (int)(t * coef6);
            bits |= intValue << (10);
        }
        //v1
        {
            float t = (v1 - v1Min) / (v1Max - v1Min);
            int intValue = (int)(t * coef5);
            bits |= intValue << 5;
        }
        //v2
        {
            float t = (v2 - v2Min) / (v2Max - v2Min);
            int intValue = (int)(t * coef5);
            bits |= intValue;
        }
        m_Data[m_Cursor++] = (byte)(bits >> 8);
        m_Data[m_Cursor++] = (byte)(bits);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Unpack3Float16(out float v0, float v0Min, float v0Max, out float v1, float v1Min, float v1Max, out float v2, float v2Min, float v2Max)
    {
        if (m_Cursor + 2 > m_Capacity)
        {
            v0 = v1 = v2 = 0;
            return;
        }
        const int coef6 = 0b_11_1111;
        const int coef5 = 0b_01_1111;
        int bits = m_Data[m_Cursor++];
        bits <<= 8;
        bits += m_Data[m_Cursor++];
        float t = (float)((bits >> 10) & coef6) / (float)(coef6);
        v0 = t * (v0Max - v0Min) + v0Min;
        t = (float)((bits >> 5) & coef5) / (float)(coef5);
        v1 = t * (v1Max - v1Min) + v1Min;
        t = (float)(bits & coef5) / (float)(coef5);
        v2 = t * (v2Max - v2Min) + v2Min;
    }
    /// <summary>
    /// v0 = 11 bits <br></br>
    /// v1 = 11 bits <br></br>
    /// v2 = 10 bits <br></br>
    /// </summary>
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Pack3Float32(float v0, float v0Min, float v0Max, float v1, float v1Min, float v1Max, float v2, float v2Min, float v2Max)
    {
        if (m_Cursor + 4 > m_Capacity)
        {
            v0 = v1 = v2 = 0;
            return;
        }
        const int coef11 = 0b_0111_1111_1111;
        const int coef10 = 0b_0011_1111_1111;
        int bits = 0;
        //v0
        {
            float t = (v0 - v0Min) / (v0Max - v0Min);
            int intValue = (int)(t * coef11);
            bits |= intValue << (11 + 10);
        }
        //v1
        {
            float t = (v1 - v1Min) / (v1Max - v1Min);
            int intValue = (int)(t * coef11);
            bits |= intValue << 10;
        }
        //v2
        {
            float t = (v2 - v2Min) / (v2Max - v2Min);
            int intValue = (int)(t * coef10);
            bits |= intValue;
        }
        m_Data[m_Cursor++] = (byte)(bits >> 24);
        m_Data[m_Cursor++] = (byte)(bits >> 16);
        m_Data[m_Cursor++] = (byte)(bits >> 8);
        m_Data[m_Cursor++] = (byte)(bits);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Unpack3Float32(out float v0, float v0Min, float v0Max, out float v1, float v1Min, float v1Max, out float v2, float v2Min, float v2Max)
    {
        if (m_Cursor + 4 > m_Capacity)
        {
            v0 = v1 = v2 = 0;
            return;
        }
        const int coef11 = 0b_0111_1111_1111;
        const int coef10 = 0b_0011_1111_1111;
        int bits = m_Data[m_Cursor++];
        bits <<= 8;
        bits += m_Data[m_Cursor++];
        bits <<= 8;
        bits += m_Data[m_Cursor++];
        bits <<= 8;
        bits += m_Data[m_Cursor++];
        float t = (float)((bits >> (11 + 10)) & coef11) / (float)(coef11);
        v0 = t * (v0Max - v0Min) + v0Min;
        t = (float)((bits >> 10) & coef11) / (float)(coef11);
        v1 = t * (v1Max - v1Min) + v1Min;
        t = (float)(bits & coef10) / (float)(coef10);
        v2 = t * (v2Max - v2Min) + v2Min;
    }
    /// <summary>
    /// v0 = 10 bits <br></br>
    /// v1 = 10 bits <br></br>
    /// v2 = 10 bits <br></br>
    /// v3 = 10 bits <br></br>
    /// </summary>
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Pack4Float40(
        float v0, float v0Min, float v0Max,
        float v1, float v1Min, float v1Max,
        float v2, float v2Min, float v2Max,
        float v3, float v3Min, float v3Max)
    {
        if (m_Cursor + 5 > m_Capacity)
        {
            v0 = v1 = v2 = 0;
            return;
        }
        const int coef10 = 0b_0011_1111_1111;
        long bits = 0;
        //v0
        {
            float t = (v0 - v0Min) / (v0Max - v0Min);
            bits |= (long)(t * coef10) << 30;
        }
        //v1
        {
            float t = (v1 - v1Min) / (v1Max - v1Min);
            bits |= (long)(t * coef10) << 20;
        }
        //v2
        {
            float t = (v2 - v2Min) / (v2Max - v2Min);
            bits |= (long)(t * coef10) << 10;
        }
        //v3
        {
            float t = (v3 - v3Min) / (v3Max - v3Min);
            bits |= (long)(t * coef10);
        }
        m_Data[m_Cursor++] = (byte)(bits >> 32);
        m_Data[m_Cursor++] = (byte)(bits >> 24);
        m_Data[m_Cursor++] = (byte)(bits >> 16);
        m_Data[m_Cursor++] = (byte)(bits >> 8);
        m_Data[m_Cursor++] = (byte)(bits);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Unpack4Float40(
        out float v0, float v0Min, float v0Max,
        out float v1, float v1Min, float v1Max,
        out float v2, float v2Min, float v2Max,
        out float v3, float v3Min, float v3Max)
    {
        if (m_Cursor + 4 > m_Capacity)
        {
            v0 = v1 = v2 = v3 = 0;
            return;
        }
        const int coef10 = 0b_0011_1111_1111;
        long bits = m_Data[m_Cursor++];
        bits <<= 8;
        bits += m_Data[m_Cursor++];
        bits <<= 8;
        bits += m_Data[m_Cursor++];
        bits <<= 8;
        bits += m_Data[m_Cursor++];
        bits <<= 8;
        bits += m_Data[m_Cursor++];
        float t = (float)((bits >> 30) & coef10) / (float)(coef10);
        v0 = t * (v0Max - v0Min) + v0Min;
        t = (float)((bits >> 20) & coef10) / (float)(coef10);
        v1 = t * (v1Max - v1Min) + v1Min;
        t = (float)((bits >> 10) & coef10) / (float)(coef10);
        v2 = t * (v2Max - v2Min) + v2Min;
        t = (float)(bits & coef10) / (float)(coef10);
        v3 = t * (v3Max - v3Min) + v3Min;
    }
    /// <summary>
    /// Intended for low precision quats. Sacrifies v3 to add precision to v0 and v1.
    /// v0 = 11 bits <br></br>
    /// v1 = 11 bits <br></br>
    /// v2 = 10 bits <br></br>
    /// v3 = 8 bits <br></br>
    /// </summary>
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Pack4Float40_11_11_10_8(
        float v0, float v0Min, float v0Max,
        float v1, float v1Min, float v1Max,
        float v2, float v2Min, float v2Max,
        float v3, float v3Min, float v3Max)
    {
        if (m_Cursor + 5 > m_Capacity)
        {
            v0 = v1 = v2 = v3 = 0;
            return;
        }
        const int coef11 = 0b_0111_1111_1111;
        const int coef10 = 0b_0011_1111_1111;
        const int coef8 = 0xFF;
        long bits = 0;
        //v0
        {
            float t = (v0 - v0Min) / (v0Max - v0Min);
            bits |= (long)(t * coef11) << 29;
        }
        //v1
        {
            float t = (v1 - v1Min) / (v1Max - v1Min);
            bits |= (long)(t * coef11) << 18;
        }
        //v2
        {
            float t = (v2 - v2Min) / (v2Max - v2Min);
            bits |= (long)(t * coef10) << 8;
        }
        //v3
        {
            float t = (v3 - v3Min) / (v3Max - v3Min);
            bits |= (long)(t * coef8);
        }
        m_Data[m_Cursor++] = (byte)(bits >> 32);
        m_Data[m_Cursor++] = (byte)(bits >> 24);
        m_Data[m_Cursor++] = (byte)(bits >> 16);
        m_Data[m_Cursor++] = (byte)(bits >> 8);
        m_Data[m_Cursor++] = (byte)(bits);
    }
#if PACKIT_IL2cppOpt
    [IL2CompilerOptionAttr(IL2CompilerOption.NullChecks, false)]
    [IL2CompilerOptionAttr(IL2CompilerOption.ArrayBoundsChecks, false)]
#endif
    public void Unpack4Float40_11_11_10_8(
        out float v0, float v0Min, float v0Max,
        out float v1, float v1Min, float v1Max,
        out float v2, float v2Min, float v2Max,
        out float v3, float v3Min, float v3Max)
    {
        if (m_Cursor + 4 > m_Capacity)
        {
            v0 = v1 = v2 = v3 = 0;
            return;
        }
        const int coef11 = 0b_0111_1111_1111;
        const int coef10 = 0b_0011_1111_1111;
        const int coef8 = 0xFF;
        long bits = m_Data[m_Cursor++];
        bits <<= 8;
        bits += m_Data[m_Cursor++];
        bits <<= 8;
        bits += m_Data[m_Cursor++];
        bits <<= 8;
        bits += m_Data[m_Cursor++];
        bits <<= 8;
        bits += m_Data[m_Cursor++];
        float t = (float)((bits >> 29) & coef11) / (float)(coef11);
        v0 = t * (v0Max - v0Min) + v0Min;
        t = (float)((bits >> 18) & coef11) / (float)(coef11);
        v1 = t * (v1Max - v1Min) + v1Min;
        t = (float)((bits >> 8) & coef10) / (float)(coef10);
        v2 = t * (v2Max - v2Min) + v2Min;
        t = (float)(bits & coef8) / (float)(coef8);
        v3 = t * (v3Max - v3Min) + v3Min;
    }

    #endregion // Mixed Precision
    #region Unity Structs

    public void PackVector2(Vector2 value)
    {
        PackFloat(value.x);
        PackFloat(value.y);
    }
    public Vector2 UnpackVector2()
    {
        return new Vector2(UnpackFloat(), UnpackFloat());
    }
    public void PackVector2_16(Vector2 value, float xMin, float xMax, float yMin, float yMax)
    {
        PackFloat8(value.x, xMin, xMax);
        PackFloat8(value.y, yMin, yMax);
    }
    public Vector2 UnpackVector2_16(float xMin, float xMax, float yMin, float yMax)
    {
        return new Vector2(UnpackFloat8(xMin, xMax), UnpackFloat8(yMin, yMax));
    }
    public void PackVector2_16(Vector2 value, float xExtent, float yExtent)
    {
        PackFloat8(value.x, -xExtent, xExtent);
        PackFloat8(value.y, -yExtent, yExtent);
    }
    public Vector2 UnpackVector2_16(float xExtent, float yExtent)
    {
        return new Vector2(UnpackFloat8(-xExtent, xExtent), UnpackFloat8(-yExtent, yExtent));
    }
    public void PackVector2_32(Vector2 value, float xMin, float xMax, float yMin, float yMax)
    {
        PackFloat16(value.x, xMin, xMax);
        PackFloat16(value.y, yMin, yMax);
    }
    public Vector2 UnpackVector2_32(float xMin, float xMax, float yMin, float yMax)
    {
        return new Vector2(UnpackFloat16(xMin, xMax), UnpackFloat16(yMin, yMax));
    }
    public void PackVector2_32(Vector2 value, float xExtent, float yExtent)
    {
        PackFloat16(value.x, -xExtent, xExtent);
        PackFloat16(value.y, -yExtent, yExtent);
    }
    public Vector2 UnpackVector2_32(float xExtent, float yExtent)
    {
        return new Vector2(UnpackFloat16(-xExtent, xExtent), UnpackFloat16(-yExtent, yExtent));
    }
    public void PackVector2_48(Vector2 value, float xMin, float xMax, float yMin, float yMax)
    {
        PackFloat24(value.x, xMin, xMax);
        PackFloat24(value.y, yMin, yMax);
    }
    public Vector2 UnpackVector2_48(float xMin, float xMax, float yMin, float yMax)
    {
        return new Vector2(UnpackFloat24(xMin, xMax), UnpackFloat24(yMin, yMax));
    }
    public void PackVector2_48(Vector2 value, float xExtent, float yExtent)
    {
        PackFloat24(value.x, -xExtent, xExtent);
        PackFloat24(value.y, -yExtent, yExtent);
    }
    public Vector2 UnpackVector2_48(float xExtent, float yExtent)
    {
        return new Vector2(UnpackFloat24(-xExtent, xExtent), UnpackFloat24(-yExtent, yExtent));
    }
    public void PackVector3(Vector3 value)
    {
        PackFloat(value.x);
        PackFloat(value.y);
        PackFloat(value.z);
    }
    public Vector3 UnpackVector3()
    {
        return new Vector3(UnpackFloat(), UnpackFloat(), UnpackFloat());
    }
    public void PackVector3_72(Vector3 value, float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        PackFloat24(value.x, xMin, xMax);
        PackFloat24(value.y, yMin, yMax);
        PackFloat24(value.z, zMin, zMax);
    }
    public Vector3 UnpackVector3_72(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        return new Vector3(UnpackFloat24(xMin, xMax), UnpackFloat24(yMin, yMax), UnpackFloat24(zMin, zMax));
    }
    public void PackVector3_72(Vector3 value, float xExtent, float yExtent, float zExtent)
    {
        PackFloat24(value.x, -xExtent, xExtent);
        PackFloat24(value.y, -yExtent, yExtent);
        PackFloat24(value.z, -zExtent, zExtent);
    }
    public Vector3 UnpackVector3_72(float xExtent, float yExtent, float zExtent)
    {
        return new Vector3(UnpackFloat24(-xExtent, xExtent), UnpackFloat24(-yExtent, yExtent), UnpackFloat24(-zExtent, zExtent));
    }
    public void PackVector3_48(Vector3 value, float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        PackFloat16(value.x, xMin, xMax);
        PackFloat16(value.y, yMin, yMax);
        PackFloat16(value.z, zMin, zMax);
    }
    public Vector3 UnpackVector3_48(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        return new Vector3(UnpackFloat16(xMin, xMax), UnpackFloat16(yMin, yMax), UnpackFloat16(zMin, zMax));
    }
    public void PackVector3_48(Vector3 value, float xExtent, float yExtent, float zExtent)
    {
        PackFloat16(value.x, -xExtent, xExtent);
        PackFloat16(value.y, -yExtent, yExtent);
        PackFloat16(value.z, -zExtent, zExtent);
    }
    public Vector3 UnpackVector3_48(float xExtent, float yExtent, float zExtent)
    {
        return new Vector3(UnpackFloat16(-xExtent, xExtent), UnpackFloat16(-yExtent, yExtent), UnpackFloat16(-zExtent, zExtent));
    }
    public void PackVector3_24(Vector3 value, float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        PackFloat8(value.x, xMin, xMax);
        PackFloat8(value.y, yMin, yMax);
        PackFloat8(value.z, zMin, zMax);
    }
    public Vector3 UnpackVector3_24(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        return new Vector3(UnpackFloat8(xMin, xMax), UnpackFloat8(yMin, yMax), UnpackFloat8(zMin, zMax));
    }
    public void PackVector3_24(Vector3 value, float xExtent, float yExtent, float zExtent)
    {
        PackFloat8(value.x, -xExtent, xExtent);
        PackFloat8(value.y, -yExtent, yExtent);
        PackFloat8(value.z, -zExtent, zExtent);
    }
    public Vector3 UnpackVector3_24(float xExtent, float yExtent, float zExtent)
    {
        return new Vector3(UnpackFloat8(-xExtent, xExtent), UnpackFloat8(-yExtent, yExtent), UnpackFloat8(-zExtent, zExtent));
    }
    public void PackQuat(Quaternion value)
    {
        PackFloat(value.x);
        PackFloat(value.y);
        PackFloat(value.z);
        PackFloat(value.w);
    }
    public Quaternion UnpackQuat()
    {
        return new Quaternion(UnpackFloat(), UnpackFloat(), UnpackFloat(), UnpackFloat());
    }
    public void PackQuat96(Quaternion value)
    {
        PackFloat24(value.x, -1, 1);
        PackFloat24(value.y, -1, 1);
        PackFloat24(value.z, -1, 1);
        PackFloat24(value.w, -1, 1);
    }
    /// <returns>Normalized Quaternion. Seealso <seealso cref="UnpackQuat96Unsafe"/></returns>
    public Quaternion UnpackQuat96()
    {
        return new Quaternion(UnpackFloat24(-1, 1), UnpackFloat24(-1, 1), UnpackFloat24(-1, 1), UnpackFloat24(-1, 1)).normalized;
    }
    /// <returns>Quaternion, not garunteed to be normalized. Seealso <seealso cref="UnpackQuat96"/></returns>
    public Quaternion UnpackQuat96Unsafe()
    {
        return new Quaternion(UnpackFloat24(-1, 1), UnpackFloat24(-1, 1), UnpackFloat24(-1, 1), UnpackFloat24(-1, 1));
    }
    public void PackQuat64(Quaternion value)
    {
        PackFloat16(value.x, -1, 1);
        PackFloat16(value.y, -1, 1);
        PackFloat16(value.z, -1, 1);
        PackFloat16(value.w, -1, 1);
    }
    /// <returns>Normalized Quaternion. Seealso <seealso cref="UnpackQuat64Unsafe"/></returns>
    public Quaternion UnpackQuat64()
    {
        return new Quaternion(UnpackFloat16(-1, 1), UnpackFloat16(-1, 1), UnpackFloat16(-1, 1), UnpackFloat16(-1, 1)).normalized;
    }
    /// <returns>Quaternion, not garunteed to be normalized. Seealso <seealso cref="UnpackQuat64"/></returns>
    public Quaternion UnpackQuat64Unsafe()
    {
        return new Quaternion(UnpackFloat16(-1, 1), UnpackFloat16(-1, 1), UnpackFloat16(-1, 1), UnpackFloat16(-1, 1));
    }
    public void PackQuat32(Quaternion value)
    {
        PackFloat8(value.x, -1, 1);
        PackFloat8(value.y, -1, 1);
        PackFloat8(value.z, -1, 1);
        PackFloat8(value.w, -1, 1);
    }
    /// <returns>Normalized Quaternion. Seealso <seealso cref="UnpackQuat32Unsafe"/></returns>
    public Quaternion UnpackQuat32()
    {
        return new Quaternion(UnpackFloat8(-1, 1), UnpackFloat8(-1, 1), UnpackFloat8(-1, 1), UnpackFloat8(-1, 1)).normalized;
    }
    /// <returns>Quaternion, not garunteed to be normalized. Seealso <seealso cref="UnpackQuat32"/></returns>
    public Quaternion UnpackQuat32Unsafe()
    {
        return new Quaternion(UnpackFloat8(-1, 1), UnpackFloat8(-1, 1), UnpackFloat8(-1, 1), UnpackFloat8(-1, 1));
    }

    #endregion // End Unity

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        switch (FingerprintType)
        {
            default:
            case EFingerprintType.None:
                sb.Append("#NONE# 0");
                sb.Append(System.Environment.NewLine);
                break;
            case EFingerprintType.B8:
                sb.Append("#B8  # ");
                sb.Append(GetFingerPrint());
                sb.Append(System.Environment.NewLine);
                break;
            case EFingerprintType.B16:
                sb.Append("#B16 # ");
                sb.Append(GetFingerPrint());
                sb.Append(System.Environment.NewLine);
                break;
            case EFingerprintType.B32:
                sb.Append("#B32 # ");
                sb.Append(GetFingerPrint());
                sb.Append(System.Environment.NewLine);
                break;
            case EFingerprintType.B64:
                sb.Append("#B64 # ");
                sb.Append(GetFingerPrint());
                sb.Append(System.Environment.NewLine);
                break;
        }
        sb.Append("RAW BYTES: ");
        sb.Append(System.BitConverter.ToString(m_Data));
        return sb.ToString();
    }
}
