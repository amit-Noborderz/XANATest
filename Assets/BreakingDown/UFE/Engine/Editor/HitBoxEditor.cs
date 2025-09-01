using UnityEngine;
using UnityEditor;
using UFE3D;

namespace BD
{
    [CustomEditor(typeof(CustomHitBoxesInfo))]
    public class HitBoxEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Hit Box Editor"))
                HitBoxEditorWindow.Init();
        }
    }
}