using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour {

    public static DebugText _instance;
    public Text Text;
    private string Str;

    private void Awake()
    {
        _instance = this;
    }

    public void Show(string msg)
    {
        Str += msg + "\n";
        Text.text = Str;
    }

    public void Clear()
    {
        Str = "";
        Text.text = Str;
    }
}
