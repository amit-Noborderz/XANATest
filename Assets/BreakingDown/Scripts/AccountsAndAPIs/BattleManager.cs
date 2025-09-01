using Photon.Pun;
using Photon.Realtime;
using System.Net.NetworkInformation;
using UFE3D;
using UnityEngine;
using UnityEngine.Events;

namespace BD
{
    public class BattleManager : MonoBehaviour
    {
        #region Fields

        private static ControlsScript localPlayerControlsScript;

        public static BattleStatus lastBattleStatus;
        #endregion

        #region Events

        public enum BattleStatus
        {
            won, lost, draw
        }

        public static UnityAction OnBattleStart;
        public static UnityAction<BattleStatus> OnBattleOver;

        #endregion

        #region Private Methods

        private static void StartUserBattle(ControlsScript player1, ControlsScript player2, StageOptions stage)
        {
            Debug.Log("BattleManager: Battle Started");

            UFE.multiplayerAPI.OnDisconnection += OnDisconnected;

            if (UFE.localPlayerNum == -1) // In case of offline mode
            {
                localPlayerControlsScript = player1;
            }
            else
            {
                if (player1.playerNum == UFE.localPlayerNum)
                {
                    localPlayerControlsScript = player1;
                }
                else
                {
                    localPlayerControlsScript = player2;
                }
            }

            if (UFE.gameMode != GameMode.NetworkGame)
            {
                return;
            }


            foreach (var move in localPlayerControlsScript.MoveSet.attackMoves)
            {
                switch (move.moveName)
                {
                    case "Heavy Punch":
                        move.hits[0]._damageOnHit = BD.PlayerStats.PlayerCharacters[ProfileSelector.Instance.CurrentProfile]
                            .Properties.HP_Damage;
                        break;
                    case "Heavy Kick":
                        move.hits[0]._damageOnHit = BD.PlayerStats.PlayerCharacters[ProfileSelector.Instance.CurrentProfile]
                            .Properties.HK_Damage;
                        break;
                    case "Power Hit":
                        move.hits[0]._damageOnHit = BD.PlayerStats.PlayerCharacters[ProfileSelector.Instance.CurrentProfile]
                            .Properties.SP_Damage;
                        break;
                    default:
                        break;
                }
            }

            foreach (var move in localPlayerControlsScript.target.MoveSet.attackMoves) // TODO: Don't use PlayerCharacters
            {
                switch (move.moveName)
                {
                    case "Heavy Punch":
                        move.hits[0]._damageOnHit = BD.PlayerStats.PlayerCharacters[ProfileSelector.Instance.OpponentProfile]
                            .Properties.HP_Damage;
                        break;
                    case "Heavy Kick":
                        move.hits[0]._damageOnHit = BD.PlayerStats.PlayerCharacters[ProfileSelector.Instance.OpponentProfile]
                            .Properties.HK_Damage;
                        break;
                    case "Power Hit":
                        move.hits[0]._damageOnHit = BD.PlayerStats.PlayerCharacters[ProfileSelector.Instance.OpponentProfile]
                            .Properties.SP_Damage;
                        break;
                    default:
                        break;
                }
            }

            OnBattleStart?.Invoke();
        }


        private bool showDebugGUI = false;

        private void OnGUI()
        {
            GUI.skin.label.fontSize = 40;
            /*GUI.Label(new Rect(Screen.width - 200 - 10, Screen.height - 100, 200, 100),
                $"Ping: {PhotonNetwork.GetPing()}");


            GUI.skin.button.fontSize = 40;
            if (GUI.Button(new Rect((Screen.width / 2) - 100, Screen.height - 80, 200, 70), "Stats"))
            {
                showDebugGUI = !showDebugGUI;
            }
*/
            if (!showDebugGUI) return;

            if (localPlayerControlsScript == null) return;


            GUI.skin.label.fontSize = 50;
            GUI.Label(new Rect(10, 10, Screen.width / 2, 100),
                $"My Player: {localPlayerControlsScript.playerNum}, " +
                $"Profile: {BD.PlayerStats.PlayerCharacters[ProfileSelector.Instance.CurrentProfile].Profile}, " +
                $"TokenID: {BD.PlayerStats.PlayerCharacters[ProfileSelector.Instance.CurrentProfile].TokenID}");

            foreach (var move in localPlayerControlsScript.MoveSet.attackMoves)
            {
                switch (move.moveName)
                {
                    case "Heavy Punch":
                        GUI.Label(new Rect(10, 110, 900, 100),
                            $"Heavy Punch: {move.hits[0]._damageOnHit}");
                        break;
                    case "Heavy Kick":
                        GUI.Label(new Rect(10, 210, 900, 100),
                            $"Heavy Kick: {move.hits[0]._damageOnHit}");
                        break;
                    case "Power Hit":
                        GUI.Label(new Rect(10, 310, 900, 100),
                            $"Power Hit: {move.hits[0]._damageOnHit}");
                        break;
                    default:
                        break;
                }
            }


            if (localPlayerControlsScript.target == null) return;

            GUI.color = Color.red;
            GUI.Label(new Rect((Screen.width / 2) + 10, 10, Screen.width / 2, 100),
                $"Other Player: {localPlayerControlsScript.target.playerNum}, " +
                $"Profile: {BD.PlayerStats.PlayerCharacters[ProfileSelector.Instance.OpponentProfile].Profile}, " +
                $"TokenID: {BD.PlayerStats.PlayerCharacters[ProfileSelector.Instance.OpponentProfile].TokenID}");

            foreach (var move in localPlayerControlsScript.target.MoveSet.attackMoves)
            {
                switch (move.moveName)
                {
                    case "Heavy Punch":
                        GUI.Label(new Rect((Screen.width / 2) + 10, 110, 900, 100),
                            $"Heavy Punch: {move.hits[0]._damageOnHit}");
                        break;
                    case "Heavy Kick":
                        GUI.Label(new Rect((Screen.width / 2) + 10, 210, 900, 100),
                            $"Heavy Kick: {move.hits[0]._damageOnHit}");
                        break;
                    case "Power Hit":
                        GUI.Label(new Rect((Screen.width / 2) + 10, 310, 900, 100),
                            $"Power Hit: {move.hits[0]._damageOnHit}");
                        break;
                    default:
                        break;
                }
            }
        }

        private static void EndUserBattle(ControlsScript winner, ControlsScript loser)
        {
            //Debug.LogError($"<color=green>BattleManager:</color> BattleEnded: winner: {winner}, loser: {loser}");

            if (winner == null)
            {
                lastBattleStatus = BattleStatus.draw;
            }
            else
            {
                if (winner == localPlayerControlsScript)
                {
                    lastBattleStatus = BattleStatus.won;
                }
                else
                {
                    lastBattleStatus = BattleStatus.lost;
                }
            }

            OnBattleOver?.Invoke(lastBattleStatus);
        }

        private static void OnDisconnected()
        {
            // Handle disconnection
        }

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            UFE.OnGameBegin += StartUserBattle;
            UFE.OnGameEnds += EndUserBattle;
        }

        private void OnDisable()
        {
            UFE.OnGameBegin -= StartUserBattle;
            UFE.OnGameEnds -= EndUserBattle;
        }

        #endregion
    }
}