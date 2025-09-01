using System;
using UnityEngine;

namespace BD
{
    public static class PlayerStats
    {
        internal static Action OnPlayerStatsChanged;

        public static string userName = "-";
        public static int userId = -1;
        public static float xeta = -1;
        public static float fuel = -1;

        internal static int currentDuelID = -1;

        private static int _inBattle = -1; // 0 = false, 1 = true, -1 = not set
        public static int InBattle
        {
            get
            {
                //if (_inBattle == -1)
                //{
                //    Debug.LogWarning("PlayerStats: InBattle has not been set from the server. Using debug value...");
                //    _inBattle = 0;
                //}
                return _inBattle;
            }
            set
            {
                _inBattle = value;
            }
        }

        public static string OpponentWalletAddress { get; set; } = "";
        public static int CurrentDuelId { get; set; } = -1;

        private static int _rank = -1;
        public static int Rank
        {
            get
            {
                if (_rank == -1)
                {
                    Debug.LogWarning("PlayerStats: Rank has not been set from the server. Using debug value...");
                }
                return _rank;
            }
            set
            {
                _rank = value;
            }
        }

        private static int _leagueID = -1;
        internal static int LeagueID
        {
            get
            {
                if (_leagueID == -1)
                {
                    Debug.LogWarning("PlayerStats: League has not been set from the server. Using debug value...");
                    _leagueID = 1;
                }
                return _leagueID;
            }
            set
            {
                _leagueID = value;
            }
        }

        private static string _leagueName = "-";
        internal static string LeagueName
        {
            get
            {
                if (_leagueName == "-")
                {
                    Debug.LogWarning("PlayerStats: LeagueName has not been set from the server. Using debug value...");
                }
                return _leagueName;
            }
            set
            {
                _leagueName = value;
            }
        }

        private static string _leagueSubTier = "";
        internal static string LeagueSubTier
        {
            get
            {
                //if (_leagueSubTier == "")
                //{
                //    Debug.LogWarning("PlayerStats: LeagueSubTier has not been set from the server. Using debug value...");
                //}
                return _leagueSubTier;
            }
            set
            {
                _leagueSubTier = value;
            }
        }

        private static int _leaguePoints = -1;
        public static int LeaguePoints
        {
            get
            {
                if (_leaguePoints == -1)
                {
                    Debug.LogWarning("PlayerStats: LeaguePoints has not been set from the server. Using debug value...");
                }
                return _leaguePoints;
            }
            set
            {
                _leaguePoints = value;
            }
        }

        private static int _pointsToNextLeague = -1;
        public static int PointsToNextLeague
        {
            get
            {
                if (_pointsToNextLeague == -1)
                {
                    Debug.LogWarning("PlayerStats: PointsToNextLeague has not been set from the server. Using debug value...");
                }
                return _pointsToNextLeague;
            }
            set
            {
                _pointsToNextLeague = value;
            }
        }

        private static int _battles = -1;
        public static int Battles
        {
            get
            {
                if (_battles == -1)
                {
                    Debug.LogWarning("PlayerStats: Battles has not been set from the server. Using debug value...");
                }
                return _battles;
            }
            set
            {
                _battles = value;
            }
        }

        private static int _wins = -1;
        public static int Wins
        {
            get
            {
                if (_wins == -1)
                {
                    Debug.LogWarning("PlayerStats: Wins has not been set from the server. Using debug value...");
                }
                return _wins;
            }
            set
            {
                _wins = value;
            }
        }

        private static int _winStreak = -1;
        public static int WinStreak
        {
            get
            {
                if (_winStreak == -1)
                {
                    Debug.LogWarning("PlayerStats: WinStreak has not been set from the server. Using default value...");
                    _winStreak = 0;
                }
                return _winStreak;
            }
            set
            {
                _winStreak = value;
            }
        }

        private static int _losses = -1;
        public static int Losses
        {
            get
            {
                if (_losses == -1)
                {
                    Debug.LogWarning("PlayerStats: Losses has not been set from the server. Using debug value...");
                }
                return _losses;
            }
            set
            {
                _losses = value;
            }
        }

        private static int _draws = -1;
        public static int Draws
        {
            get
            {
                if (_draws == -1)
                {
                    Debug.LogWarning("PlayerStats: Draws has not been set from the server. Using debug value...");
                }
                return _draws;
            }
            set
            {
                _draws = value;
            }
        }

        private static int _season = -1;

        public static int Season
        {
            get
            {
                if (_season == -1)
                {
                    Debug.LogWarning("<color=cyan>PlayerStats:</color> Season has not been set from the server. Using debug value...");
                }
                return _season;
            }
            set
            {
                _season = value;
            }
        }

        public static System.Collections.Generic.List<NFTCharacter> PlayerCharacters { get; set; } = new();
    }
}
