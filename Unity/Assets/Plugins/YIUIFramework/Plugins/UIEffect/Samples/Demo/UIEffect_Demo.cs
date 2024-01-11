using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIEffects
{
    public class UIEffect_Demo : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            GetComponentInChildren<RectMask2D>().enabled = true;
        }

        public void SetTimeScale(float scale)
        {
            Time.timeScale = scale;
        }

        public void Open(Animator anim)
        {
            // anim.GetComponentInChildren<UIEffectCapturedImage>().Capture();
            anim.gameObject.SetActive(true);
            anim.SetTrigger("Open");
        }

        public void Close(Animator anim)
        {
            anim.SetTrigger("Close");
        }

        public void Capture(Animator anim)
        {
            // anim.GetComponentInChildren<UIEffectCapturedImage>().Capture();
            anim.SetTrigger("Capture");
        }

        public void SetCanvasOverlay(bool isOverlay)
        {
            GetComponent<Canvas>().renderMode =
                isOverlay ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
        }

        public void SetRenderMode(int mode)
        {
            var canvas = GetComponent<Canvas>();
            var cam = canvas.worldCamera;
            var pos = new Vector3(0, 0, -25);
            var rot = new Vector3(0, 0, 0);

            if ((RenderMode) mode == RenderMode.WorldSpace)
            {
                SetRenderMode((int) RenderMode.ScreenSpaceCamera);
                canvas.renderMode = RenderMode.WorldSpace;
                pos.x = 45;
                rot.y = -20;
            }
            else
            {
                canvas.renderMode = (RenderMode) mode;
            }

            cam.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
        }
    }
}
