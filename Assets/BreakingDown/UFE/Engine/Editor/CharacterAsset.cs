using UnityEngine;
using UnityEditor;
using System;

namespace BD
{
    public class CharacterAsset
    {
        [MenuItem("Assets/Create/U.F.E./Character File")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<UFE3D.CharacterInfo>();
        }
    }
}