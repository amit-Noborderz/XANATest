using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
public class gizmoHelper : MonoBehaviour
{
    public float radius = 0.1f;
    GUIStyle style = new GUIStyle();
    [SerializeField] Color color = Color.black;
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, radius);
        style.normal.textColor = color;
        // Handles.Label(transform.position, transform.name, style);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, radius);
    }
}
#endif