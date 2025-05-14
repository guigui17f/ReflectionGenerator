using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GUIGUI17F.ReflectionGenerator
{
    /// <summary>
    /// plugin main window, used to generate reflection wrappers
    /// </summary>
    public class ReflectionGeneratorWindow : EditorWindow
    {
        private readonly Type _scriptType = typeof(MonoScript);
        private Object _currentObject;

        private bool _showNonPublic = true;
        private bool _showPublic = false;
        private bool _showInstance = true;
        private bool _showStatic = false;

        private Type[] _typeInfos;
        private bool[] _typeOptions;
        private FieldInfo[] _fieldInfos;
        private bool[] _fieldOptions;

        private List<Type> _selectTypeList = new List<Type>();
        private List<int> _displayFieldIndexList = new List<int>();

        private Vector2 _scrollPosition;

        [MenuItem("Tools/ReflectionGenerator/GeneratorWindow", false, 0)]
        private static void ShowWindow()
        {
            ReflectionGeneratorWindow window = GetWindow<ReflectionGeneratorWindow>();
            window.titleContent = new GUIContent("Reflection Generator");
            window.minSize = new Vector2(640, 480);
            window.Show();
        }
 
        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 115;
            EditorGUI.BeginChangeCheck();
            _currentObject = EditorGUILayout.ObjectField("Select target script:", _currentObject, _scriptType, false);
            if (EditorGUI.EndChangeCheck())
            {
                _typeInfos = null;
                _typeOptions = null;
                _fieldInfos = null;
                _fieldOptions = null;
                _selectTypeList.Clear();
                _displayFieldIndexList.Clear();
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Refresh"))
            {
                if (_currentObject == null)
                {
                    EditorUtility.DisplayDialog("Warning", "Please setup target script at first.", "OK");
                }
                else
                {
                    UpdateReflectionInfos();
                }
            }
            GUILayout.Space(5);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUIUtility.labelWidth = 240;
            _showNonPublic = EditorGUILayout.ToggleLeft("Show NonPublic Field", _showNonPublic);
            _showPublic = EditorGUILayout.ToggleLeft("Show Public Field", _showPublic);
            _showInstance = EditorGUILayout.ToggleLeft("Show Instance Field", _showInstance);
            _showStatic = EditorGUILayout.ToggleLeft("Show Static Field", _showStatic);
            ShowTypeToggles();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            ShowFieldToggles();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            GUILayout.Space(5);
            if (GUILayout.Button("Generate"))
            {
                if (_currentObject == null)
                {
                    EditorUtility.DisplayDialog("Warning", "Please setup target script at first.", "OK");
                }
                else if (_fieldInfos == null)
                {
                    EditorUtility.DisplayDialog("Warning", "Please click \"Refresh\" and setup target fields at first.", "OK");
                }
                else if (EditorUtility.DisplayDialog("Notice", "Going to generate wrapper into destination directory setup in config, continue?", "Yes", "Cancel"))
                {
                    GenerateWrapper();
                }
            }
            GUILayout.Space(5);
        }

        private void UpdateReflectionInfos()
        {
            Type type = (_currentObject as MonoScript).GetClass();
            _typeInfos = ReflectionGeneratorUtility.GetBaseTypes(type);
            _typeOptions = new bool[_typeInfos.Length];
            if (_typeOptions.Length > 0)
            {
                _typeOptions[0] = true;
            }
            _fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            _fieldOptions = new bool[_fieldInfos.Length];
        }

        private void ShowTypeToggles()
        {
            if (_typeInfos == null)
            {
                return;
            }
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
            for (int i = 0; i < _typeInfos.Length; i++)
            {
                _typeOptions[i] = EditorGUILayout.ToggleLeft(_typeInfos[i].Name, _typeOptions[i]);
            }
        }

        private void ShowFieldToggles()
        {
            if (_fieldInfos == null)
            {
                return;
            }
            _selectTypeList.Clear();
            for (int i = 0; i < _typeOptions.Length; i++)
            {
                if (_typeOptions[i])
                {
                    _selectTypeList.Add(_typeInfos[i]);
                }
            }
            _displayFieldIndexList.Clear();
            for (int i = 0; i < _fieldInfos.Length; i++)
            {
                if (_showNonPublic && !_fieldInfos[i].IsPublic || _showPublic && _fieldInfos[i].IsPublic || _showInstance && !_fieldInfos[i].IsStatic || _showStatic && _fieldInfos[i].IsStatic)
                {
                    if (_selectTypeList.Exists(type => type.FullName == _fieldInfos[i].DeclaringType.FullName))
                    {
                        _displayFieldIndexList.Add(i);
                    }
                }
            }
            _displayFieldIndexList.ForEach(index => _fieldOptions[index] = EditorGUILayout.ToggleLeft(_fieldInfos[index].Name, _fieldOptions[index]));
        }

        private void GenerateWrapper()
        {
            Type type = (_currentObject as MonoScript).GetClass();
            List<FieldInfo> fieldList = new List<FieldInfo>();
            _displayFieldIndexList.ForEach(index =>
            {
                if (_fieldOptions[index])
                {
                    fieldList.Add(_fieldInfos[index]);
                }
            });
            ReflectionGeneratorConfig config = ReflectionGeneratorUtility.GetGeneratorConfig();
            ReflectionGeneratorUtility.GenerateWrapper(type, fieldList, config);
            AssetDatabase.Refresh();
            Debug.Log($"Type {type.Name} wrapper generated at {config.WrapperSaveDirectory}.");
        }
    }
}