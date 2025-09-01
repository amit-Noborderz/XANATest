using BD;
using UFE3D;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BD
{
    public class SetWinLoseUI : MonoBehaviour
    {
        [SerializeField] Image[] _resultImages; // 0: Win, 1: Lose, 2: Draw
        [SerializeField] TextMeshProUGUI _winLoseText, _lpText;



        private void Start()
        {
            if (UFE.gameMode != GameMode.NetworkGame)
            {
                WinLoseOffline(BattleManager.lastBattleStatus);
            }
            else
            {
                WinLoseOnline(BattleManager.lastBattleStatus);
            }
        }

        public void WinLoseOffline(BattleManager.BattleStatus battleResult)
        {
            _winLoseText.gameObject.SetActive(false);
            _lpText.gameObject.SetActive(false);

            //string winText;
            for (int i = 0; i < _resultImages.Length; i++)
            {
                _resultImages[i].gameObject.SetActive(false);
            }

            if (battleResult == BattleManager.BattleStatus.won)
            {
                _resultImages[0].gameObject.SetActive(true);
                //winText = "YOU GOT LP !";
                //_lpText.text = $"0";
            }
            else if (battleResult == BattleManager.BattleStatus.lost)
            {
                _resultImages[1].gameObject.SetActive(true);
                //winText = "YOU LOSE LP !";
                //_lpText.text = $"0";
            }
            else
            {
                _resultImages[2].gameObject.SetActive(true);
                //winText = "YOUR MATCH IS DRAW";
                //_lpText.text = $"0";
            }
            //_winLoseText.text = winText;
        }

        public void WinLoseOnline(BattleManager.BattleStatus battleResult)
        {
            _winLoseText.gameObject.SetActive(true);
            _lpText.gameObject.SetActive(true);

            string winText;

            for (int i = 0; i < _resultImages.Length; i++)
            {
                _resultImages[i].gameObject.SetActive(false);
            }

            if (battleResult == BattleManager.BattleStatus.won)
            {
                _resultImages[0].gameObject.SetActive(true);
                winText = "YOU GOT LP !";
                _lpText.text = $"+ {20}"; // TODO: needs to fetched from API.
            }
            else if (battleResult == BattleManager.BattleStatus.lost)
            {
                _resultImages[1].gameObject.SetActive(true);
                winText = "YOU LOSE LP !";

                if (BD.PlayerStats.LeaguePoints == 0)
                {
                    _lpText.text = "0";
                }
                else
                {
                    _lpText.text = $"- {10}"; // TODO: needs to fetched from API.
                }
            }
            else
            {
                _resultImages[2].gameObject.SetActive(true);
                winText = "YOUR MATCH IS DRAW";
                _lpText.text = "0"; // TODO: needs to fetched from API.
            }
            _winLoseText.text = winText;
        }
    }
}