using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BD
{
    public class BreakingDown : MonoBehaviour
    {
        public AudioClip stageMusic;
        void Start()
        {
            //kush
            UFE.StopMusic();
            UFE.PlayMusic(stageMusic);
        }

        private void OnDestroy()
        {
            FindObjectOfType<DefaultMainMenuScreen>().OnShow();//kush
        }
    }
}
