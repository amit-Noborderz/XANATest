using UnityEditor;
using UnityEngine;
using System.IO;
using UFE3D;

public class CopyBasicMovesToAll : EditorWindow
{
    private StanceInfo sourceStance;

    [MenuItem("Tools/UFE/Copy Basic Moves To All Stances")]
    public static void ShowWindow()
    {
        GetWindow<CopyBasicMovesToAll>("Copy Basic Moves");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Source Stance Info", EditorStyles.boldLabel);
        sourceStance = (StanceInfo)EditorGUILayout.ObjectField("Source", sourceStance, typeof(StanceInfo), false);

        EditorGUILayout.Space();

        if (sourceStance == null)
        {
            EditorGUILayout.HelpBox("Please assign a source StanceInfo.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("Copy Basic Moves to All Stances"))
        {
            CopyBasicMoves();
        }
    }

    private void CopyBasicMoves()
    {
        string[] guids = AssetDatabase.FindAssets("t:StanceInfo");

        if (guids.Length == 0)
        {
            Debug.LogWarning("No StanceInfo assets found.");
            return;
        }

        MoveSetData sourceData = sourceStance.ConvertData();

        int modifiedCount = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            StanceInfo targetStance = AssetDatabase.LoadAssetAtPath<StanceInfo>(path);

            if (targetStance != null && targetStance != sourceStance)
            {
               /* MoveSetData targetData = targetStance.ConvertData();
                targetData.basicMoves = JsonUtility.FromJson<BasicMoves>(JsonUtility.ToJson(sourceData.basicMoves));
                targetStance.LoadData(targetData);*/
               targetStance.basicMoves = sourceData.basicMoves;
                
                EditorUtility.SetDirty(targetStance);
                modifiedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Copied basic moves to {modifiedCount} StanceInfo assets.");
    }
}
