using ET;
using UnityEngine;
using UnityEngine.UI;

public class RedDotMonoView: MonoBehaviour
{
    [HideInInspector]
    public bool isRedDotActive = false;
    private GameObject redDotGameObject = null;
    private Text redDotCountLabel = null;
    
    public Vector3 RedDotScale    = Vector3.one;
    public Vector2 PositionOffset = Vector2.zero;
    
    private void Awake()
    {
        this.isRedDotActive = false;
    }

    public void Show( GameObject redDotGameObject )
    {
        this.isRedDotActive = true;
        this.redDotGameObject = redDotGameObject;
        redDotGameObject.transform.SetParent(this.transform,false);
        redDotGameObject.transform.localScale = RedDotScale;
        redDotGameObject.transform.GetComponent<RectTransform>().anchoredPosition = this.PositionOffset;
        this.redDotCountLabel = redDotGameObject.GetComponentInChildren<Text>();
        redDotGameObject.SetActive(true);
    }
    
    public void RefreshRedDotCount(int count)
    {
        if ( null == this.redDotGameObject )
        {
            return;
        }
        this.redDotGameObject.transform.localScale = RedDotScale;
        this.redDotCountLabel.text = count <= 0? string.Empty: count.ToString();
    }

    public GameObject Recovery()
    {
        if (this.redDotCountLabel != null)
        {
            this.redDotCountLabel.text = "";
        }
        
        this.isRedDotActive = false;
        this.redDotCountLabel = null;
        this.redDotGameObject?.SetActive(false);
        GameObject go = this.redDotGameObject;
        this.redDotGameObject = null;
        return go;
    }
}