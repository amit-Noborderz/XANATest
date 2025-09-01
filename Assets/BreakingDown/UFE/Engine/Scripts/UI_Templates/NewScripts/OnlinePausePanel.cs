//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

namespace BD
{
    public class OnlinePausePanel : MonoBehaviour // Muneeb: Probably unused. Should be removed.
    {
        public void GoToMainMenu()
        {
            UFE.EndGame();
            UFE.PauseGame(false);
            UFE.StartMainMenuScreen();
            UFE.MyPlayerDisconnectedFromMatch();
            //   FightingGameManager.instance.RestartScene();
            Destroy(this.gameObject);
        }

        public void ResumeGame()
        {
            //OnlineExit scriptInstance = FindObjectOfType<OnlineExit>();
            UFE.touchControllerBridge.ShowBattleControls(true, true);
            //if (scriptInstance != null)
            //{
            //    scriptInstance.CheckMode();
            //}
            Destroy(this.gameObject);
        }
    }
}