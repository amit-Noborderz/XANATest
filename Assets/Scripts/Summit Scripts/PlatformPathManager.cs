using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class PlatformPathManager : MonoBehaviour
{
    public string Json; // Public variable to hold the JSON content

    [System.Serializable]
    public class ChildObjectData
    {
        public string name;
        public Vector3 position;
        public Vector3 rotation;
    }

    [System.Serializable]
    public class ChildObjectsContainer
    {
        public List<ChildObjectData> children = new List<ChildObjectData>();
    }

    public void SaveChildObjectsToJson()
    {
        // Create a container for child objects
        ChildObjectsContainer container = new ChildObjectsContainer();

        // Iterate through all child objects
        foreach (Transform child in transform)
        {
            ChildObjectData data = new ChildObjectData
            {
                name = child.name,
                position = child.localPosition,
                rotation = child.localEulerAngles
            };

            container.children.Add(data);
        }

        // Convert the container to JSON (single-line format)
        string json = JsonUtility.ToJson(container, false);

        // Save the JSON to a file in the persistent data path
        string path = Path.Combine(Application.persistentDataPath, "PlatformPath.json");
        File.WriteAllText(path, json);

        // Copy the JSON content to the public variable
        Json = json;

        // Open the file location in the file explorer
        EditorUtility.RevealInFinder(path);

        Debug.Log($"Child objects saved to JSON at: {path}");
    }
}
[CustomEditor(typeof(PlatformPathManager))]
public class ChildObjectsToJsonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Add a button to the Inspector
        if (GUILayout.Button("Save Child Objects to JSON"))
        {
            // Get the target script
            PlatformPathManager script = (PlatformPathManager)target;

            // Call the method to save child objects to JSON
            script.SaveChildObjectsToJson();
        }
    }
}
#endif
