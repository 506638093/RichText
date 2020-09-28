/***
 *              HuaHua
 *              2020-09-25
 *              富文本，一个drawcall 支持文字和图片混排
 **/

using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

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
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Text);
            EditorGUILayout.PropertyField(m_FontData);
            EditorGUILayout.PropertyField(m_AtlasTexture);
            EditorGUILayout.PropertyField(m_UiMode);

            AppearanceControlsGUI();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }

        private SerializedProperty m_Text;
        private SerializedProperty m_FontData;
        private SerializedProperty m_AtlasTexture;
        private SerializedProperty m_UiMode;

    }


}
