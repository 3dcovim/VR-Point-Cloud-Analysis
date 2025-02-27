// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;
using UnityEditor;

namespace Pcx
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PointCloudRenderer))]
    public class PointCloudRendererInspector : Editor
    {
        SerializedProperty _sourceData;
        SerializedProperty _sourceDataExt;
        SerializedProperty _sourceDataProps;
        SerializedProperty _pointTint;
        SerializedProperty _pointSize;

        void OnEnable()
        {
            _sourceData = serializedObject.FindProperty("_sourceData");
            _sourceDataExt = serializedObject.FindProperty("_sourceDataExt");
            _sourceDataProps = serializedObject.FindProperty("_sourceDataProps");
            _pointTint = serializedObject.FindProperty("_pointTint");
            _pointSize = serializedObject.FindProperty("_pointSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_sourceData);
            EditorGUILayout.PropertyField(_sourceDataExt);
            EditorGUILayout.PropertyField(_sourceDataProps);
            EditorGUILayout.PropertyField(_pointTint);
            EditorGUILayout.PropertyField(_pointSize);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
