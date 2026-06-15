using GameFrameX.Startup.Runtime;

using UnityEditor;
using UnityEditorInternal;

using UnityEngine;

namespace GameFrameX.Startup.Editor
{
    /// <summary>
    /// StartupOptions 自定义 Inspector。为 GlobalInfoUrls 字段提供可拖拽排序的列表 UI。
    /// </summary>
    [CustomEditor(typeof(StartupOptions))]
    public sealed class StartupOptionsInspector : UnityEditor.Editor
    {
        private ReorderableList _urlList;
        private SerializedProperty _urlListProperty;

        private const string UrlListHeader = "Global Info URLs (Primary-Backup Order)";
        private const string ScriptPropertyName = "m_Script";

        private void OnEnable()
        {
            _urlListProperty = serializedObject.FindProperty(nameof(StartupOptions.GlobalInfoUrls));

            _urlList = new ReorderableList(serializedObject, _urlListProperty, true, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, UrlListHeader);
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = _urlListProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 可重排的 URL 列表
            _urlList.DoLayoutList();

            EditorGUILayout.Space();

            // 其他字段使用默认绘制（跳过 URL 列表和 m_Script）
            DrawPropertiesExcludingUrlAndScript();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPropertiesExcludingUrlAndScript()
        {
            var iterator = serializedObject.GetIterator();
            if (!iterator.NextVisible(true))
            {
                return;
            }

            do
            {
                if (iterator.name == nameof(StartupOptions.GlobalInfoUrls) || iterator.name == ScriptPropertyName)
                {
                    continue;
                }

                EditorGUILayout.PropertyField(iterator, true);
            } while (iterator.NextVisible(false));
        }
    }
}
