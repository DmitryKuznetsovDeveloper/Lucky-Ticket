using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Tools.Editor
{
    public class SceneSwitcherWindow : EditorWindow
    {
        [MenuItem("Tools/Scene Switcher")]
        public static void ShowWindow() => GetWindow<SceneSwitcherWindow>("Scene Switcher");

        private void OnGUI()
        {
            GUILayout.Label("Scene Switcher", EditorStyles.boldLabel);

            // Получаем список всех сцен, добавленных в Build Settings
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                string scenePath = EditorBuildSettings.scenes[i].path;
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                if (GUILayout.Button(sceneName))
                {
                    // Проверяем, если сцена не загружена, открываем её
                    if (!EditorSceneManager.GetActiveScene().name.Equals(sceneName))
                    {
                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        EditorSceneManager.OpenScene(scenePath);
                    }
                }
            }
        }
    }
}