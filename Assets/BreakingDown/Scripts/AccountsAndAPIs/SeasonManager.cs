using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.InputSystem;
using System;

namespace BD
{
    public class SeasonManager : MonoBehaviour
    {
        // Leagues: Trial, Bronze, Silver, Gold, Elite
        // LP (League Points) are used to determine the player's rank in the league.
        // 0-100 LP: Trial
        // 101-200 LP: Bronze
        // 201-300 LP: Silver
        // 301-400 LP: Gold
        // 401-500 LP: Elite
        // Players can earn LP by winning battles and lose LP by losing battles.
        // Players can earn more LP by defeating opponents with higher LP.

        // Fuel required to play any battle. Both players must pay the entry fuel.
        // 10% of the entry fuel collected as a system fee
        // Amount of fuel gained in case of victory(70%-130%)
        // More LP is gained by maintaining a winning streak.


        // Seasons: Start and end dates may vary, but the number of days in the season is the same
        // Seasonal Rewards Distribution Method: Treasure Boxes for the top 30%. Special Treasure Boxes for a smaller percentage.
        // Reset LP, etc. to 0 at the start of the next season.

        #region League

        private enum League
        {
            Trial,
            Bronze,
            Silver,
            Gold,
            Elite
        }
        private static League _currentLeague;


        private static int _currentLeaguePoints = -1;
        public static int CurrentLeaguePoints
        {
            get
            {
                if (_currentLeaguePoints == -1)
                {
                    // get from server
                    return 0;
                }
                else
                {
                    return _currentLeaguePoints;
                }
            }
            set
            {
                _currentLeaguePoints = value;
                UpdateLeague();
            }
        }

        private static void UpdateLeague()
        {
            if (_currentLeaguePoints >= 0 && _currentLeaguePoints <= 100)
            {
                _currentLeague = League.Trial;
            }
            else if (_currentLeaguePoints >= 101 && _currentLeaguePoints <= 200)
            {
                _currentLeague = League.Bronze;
            }
            else if (_currentLeaguePoints >= 201 && _currentLeaguePoints <= 300)
            {
                _currentLeague = League.Silver;
            }
            else if (_currentLeaguePoints >= 301 && _currentLeaguePoints <= 400)
            {
                _currentLeague = League.Gold;
            }
            else if (_currentLeaguePoints >= 401 && _currentLeaguePoints <= 500)
            {
                _currentLeague = League.Elite;
            }
        }

        private void BattleStart()
        {
            // Deduct fuel on server
            FuelManager.CurrentFuel -= 1;
        }

        private void BattleOver(BattleManager.BattleStatus status)
        {
            if (status == BattleManager.BattleStatus.won)
            {
                CurrentLeaguePoints += 10;
            }
            else if (status == BattleManager.BattleStatus.lost)
            {
                CurrentLeaguePoints -= 10;
            }
            else
            {
                // draw
            }
        }


        #endregion

        #region Unity Callbacks

        // private void OnEnable()
        // {
        //     //BattleManager.OnBattleStart += BattleStart;
        //     //BattleManager.OnBattleOver += BattleOver;
        // }

        // private void OnDisable()
        // {
        //     //BattleManager.OnBattleStart -= BattleStart;
        //     //BattleManager.OnBattleOver -= BattleOver;
        // }

        #endregion
    }
}