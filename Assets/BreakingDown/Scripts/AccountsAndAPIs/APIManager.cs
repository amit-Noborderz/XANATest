using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace BD
{
    public class APIManager : MonoBehaviour
    {
        internal static bool IsMainNet => APIBasepointManager.instance.IsXanaLive;

        private static readonly string BD_TESTNET_API_BASE = "https://breaking-down-stg.xana.net/api/";
        private static readonly string BD_MAINNET_API_BASE = "https://breaking-down-stg.xana.net/api/";

        internal static string BD_API_BASE => IsMainNet ? BD_MAINNET_API_BASE : BD_TESTNET_API_BASE;

        private static string BD_StartUserBattle_API => BD_API_BASE + "battle/start-user-duel";
        private static string BD_EndUserBattle_API => BD_API_BASE + "battle/end-user-duel";
        private static string BD_ForceEndBattle_API => BD_API_BASE + "battle/force-end-battle";
        private static string BD_GetUserInBattleByWallet_API => BD_API_BASE + "users/get-user-in-duel/:wallet";

        public static string BD_GetAllUserGlobalRanking_API => BD_API_BASE + "users/get-users-global-rank";

        public static User user = new();
        public static bool WalletLoginTap;

        public static string WalletAddress
        {
            get
            {
                return PlayerPrefs.GetString("publicID", "");
            }
            set
            {
                PlayerPrefs.SetString("publicID", value);
            }
        }

        public static string AUTH_TOKEN
        {
            get
            {
                return PlayerPrefs.GetString("AUTH_TOKEN", "");
            }
            set
            {
                PlayerPrefs.SetString("AUTH_TOKEN", value);
            }
        }

        #region Battle related stuff

        #region StartUserBattle

        private void StartUserBattle()
        {
            Debug.Log("<color=cyan>APIManager:</color> StartUserBattle()");

            if (!PhotonNetwork.InRoom)
            {
                Debug.LogError("<color=cyan>APIManager:</color> Not an online p2p match.");
                return;
            }

            UFE.multiplayerAPI.OnDisconnection += OnDisconnected;

            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.Log("<color=cyan>APIManager:</color> Only master client can start/end battles.");
                return;
            }


            if (string.IsNullOrEmpty(AUTH_TOKEN))
            {
                Debug.LogError("<color=cyan>APIManager:</color> Auth Token is not available. Please login first.");
                return;
            }

            StartCoroutine(StartUserBattleCor());
        }

        private IEnumerator StartUserBattleCor()
        {
            WWWForm form = new();
            form.AddField("roomId", PhotonNetwork.CurrentRoom.Name);
            form.AddField("userId", PlayerStats.userId.ToString());
            form.AddField("oponentId", FightingGameManager.instance.OpponentID.ToString());

            Debug.Log($"<color=cyan>APIManager:</color> Auth Token: {AUTH_TOKEN}, " +
                $"RoomID: {PhotonNetwork.CurrentRoom.Name}, " +
                $"UserID: {PlayerStats.userId}, " +
                $"OpponentID: {FightingGameManager.instance.OpponentID}");

            UnityWebRequest request = UnityWebRequest.Post(BD_StartUserBattle_API, form);

            request.SetRequestHeader("Authorization", $"{AUTH_TOKEN}");

            // It's unusual to manually set the Content-Type for WWWForm, as Unity sets it automatically.
            // If needed, use the correct type, e.g., "application/x-www-form-urlencoded"


            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(request.error);
                Debug.LogError(request.downloadHandler.text);
            }
            else
            {
                //Debug.Log("Response: " + request.downloadHandler.text);

                if (request.downloadHandler.text.Contains("<!DOCTYPE html>"))
                {
                    Debug.LogError("<color=cyan>APIManager:</color> Possible IP blocked issue.");
                    Debug.LogError("<color=cyan>APIManager:</color> " + request.downloadHandler.text);
                    yield break;
                }

                StartBattleResponse response = JsonUtility.FromJson<StartBattleResponse>(request.downloadHandler.text);

                if (response.success)
                {
                    Debug.LogError("<color=cyan>APIManager:</color> Battle data submitted successfully");
                    PlayerStats.currentDuelID = response.data.userDuelId;

                    Debug.LogError($"<color=cyan>APIManager:</color> Duel ID: {PlayerStats.currentDuelID}");
                }
                else
                {
                    Debug.LogError("<color=cyan>APIManager:</color> Failed to submit battle data");
                    Debug.LogError("<color=cyan>APIManager:</color> " + response.msg);
                    // need to close this battle and show a message to the user
                }
            }
        }

        [Serializable]
        class StartBattleResponse
        {
            public bool success;
            public StartBattleResponseData data;
            public string msg;
        }

        [Serializable]
        class StartBattleResponseData
        {
            public int userDuelId;
        }

        #endregion

        #region EndUserBattle

        private void EndUserBattle(BattleManager.BattleStatus status)
        {
            if (!PhotonNetwork.InRoom)
            {
                Debug.LogError("<color=cyan>APIManager:</color> Not an online p2p match.");
                return;
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.Log("<color=cyan>APIManager:</color> Only master client can start/end battles.");
                return;
            }

            if (string.IsNullOrEmpty(AUTH_TOKEN))
            {
                Debug.LogError("<color=cyan>APIManager:</color> Auth Token is not available. Please login first.");
                return;
            }

            if (status == BattleManager.BattleStatus.won)
            {
                StartCoroutine(EndUserBattleCor("WIN", "LOOSE"));
            }
            else if (status == BattleManager.BattleStatus.lost)
            {
                StartCoroutine(EndUserBattleCor("LOOSE", "WIN"));
            }
            else
            {
                StartCoroutine(EndUserBattleCor("DRAW", "DRAW"));
            }

        }

        private IEnumerator EndUserBattleCor(string p1_battleStatus = "DRAW", string p2_battleStatus = "DRAW")
        {
            WWWForm form = new();
            form.AddField("dualId", PlayerStats.currentDuelID);
            form.AddField("p1_id", PlayerStats.userId);
            form.AddField("p2_id", FightingGameManager.instance.OpponentID);
            form.AddField("p1_battleStatus", p1_battleStatus);
            form.AddField("p2_battleStatus", p2_battleStatus);

            Debug.Log($"<color=cyan>APIManager:</color> Auth Token: {AUTH_TOKEN}\n" +
                           $"DuelID: {PlayerStats.currentDuelID}\n" +
                           $"P1_ID: {PlayerStats.userId}\n" +
                           $"P2_ID: {FightingGameManager.instance.OpponentID}\n" +
                           $"P1_BattleStatus: {p1_battleStatus}\n" +
                           $"P2_BattleStatus: {p2_battleStatus}");

            UnityWebRequest request = UnityWebRequest.Post(BD_EndUserBattle_API, form);

            request.SetRequestHeader("Authorization", $"{AUTH_TOKEN}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("<color=cyan>APIManager:</color> " + request.error);
                Debug.LogError("<color=cyan>APIManager:</color> " + request.downloadHandler.text);
            }
            else
            {
                //Debug.Log("Response: " + request.downloadHandler.text);

                if (request.downloadHandler.text.Contains("<!DOCTYPE html>"))
                {
                    Debug.LogError("<color=cyan>APIManager:</color> Possible IP blocked issue.");
                    Debug.LogError("<color=cyan>APIManager:</color> " + request.downloadHandler.text);
                    yield break;
                }

                Debug.Log("<color=yellow>APIManager:</color> EndUserBattle Response: " + request.downloadHandler.text);
                EndBattleResponse response = JsonUtility.FromJson<EndBattleResponse>(request.downloadHandler.text);

                if (response.success)
                {
                    Debug.LogError("<color=cyan>APIManager:</color> Battle Ended successfully");
                }
                else
                {
                    Debug.LogError("<color=cyan>APIManager:</color> Failed to End Battle");
                    Debug.LogError("<color=cyan>APIManager:</color> " + response.message);
                }
            }
        }

        [Serializable]
        class EndBattleResponse
        {
            public bool success;
            public object data;
            public string reason;
            public object error;
            public string message;
        }

        #endregion

        #region ForceEndUserBattle

        //internal static void ForceEndUserBattle()
        //{
        //    if (string.IsNullOrEmpty(AUTH_TOKEN))
        //    {
        //        Debug.LogError("<color=cyan>APIManager:</color> Auth Token is not available. Please login first.");
        //        return;
        //    }

        //    ForceEndUserBattleAsync();
        //}

        internal static async void ForceEndUserBattleAsync(UnityAction<bool> callback)
        {
            if (string.IsNullOrEmpty(AUTH_TOKEN))
            {
                Debug.LogError("<color=cyan>APIManager:</color> Auth Token is not available. Please login first.");
                return;
            }

            WWWForm form = new();
            form.AddField("dualId", PlayerStats.CurrentDuelId);
            form.AddField("userWallet", WalletAddress);
            form.AddField("oponentWallet", PlayerStats.OpponentWalletAddress);

            UnityWebRequest request = UnityWebRequest.Post(BD_ForceEndBattle_API, form);

            request.SetRequestHeader("Authorization", $"{AUTH_TOKEN}");

            await request.SendWebRequest();

            Debug.Log("<color=cyan>APIManager:</color> ForceEndUserBattle Response: " + request.downloadHandler.text);

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("<color=cyan>APIManager:</color> ForceEndUserBattle() " + request.error);
                Debug.LogError("<color=cyan>APIManager:</color> ForceEndUserBattle() " + request.downloadHandler.text);
                callback?.Invoke(false);
            }
            else
            {
                if (request.downloadHandler.text.Contains("<!DOCTYPE html>"))
                {
                    Debug.LogError("<color=cyan>APIManager:</color> ForceEndUserBattle() Possible IP blocked issue.");
                    Debug.LogError("<color=cyan>APIManager:</color> ForceEndUserBattle() " + request.downloadHandler.text);
                    callback?.Invoke(false);
                    return;
                }

                ForceEndUserBattleResponse response = JsonUtility.FromJson<ForceEndUserBattleResponse>(request.downloadHandler.text);

                if (response.success)
                {
                    Debug.Log("<color=yellow>APIManager:</color> Battle ended forcefully");
                    callback?.Invoke(true);

                    /* Sample Response
                    
                    HTTP/1.1 400 Bad Request
                    {
                      "success": false,
                      "data": null,
                      "message": "Invalid Duel Id."
                    }
                    
                    */
                }
                else
                {
                    Debug.LogError("<color=cyan>APIManager:</color> Failed to forcefully end battle: " + response.message);
                    callback?.Invoke(false);
                }
            }
        }

        [Serializable]
        class ForceEndUserBattleResponse
        {
            public bool success;
            public object data;
            public string message;
        }

        #endregion

        #region Disconnected

        private void OnDisconnected() // other player disconnected
        {
            Debug.LogError("<color=cyan>APIManager:</color> OnDisconnected(): Opponent disconnected.");

            // TODO: check if we are in a battle

            EndUserBattle(BattleManager.BattleStatus.won);
        }

        #endregion

        #endregion

        #region Account and User Details

        #region GetUserDetails

        //private static string BD_GetUserDetails_API => BD_API_BASE + "users/get-user-details";

        //internal void GetUserDetails()
        //{
        //    if (string.IsNullOrEmpty(AUTH_TOKEN))
        //    {
        //        Debug.LogError("<color=cyan>APIManager:</color> Auth Token is not available. Please login first.");
        //        return;
        //    }

        //    if (!string.IsNullOrEmpty(WalletAddress))
        //    {
        //        // To avoid repeated calls to the API
        //        //GetUserDetailsByWalletAddress();
        //    }

        //    StartCoroutine(GetUserDetailsCor());
        //}

        //private IEnumerator GetUserDetailsCor()
        //{
        //    UnityWebRequest request = UnityWebRequest.Get(BD_GetUserDetails_API);

        //    request.SetRequestHeader("Authorization", $"{AUTH_TOKEN}");

        //    yield return request.SendWebRequest();

        //    if (request.result == UnityWebRequest.Result.ConnectionError)
        //    {
        //        Debug.LogError(request.error);
        //        Debug.LogError(request.downloadHandler.text);
        //    }
        //    else
        //    {
        //        // Debug.Log("GetUserDetails Response: " + request.downloadHandler.text);

        //        if (request.downloadHandler.text.Contains("<!DOCTYPE html>"))
        //        {
        //            Debug.LogError("<color=cyan>APIManager:</color> Possible IP blocked issue.");
        //            Debug.LogError("<color=cyan>APIManager:</color> " + request.downloadHandler.text);
        //            yield break;
        //        }

        //        GetUserDetailsResponse response = JsonUtility.FromJson<GetUserDetailsResponse>(request.downloadHandler.text);

        //        if (response.success)
        //        {
        //            Debug.Log("<color=cyan>APIManager:</color> Got user details successfully");
        //            FuelManager.hasMinimumFuel = response.data.isHaveMinFuel;
        //            WalletAddress = response.data.walletAddress;
        //            Debug.Log($"<color=cyan>APIManager:</color> Wallet Address: {WalletAddress}");

        //            //GetUserDetailsByWalletAddress();
        //        }
        //        else
        //        {
        //            Debug.LogError("<color=cyan>APIManager:</color> Failed to get user details");
        //            Debug.LogError(response.message);
        //        }
        //    }

        //    /*

        //    {
        //        "success": true,
        //        "data":
        //        {
        //            "walletAddress": "0x1cf96764a5fb67c5ba2c5fe98cae9772d818a01d",
        //            "id": 9948,
        //            "name": "helicopter",
        //            "dob": null,
        //            "phoneNumber": null,
        //            "email": "gigroffagufri-7515@yopmail.com",
        //            "avatar": null,
        //            "tcgAvatar": null,
        //            "aiStatus": null,
        //            "role": 2,
        //            "coins": "0.00",
        //            "isHaveMinFuel": false,
        //            "cryptoEmailVerify": null,
        //            "tcg_first_login": true,
        //            "userLeagues": []
        //        },
        //        "msg": "User details fetched successfully"
        //    }

        //    */
        //}

        //[Serializable]
        //class GetUserDetailsResponse
        //{
        //    public bool success;
        //    public GetUserDetailsData data;
        //    public string message;
        //}

        //[Serializable]
        //class GetUserDetailsData
        //{
        //    public string walletAddress;
        //    public int id;
        //    public string name;
        //    public object dob;
        //    public object phoneNumber;
        //    public string email;
        //    public object avatar;
        //    public object tcgAvatar;
        //    public object aiStatus;
        //    public int role;
        //    public string coins;
        //    public bool isHaveMinFuel;
        //    public object cryptoEmailVerify;
        //    public bool tcg_first_login;
        //    public object userLeagues;
        //}

        #endregion

        #region GetMyDetails

        private static string BD_GetMyDetails_API => BD_API_BASE + "users/me";

        internal static async void GetMyDetails(UnityAction<bool> callback = null)
        {
            if (string.IsNullOrEmpty(AUTH_TOKEN))
            {
                Debug.LogError("<color=cyan>APIManager.GetMyDetails:</color> Auth Token is not available. Please login first.");
                callback?.Invoke(false);
                return;
            }

            UnityWebRequest request = UnityWebRequest.Get(BD_GetMyDetails_API);

            request.SetRequestHeader("Authorization", $"{AUTH_TOKEN}");

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("<color=cyan>APIManager.GetMyDetails</color> " + request.error);
                Debug.LogError("<color=cyan>APIManager.GetMyDetails</color> " + request.downloadHandler.text);
                callback?.Invoke(false);
                return;
            }
            else
            {
                Debug.Log("<color=cyan>APIManager.GetMyDetails</color> Response: " + request.downloadHandler.text);

                if (request.downloadHandler.text.Contains("<!DOCTYPE html>"))
                {
                    Debug.LogError("<color=cyan>APIManager.GetMyDetails:</color> Possible IP blocked issue.");
                    Debug.LogError("<color=cyan>APIManager.GetMyDetails:</color> " + request.downloadHandler.text);
                    callback?.Invoke(false);
                    return;
                }

                GetMyDetailsResponse response = JsonUtility.FromJson<GetMyDetailsResponse>(request.downloadHandler.text);

                if (response.success)
                {
                    FuelManager.hasMinimumFuel = response.data.isHaveMinFuel;
                    WalletAddress = response.data.walletAddress;

                    //
                    ////Debug.Log("<color=cyan>APIManager.GetMyDetails:</color> GetMyDetails success.");
                    ////FuelManager.CurrentFuel = int.Parse(response.data.battle_data.fuel);
                    ////PlayerStats.fuel = float.Parse(response.data.battle_data.fuel);
                    ////XetaManager.CurrentXeta = int.Parse(response.data.battle_data.xeta);
                    ////PlayerStats.xeta = float.Parse(response.data.battle_data.xeta);

                    ////PlayerStats.userName = response.data.userName;
                    ////PlayerStats.userId = response.data.userId;

                    ////PlayerStats.Rank = response.data.battle_data.rank;
                    ////PlayerStats.LeagueID = response.data.league_data.leagueId;
                    ////PlayerStats.LeaguePoints = response.data.battle_data.leaguePoints;
                    ////PlayerStats.PointsToNextLeague = int.Parse(response.data.league_data.pointsToNextLeague);
                    ////PlayerStats.Battles = response.data.battle_data.battles;
                    ////PlayerStats.Wins = response.data.battle_data.wins;
                    ////PlayerStats.Losses = response.data.battle_data.loses;
                    ////PlayerStats.Draws = response.data.battle_data.draws;


                    ////if (int.TryParse(response.data.battle_data.winStreak, out int winStreak))
                    ////{
                    ////    PlayerStats.WinStreak = winStreak;
                    ////}
                    ////else
                    ////{
                    ////    Debug.LogWarning("<color=yellow>APIManager.GetMyDetails:</color> Error parsing winStreak: " +
                    ////        response.data.battle_data.winStreak);
                    ////}

                    ////var (leagueName, leagueSubTier) = GetLeagueNameAndSubTier(response.data.league_data.leagueName);

                    ////if (!string.IsNullOrEmpty(leagueName))
                    ////{
                    ////    PlayerStats.LeagueName = leagueName;
                    ////    PlayerStats.LeagueSubTier = leagueSubTier;
                    ////}
                    ////else
                    ////{
                    ////    Debug.LogWarning($"<color=yellow>APIManager.GetMyDetails:</color> Error parsing leagueName: " +
                    ////        $"{response.data.league_data.leagueName}. Setting default league");
                    ////    PlayerStats.LeagueName = "League";
                    ////}

                    //if (Enum.TryParse(response.data.league_data.leagueName, out League lg))
                    //{
                    //    PlayerStats.LeagueName = lg.ToString();
                    //}
                    //else
                    //{
                    //    Debug.LogWarning($"<color=yellow>APIManager:</color> Error parsing leagueName: " +
                    //        $"{response.data.league_data.leagueName}. Setting default league");
                    //    PlayerStats.LeagueName = "League";
                    //}

                    PlayerStats.OnPlayerStatsChanged?.Invoke();

                    await GetUserInBattleByWalletAddress();
                    callback?.Invoke(true);
                }
                else
                {
                    Debug.LogError("<color=cyan>APIManager.GetMyDetails:</color> Failed to my details");
                    Debug.LogError(response.msg);
                    callback?.Invoke(false);
                    return;
                }
            }
        }


        /*

        {
            "success": true,
            "data":
            {
                "walletAddress": "0x1cf96764a5fb67c5ba2c5fe98cae9772d818a01d",
                "id": 9948,
                "name": "helicopter",
                "dob": null,
                "phoneNumber": null,
                "email": "gigroffagufri-7515@yopmail.com",
                "avatar": null,
                "tcgAvatar": null,
                "aiStatus": null,
                "role": 2,
                "coins": "0.00",
                "isHaveMinFuel": false,
                "cryptoEmailVerify": null,
                "tcg_first_login": true,
                "userLeagues": []
            },
            "msg": "User details fetched successfully"
        }

        */

        [Serializable]
        class GetMyDetailsResponse
        {
            public bool success;
            public GetMyDetailsData data;
            public string msg;

            [Serializable]
            public class GetMyDetailsData
            {
                public string walletAddress;
                public int id;
                public string name;
                public object dob;
                public object phoneNumber;
                public string email;
                public object avatar;
                public object tcgAvatar;
                public object aiStatus;
                public int role;
                public string coins;
                public bool isHaveMinFuel;
                public object cryptoEmailVerify;
                public bool tcg_first_login;
                public object userLeagues;
            }
        }

        #endregion

        #region GetUserDetailsByWalletAddress

        private static string BD_GetUserDetailsByWallet_API => BD_API_BASE + "users/get-user/:wallet";

        internal static async Task GetUserDetailsByWallet(UnityAction<bool> callback = null)
        {
            if (string.IsNullOrEmpty(AUTH_TOKEN))
            {
                Debug.LogError("<color=cyan>APIManager:</color> GetUserDetailsByWallet(): Auth Token is not available. Please login first.");
                callback?.Invoke(false);
                return;
            }

            UnityWebRequest request = UnityWebRequest.Get(BD_GetUserDetailsByWallet_API);

            request.SetRequestHeader("Authorization", $"{AUTH_TOKEN}");

            // add parameter wallet address to the request
            request.url = request.url.Replace(":wallet", WalletAddress);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(request.error);
                Debug.LogError(request.downloadHandler.text);
                callback?.Invoke(false);
                return;
            }
            else
            {
                //Debug.Log("<color=cyan>APIManager:</color> GetUserDetailsByWallet Response: " + request.downloadHandler.text);

                if (request.downloadHandler.text.Contains("<!DOCTYPE html>"))
                {
                    Debug.LogError("<color=cyan>APIManager:</color> Possible IP blocked issue.");
                    Debug.LogError(request.downloadHandler.text);
                    callback?.Invoke(false);
                    return;
                }

                GetUserDetailsByWalletResponse response = JsonUtility.FromJson<GetUserDetailsByWalletResponse>(request.downloadHandler.text);

                if (response.success)
                {
                    Debug.Log("<color=cyan>APIManager:</color> Got user details by wallet successfully.");
                    FuelManager.CurrentFuel = int.Parse(response.data.battle_data.fuel);
                    PlayerStats.fuel = float.Parse(response.data.battle_data.fuel);
                    XetaManager.CurrentXeta = int.Parse(response.data.battle_data.xeta);
                    PlayerStats.xeta = float.Parse(response.data.battle_data.xeta);

                    PlayerStats.userName = response.data.userName;
                    PlayerStats.userId = response.data.userId;

                    PlayerStats.Rank = response.data.battle_data.rank;
                    PlayerStats.LeagueID = response.data.league_data.leagueId;
                    PlayerStats.LeaguePoints = response.data.battle_data.leaguePoints;
                    PlayerStats.PointsToNextLeague = int.Parse(response.data.league_data.pointsToNextLeague);
                    PlayerStats.Battles = response.data.battle_data.battles;
                    PlayerStats.Wins = response.data.battle_data.wins;
                    PlayerStats.Losses = response.data.battle_data.loses;
                    PlayerStats.Draws = response.data.battle_data.draws;


                    Debug.Log($"<color=cyan>APIManager:</color> Win Streak: {response.data.battle_data.winStreak}");
                    if (int.TryParse(response.data.battle_data.winStreak, out int winStreak))
                    {
                        PlayerStats.WinStreak = winStreak;
                    }
                    else
                    {
                        Debug.LogWarning("<color=yellow>APIManager:</color> Error parsing winStreak: " +
                            response.data.battle_data.winStreak);
                    }

                    var (leagueName, leagueSubTier) = GetLeagueNameAndSubTier(response.data.league_data.leagueName);

                    if (!string.IsNullOrEmpty(leagueName))
                    {
                        PlayerStats.LeagueName = leagueName;
                        PlayerStats.LeagueSubTier = leagueSubTier;
                    }
                    else
                    {
                        Debug.LogWarning($"<color=yellow>APIManager:</color> Error parsing leagueName: " +
                            $"{response.data.league_data.leagueName}. Setting default league");
                        PlayerStats.LeagueName = "League";
                    }

                    //if (Enum.TryParse(response.data.league_data.leagueName, out League lg))
                    //{
                    //    PlayerStats.LeagueName = lg.ToString();
                    //}
                    //else
                    //{
                    //    Debug.LogWarning($"<color=yellow>APIManager:</color> Error parsing leagueName: " +
                    //        $"{response.data.league_data.leagueName}. Setting default league");
                    //    PlayerStats.LeagueName = "League";
                    //}

                    PlayerStats.OnPlayerStatsChanged?.Invoke();

                    await GetUserInBattleByWalletAddress();
                    callback?.Invoke(true);
                }
                else
                {
                    Debug.LogError("<color=cyan>APIManager:</color> Failed to get user details by wallet.");
                    Debug.LogError(response.msg);
                    callback?.Invoke(false);
                    return;
                }
            }
        }

        /* Sample Response:
                {
                    "success": true,
                    "data":
                        {
                            "userId": 9388,
                            "userName": "Muneeb",
                            "walletAddress": "0xb4e85746efc403488f985802d2c5b8d6a815a4c9",
                            "email": "leiyenatreimi-7959@yopmail.com",
                            "avatar_name": null,
                            "avatar_index": null,
                            "battle_data":
                                {
                                    "rank": 1,
                                    "rankByLeague": 1,
                                    "leaguePoints": 120,
                                    "battles": 20,
                                    "wins": 9,
                                    "loses": 10,
                                    "draws": 1,
                                    "winStreak": "-",
                                    "fuel": "0",
                                    "xeta": "0",
                                    "isHaveMinFuel": true
                                },
                            "league_data":
                                {
                                    "leagueId": 1,
                                    "leagueName": "Trial",
                                    "defaultPoint": 20,
                                    "mininumPoints": 0,
                                    "maximumPoints": 199,
                                    "pointsToNextLeague": "80",
                                    "playerHp": 200,
                                    "initialManaCost": 1,
                                    "increaseManaCount": 1
                                }
                        },
                    "msg": "successfully fetched user details"
                }

            */

        [Serializable]
        class GetUserDetailsByWalletResponse
        {
            public bool success;
            public GetUserDetailsByWalletData data;
            public string msg;
        }

        [Serializable]
        class GetUserDetailsByWalletData
        {
            public int userId;
            public string userName;
            public string walletAddress;
            public string email;
            public object avatar_name;
            public object avatar_index;
            public GetUserDetailsByWalletDataBattleData battle_data;
            public GetUserDetailsByWalletDataLeagueData league_data;
        }

        [Serializable]
        class GetUserDetailsByWalletDataBattleData
        {
            public int rank;
            public int rankByLeague;
            public int leaguePoints;
            public int battles;
            public int wins;
            public int loses;
            public int draws;
            public string winStreak;
            public string fuel;
            public string xeta;
            public bool isHaveMinFuel;
        }

        [Serializable]
        class GetUserDetailsByWalletDataLeagueData
        {
            public int leagueId;
            public string leagueName;
            public int defaultPoint;
            public int mininumPoints;
            public int maximumPoints;
            public string pointsToNextLeague;
            public int playerHp;
            public int initialManaCost;
            public int increaseManaCount;
        }

        internal static (string leagueName, string leagueSubTier) GetLeagueNameAndSubTier(string league)
        {
            if (string.IsNullOrEmpty(league))
            {
                return (string.Empty, string.Empty);
            }

            var match = Regex.Match(league, @"^(?<name>[^\d]+)\s*(?<subtier>\d*)$");
            if (match.Success)
            {
                var leagueName = match.Groups["name"].Value.Trim();
                var leagueSubTier = match.Groups["subtier"].Value.Trim();
                return (leagueName, leagueSubTier);
            }

            return (league, string.Empty);
        }

        #endregion

        #region GetUserInBattleByWalletAddress

        internal static async Task GetUserInBattleByWalletAddress()
        {
            if (string.IsNullOrEmpty(AUTH_TOKEN))
            {
                Debug.LogError("<color=cyan>APIManager:</color> Auth Token is not available. Please login first.");
                return;
            }

            UnityWebRequest request = UnityWebRequest.Get(BD_GetUserInBattleByWallet_API);
            request.SetRequestHeader("Authorization", $"{AUTH_TOKEN}");

            request.url = request.url.Replace(":wallet", WalletAddress);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(request.error);
                Debug.LogError(request.downloadHandler.text);
            }
            else
            {
                Debug.Log("<color=cyan>APIManager:</color> GetUserInBattleByWallet Response: " + request.downloadHandler.text);

                if (request.downloadHandler.text.Contains("<!DOCTYPE html>"))
                {
                    Debug.LogError("<color=cyan>APIManager:</color> Possible IP blocked issue.");
                    Debug.LogError(request.downloadHandler.text);
                    return;
                }

                GetUserInBattleByWalletResponse response = JsonUtility.FromJson<GetUserInBattleByWalletResponse>(request.downloadHandler.text);

                if (response.success)
                {
                    PlayerStats.InBattle = response.data.in_duel ? 1 : 0;
                    PlayerStats.OpponentWalletAddress = response.data.opponent;
                    PlayerStats.CurrentDuelId = response.data.duel_id;

                    //Debug.Log($"<color=cyan>APIManager:</color> Got user In Battle by wallet successfully: " +
                    //    $"InBattle: {PlayerStats.InBattle}, OpponentWallet: {PlayerStats.OpponentWalletAddress}, DuelID: {PlayerStats.CurrentDuelId}");

                    /* Sample Response
                    
                    {
                        "success": true,
                        "data": {
                            "in_duel": false,
                            "opponent": "0x0000000000000000000000000000000000000000",
                            "duel_id": 0
                        },
                        "msg": "successfully fetched user data from chain"
                    }
                    
                    */
                }
                else
                {
                    PlayerStats.InBattle = 0;
                    Debug.LogError("<color=cyan>APIManager:</color> Failed to get user In Battle by wallet (async)");
                    Debug.LogError(response.message);

                }
            }
        }

        [Serializable]
        class GetUserInBattleByWalletResponse
        {
            public bool success;
            public GetUserInBattleByWalletResponseData data;
            public string message;
        }


        [Serializable]
        class GetUserInBattleByWalletResponseData
        {
            public bool in_duel;
            public string opponent;
            public int duel_id;
        }

        #endregion

        #region Set Player Name

        private static string BD_SetPlayerName_API => BD_API_BASE + "users/set-name";

        internal static async Task SetPlayerName(string newName)
        {
            if (string.IsNullOrEmpty(AUTH_TOKEN))
            {
                Debug.LogError("<color=cyan>APIManager:</color> SetPlayerName(): Auth Token is not available. Please login first.");
                return;
            }

            WWWForm form = new();
            form.AddField("name", newName);

            using UnityWebRequest webRequest = UnityWebRequest.Post(BD_SetPlayerName_API, form);
            webRequest.SetRequestHeader("Authorization", $"{AUTH_TOKEN}");

            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("<color=cyan>APIManager:</color> SetPlayerName(): " + webRequest.error);
            }
            else
            {
                if (webRequest.downloadHandler.text.Contains("<!DOCTYPE html>"))
                {
                    Debug.LogError("<color=cyan>APIManager:</color> SetPlayerName(): Possible IP blocked issue.");
                    Debug.LogError("<color=cyan>APIManager:</color> SetPlayerName(): " + webRequest.downloadHandler.text);
                    return;
                }

                Debug.Log("<color=cyan>APIManager:</color> SetPlayerName(): " + webRequest.downloadHandler.text);

                SetPlayerNameResponse response = JsonUtility.FromJson<SetPlayerNameResponse>(webRequest.downloadHandler.text);

                if (response.success)
                {
                    Debug.Log("<color=cyan>APIManager:</color> Player name updated.");
                    PlayerStats.userName = newName;
                    PlayerStats.OnPlayerStatsChanged?.Invoke();
                }
                else
                {
                    Debug.LogError("<color=cyan>APIManager:</color> Failed to update player name: " + response.message);
                }
            }
        }

        /* Sample Response
         {
            "success": true,
            "data": null,
            "msg": "User name updated successfully"
         }
         */

        [Serializable]
        class SetPlayerNameResponse
        {
            public bool success;
            public object data;
            public string message;
        }

        #endregion

        #region SaveUserNonce

        private static string BD_SaveNonce_API => BD_API_BASE + "auth/save-user-nonce";

        //public static void SaveUserNonce(string walletAddress, string nonce, string sign)
        //{
        //    SaveUserNonceAsync(walletAddress, nonce, sign);
        //}
        internal static async void SaveUserNonce(string _walletAddress, string nonce, string sign)
        {
            string url = BD_SaveNonce_API;
            Debug.Log("<color=cyan>APIManager:</color> SaveUserNonce(): url: " + url);

            WWWForm form = new();
            WalletAddress = _walletAddress;
            form.AddField("walletAddress", _walletAddress);
            form.AddField("nonce", nonce);

            using UnityWebRequest www = UnityWebRequest.Post(url, form);

            await www.SendWebRequest();


            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("<color=cyan>APIManager:</color> SaveUserNonce() Error saving nonce: " + www.error);
            }
            else
            {
                string data = www.downloadHandler.text;
                Debug.Log("<color=cyan>APIManager:</color> SaveUserNonce() saved user nonce. data: " + data);

                if (data.StartsWith("<!DOCTYPE html>"))
                {
                    //UserLoginManager.Instance.LoginEmailErrorTxt.text = "SaveUserNonce(): Possible IP Block Issue";
                    Debug.LogError("<color=cyan>APIManager:</color> SaveUserNonce(): Possible IP Block Issue");
                    return;
                }

                var saveNonceRoot = JsonConvert.DeserializeObject<NonceRoot>(data);
                if (saveNonceRoot.success)
                {
                    //StartCoroutine(IEverifySignature(sign, saveNonceRoot.data.nonce));
                    VerifySignature(sign, saveNonceRoot.data.nonce);
                }
            }
        }

        [System.Serializable]
        class NonceData
        {
            public string nonce;
        }

        [System.Serializable]
        class NonceRoot
        {
            public bool success;
            public NonceData data = new();
            public string msg;
        }

        #endregion

        #region Verify Signature

        private static readonly string VerifySignature_API = "auth/verify-signature";

        private static async void VerifySignature(string sign, string nonce)
        {
            await Task.Delay(500);

            string url = BD_API_BASE + VerifySignature_API;

            Debug.Log("<color=cyan>APIManager:</color> VerifySignature(): " +
                "url: " + url + " sign: " + sign + " nonce: " + nonce + " deviceId: " + SystemInfo.deviceUniqueIdentifier);


            WWWForm form = new();
            form.AddField("nonce", nonce);
            form.AddField("signature", sign);
            form.AddField("deviceId", SystemInfo.deviceUniqueIdentifier);

            using UnityWebRequest www = UnityWebRequest.Post(url, form);

            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("<color=cyan>APIManager:</color> Error verifying signature: " + www.error);
            }
            else
            {
                string data = www.downloadHandler.text;

                if (data.StartsWith("<!DOCTYPE html>"))
                {
                    // The error msg could likely be in the title tag.
                    int titleStart = data.IndexOf("<title>") + 7;
                    int titleEnd = data.IndexOf("</title>");
                    string error = data[titleStart..titleEnd]; // range operator

                    //UserLoginManager.Instance.LoginEmailErrorTxt.text = "Error: " + error;
                    Debug.LogError("<color=cyan>APIManager:</color> VerifySignature(): Error: " + error);
                    Debug.LogError("<color=cyan>APIManager:</color> VerifySignature(): Verify Signature data: " + data);
                    return;
                }

                var loginRoot = JsonConvert.DeserializeObject<LoginResponseRoot>(data);
                Debug.Log("<color=cyan>APIManager:</color> VerifySignature(): " + data);

                if (loginRoot.success)
                {
                    WalletAddress = loginRoot.data.user.walletAddress;
                    AUTH_TOKEN = loginRoot.data.token;
                    await GetUserDetailsByWallet();
                    await NFTManager.GetNFTs(WalletAddress);
                    //   UserLoginManager.LoadMainMenuScene(); // TODO: Move this to a callback
                    SceneManager.LoadScene("Demo_Fighter3D - Type 2");
                }
                else
                {
                    Debug.LogError("<color=cyan>APIManager:</color> VerifySignature(): Error: " + loginRoot.msg);

                    if (loginRoot.msg.Contains("You are already logged in from multiple devices. Logout from all devices first")
                        || loginRoot.msg.Contains("You are already logged in from another device")) // TODO: Muneeb: !!!
                    {
                        VerifySignatureDestroyDevice(sign, nonce);
                    }

                    // Web3AuthCustom.Instance.logout(); // Already done below
                    //Debug.LogError("verifySignature data: " + data);
                }
                Web3AuthCustom.Instance.logout();
            }
        }

        [System.Serializable]
        public class LoginData
        {
            public string token;
            public string encryptedId;
            public int code;
            public string msg;
            public UserDetails user;
        }
        [System.Serializable]
        public class LoginResponseRoot
        {
            public bool success;
            public LoginData data;
            public string msg;
        }
        [System.Serializable]
        public class UserDetails
        {
            public int id;
            public string name;
            public string walletAddress;
            public string email;
            public string phoneNumber;
            public string coins;
            public bool new_login;
        }

        #endregion

        #region Verify Signature Destroy Device

        private static string VerifySignatureDestroyDevices => BD_API_BASE + "auth/verify-signature-destroy-devices";

        public static async void VerifySignatureDestroyDevice(string sign, string nonce)
        {
            await Task.Delay(500);

            string url = VerifySignatureDestroyDevices;
            Debug.Log($"<color=cyan>APIManager:</color> VerifySignatureDestroyDevices() URL: {url}, sign: {sign}, nonce: {nonce}, device ID: {SystemInfo.deviceUniqueIdentifier}");

            WWWForm form = new();
            form.AddField("nonce", nonce);
            form.AddField("signature", sign);
            form.AddField("deviceId", SystemInfo.deviceUniqueIdentifier);

            using UnityWebRequest request = UnityWebRequest.Post(url, form);
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("<color=cyan>APIManager:</color> Error in verifySignature API: " + request.error);
            }
            else
            {
                string data = request.downloadHandler.text;
                Debug.LogError("<color=cyan>APIManager:</color> verifySignature data: " + data);
                var loginResponseDestroyDevice = JsonConvert.DeserializeObject<LoginResponseRoot>(data);
                if (loginResponseDestroyDevice.success)
                {
                    WalletAddress = loginResponseDestroyDevice.data.user.walletAddress;
                    AUTH_TOKEN = loginResponseDestroyDevice.data.token;
                    Web3AuthCustom.Instance.logout();
                    await GetUserDetailsByWallet();
                    await NFTManager.GetNFTs(WalletAddress);
                    // UserLoginManager.LoadMainMenuScene(); // TODO: Move this to a callback
                    SceneManager.LoadScene("Demo_Fighter3D - Type 2");
                }
            }
        }

        #endregion

        #region Refresh Token

        //public static string deviceId;
        //private static string REFRESHTOKEN_URL => BD_API_BASE + "auth/refresh-user-token";
        //private bool alreadyLogin;

        //internal void HitRefreshTokenAPI()
        //{
        //    StartCoroutine(IERefreshToken());
        //}
        //private IEnumerator IERefreshToken()
        //{
        //    WWWForm form = new();
        //    form.AddField("deviceId", deviceId);

        //    //UnityWebRequest request = UnityWebRequest.Post(UserRegisterationManager.currentAPI + REFRESHTOKEN_URL, form);
        //    UnityWebRequest request = UnityWebRequest.Post(REFRESHTOKEN_URL, form);

        //    request.SetRequestHeader("Authorization", AUTH_TOKEN);

        //    yield return request.SendWebRequest();

        //    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        //    {
        //        Debug.LogError("<color=cyan>APIManager:</color> RefreshToken() Error: " + request.error);
        //    }
        //    else
        //    {
        //        string responseData = request.downloadHandler.text;
        //        Debug.Log("<color=cyan>APIManager:</color> RefreshToken Response: " + responseData);
        //        var refreshTokenRoot = JsonConvert.DeserializeObject<RefreshTokenRoot>(responseData);
        //        if (!string.IsNullOrEmpty(refreshTokenRoot.data.token))
        //        {
        //            alreadyLogin = true;
        //            UserStatics.user/* = user*/ = refreshTokenRoot.data.user;
        //            UserStatics.walletAddress = WalletAddress = UserStatics.user.walletAddress;
        //            PlayerPrefs.SetString("LoginToken", refreshTokenRoot.data.token);
        //            SceneManager.LoadSceneAsync("AuthenticationKit", LoadSceneMode.Additive); // TODO: No such scene
        //        }
        //    }
        //}

        //[System.Serializable]
        //class RefreshTokenData
        //{
        //    public string token;
        //    public object xanaliaToken;
        //    public string encryptedId;
        //    public User user = new User();
        //    public bool isAdmin;
        //}

        //[System.Serializable]
        //class RefreshTokenRoot
        //{
        //    public bool success;
        //    public RefreshTokenData data;
        //    public string msg;
        //}

        #endregion

        #region Logout

        private static string LogOut => BD_API_BASE + "auth/logout";

        public async static Task LogOutAsync()
        {
            string url = LogOut;
            //Debug.Log("<color=yellow>APIManager:</color> LogOut(): url: " + url + ", deviceID: " + SystemInfo.deviceUniqueIdentifier);

            WWWForm form = new();
            form.AddField("deviceId", SystemInfo.deviceUniqueIdentifier);

            using UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Authorization", AUTH_TOKEN);
            await request.SendWebRequest();


            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"<color=cyan>APIManager:</color> LogOut() Error: {request.error}. Local logout...");
                //callback?.Invoke(false);
                //return;
            }
            else
            {
                string data = request.downloadHandler.text;

                if (data.StartsWith("<!DOCTYPE html>"))
                {
                    Debug.LogError("<color=cyan>APIManager:</color> LogOut(): Possible IP Block Issue. Local logout...");
                    //"UserDataManager.LogOut(): Possible IP Block Issue";
                    //callback?.Invoke(false);
                    //return;
                }

                //Debug.Log("<color=yellow>APIManager:</color> LogOut(): Logout response data: " + data);
                LogOutResponse logoutResponse = JsonConvert.DeserializeObject<LogOutResponse>(data);

                if (logoutResponse.success)
                {
                    Debug.Log("<color=yellow>APIManager:</color> LogOut(): Logged out successfully");
                }
                else
                {
                    Debug.LogError($"<color=cyan>APIManager:</color> Logout error: {logoutResponse.msg}." +
                        $" deviceId: {SystemInfo.deviceUniqueIdentifier}. Local logout...");
                }

                // Local logout even if the server logout fails
                PlayerPrefs.DeleteKey("WalletAddress");
                PlayerPrefs.DeleteKey("AUTH_TOKEN");
                PlayerPrefs.DeleteKey("Account");
                PlayerPrefs.DeleteKey("sessionId");
                return;
            }
        }

        [System.Serializable]
        public class LogOutResponse
        {
            public bool success;
            public object data;
            public string msg;
        }

        #endregion

        #region Check Device ID

        private static string BD_CheckDeviceID_API => BD_API_BASE + "users/check-device-id";

        internal async static Task CheckDeviceID(UnityAction<bool, bool> callback = null) // (apiSuccess, deviceExists)
        {
            if (string.IsNullOrEmpty(AUTH_TOKEN)) // not logged in
            {
                return;
            }

            //Debug.Log("<color=yellow>APIManager:</color> CheckDeviceID(): url: " + BD_CheckDeviceID_API + ", deviceID: " + SystemInfo.deviceUniqueIdentifier);

            WWWForm form = new();
            form.AddField("deviceId", SystemInfo.deviceUniqueIdentifier);

            using UnityWebRequest request = UnityWebRequest.Post(BD_CheckDeviceID_API, form);
            request.SetRequestHeader("Authorization", AUTH_TOKEN);
            await request.SendWebRequest();


            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("<color=cyan>APIManager:</color> CheckDeviceID() Error: " + request.error);
                callback?.Invoke(false, false);
                return;
            }
            else
            {
                string data = request.downloadHandler.text;

                if (data.StartsWith("<!DOCTYPE html>"))
                {
                    Debug.LogError("<color=cyan>APIManager:</color> CheckDeviceID(): Possible IP Block Issue");
                    Debug.LogError($"<color=cyan>APIManager:</color> CheckDeviceID(): {data}");
                    callback?.Invoke(false, false);
                    return;
                }

                //Debug.Log("<color=yellow>APIManager:</color> CheckDeviceID() response: " + data);
                CheckDeviceIDResponse response = JsonConvert.DeserializeObject<CheckDeviceIDResponse>(data);

                Debug.Log($"<color=yellow>APIManager:</color> CheckDeviceID(): {response.msg}, deviceID: {SystemInfo.deviceUniqueIdentifier}");
                if (response.success)
                {
                    if (response.msg.Contains("Device Id exits"))
                    {
                        callback?.Invoke(true, true);
                        return;
                    }
                    else if (response.msg.Contains("Device Id Does not exits"))
                    {
                        callback?.Invoke(true, false);
                        return;
                    }
                    else
                    {
                        Debug.LogError("<color=cyan>APIManager:</color> CheckDeviceID(): Unknown response: " + response.msg);
                        callback?.Invoke(false, false);
                        return;
                    }
                }
                else
                {
                    callback?.Invoke(false, false);
                    return;
                }
            }
        }

        [System.Serializable]
        public class CheckDeviceIDResponse
        {
            public bool success;
            public object data;
            public string msg;
        }

        #endregion

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            BattleManager.OnBattleStart += StartUserBattle;
            BattleManager.OnBattleOver += EndUserBattle;
        }

        private void OnDisable()
        {
            BattleManager.OnBattleStart -= StartUserBattle;
            BattleManager.OnBattleOver -= EndUserBattle;
        }



        #endregion

        #region Debug

        [ContextMenu("Random Fuel")]
        public void RandomFuel()
        {
            FuelManager.CurrentFuel = UnityEngine.Random.Range(0, 100);
        }

        [ContextMenu("Check NFTs")]
        public void CheckNFTs()
        {
            _ = NFTManager.GetNFTs(WalletAddress);
        }

        #endregion
    }
}
