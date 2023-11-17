// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;
using UnityEditor;

namespace Pcx
{
    [CustomEditor(typeof(PointCloudDataExtended))]
    public sealed class PointCloudDataInspectorExtended : Editor
    {
        public override void OnInspectorGUI()
        {
            var count = ((PointCloudDataExtended)target).pointCountExt;
            EditorGUILayout.LabelField("Point Count", count.ToString("N0"));

            var numprop = ((PointCloudDataExtended)target).propsCount;
            EditorGUILayout.LabelField("Property Count", numprop.ToString("N0"));
        }
    }
}
