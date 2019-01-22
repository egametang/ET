#if SERVER
namespace UnityEngine
{
	/// <summary>
	///   <para>A single keyframe that can be injected into an animation curve.</para>
	/// </summary>
	public struct Keyframe
	{
	    private float m_Time;
	
	    private float m_Value;
	
	    private float m_InTangent;
	
	    private float m_OutTangent;
	
	    private int m_TangentMode;
	
	    /// <summary>
	    ///   <para>The time of the keyframe.</para>
	    /// </summary>
	    public float time
	    {
	        get
	        {
	            return m_Time;
	        }
	        set
	        {
	            m_Time = value;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The value of the curve at keyframe.</para>
	    /// </summary>
	    public float value
	    {
	        get
	        {
	            return m_Value;
	        }
	        set
	        {
	            m_Value = value;
	        }
	    }
	
	    /// <summary>
	    ///   <para>Describes the tangent when approaching this point from the previous point in the curve.</para>
	    /// </summary>
	    public float inTangent
	    {
	        get
	        {
	            return m_InTangent;
	        }
	        set
	        {
	            m_InTangent = value;
	        }
	    }
	
	    /// <summary>
	    ///   <para>Describes the tangent when leaving this point towards the next point in the curve.</para>
	    /// </summary>
	    public float outTangent
	    {
	        get
	        {
	            return m_OutTangent;
	        }
	        set
	        {
	            m_OutTangent = value;
	        }
	    }
	
	    /// <summary>
	    ///   <para>TangentMode is deprecated.  Use AnimationUtility.SetKeyLeftTangentMode or AnimationUtility.SetKeyRightTangentMode instead.</para>
	    /// </summary>
	    public int tangentMode
	    {
	        get
	        {
	            return m_TangentMode;
	        }
	        set
	        {
	            m_TangentMode = value;
	        }
	    }
	
	    /// <summary>
	    ///   <para>Create a keyframe.</para>
	    /// </summary>
	    /// <param name="time"></param>
	    /// <param name="value"></param>
	    public Keyframe(float time, float value)
	    {
	        m_Time = time;
	        m_Value = value;
	        m_InTangent = 0f;
	        m_OutTangent = 0f;
	        m_TangentMode = 0;
	    }
	
	    /// <summary>
	    ///   <para>Create a keyframe.</para>
	    /// </summary>
	    /// <param name="time"></param>
	    /// <param name="value"></param>
	    /// <param name="inTangent"></param>
	    /// <param name="outTangent"></param>
	    public Keyframe(float time, float value, float inTangent, float outTangent)
	    {
	        m_Time = time;
	        m_Value = value;
	        m_InTangent = inTangent;
	        m_OutTangent = outTangent;
	        m_TangentMode = 0;
	    }
	}
}
#endif