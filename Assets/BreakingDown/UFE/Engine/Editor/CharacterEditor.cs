using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
namespace BD
{

	[CustomEditor(typeof(UFE3D.CharacterInfo))]
	public class CharacterEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Open Character Editor"))
				CharacterEditorWindow.Init();

		}
	}
}