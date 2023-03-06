using UnityEditor;

namespace ET
{
    [InitializeOnLoad]
    public class EditorLogHelper
    {
        static EditorLogHelper()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += CheckCompolingFinish;
        }

        private static void CheckCompolingFinish()
        {
            if (!EditorApplication.isCompiling)
            {
                CreateLog();
                EditorApplication.update -= CheckCompolingFinish;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.ExitingPlayMode:
                    CreateLog();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    DestroyLog();
                    break;
                default:
                    break;
            }
        }

        private static void CreateLog()
        {
            if (Logger.Instance != null)
            {
                return;
            }

            var log = new Logger();
            ((ISingleton)log).Register();
            log.ILog = new UnityLogger();
        }

        private static void DestroyLog()
        {
            if (Logger.Instance == null)
            {
                return;
            }

            ((ISingleton)Logger.Instance).Destroy();
        }
    }
}