/*********************************************
 * BFramework
 * Log显示
 * 创建时间：2023/01/08 20:40:23
 *********************************************/

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MainPackage
{
    public class Log : MonoBehaviour
    {
        public static Log Instance;
        private Vector2 _scorllPos;
        private bool _isShowLog = false;
        private float _logBtnWidth = 80;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Log脚本重复挂载");
            }
            Instance = this;
            //enabled = false;
            Application.logMessageReceived -= LogCallback;
            Application.logMessageReceived += LogCallback;
        }

        #region 日志输出相关
        private Dictionary<LogType, StringBuilder> _logDic = new Dictionary<LogType, StringBuilder>()
        {
            [LogType.Log] = new StringBuilder(""),
            [LogType.Error] = new StringBuilder(""),
            [LogType.Warning] = new StringBuilder(""),
        };
        private LogType _logType = LogType.Log;
        private void LogCallback(string condition, string stackTrace, LogType type)
        {
            LogType saveLog = type;
            if ((type != LogType.Log) && (type != LogType.Warning))
            {
                saveLog = LogType.Error;
            }

            if (!_logDic.TryGetValue(saveLog, out StringBuilder sb))
            {
                sb = new StringBuilder("");
                _logDic.Add(saveLog, sb);
            }

            sb.Insert(0, "\n\n");
            if (type != LogType.Log)
            {
                sb.Insert(0, stackTrace);
                sb.Insert(0, "\n");
            }
            sb.Insert(0, condition);
            sb.Insert(0, ":");
            sb.Insert(0, type.ToString());

            if ((type == LogType.Error) || (type == LogType.Exception))
            {
                _logType = LogType.Error;
                _isShowLog = true;
            }
        }

        private int _fpsCnt = 0;
        private float _fpsTime;
        private string _fpsStr;
        private Color _fpsColor;
        private void Update()
        {
            _fpsCnt++;
            _fpsTime += Time.unscaledDeltaTime;
            if (_fpsTime >= 1)
            {
                _fpsStr = _fpsCnt.ToString();
                float c = Mathf.Clamp01(_fpsCnt / 60f);
                _fpsColor = new Color(1 - c, c, 0, 1);
                _fpsTime = 0;
                _fpsCnt = 0;
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(200);
            GUI.skin.button.fontSize = 20;
            GUI.skin.label.fontSize = 18;

            GUILayout.Space(20);
            GUI.color = _fpsColor;
            GUILayout.Label("FPS:" + _fpsStr, GUILayout.Height(50));

            GUI.color = new Color(1, 1, 1);
            if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height - 50, 100, 40), "Log"))
            {
                _isShowLog = !_isShowLog;
            }

            if (GUI.Button(new Rect(Screen.width / 2 + 40, Screen.height - 50, 100, 40), "Destroy"))
            {
                Destroy(this);
            }

            // 日志输出
            if (!_isShowLog)
            {
                return;
            }

            if (GUILayout.Button("Log", GUILayout.Height(35), GUILayout.Width(_logBtnWidth)))
            {
                _scorllPos = Vector2.zero;
                _logType = LogType.Log;
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Error", GUILayout.Height(35), GUILayout.Width(_logBtnWidth)))
            {
                _scorllPos = Vector2.zero;
                _logType = LogType.Error;
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Warnning", GUILayout.Height(35), GUILayout.Width(_logBtnWidth)))
            {
                _scorllPos = Vector2.zero;
                _logType = LogType.Warning;
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Clear", GUILayout.Height(35), GUILayout.Width(_logBtnWidth)))
            {
                foreach (var item in _logDic.Values)
                {
                    item.Clear();
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(_logBtnWidth);
            _scorllPos = GUILayout.BeginScrollView(_scorllPos, GUILayout.Height(1000), GUILayout.Width(650));
            GUILayout.TextArea(_logDic[_logType].ToString(), GUILayout.Width(730));
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }
        #endregion

        private void OnDestroy()
        {
            Application.logMessageReceived -= LogCallback;
        }
    }
}