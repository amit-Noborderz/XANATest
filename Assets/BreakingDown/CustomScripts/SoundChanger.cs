///Attizaz, using this script for overriding audio source clip on runtime
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BD
{
    public class SoundChanger : MonoBehaviour
    {
        public GameObject WinnerAvatar;
        public RuntimeAnimatorController Controller;
        // Start is called before the first frame update
       /* void Start()
        {
            FightingGameManager.instance.PlayCrowdSound();
            print("On enable changing sound to crowd cheering sound");
        }

        private void OnDisable()
        {
            FightingGameManager.instance.PlayMenuMusic();
            print("On disable changing sound to menu sound");
        }*/

    }
}