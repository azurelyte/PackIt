/// ////////////////////////////////////////////////////////////////////// ///
/// Copyright(c) 2024, Jared Gray                                          ///
/// All rights reserved.                                                   ///
///                                                                        ///
/// This source code is licensed under the BSD-style license found in the  ///
/// LICENSE file in the root directory of this source tree.                ///
/// ////////////////////////////////////////////////////////////////////// ///

#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

public class PackItTest
{
    private static class Log
    {
        const string LOG_STAMP = "[Packit] ";
        private static string ToMessage(object o)
        {
            return LOG_STAMP + (o?.ToString() ?? "NULL");
        }
        public static void Info(object o)
        {
            TestContext.WriteLine(ToMessage(o));
        }
        public static void Warning(object o)
        {
            TestContext.WriteLine(ToMessage(o));
        }
        public static void Error(object o)
        {
            UnityEngine.Debug.LogError(ToMessage(o));
        }
    }
    [Test]
    public void ConstructorTest()
    {
        byte[] barr = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        PackIt p = new PackIt(32, PackIt.EFingerprintType.B16);
        Assert.AreEqual(32, p.Length);
        Assert.AreEqual(34, p.Data.Length);
        p = new PackIt(barr, containsFingerprint: false, noCopy: false, PackIt.EFingerprintType.B16);
        Assert.AreEqual(10, p.Length);
        Assert.AreEqual(12, p.Data.Length);
        p = new PackIt(barr, containsFingerprint: true, noCopy: false, PackIt.EFingerprintType.B16);
        Assert.AreEqual(8, p.Length);
        Assert.AreEqual(10, p.Data.Length);
        p = new PackIt(barr, containsFingerprint: false, noCopy: true, PackIt.EFingerprintType.B16);
        Assert.AreEqual(8, p.Length);
        Assert.AreEqual(10, p.Data.Length);
        AssertArrayMatch(barr, new byte[] { 0, 0, 0, 1, 2, 3, 4, 5, 6, 7 }); // note that barr changed to make room for the fingerprint! That's nocopy at work.
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    private struct SampleStruct
    {
        public int a;
        public byte b;
        public char c;
        public double d;
    }
    [Test]
    public void StructTest()
    {
        PackIt p = new PackIt(256);
        SampleStruct t = new SampleStruct() { a = 1, b = 2, c = '3', d = 4.4d };
        SampleStruct s = new SampleStruct();
        p.Pack(t);
        p.SeekToStart();
        p.Unpack(ref s);
        Assert.AreEqual(t.a, s.a);
        Assert.AreEqual(t.b, s.b);
        Assert.AreEqual(t.c, s.c);
        Assert.AreEqual(t.d, s.d);
    }
    readonly byte[] SampleDataBuffer = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 4, 8, 16, 32, 64, 128, 255, 127, 63, 31, 15, 7, 3 };
    private static void TestFingerprintPrintIntegrity(PackIt.EFingerprintType type, byte[] data)
    {
        PackIt p = new PackIt((uint)data.Length + 2, type);
        p.PackBytes(data);
        p.GenerateFingerPrint();
        Assert.IsTrue(p.HasValidFingerprint());
        if (type != PackIt.EFingerprintType.None)
        {
            p[p.Length / 2] ^= 0xFF;
            Assert.IsFalse(p.HasValidFingerprint());
        }
    }
    [Test] public void FingerprintTest_Integrity_None() => TestFingerprintPrintIntegrity(PackIt.EFingerprintType.None, SampleDataBuffer);
    [Test] public void FingerprintTest_Integrity_8() => TestFingerprintPrintIntegrity(PackIt.EFingerprintType.B8, SampleDataBuffer);
    [Test] public void FingerprintTest_Integrity_16() => TestFingerprintPrintIntegrity(PackIt.EFingerprintType.B16, SampleDataBuffer);
    [Test] public void FingerprintTest_Integrity_32() => TestFingerprintPrintIntegrity(PackIt.EFingerprintType.B32, SampleDataBuffer);
    [Test] public void FingerprintTest_Integrity_64() => TestFingerprintPrintIntegrity(PackIt.EFingerprintType.B64, SampleDataBuffer);
    private static void TestFingerprintPrint(ulong expectedFingerprintValue, PackIt.EFingerprintType type, byte[] data)
    {
        PackIt p = new PackIt(data, containsFingerprint: false, noCopy: false, type);
        p.GenerateFingerPrint();
        Assert.AreEqual(type, p.FingerprintType);
        Assert.AreEqual(expectedFingerprintValue, p.GetFingerPrint());
    }
    [Test] public void FingerprintTest_None() => TestFingerprintPrint(0, PackIt.EFingerprintType.None, SampleDataBuffer);
    [Test] public void FingerprintTest_B8() => TestFingerprintPrint(252, PackIt.EFingerprintType.B8, SampleDataBuffer);
    [Test] public void FingerprintTest_B16() => TestFingerprintPrint(44284, PackIt.EFingerprintType.B16, SampleDataBuffer);
    [Test] public void FingerprintTest_B32() => TestFingerprintPrint(1439324008, PackIt.EFingerprintType.B32, SampleDataBuffer);
    [Test] public void FingerprintTest_B64() => TestFingerprintPrint(9859764967531948136, PackIt.EFingerprintType.B64, SampleDataBuffer);
    [Test] public void ByteArrayTest()
    {
        byte[] barr = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        System.ReadOnlySpan<byte> rSpan = new System.ReadOnlySpan<byte>(barr, 2, 3);
        System.ReadOnlyMemory<byte> rMem = new System.ReadOnlyMemory<byte>(barr, 2, 4);
        PackIt p = new PackIt(64);
        p.PackBytes(barr); // 1-9
        p.PackBytes(barr, 5); // 5-9
        p.PackBytes(barr, 4, 3); // 4-6
        p.PackBytes(rSpan); // 2-4
        p.PackBytes(rMem); // 2-5
        p.SeekToStart();
        AssertArrayMatch(p.UnpackBytes(), barr);
        AssertArrayMatch(p.UnpackBytes(), new byte[] { 5, 6, 7, 8, 9 });
        AssertArrayMatch(p.UnpackBytes(), new byte[] { 4, 5, 6 });
        AssertArrayMatch(p.UnpackBytes(), new byte[] { 2, 3, 4 });
        AssertArrayMatch(p.UnpackBytes(), new byte[] { 2, 3, 4, 5 });
    }
    [Test] public void PrimitivesTest()
    {
        PackIt p = new PackIt(256);
        p.PackSByte(sbyte.MinValue);
        p.PackSByte(sbyte.MaxValue);
        p.PackByte(byte.MaxValue);
        p.PackShort(short.MinValue);
        p.PackShort(short.MaxValue);
        p.PackUShort(ushort.MaxValue);
        p.PackInt(int.MinValue);
        p.PackInt(int.MaxValue);
        p.PackUInt(uint.MaxValue);
        p.PackLong(long.MinValue);
        p.PackLong(long.MaxValue);
        p.PackULong(ulong.MaxValue);
        p.PackFloat(float.MinValue);
        p.PackFloat(float.MaxValue);
        p.PackDouble(double.MinValue);
        p.PackDouble(double.MaxValue);
        p.SeekToStart();
        Assert.AreEqual(sbyte.MinValue, p.UnpackSByte());
        Assert.AreEqual(sbyte.MaxValue, p.UnpackSByte());
        Assert.AreEqual(byte.MaxValue, p.UnpackByte());
        Assert.AreEqual(short.MinValue, p.UnpackShort());
        Assert.AreEqual(short.MaxValue, p.UnpackShort());
        Assert.AreEqual(ushort.MaxValue, p.UnpackUShort());
        Assert.AreEqual(int.MinValue, p.UnpackInt());
        Assert.AreEqual(int.MaxValue, p.UnpackInt());
        Assert.AreEqual(uint.MaxValue, p.UnpackUInt());
        Assert.AreEqual(long.MinValue, p.UnpackLong());
        Assert.AreEqual(long.MaxValue, p.UnpackLong());
        Assert.AreEqual(ulong.MaxValue, p.UnpackULong());
        Assert.AreEqual(float.MinValue, p.UnpackFloat());
        Assert.AreEqual(float.MaxValue, p.UnpackFloat());
        Assert.AreEqual(double.MinValue, p.UnpackDouble());
        Assert.AreEqual(double.MaxValue, p.UnpackDouble());
    }
    private static void AssertAreEqual(double expected, double actual, double tolerance)
    {
        if (System.Math.Abs(expected - actual) > System.Math.Abs(tolerance)) Assert.AreEqual(expected, actual);
    }
    private static void AssertAreEqual(Vector2 expected, Vector2 actual, double tolerance)
    {
        AssertAreEqual(expected.x, actual.x, tolerance);
        AssertAreEqual(expected.y, actual.y, tolerance);
    }
    private static void AssertAreEqual(Vector3 expected, Vector3 actual, double tolerance)
    {
        AssertAreEqual(expected.x, actual.x, tolerance);
        AssertAreEqual(expected.y, actual.y, tolerance);
        AssertAreEqual(expected.z, actual.z, tolerance);
    }
    private static void AssertAreEqual(Quaternion expected, Quaternion actual, double tolerance)
    {
        AssertAreEqual(expected.x, actual.x, tolerance);
        AssertAreEqual(expected.y, actual.y, tolerance);
        AssertAreEqual(expected.z, actual.z, tolerance);
        AssertAreEqual(expected.w, actual.w, tolerance);
    }
    private static void AssertAreSimilar(Quaternion expected, Quaternion actual, double maxAngleDeltaDeg)
    {
        float dot = Quaternion.Dot(expected, actual);
        float delta = Mathf.Acos(dot) * Mathf.Rad2Deg;
        if (delta > maxAngleDeltaDeg)
        {
            Assert.Fail("Expected value <= " + maxAngleDeltaDeg + ", got " + delta);
        }
    }
    [Test] public void LossyFloatingPointTest()
    {
        const float fMin = -1000;
        const float fMax = 1000;
        const double coef = 2.0d;
        const double fEpsilon8 = (((double)fMax - (double)fMin) / 0xFF) * coef - float.Epsilon;
        const double fEpsilon16 = (((double)fMax - (double)fMin) / 0xFFFF) * coef - float.Epsilon;
        const double fEpsilon24 = (((double)fMax - (double)fMin) / 0xFFFFFF) * coef - float.Epsilon;
        const double dMin = -1000;
        const double dMax = 1000;
        const double dEpsilon8 = (((double)fMax - (double)fMin) / 0xFF) * coef - double.Epsilon;
        const double dEpsilon16 = (((double)fMax - (double)fMin) / 0xFFFF) * coef - double.Epsilon;
        const double dEpsilon24 = (((double)fMax - (double)fMin) / 0xFFFFFF) * coef - double.Epsilon;
        const double dEpsilon32 = (((double)fMax - (double)fMin) / 0xFFFFFFFF) * coef - double.Epsilon;
        const double dEpsilon40 = (((double)fMax - (double)fMin) / 0xFFFFFFFFFF) * coef - double.Epsilon;
        const double dEpsilon48 = (((double)fMax - (double)fMin) / 0xFFFFFFFFFFFF) * coef - double.Epsilon;
        const double dEpsilon56 = (((double)fMax - (double)fMin) / 0xFFFFFFFFFFFFFF) * coef - double.Epsilon;
        PackIt p = new PackIt(2048);
        p.PackFloat8(500, fMin, fMax);
        p.PackFloat16(501, fMin, fMax);
        p.PackFloat24(502, fMin, fMax);
        p.PackDouble8(510, dMin, dMax);
        p.PackDouble16(511, dMin, dMax);
        p.PackDouble24(512, dMin, dMax);
        p.PackDouble32(513, dMin, dMax);
        p.PackDouble40(514, dMin, dMax);
        p.PackDouble48(515, dMin, dMax);
        p.PackDouble56(516, dMin, dMax);
        p.SeekToStart();
        // Due to the way PackIt quantizes floats into bits, it's possible for floating point error to accumulate.
        // So long as min/max values are reasonable, the deviation of each possible increment for the underlaying integral
        // value when it is swizzled back into a float shall never be larger than double the expected increment between values.
        AssertAreEqual(500, p.UnpackFloat8(fMin, fMax), fEpsilon8);
        AssertAreEqual(501, p.UnpackFloat16(fMin, fMax), fEpsilon16);
        AssertAreEqual(502, p.UnpackFloat24(fMin, fMax), fEpsilon24);
        AssertAreEqual(510, p.UnpackDouble8(fMin, fMax), dEpsilon8);
        AssertAreEqual(511, p.UnpackDouble16(fMin, fMax), dEpsilon16);
        AssertAreEqual(512, p.UnpackDouble24(fMin, fMax), dEpsilon24);
        AssertAreEqual(513, p.UnpackDouble32(fMin, fMax), dEpsilon32);
        AssertAreEqual(514, p.UnpackDouble40(fMin, fMax), dEpsilon40);
        AssertAreEqual(515, p.UnpackDouble48(fMin, fMax), dEpsilon48);
        AssertAreEqual(516, p.UnpackDouble56(fMin, fMax), dEpsilon56);
    }
    [Test] public void UnityTypesTest()
    {
        const float fMin = -1000;
        const float fMax = 1000;
        const double coef = 2.0d;
        const double fEpsilon8 = (((double)fMax - (double)fMin) / 0xFF) * coef - float.Epsilon;
        const double fEpsilon16 = (((double)fMax - (double)fMin) / 0xFFFF) * coef - float.Epsilon;
        const double fEpsilon24 = (((double)fMax - (double)fMin) / 0xFFFFFF) * coef - float.Epsilon;
        const double fEpsilonQuat8 = (2.0d / 0xFF) * coef - float.Epsilon;
        const double fEpsilonQuat16 = (2.0d / 0xFFFF) * coef - float.Epsilon;
        const double fEpsilonQuat24 = (2.0d / 0xFFFFFF) * coef - float.Epsilon;
        const double fDeltaToleranceQuat8 = (2.0d / 0xFF) * 180 + float.Epsilon;
        const double fDeltaToleranceQuat16 = (2.0d / 0xFFFF) * 180 + float.Epsilon;
        const double fDeltaToleranceQuat24 = (2.0d / 0xFFFFFF) * 180 + float.Epsilon;
        Vector2 vec2 = new Vector2(950, 820);
        Vector3 vec3 = new Vector3(-940, 22, 830);
        Quaternion quat = Quaternion.Euler(0, 0, 53) * Quaternion.Euler(30, 120, 0);
        Log.Info("fEpsilon8 : " + fEpsilon8);
        Log.Info("fEpsilon16: " + fEpsilon16);
        Log.Info("fEpsilon24: " + fEpsilon24);
        Log.Info("fEpsilonQuat8 : " + fEpsilonQuat8);
        Log.Info("fEpsilonQuat16: " + fEpsilonQuat16);
        Log.Info("fEpsilonQuat24: " + fEpsilonQuat24);
        Log.Info("fDeltaToleranceQuat8 : " + fDeltaToleranceQuat8);
        Log.Info("fDeltaToleranceQuat16: " + fDeltaToleranceQuat16);
        Log.Info("fDeltaToleranceQuat24: " + fDeltaToleranceQuat24);
        PackIt p = new PackIt(1024);
        p.PackVector2_16(vec2, fMin, fMax, fMin, fMax);
        p.PackVector2_32(vec2, fMin, fMax, fMin, fMax);
        p.PackVector2_48(vec2, fMin, fMax, fMin, fMax);
        p.PackVector2(vec2);
        p.PackVector3_24(vec3, fMin, fMax, fMin, fMax, fMin, fMax);
        p.PackVector3_48(vec3, fMin, fMax, fMin, fMax, fMin, fMax);
        p.PackVector3_72(vec3, fMin, fMax, fMin, fMax, fMin, fMax);
        p.PackVector3(vec3);
        p.PackQuat32(quat);
        p.PackQuat32(quat);
        p.PackQuat64(quat);
        p.PackQuat64(quat);
        p.PackQuat96(quat);
        p.PackQuat96(quat);
        p.PackQuat(quat);
        p.SeekToStart();
        Log.Info("..:Testing unpacked values:..");
        AssertAreEqual(vec2, p.UnpackVector2_16(fMin, fMax, fMin, fMax), fEpsilon8);
        Log.Info("PackVector2_16 Passed.");
        AssertAreEqual(vec2, p.UnpackVector2_32(fMin, fMax, fMin, fMax), fEpsilon16);
        Log.Info("PackVector2_32 Passed.");
        AssertAreEqual(vec2, p.UnpackVector2_48(fMin, fMax, fMin, fMax), fEpsilon24);
        Log.Info("PackVector2_48 Passed.");
        AssertAreEqual(vec2, p.UnpackVector2(), 0);
        Log.Info("PackVector2 Passed.");
        AssertAreEqual(vec3, p.UnpackVector3_24(fMin, fMax, fMin, fMax, fMin, fMax), fEpsilon8);
        Log.Info("PackVector3_24 Passed.");
        AssertAreEqual(vec3, p.UnpackVector3_48(fMin, fMax, fMin, fMax, fMin, fMax), fEpsilon16);
        Log.Info("PackVector3_48 Passed.");
        AssertAreEqual(vec3, p.UnpackVector3_72(fMin, fMax, fMin, fMax, fMin, fMax), fEpsilon24);
        Log.Info("PackVector3_72 Passed.");
        AssertAreEqual(vec3, p.UnpackVector3(), 0);
        Log.Info("PackVector3 Passed.");
        // When unpacking quaternions, the values are normalized due to precision loss. That's why they're checked against an angular tolerance.
        AssertAreSimilar(quat, p.UnpackQuat32(), fDeltaToleranceQuat8);
        Log.Info("PackQuat32 Passed.");
        AssertAreEqual(quat, p.UnpackQuat32Unsafe(), fEpsilonQuat8);
        Log.Info("PackQuat32Unsafe Passed.");
        AssertAreSimilar(quat, p.UnpackQuat64(), fDeltaToleranceQuat16);
        Log.Info("PackQuat64 Passed.");
        AssertAreEqual(quat, p.UnpackQuat64Unsafe(), fEpsilonQuat16);
        Log.Info("PackQuat64Unsafe Passed.");
        AssertAreSimilar(quat, p.UnpackQuat96(), fDeltaToleranceQuat24);
        Log.Info("PackQuat96 Passed.");
        AssertAreEqual(quat, p.UnpackQuat96Unsafe(), fEpsilonQuat24);
        Log.Info("PackQuat96Unsafe Passed.");
        AssertAreEqual(quat, p.UnpackQuat(), 0);
        Log.Info("PackQuat Passed.");
    }
    [Test] public void MixedFloatingPointPrecisionTest()
    {
        const float fMin = -1000;
        const float fMax = 1000;
        const double coef = 2.0d;
        const double fEpsilon5 = (((double)fMax - (double)fMin) / 0b_01_1111) * coef - float.Epsilon;
        const double fEpsilon6 = (((double)fMax - (double)fMin) / 0b_11_1111) * coef - float.Epsilon;
        const double fEpsilon8 = (((double)fMax - (double)fMin) / 0xFF) * coef - float.Epsilon;
        const double fEpsilon10 = (((double)fMax - (double)fMin) / 0b_11_1111_1111) * coef - float.Epsilon;
        const double fEpsilon11 = (((double)fMax - (double)fMin) / 0b_111_1111_1111) * coef - float.Epsilon;
        PackIt p = new PackIt(1024);
        p.Pack3Float16(940, fMin, fMax, 941, fMin, fMax, 942, fMin, fMax);
        p.Pack3Float32(950, fMin, fMax, 951, fMin, fMax, 952, fMin, fMax);
        p.Pack4Float40(960, fMin, fMax, 961, fMin, fMax, 962, fMin, fMax, 963, fMin, fMax);
        p.Pack4Float40_11_11_10_8(970, fMin, fMax, 971, fMin, fMax, 972, fMin, fMax, 973, fMin, fMax);
        p.SeekToStart();
        {
            Log.Info(fEpsilon5);
            Log.Info(fEpsilon6);
            p.Unpack3Float16(out float f0, fMin, fMax, out float f1, fMin, fMax, out float f2, fMin, fMax);
            AssertAreEqual(940, f0, fEpsilon6);
            AssertAreEqual(941, f1, fEpsilon5);
            AssertAreEqual(942, f2, fEpsilon5);
        }
        {
            p.Unpack3Float32(out float f0, fMin, fMax, out float f1, fMin, fMax, out float f2, fMin, fMax);
            AssertAreEqual(950, f0, fEpsilon11);
            AssertAreEqual(951, f1, fEpsilon11);
            AssertAreEqual(952, f2, fEpsilon10);
        }
        {
            p.Unpack4Float40(out float f0, fMin, fMax, out float f1, fMin, fMax, out float f2, fMin, fMax, out float f3, fMin, fMax);
            AssertAreEqual(960, f0, fEpsilon10);
            AssertAreEqual(961, f1, fEpsilon10);
            AssertAreEqual(962, f2, fEpsilon10);
            AssertAreEqual(963, f3, fEpsilon10);
        }
        {
            p.Unpack4Float40_11_11_10_8(out float f0, fMin, fMax, out float f1, fMin, fMax, out float f2, fMin, fMax, out float f3, fMin, fMax);
            AssertAreEqual(970, f0, fEpsilon11);
            AssertAreEqual(971, f1, fEpsilon11);
            AssertAreEqual(972, f2, fEpsilon10);
            AssertAreEqual(973, f3, fEpsilon8);
        }
    }
    private static void AssertArrayMatch<T>(T[] a, T[] b) where T : System.IComparable
    {
        if (a == b) return;
        if (a == null || b == null)
        {
            Assert.AreEqual(b, a);
            return;
        }
        if (a.Length != b.Length)
        {
            Assert.AreEqual(a.Length, b.Length, $"Expected array length {b.Length}, got {a.Length}");
            return;
        }
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i].CompareTo(b[i]) != 0)
            {
                Assert.AreEqual(a[i], b[i], $"Expected value of {b[i]} at index {i}, got {a[i]} instead.");
                return;
            }
        }
    }
}
#endif