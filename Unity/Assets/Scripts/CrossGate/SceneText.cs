using UnityEngine;

public class SceneText : MonoBehaviour
{
    private TextMesh FrontText;
    private TextMesh BackText;

    private void Awake()
    {
        FrontText = GetComponent<TextMesh>();
        BackText = FrontText.transform.Find("Shadow").GetComponent<TextMesh>();
        GetComponent<MeshRenderer>().sortingLayerName = "Text";
        BackText.gameObject.GetComponent<MeshRenderer>().sortingLayerName = "Text";
    }

    public void Show(string str, Color color)
    {
        FrontText.text = str;
        FrontText.color = color;
        BackText.text = str;
    }
}
