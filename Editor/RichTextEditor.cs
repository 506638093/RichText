/***
 *              HuaHua
 *              2020-09-25
 *              富文本，一个drawcall 支持文字和图片混排
 **/

using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System;

namespace UnityEngine.UI
{
    [CustomEditor(typeof(RichText))]
    [CanEditMultipleObjects]
    public class RichTextEditor : GraphicEditor
    {

        protected override void OnEnable()
        {
            base.OnEnable();

            var serializedObject = this.serializedObject;
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontData = serializedObject.FindProperty("m_FontData");
            m_AtlasTexture = serializedObject.FindProperty("m_AtlasTexture");
            m_UiMode = serializedObject.FindProperty("m_UiMode");

            m_lpfnParseText = System.Delegate.CreateDelegate(typeof(Action), serializedObject.targetObject, "parseText") as Action;

            EditorApplication.update += CheckText;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            EditorApplication.update -= CheckText;
        }

        private void CheckText()
        {
            var currentTextString = m_Text.stringValue;
            if (m_lastTextString != currentTextString)
            {
                m_lastTextString = currentTextString;

                var richText = serializedObject.targetObject as RichText;
                if (richText.IsActive() && null != m_lpfnParseText)
                {
                    m_lpfnParseText();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var richText = serializedObject.targetObject as RichText;

            EditorGUILayout.PropertyField(m_Text);
            EditorGUILayout.PropertyField(m_FontData);
            EditorGUILayout.PropertyField(m_AtlasTexture);
            if (richText.m_AtlasTexture)
            {
                var atlasPath = AssetDatabase.GetAssetPath(richText.m_AtlasTexture);
                atlasPath = atlasPath.Substring("Assets/Resources/".Length, atlasPath.Length - "Assets/Resources/".Length - ".png".Length) + "/";
                if (atlasPath != richText.AtlasTexturePath)
                {
                    richText.AtlasTexturePath = atlasPath;
                }
            }
            EditorGUILayout.PropertyField(m_UiMode);

            AppearanceControlsGUI();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }

        private SerializedProperty m_Text;
        private SerializedProperty m_FontData;
        private SerializedProperty m_AtlasTexture;
        private SerializedProperty m_UiMode;

        private string m_lastTextString;
        private Action m_lpfnParseText;

    }


}
