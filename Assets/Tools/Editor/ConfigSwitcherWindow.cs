using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    public class ConfigSwitcherWindow : EditorWindow
    {
        private Vector2 _scrollPos;

        [MenuItem("Tools/Config Switcher")]
        public static void ShowWindow() => GetWindow<ConfigSwitcherWindow>("Config Switcher");

        private void OnGUI()
        {
            GUILayout.Label("Config Switcher", EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            string configRoot = "Assets/Configs";
            if (Directory.Exists(configRoot))
                DisplayConfigs(configRoot, 0);
            else
                GUILayout.Label($"Папка {configRoot} не найдена!", EditorStyles.helpBox);

            EditorGUILayout.EndScrollView();
        }

        private void DisplayConfigs(string directory, int indentLevel)
        {
            string[] subdirectories = Directory.GetDirectories(directory);
            foreach (string subdir in subdirectories)
            {
                string folderName = Path.GetFileName(subdir);
                GUILayout.BeginHorizontal();
                GUILayout.Space(indentLevel * 15); 
                GUILayout.Label(folderName, EditorStyles.foldoutHeader);
                GUILayout.EndHorizontal();
                
                DisplayConfigs(subdir, indentLevel + 1);
            }
            
            string[] configPaths = Directory.GetFiles(directory, "*.asset", SearchOption.TopDirectoryOnly);
            foreach (string configPath in configPaths)
            {
                ScriptableObject config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(configPath);

                if (config != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indentLevel * 15 + 20); 
                    if (GUILayout.Button(config.name, GUILayout.Height(25)))
                        AssetDatabase.OpenAsset(config);
                    
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
