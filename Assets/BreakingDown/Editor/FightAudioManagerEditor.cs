using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.Collections.Generic;

[CustomEditor(typeof(FightAudioManager))]
public class FightAudioManagerEditor : Editor
{
    private AddressableAssetSettings settings;
    private List<AddressableAssetGroup> groups = new();
    private string[] groupNames;
    private int selectedGroupIndex = 0;
    private Vector2 scrollPosGroup;
    private void OnEnable()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        groups.Clear();

        if (settings != null)
        {
            foreach (var group in settings.groups)
            {
                if (group != null && !group.ReadOnly)
                    groups.Add(group);
            }

            groupNames = groups.ConvertAll(g => g.Name).ToArray();
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var manager = (FightAudioManager)target;

        GUILayout.Space(10);
   

        if (groups.Count == 0)
        {
            EditorGUILayout.HelpBox("No Addressable groups found.", MessageType.Info);
            return;
        }

        GUILayout.Label("Select Addressable Group", EditorStyles.boldLabel);


        scrollPosGroup = EditorGUILayout.BeginScrollView(scrollPosGroup, GUILayout.Height(200));

        selectedGroupIndex = GUILayout.SelectionGrid(
            selectedGroupIndex,
            groupNames,
            1, // one item per row
            EditorStyles.radioButton
        );

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        if (GUILayout.Button("Add All Keys from Group"))
        {
            var selectedGroup = groups[selectedGroupIndex];

            int addedCount = 0;
            Undo.RecordObject(manager, "Add Keys From Group");

            foreach (var entry in selectedGroup.entries)
            {
                if (!manager.audioAddressableKeys.Contains(entry.address))
                {
                    manager.audioAddressableKeys.Add(entry.address);
                    addedCount++;
                }
            }

            EditorUtility.SetDirty(manager);

            EditorUtility.DisplayDialog(
                "Keys Added",
                $"{addedCount} new keys were added from group \"{selectedGroup.Name}\".",
                "OK"
            );
        }

    }
}
