using System;
using System.Collections;
using System.Collections.Generic;
using ET;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MainPackage
{
    public class EntryLoading : MonoBehaviour
    {
        /// <summary>
        /// 进度条
        /// </summary>
        [SerializeField]
        private Image Img_Progress;
        /// <summary>
        /// 检查更新中
        /// </summary>
        [SerializeField]
        private Text Txt_VersionCheck;
        /// <summary>
        /// 加载资源中
        /// </summary>
        [FormerlySerializedAs("Txt_LoadingAsset")]
        [SerializeField]
        private Text Txt_Downloading;
        /// <summary>
        /// 退出按钮
        /// </summary>
        [SerializeField]
        private Button Btn_Quit;
        /// <summary>
        /// 加载报错
        /// </summary>
        [SerializeField]
        private RectTransform Rt_Error;

        /// <summary>
        /// 下载状态
        /// </summary>
        private int _currStatus = -1;

        private void Awake()
        {
            Btn_Quit.onClick.AddListener(Application.Quit);
            gameObject.SetActive(true);
        }

        private void Start()
        {
            
        }

        /// <summary>
        /// 检测版本
        /// </summary>
        public void CheckUpdate()
        {
            Txt_VersionCheck.gameObject.SetActive(true);
            this.Txt_Downloading.gameObject.SetActive(false);
            Img_Progress.fillAmount = 0;
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        public void StartDownload()
        {
            Txt_VersionCheck.gameObject.SetActive(false);
            Txt_Downloading.gameObject.SetActive(true);
            Img_Progress.fillAmount = 0;
        }

        /// <summary>
        /// Error
        /// </summary>
        public void ShowError()
        {
            Rt_Error.gameObject.SetActive(true);
        }

        /// <summary>
        /// 实时更新进度
        /// </summary>
        private void Update()
        {
            if (ResourcesComponent.Instance == null)
            {
                return;
            }

            if (_currStatus == 3)
            {
                //显示进度
                var download = ResourcesComponent.Instance.Downloader;
                Txt_Downloading.text =download.CurrentDownloadCount.ToString() + "/" + download.TotalDownloadCount.ToString();
                Img_Progress.fillAmount = (float)download.CurrentDownloadCount / download.TotalDownloadCount;
            }
            
            if (_currStatus == ResourcesComponent.Instance.DownloadStatus)
            {
                return;
            }
            
            _currStatus = ResourcesComponent.Instance.DownloadStatus;
            switch (_currStatus)
            {
                case 0:
                    //未开始
                    CheckUpdate();
                    break;
                
                case 1:
                    //版本检测完毕
                    
                    break;
                
                case 2:
                    //下载清单检测完毕
                    
                    break;
                
                case 3:
                    //下载中
                    StartDownload();
                    break;
                
                case 4:
                    //下载成功
                    gameObject.SetActive(false);
                    break;
                
                case 5:
                    //下载失败
                    ShowError();
                    break;
            }
        }
    }
}