using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UFE3D;

namespace BD
{

    public class MainMenuUIManager : MonoBehaviour
    {
        //Buttons
        [SerializeField] Button leagueBTN, trainingBTN, rankingBTN;
        [SerializeField] Button shopBTN, craftBTN, treasureBoxBTN, settingsBTN;
        [SerializeField] Button notificationBTN, refreshBTN, letsduelBTN;

        //Text Fields
        [SerializeField] TextMeshProUGUI playerNameText, fuelText, xetaText, seasonText;
        [SerializeField] TextMeshProUGUI rankText, platinumText, lpText, toNextText, fightText, winText, lostText, drawText, nftNameText;


        //Misc. vars
        int gameMode = -1;
        int fuel = 0;

        #region Unity Functions
        void Start()
        {

            SetGameMode(0);
            print("DO COMMENT/REMOVE ABOVE LINE!!!! & DO USE 'Rectangle 4654- OLd' SPRITE FOR TRAINING MODE DEFAULT SPRITE ON BUTTON");
            int seasonNumber = 01;
            int val1 = 13;
            int val2 = 30;
            string richText = "<size=32.4>" + seasonNumber + "</size>    <size=28.2>" + val1 + "/<size=18.9>" + val2 + "</size></size>";

            seasonText.text = richText;
            ////
        }


        private void OnEnable()
        {
            leagueBTN.onClick.AddListener(() => SetGameMode(0));
            trainingBTN.onClick.AddListener(() => SetGameMode(0)); // ORIGINAL VALUE IS 1 NOT 0 TO TRIGGER TRAINING MODE
            print("DO CHECK ABOVE LINE COMMENT");
            letsduelBTN.onClick.AddListener(() => LetsDuel());
            rankingBTN.onClick.AddListener(() => OpenLayoutPanel("lbPanel"));
            //rankingBTN.onClick.AddListener(() => SetGameMode(2));
        }

        private void OnDisable()
        {
            leagueBTN.onClick.RemoveAllListeners();
            trainingBTN.onClick.RemoveAllListeners();
            //rankingBTN.onClick.AddListener(() => SetGameMode(2));
        }


        #endregion


        #region Custom Functions

        //Open Custom Panels
        public void OpenLayoutPanel(string pid)
        {
            if (pid == "lopanel")
                EventManager.activateLayoutPanel?.Invoke("layoutPanel");
            else if (pid == "lbPanel")
                EventManager.activateLayoutPanel?.Invoke("leaderboardPanel");
        }

        //Setting up game mode for gamemode online = 0 for training mode = 1 
        void SetGameMode(int _gameMode)
        {
            gameMode = _gameMode;
            Debug.Log("Game Mode value: " + gameMode);
        }

        //Match first it will check for fuel then game mode
        void LetsDuel()
        {
            //if (fuel > 0) { } // Will use this condition when actual values are provided

            if (gameMode == 0)
            {
                UFE.StartSearchMatchScreen();
                print("HEREE");
            }
            else if (gameMode == 1)
            {
                UFE.gameMode = GameMode.VersusMode;
                UFE.StartPlayerVersusCpu(); // Attizaz
            }
            else { print("Select GameMode First"); }

        }
        #endregion
    }
}