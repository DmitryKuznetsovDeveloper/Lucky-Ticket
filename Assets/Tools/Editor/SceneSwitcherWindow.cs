using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Tools.Editor
{
    public class SceneSwitcherWindow : EditorWindow
    {
        [MenuItem("Tools/Scene Switcher")]
        public static void ShowWindow() => GetWindow<SceneSwitcherWindow>("Scene Switcher");

        private void OnGUI()
        {
            GUILayout.Label("Scene Switcher", EditorStyles.boldLabel);
            GUILayout.Space(10); // Отступ сверху

            // Группа сцен, добавленных в Build Settings
            GUILayout.Label("Сцены в Build Settings", EditorStyles.boldLabel);
            GUILayout.Space(5); // Отступ между заголовком и кнопками
            foreach (var scene in EditorBuildSettings.scenes)
            {
                string scenePath = scene.path;
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

            GUILayout.Space(20); // Отступ между группами

            // Группа всех остальных сцен
            GUILayout.Label("Все остальные сцены", EditorStyles.boldLabel);
            GUILayout.Space(5); // Отступ между заголовком и кнопками

            // Получаем все сцены из папки "Assets"
            string[] allScenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            HashSet<string> buildScenes = new HashSet<string>(EditorBuildSettings.scenes.Select(s => s.path));

            foreach (var sceneGUID in allScenePaths)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
                
                // Проверяем, если сцена не добавлена в Build Settings
                if (!buildScenes.Contains(scenePath))
                {
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
}
