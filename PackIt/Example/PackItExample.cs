/// ////////////////////////////////////////////////////////////////////// ///
/// Copyright(c) 2024, Jared Gray                                          ///
/// All rights reserved.                                                   ///
///                                                                        ///
/// This source code is licensed under the BSD-style license found in the  ///
/// LICENSE file in the root directory of this source tree.                ///
/// ////////////////////////////////////////////////////////////////////// ///

#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;

/// <summary>
/// This monobehaviour is meant to illustrate the use of the <see cref="PackIt"/> class. In this case, copy values from
/// a source transform and apply it to 4 other target transforms with varying degrees of precision.
/// </summary>
public class PackItExample : MonoBehaviour
{
    private struct InterpVector3
    {
        public Vector3 Result;
        public Vector3 Target;
        public float MaxDeltaPerSecond;
        public void Update(float deltaTime)
        {
            // If it's a constant rate, this works.
            //Result = Vector3.MoveTowards(Result, Target, MaxDeltaPerSecond * deltaTime);
            // But lerp in this fashion is great for ease-in. (Only on ~60fps+ fixed update though) :)
            Result = Vector3.Lerp(Result, Target, MaxDeltaPerSecond * deltaTime);
        }
        public void MatchTarget()
        {
            Result = Target;
        }
    }
    private struct InterpQuat
    {
        public Quaternion Result;
        public Quaternion Target;
        public float MaxDegreesPerSecond;
        public void Update(float deltaTime)
        {
            // If it's a constant rate, this works.
            // Result = Quaternion.RotateTowards(Result, Target, MaxDegreesPerSecond * deltaTime);
            // But lerp in this fashion is great for ease-in. (Only on ~60fps+ fixed update though) :)
            Result = Quaternion.Slerp(Result, Target, MaxDegreesPerSecond * deltaTime);
        }
        public void MatchTarget()
        {
            Result = Target;
        }
    }
    private class TransformTarget
    {
        public Transform Target;
        private Vector3 m_Offset;
        private Vector3 m_WorldExtents;
        private float m_MaxUniformScale;
        private InterpVector3 m_Position = new InterpVector3();
        private InterpQuat m_Rotation = new InterpQuat();
        private InterpVector3 m_Scale = new InterpVector3();

        public TransformTarget(Transform target, Vector3 offset, Vector3 worldExtents, float maxUniformScale)
        {
            if (!target) throw new System.ArgumentNullException("target");
            Target = target;
            m_Offset = offset;
            m_WorldExtents = worldExtents;
            m_MaxUniformScale = maxUniformScale;
            m_Rotation.Target = m_Rotation.Result = Quaternion.identity;
            m_Scale.Target = m_Scale.Result = Vector3.one;
        }
        public void Update(float deltaTime, bool useInterpolators)
        {
            if (useInterpolators)
            {
                m_Position.Update(deltaTime);
                m_Rotation.Update(deltaTime);
                m_Scale.Update(deltaTime);
            }
            else
            {
                m_Position.MatchTarget();
                m_Rotation.MatchTarget();
                m_Scale.MatchTarget();
            }
            Target.position = m_Offset + m_Position.Result;
            Target.rotation = m_Rotation.Result;
            Target.localScale = m_Scale.Result;
        }
        public void SetInterpolatorRates(float positionRate, float roationRate, float scaleRate)
        {
            m_Position.MaxDeltaPerSecond = positionRate;
            m_Rotation.MaxDegreesPerSecond = roationRate;
            m_Scale.MaxDeltaPerSecond = scaleRate;
        }
        public void UnpackFullPrecision(PackIt p)
        {
            p.SeekToStart();
            m_Position.Target = p.UnpackVector3();
            m_Rotation.Target = p.UnpackQuat();
            m_Scale.Target = p.UnpackVector3();
        }
        public void UnpackHalfPrecision(PackIt p)
        {
            p.SeekToStart();
            m_Position.Target = p.UnpackVector3_48(m_WorldExtents.x, m_WorldExtents.y, m_WorldExtents.z);
            m_Rotation.Target = p.UnpackQuat64();
            m_Scale.Target = p.UnpackVector3_48(0, m_MaxUniformScale, 0, m_MaxUniformScale, 0, m_MaxUniformScale);
        }
        public void UnpackLowPrecision(PackIt p)
        {
            p.SeekToStart();
            m_Position.Target = p.UnpackVector3_24(m_WorldExtents.x, m_WorldExtents.y, m_WorldExtents.z);
            m_Rotation.Target = p.UnpackQuat32();
            m_Scale.Target = p.UnpackVector3_24(0, m_MaxUniformScale, 0, m_MaxUniformScale, 0, m_MaxUniformScale);
        }
        public void UnpackBalancedPrecision(PackIt p)
        {
            p.SeekToStart();
            Vector3 vec = p.UnpackVector3();
            Quaternion q = new Quaternion();
            p.Unpack4Float40_11_11_10_8(
                out q.x, -1, 1,
                out q.z, -1, 1,
                out q.y, -1, 1,
                out q.w, -1, 1
                );
            Vector3 scale = new Vector3();
            p.Unpack3Float16(out scale.x, 0, m_MaxUniformScale, out scale.y, 0, m_MaxUniformScale, out scale.z, 0, m_MaxUniformScale);
            m_Position.Target = vec;
            m_Rotation.Target = q;
            m_Scale.Target = scale;
        }
        public void DrawGizmos(Color col)
        {
            Gizmos.color = col;
            Gizmos.DrawWireCube(m_Offset, m_WorldExtents * 2 - Vector3.one * 0.5f);
        }
    }
    
    [Header("Leave these untouched. Feel free to manipulate the Source_Cube in the scene view during play.")]
    [Tooltip("Changes to this transform will be reflected in the following 4 linked objects.")]
    public Transform SourceObject;
    [Tooltip("Full precision object target. Least space efficient, totally accurate.")]
    public Transform FullPrecisionObject;
    [Tooltip("Half precision object target. Space efficient and reasonably precise.")]
    public Transform HalfPrecisionObject;
    [Tooltip("Low precision object target. Extreamly space efficient and very imprecise.")]
    public Transform LowestPrecisionObject;
    [Tooltip("Balanced precision object target. Reasonably space efficient with taliored accuracy.")]
    public Transform BalancedPrecisionObject;

    [Header("The 'play space' of the source transform")]
    [Tooltip("This is the bounds for each object in the scene. Since they're offset, each object needs it's own slice of the world to operate within.")]
    public Vector3 WorldExtents = Vector3.one * 25;
    [Tooltip("This is the maximum xyz scale of any given transform.")]
    public float MaxUniformScale = 15.0f;
    [Header("Options")]
    [Tooltip("Interpolators are used to hide the loss of precision when applying lossy values to the transforms. When false, it illustrates what the loss in precision does.")]
    public bool UseInterpolators = false;
    [Tooltip("Instead of updating the transforms following the source cube every frame, you can do that in fixed update instead." +
        "Closer mimicry to a networked senario where delay + interpolators can still give a reasonable result.")]
    public bool UseFixedUpdate = false;
    [Tooltip("When no constant position rate is given, this is the rate at which the interpolators translate a transform to match the source transform.")]
    public float InterpolatorPositionRate = 25f;
    [Tooltip("When no constant rotation rate is given, this is the rate at which the interpolators rotate a transform to match the source transform.")]
    public float InterpolatorRotationRate = 360;
    [Tooltip("When no constant scale rate is given, this is the rate at which the interpolators scale a transform to match the source transform.")]
    public float InterpolatorScaleRate = 15;
    
    [Header("'Auto-play' features")]
    [Tooltip("Applies a constantly changing position to the source transform.")]
    [Range(0, 1)] public float ConstantPositionRate = 0;
    [Tooltip("The amplitude of the position changes.")]
    [Range(1, 25)] public float ConstantPositionAmplitude = 12;
    [Tooltip("Applies a constantly changing rotation to the source transform.")]
    [Range(0, 180)] public float ConstantRotationRate = 0;
    [Tooltip("Applies a constantly changing scale to the source transform.")]
    [Range(0, 1)] public float ConstantScaleRate = 0;
    [Tooltip("The amplitude of the scalar changes.")]
    [Range(0.5f, 15)] public float ConstantScaleAmplitude = 3;

    [Header("The number of bytes each cube needs for its visual result.")]
    [ShowOnly] public int NumBytesForFullPrecision;
    [ShowOnly] public int NumBytesFoHalfPrecision;
    [ShowOnly] public int NumBytesForLowPrecision;
    [ShowOnly] public int NumBytesForBalancedPrecision;

    private double m_RotationAccumulator = 0;
    private double m_PositionAccumulator = 0;
    private double m_ScaleAccumulator = 0;
    private Vector3 m_WorldExtents;
    private float m_MaxUniformScale;
    private TransformTarget[] m_TransformTargets;
    private TransformTarget m_TTFull;
    private TransformTarget m_TTHalf;
    private TransformTarget m_TTLow;
    private TransformTarget m_TTBal;
    private PackIt m_Packit;

    private void Start()
    {
        m_WorldExtents = WorldExtents;
        m_MaxUniformScale = MaxUniformScale;
        if (!SourceObject)
        {
            gameObject.SetActive(false); // Update will run once if not done.
            Debug.LogError("Must have a source object to run the example scene...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
            return;
        }
        m_Packit = new PackIt(64); // More than enough space...
        Vector3 halfExtNoY = WorldExtents; halfExtNoY.y = 0;
        m_TTFull = FullPrecisionObject ? new TransformTarget(FullPrecisionObject, halfExtNoY, m_WorldExtents, m_MaxUniformScale) : null;
        halfExtNoY.z = -halfExtNoY.z;
        m_TTHalf = HalfPrecisionObject ? new TransformTarget(HalfPrecisionObject, halfExtNoY, m_WorldExtents, m_MaxUniformScale) : null;
        halfExtNoY.x = -halfExtNoY.x;
        m_TTLow = LowestPrecisionObject ? new TransformTarget(LowestPrecisionObject, halfExtNoY, m_WorldExtents, m_MaxUniformScale) : null;
        halfExtNoY.z = -halfExtNoY.z;
        m_TTBal = BalancedPrecisionObject ? new TransformTarget(BalancedPrecisionObject, halfExtNoY, m_WorldExtents, m_MaxUniformScale) : null;
        m_TransformTargets = new TransformTarget[] { m_TTFull, m_TTHalf, m_TTLow, m_TTBal };
        SendPackItsToTransforms();
    }
    private void Update()
    {
        float deltaTime = Time.deltaTime;
        // If any rate > 0, when we are applying a constant motion to our source cube.
        if (ConstantPositionRate > 0)
        {
            m_PositionAccumulator += ConstantPositionRate * deltaTime;
            float f = (float)(m_PositionAccumulator % (Mathf.PI * 2));
            SourceObject.position = Vector3.forward * Mathf.Sin(f) * ConstantPositionAmplitude +
                Vector3.right * Mathf.Cos(f) * ConstantPositionAmplitude +
                Vector3.up * Mathf.Sin(f + f) * ConstantPositionAmplitude * 0.33f;
        }
        if (ConstantRotationRate > 0)
        {
            m_RotationAccumulator += ConstantRotationRate * deltaTime;
            float d = (float)(m_RotationAccumulator % 360.0d);
            SourceObject.rotation = Quaternion.Euler(d, d + 35, d + 70);
        }
        if (ConstantScaleRate > 0)
        {
            const double lilPi = Mathf.PI / 3;
            const float MinScale = 0.4f;
            m_ScaleAccumulator += ConstantScaleRate * deltaTime;
            float x = (float)(m_ScaleAccumulator % Mathf.PI);
            float y = (float)((m_ScaleAccumulator + lilPi) % Mathf.PI);
            float z = (float)((m_ScaleAccumulator + lilPi + lilPi) % Mathf.PI);
            float amplitute = Mathf.Clamp(ConstantScaleAmplitude - MinScale, MinScale, 10000);
            SourceObject.localScale = new Vector3(
                (Mathf.Sin(x) * amplitute) + MinScale,
                (Mathf.Sin(y) * amplitute) + MinScale,
                (Mathf.Sin(z) * amplitute) + MinScale);
        }
        // Rates are per second, but if we translate them to factor in constant rates, we can eliminate all stuttering from low precision
        // when using a constant translation.
        SetInterpolatorMaxDeltas(ConstantPositionRate <= 0 ? InterpolatorPositionRate : ConstantPositionRate * Mathf.Sqrt(ConstantPositionAmplitude * ConstantPositionAmplitude * 3),
                ConstantRotationRate <= 0 ? InterpolatorRotationRate : ConstantRotationRate * 3,
                ConstantScaleRate <= 0 ? InterpolatorScaleRate : ConstantScaleRate * Mathf.Sqrt(ConstantScaleAmplitude * ConstantScaleAmplitude * 3));
        if (SourceObject.hasChanged && !UseFixedUpdate) SendPackItsToTransforms();
        foreach (TransformTarget t in m_TransformTargets) t?.Update(deltaTime, UseInterpolators);
    }
    private void FixedUpdate()
    {
        if (SourceObject.hasChanged && UseFixedUpdate) SendPackItsToTransforms();
    }
    void SetInterpolatorMaxDeltas(float positionRate, float rotationRate, float scaleRate)
    {
        foreach (TransformTarget t in m_TransformTargets) t?.SetInterpolatorRates(positionRate, rotationRate, scaleRate);
    }
    /// <summary>
    /// Re-uses and re-packs a PackIt with appropriate data then shuffles it off to a TargetTransform.
    /// </summary>
    void SendPackItsToTransforms()
    {
        if (m_TTFull != null)
        {
            PackFullPrecisionFrom(SourceObject, m_Packit);
            NumBytesForFullPrecision = (int)m_Packit.Cursor;
            m_TTFull.UnpackFullPrecision(m_Packit);
        }
        if (m_TTHalf != null)
        {
            PackHalfPrecisionFrom(SourceObject, m_Packit, m_WorldExtents, m_MaxUniformScale);
            NumBytesFoHalfPrecision = (int)m_Packit.Cursor;
            m_TTHalf.UnpackHalfPrecision(m_Packit);
        }
        if (m_TTLow != null)
        {
            PackLowPrecisionFrom(SourceObject, m_Packit, m_WorldExtents, m_MaxUniformScale);
            NumBytesForLowPrecision = (int)m_Packit.Cursor;
            m_TTLow.UnpackLowPrecision(m_Packit);
        }
        if (m_TTBal != null)
        {
            PackBalancedPrecisionFrom(SourceObject, m_Packit, m_MaxUniformScale);
            NumBytesForBalancedPrecision = (int)m_Packit.Cursor;
            m_TTBal.UnpackBalancedPrecision(m_Packit);
        }
    }
    /// <summary>
    /// 40 bytes, no loss of precision, works anywhere.
    /// </summary>
    static void PackFullPrecisionFrom(Transform t, PackIt p)
    {
        p.SeekToStart();
        p.PackVector3(t.position);
        p.PackQuat(t.rotation);
        p.PackVector3(t.localScale);
    }
    /// <summary>
    /// 20 bytes, not half bad. ;)
    /// </summary>
    static void PackHalfPrecisionFrom(Transform t, PackIt p, Vector3 extents, float maxUniformScale)
    {
        p.SeekToStart();
        p.PackVector3_48(t.position, extents.x, extents.y, extents.z);
        p.PackQuat64(t.rotation);
        p.PackVector3_48(t.localScale, 0, maxUniformScale, 0, maxUniformScale, 0, maxUniformScale);
    }
    /// <summary>
    /// 10 bytes, bad quality but very small.
    /// </summary>
    static void PackLowPrecisionFrom(Transform t, PackIt p, Vector3 extents, float maxUniformScale)
    {
        p.SeekToStart();
        p.PackVector3_24(t.position, extents.x, extents.y, extents.z);
        p.PackQuat32(t.rotation);
        p.PackVector3_24(t.localScale, 0, maxUniformScale, 0, maxUniformScale, 0, maxUniformScale);
    }
    /// <summary>
    /// 19 bytes, compromises on scale and rotation. Full positional accuracy
    /// </summary>
    static void PackBalancedPrecisionFrom(Transform t, PackIt p, float maxUniformScale)
    {
        p.SeekToStart();
        p.PackVector3(t.position);
        Quaternion q = t.rotation;
        p.Pack4Float40_11_11_10_8(
            q.x, -1, 1,
            q.z, -1, 1,
            q.y, -1, 1,
            q.w, -1, 1);
        Vector3 scale = t.localScale;
        p.Pack3Float16(scale.x, 0, maxUniformScale, scale.y, 0, maxUniformScale, scale.z, 0, maxUniformScale);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Vector3.zero, Application.isPlaying ? m_WorldExtents * 2 : WorldExtents * 2);
        m_TTFull?.DrawGizmos(Color.green);
        m_TTHalf?.DrawGizmos(Color.yellow);
        m_TTLow?.DrawGizmos(Color.red);
        m_TTBal?.DrawGizmos(Color.magenta);
    }
    private void OnValidate()
    {
        WorldExtents.x = Mathf.Abs(WorldExtents.x);
        WorldExtents.y = Mathf.Abs(WorldExtents.y);
        WorldExtents.z = Mathf.Abs(WorldExtents.z);
        if (WorldExtents.sqrMagnitude < 1) WorldExtents = Vector3.one;
    }
    public class ShowOnlyAttribute : PropertyAttribute { }
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
    public class ShowOnly : UnityEditor.PropertyDrawer
    {

        public override void OnGUI(
          Rect position,
          UnityEditor.SerializedProperty prop,
          GUIContent label
        )
        {
            string valueString;
            switch (prop.propertyType)
            {
                case UnityEditor.SerializedPropertyType.Boolean:
                    valueString = prop.boolValue.ToString();
                    break;
                case UnityEditor.SerializedPropertyType.Integer:
                    valueString = prop.intValue.ToString();
                    break;
                case UnityEditor.SerializedPropertyType.Float:
                    valueString = prop.floatValue.ToString();
                    break;
                case UnityEditor.SerializedPropertyType.String:
                    valueString = prop.stringValue;
                    break;
                default:
                    valueString = "( Not Supported )";
                    break;
            }
            UnityEditor.EditorGUI.LabelField(position, label.text, valueString);
        }
    }
#endif
}
#endif