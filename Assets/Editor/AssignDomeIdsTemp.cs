using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AssignDomeIdsTemp : EditorWindow
{
    [MenuItem("Tools/EditorScipts-AP/Assign Dome-id on GameObjects")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AssignDomeIdsTemp));
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Assign Dome-id on Selected GameObjects"))
        {
            NumberSelectedGameObjects();
        }
    }

    private void NumberSelectedGameObjects()
    {
        GameObject[] selectedObjects=Selection.GetFiltered<GameObject>(SelectionMode.Unfiltered);
        //GameObject[] selectedObjects = Selection.gameObjects;

        // Sort objects by name to maintain a consistent order
        //System.Array.Sort(selectedObjects, (a, b) => a.name.CompareTo(b.name));

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            // Assigning numbers incrementally
            selectedObjects[i].GetComponent<SetDomeId>().domeId = int.Parse(selectedObjects[i].name.Split('-')[1]);

        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
