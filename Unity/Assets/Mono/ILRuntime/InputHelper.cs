using UnityEngine;

namespace ET
{
    public static class InputHelper
    {
        public static bool GetKeyDown(int code)
        {
            return Input.GetKeyDown((KeyCode) code);
        }

        public static bool GetMouseButtonDown(int code)
        {
            return Input.GetMouseButtonDown(code);
        }
    }
}