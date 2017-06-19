using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
/// <summary>
/// 绘制图片
/// </summary>
public class UESTexture : UESDisplayObject
{
    protected Texture m_Image;
    protected ScaleMode m_ScaleMode;
    protected bool m_AlphaBlend;
    protected float m_ImageAspect;
    protected int drawingStyle;
    protected Rect m_TexCoords;

    protected override void Awake()
    {
        base.Awake();
        m_Image = UESTool.GetTexture("DefaultTexture");
        Rect = new Rect(Vector2.zero, new Vector2(INIT_WIDTH, 32));
        SetDrawingStyle(ScaleMode.StretchToFill,true,1);
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                GUI.DrawTexture(m_DrawingRect, m_Image);
                break;

            case 2:
                GUI.DrawTexture(m_DrawingRect,m_Image,m_ScaleMode);
                break;

            case 3:
                GUI.DrawTexture(m_DrawingRect, m_Image, m_ScaleMode, m_AlphaBlend);
                break;

            case 4:
                GUI.DrawTexture(m_DrawingRect, m_Image, m_ScaleMode, m_AlphaBlend, m_ImageAspect);
                break;

            case 5:
                GUI.DrawTextureWithTexCoords(m_DrawingRect, m_Image, m_TexCoords);
                break;

            case 6:
                GUI.DrawTextureWithTexCoords(m_DrawingRect, m_Image, m_TexCoords,m_AlphaBlend);
                break;
        }
    }

    public void SetDrawingStyle()
    {
        drawingStyle = 1;
    }

    public void SetDrawingStyle(ScaleMode scaleMode)
    {
        drawingStyle = 2;

        m_ScaleMode = scaleMode;
    }

    public void SetDrawingStyle(ScaleMode scaleMode,bool alphaBlend)
    {
        drawingStyle = 3;

        m_ScaleMode = scaleMode;
        m_AlphaBlend = alphaBlend;
    }

    public void SetDrawingStyle(ScaleMode scaleMode, bool alphaBlend,float imageAspect)
    {
        drawingStyle = 4;

        m_ScaleMode = scaleMode;
        m_AlphaBlend = alphaBlend;
        m_ImageAspect = imageAspect;
    }

    public void SetDrawingStyle(Rect texCoords)
    {
        drawingStyle = 5;

        m_TexCoords = texCoords;
    }

    public void SetDrawingStyle(Rect texCoords,bool alphaBlend)
    {
        drawingStyle = 6;

        m_TexCoords = texCoords;
        m_AlphaBlend = alphaBlend;
    }
}
