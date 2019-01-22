#if SERVER
namespace UnityEngine
{
    /// <summary>
    ///   <para>LayerMask allow you to display the LayerMask popup menu in the inspector.</para>
    /// </summary>
    public struct LayerMask
	{
	    private int m_Mask;
	
	    /// <summary>
	    ///   <para>Converts a layer mask value to an integer value.</para>
	    /// </summary>
	    public int value
	    {
	        get
	        {
	            return m_Mask;
	        }
	        set
	        {
	            m_Mask = value;
	        }
	    }
	
	    public static implicit operator int(LayerMask mask)
	    {
	        return mask.m_Mask;
	    }
	
	    public static implicit operator LayerMask(int intVal)
	    {
	        LayerMask result = default(LayerMask);
	        result.m_Mask = intVal;
	        return result;
	    }
	}
}
#endif
