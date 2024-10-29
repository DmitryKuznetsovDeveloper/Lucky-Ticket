using UnityEditor;
using UnityEngine;

namespace Game
{
    public sealed class ApplicationExit
    {
        public void ExitApp()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit(0);
#endif
        }
    }
}