#if SERVER
namespace UnityEngine
{
	/// <summary>
	///   <para>Determines how time is treated outside of the keyframed range of an AnimationClip or AnimationCurve.</para>
	/// </summary>
	public enum WrapMode
	{
	    /// <summary>
	    ///   <para>When time reaches the end of the animation clip, the clip will automatically stop playing and time will be reset to beginning of the clip.</para>
	    /// </summary>
	    Once = 1,
	    /// <summary>
	    ///   <para>When time reaches the end of the animation clip, time will continue at the beginning.</para>
	    /// </summary>
	    Loop = 2,
	    /// <summary>
	    ///   <para>When time reaches the end of the animation clip, time will ping pong back between beginning and end.</para>
	    /// </summary>
	    PingPong = 4,
	    /// <summary>
	    ///   <para>Reads the default repeat mode set higher up.</para>
	    /// </summary>
	    Default = 0,
	    /// <summary>
	    ///   <para>Plays back the animation. When it reaches the end, it will keep playing the last frame and never stop playing.</para>
	    /// </summary>
	    ClampForever = 8,
	    Clamp = 1
	}
}
#endif
