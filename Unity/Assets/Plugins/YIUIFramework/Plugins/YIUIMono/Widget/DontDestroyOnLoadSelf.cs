using UnityEngine;

namespace YIUIFramework
{
    public class DontDestroyOnLoadSelf : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}