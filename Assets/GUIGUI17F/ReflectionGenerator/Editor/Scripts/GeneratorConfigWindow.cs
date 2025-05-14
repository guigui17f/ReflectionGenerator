using System.IO;
using UnityEditor;
using UnityEngine;

namespace GUIGUI17F.ReflectionGenerator
{
    /// <summary>
    /// editor window to setup the reflection generator plugin configs
    /// </summary>
    public class GeneratorConfigWindow : EditorWindow
    {
        private bool _modifyOriginName;
        private string _getMethodPrefix;
        private string _setMethodPrefix;
        private string _getMethodPostfix;
        private string _setMethodPostfix;
        private string _wrapperSaveDirectory;
        private string _configSaveDirectory = "Assets/GUIGUI17F/ReflectionGenerator/Editor/Resources";

        [MenuItem("Tools/ReflectionGenerator/GeneratorConfig", false, 1)]
        private static void ShowWindow()
        {
            GeneratorConfigWindow window = GetWindow<GeneratorConfigWindow>();
            window.titleContent = new GUIContent("Generator Config");
            window.minSize = new Vector2(480, 240);
            ReflectionGeneratorConfig config = ReflectionGeneratorUtility.GetGeneratorConfig();
            window._modifyOriginName = config.ModifyOriginName;
            window._getMethodPrefix = config.GetMethodPrefix;
            window._setMethodPrefix = config.SetMethodPrefix;
            window._getMethodPostfix = config.GetMethodPostfix;
            window._setMethodPostfix = config.SetMethodPostfix;
            window._wrapperSaveDirectory = config.WrapperSaveDirectory;
            string configPath = AssetDatabase.GetAssetPath(config);
            if (!string.IsNullOrEmpty(configPath))
            {
                window._configSaveDirectory = Path.GetDirectoryName(configPath).Replace('\\', '/');
            }
            window.Show();
        }

        private void OnGUI()
        {
            float originWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 245;
            _modifyOriginName = EditorGUILayout.Toggle("Auto modify the name from fields:", _modifyOriginName);
            EditorGUIUtility.labelWidth = originWidth;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Setup the prefix of \"Get\" method name:", GUILayout.Width(240));
            _getMethodPrefix = EditorGUILayout.TextField(_getMethodPrefix);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Setup the prefix of \"Set\" method name:", GUILayout.Width(240));
            _setMethodPrefix = EditorGUILayout.TextField(_setMethodPrefix);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Setup the postfix of \"Get\" method name:", GUILayout.Width(240));
            _getMethodPostfix = EditorGUILayout.TextField(_getMethodPostfix);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Setup the postfix of \"Set\" method name:", GUILayout.Width(240));
            _setMethodPostfix = EditorGUILayout.TextField(_setMethodPostfix);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Setup generated wrappers save directory:", GUILayout.Width(240));
            _wrapperSaveDirectory = EditorGUILayout.TextField(_wrapperSaveDirectory);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                _wrapperSaveDirectory = GetSaveDirectory(_wrapperSaveDirectory);
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Setup config data save directory:", GUILayout.Width(240));
            _configSaveDirectory = EditorGUILayout.TextField(_configSaveDirectory);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                _configSaveDirectory = GetSaveDirectory(_configSaveDirectory);
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);
            if (GUILayout.Button("Save"))
            {
                SaveConfigData();
            }
        }

        private string GetSaveDirectory(string originDirectory)
        {
            string saveDirectory = originDirectory;
            if (string.IsNullOrEmpty(saveDirectory) || !Directory.Exists(saveDirectory))
            {
                saveDirectory = Application.dataPath;
            }
            string directory = EditorUtility.OpenFolderPanel("Choose save directory", saveDirectory, string.Empty);
            //convert absolute path to relative path to keep flexibility
            if (!string.IsNullOrEmpty(directory) && directory.Contains(Application.dataPath))
            {
                saveDirectory = directory.Substring(Application.dataPath.Length - 6);
            }
            return saveDirectory;
        }

        private void SaveConfigData()
        {
            if (string.IsNullOrEmpty(_getMethodPrefix) && string.IsNullOrEmpty(_getMethodPostfix))
            {
                EditorUtility.DisplayDialog("Warning", "\"Get\" method prefix and postfix can't be both empty!", "OK");
            }
            else if (string.IsNullOrEmpty(_setMethodPrefix) && string.IsNullOrEmpty(_setMethodPostfix))
            {
                EditorUtility.DisplayDialog("Warning", "\"Set\" method prefix and postfix can't be both empty!", "OK");
            }
            else if (string.IsNullOrEmpty(_wrapperSaveDirectory))
            {
                EditorUtility.DisplayDialog("Warning", "Please setup the generated wrappers save directory!", "OK");
            }
            else if (string.IsNullOrEmpty(_configSaveDirectory))
            {
                EditorUtility.DisplayDialog("Warning", "Please setup the config data save directory!", "OK");
            }
            else if (!_configSaveDirectory.EndsWith("Resources"))
            {
                EditorUtility.DisplayDialog("Warning", "Config file should be placed in \"Resources\" folder root!", "OK");
            }
            else
            {
                ReflectionGeneratorUtility.SaveGeneratorConfig(_modifyOriginName, _getMethodPrefix, _setMethodPrefix, _getMethodPostfix, _setMethodPostfix, _wrapperSaveDirectory, _configSaveDirectory);
                EditorUtility.DisplayDialog("Information", "Generator config saved successfully.", "OK");
                Close();
            }
        }
    }
}