//using Nethereum.Signer;
//using Newtonsoft.Json;
//using Org.BouncyCastle.Utilities.Net;
//using SDKConfiguration;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Net;
//using System.Threading.Tasks;
using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.Networking.Types;
//using UnityEngine.SceneManagement;

namespace BD
{
    public class UserDataManager : MonoBehaviour
    {
        //public static UserDataManager Instance;
        //public static string AUTH_TOKEN
        //{
        //    get
        //    {
        //        return PlayerPrefs.GetString("AUTH_TOKEN", "");
        //    }
        //    set
        //    {
        //        PlayerPrefs.SetString("AUTH_TOKEN", value);
        //    }
        //}
        // private int userID;
        //public string walletAddress
        //{
        //    get
        //    {
        //        return PlayerPrefs.GetString("WalletAddress", "");
        //    }
        //    set
        //    {
        //        PlayerPrefs.SetString("WalletAddress", value);
        //    }
        //}

        //public string authorizationToken;
        //public string pwalletAddress;
        //public string deviceId;
        //public bool alreadyLogin;
        //public bool WalletLoginTap;
        //public bool clear;
        //public User user = new User();
        //public RefreshTokenRoot refreshTokenRoot = new RefreshTokenRoot();
        //public NonceRoot getNonceRoot = new NonceRoot();
        //public NonceRoot saveNonceRoot = new NonceRoot();
        //public LoginResponseRoot loginRoot = new LoginResponseRoot();
        //public LoginResponseRoot loginResponseDestroyDevice = new LoginResponseRoot();
        //public UserDataMainRoot userDataMainRoot = new UserDataMainRoot();

        //private void Awake()
        //{
        //    if (Instance == null)
        //    {
        //        Instance = this;
        //        DontDestroyOnLoad(gameObject);
        //    }
        //    else
        //    {
        //        Destroy(gameObject);
        //    }
        //}

        //void Start()
        //{
        //Application.runInBackground = true;
        //BD.APIManager.CheckAutoLogin();
        //}

        //public void CheckAutoLogin()
        //{
        //    authorizationToken = PlayerPrefs.GetString("AUTH_TOKEN");
        //    pwalletAddress = PlayerPrefs.GetString("WalletAddress");
        //    //deviceId = SystemInfo.deviceUniqueIdentifier;
        //    if (!string.IsNullOrEmpty(authorizationToken) && !string.IsNullOrEmpty(APIManager.WalletAddress))
        //    {
        //        APIManager.GetUserDetailsByWallet();
        //    }
        //}

        //public void LoadMainMenuScene()
        //{
        //    StartCoroutine(IELoadMainMenuScene());
        //}

        //public IEnumerator IELoadMainMenuScene()
        //{
        //    yield return new WaitForSeconds(.5f);
        //    SceneManager.LoadScene("Demo_Fighter3D - Type 2");
        //}


        //public static string GetSignature(string prvKey)
        //{
        //    // get current timestamp
        //    int timestamp = (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
        //    // set expiration time
        //    int expirationTime = timestamp + 60;
        //    // set message
        //    string msg1 = expirationTime.ToString();
        //    string msg2 = "Welcome. By signing this message you are verifying your digital identity. This is completely secure and does not cost anything!";
        //    var signer = new EthereumMessageSigner();
        //    var signature1 = signer.EncodeUTF8AndSign(msg1, new EthECKey(prvKey));
        //    var signature2 = signer.EncodeUTF8AndSign(msg2, new EthECKey(prvKey));
        //    return signature1;
        //}

        //public void GetUserDetailsByWallet(bool onlyUpdateData = false)
        //{
        //    StartCoroutine(IEGetUserDetailsByWallet(onlyUpdateData));
        //}
        //public IEnumerator IEGetUserDetailsByWallet(bool onlyUpdateData = false)
        //{
        //    string url = UserLoginManager.currentAPI + UserLoginManager.GetUserDetailsByWallet + pwalletAddress;
        //    Debug.Log("UserDataManager.IEGetUserDetailsByWallet(): " + url);

        //    using UnityWebRequest request = UnityWebRequest.Get(url);
        //    request.SetRequestHeader("Authorization", AUTH_TOKEN);

        //    yield return request.SendWebRequest();

        //    //if (www.isNetworkError || www.isHttpError)
        //    if (request.result == UnityWebRequest.Result.ConnectionError)
        //    {
        //        Debug.LogError("UserDataManager.IEGetUserDetailsByWallet(): " + request.error);
        //        Debug.LogError("UserDataManager.IEGetUserDetailsByWallet(): " + request.downloadHandler.text);
        //    }
        //    else
        //    {
        //        string data = request.downloadHandler.text;
        //        Debug.Log("UserDataManager.IEGetUserDetail(): Fetched user details" + data);

        //        if (data.StartsWith("<!DOCTYPE html>"))
        //        {
        //            Debug.LogError("UserDataManager.IEGetUserDetail(): Possible IP Block Issue");
        //            UserLoginManager.instance.LoginEmailErrorTxt.text = "UserDataManager.IEGetUserDetail(): Possible IP Block Issue";
        //            yield break;
        //        }

        //        userDataMainRoot = JsonConvert.DeserializeObject<UserDataMainRoot>(data);

        //        PlayerStats.Rank = userDataMainRoot.data.battle_data.rank;
        //        PlayerStats.userName = userDataMainRoot.data.userName;
        //        PlayerStats.userId = userDataMainRoot.data.userId;
        //        PlayerStats.xeta = float.Parse(userDataMainRoot.data.battle_data.xeta);
        //        XetaManager.CurrentXeta = (int)PlayerStats.xeta;
        //        PlayerStats.fuel = float.Parse(userDataMainRoot.data.battle_data.fuel);
        //        FuelManager.CurrentFuel = (int)PlayerStats.fuel;

        //        if (int.TryParse(userDataMainRoot.data.battle_data.winStreak, out int winStreak))
        //        {
        //            PlayerStats.Wins = winStreak;
        //        }
        //        else
        //        {
        //            Debug.LogError("UserDataManager.IEGetUserDetail(): " +
        //                "Error parsing winStreak: " + userDataMainRoot.data.battle_data.winStreak);
        //        }

        //        //PlayerStats.Losses = int.Parse(userDataMainRoot.data.battle_data.winStreak);
        //        //PlayerStats.Draws = int.Parse(userDataMainRoot.data.battle_data.winStreak);
        //        //PlayerStats.Season = int.Parse(userDataMainRoot.data.league_data.season);
        //        Leagues lg = (Leagues)Enum.Parse(typeof(Leagues), userDataMainRoot.data.league_data.leagueName);
        //        PlayerStats.League = (int)lg;
        //        PlayerStats.Battles = userDataMainRoot.data.battle_data.battles;
        //        PlayerStats.PointsToNextLeague = int.Parse(userDataMainRoot.data.league_data.pointsToNextLeague);
        //        PlayerStats.LeaguePoints = userDataMainRoot.data.battle_data.leaguePoints;

        //        APIManager.GetUserInBattleByWalletAddress(); // TODO: Move this to APIManager

        //        if (!onlyUpdateData)
        //        {
        //            LoadMainMenuScene();
        //        }
        //    }
        //}

        //[System.Serializable]
        //public class UserDataMainRoot
        //{
        //    public bool success;
        //    public UserData data;
        //    public string msg;
        //}

        //[System.Serializable]
        //public class BattleData
        //{
        //    public int rank;
        //    public int rankByLeague;
        //    public int leaguePoints;
        //    public int battles;
        //    public string winStreak;
        //    public string fuel;
        //    public string xeta;
        //    public bool isHaveMinFuel;
        //}
        //[System.Serializable]
        //public class UserData
        //{
        //    public int userId;
        //    public string userName;
        //    public string walletAddress;
        //    public object email;
        //    public object avatar_name;
        //    public object avatar_index;
        //    public BattleData battle_data;
        //    public LeagueData league_data;
        //}
        //[System.Serializable]
        //public class LeagueData
        //{
        //    public int leagueId;
        //    public string leagueName;
        //    public int defaultPoint;
        //    public int mininumPoints;
        //    public int maximumPoints;
        //    public string pointsToNextLeague;
        //    public int playerHp;
        //    public int initialManaCost;
        //    public int increaseManaCount;
        //}

        //public void SaveUserNonce(string walletAddress, string nonce, string sign)
        //{
        //    StartCoroutine(IEsaveUserNonce(walletAddress, nonce, sign));
        //}
        //public IEnumerator IEsaveUserNonce(string walletAddress, string nonce, string sign)
        //{
        //    string url = UserLoginManager.currentAPI + UserLoginManager.SaveNonce;
        //    Debug.Log("IEsaveUserNonce(): url: " + url);
        //    WWWForm form = new WWWForm();
        //    this.walletAddress = walletAddress;
        //    form.AddField("walletAddress", walletAddress);
        //    form.AddField("nonce", nonce);
        //    using UnityWebRequest www = UnityWebRequest.Post(url, form);
        //    yield return www.SendWebRequest();


        //    if (www.result == UnityWebRequest.Result.ConnectionError)
        //    {
        //        Debug.LogError("IEsaveUserNonce() Error saving nonce: " + www.error);
        //    }
        //    else
        //    {
        //        string data = www.downloadHandler.text;
        //        Debug.Log("IEsaveUserNonce() saved user nonce. data: " + data);

        //        if (data.StartsWith("<!DOCTYPE html>"))
        //        {
        //            UserLoginManager.instance.LoginEmailErrorTxt.text = "IEsaveUserNonce(): Possible IP Block Issue";
        //            Debug.LogError("IEsaveUserNonce(): Possible IP Block Issue");
        //            yield break;
        //        }

        //        saveNonceRoot = JsonConvert.DeserializeObject<NonceRoot>(data);
        //        if (saveNonceRoot.success)
        //        {
        //            StartCoroutine(IEverifySignature(sign, saveNonceRoot.data.nonce));
        //        }
        //    }
        //}

        //public IEnumerator IEverifySignature(string sign, string nonce)
        //{
        //    yield return new WaitForSeconds(.5f);
        //    string url = UserLoginManager.currentAPI + UserLoginManager.VerifySignature;

        //    Debug.Log("UserDataManager.IEverifySignature(): " +
        //        "url: " + url + " sign: " + sign + " nonce: " + nonce + "deviceId: " + SystemInfo.deviceUniqueIdentifier);


        //    WWWForm form = new WWWForm();
        //    form.AddField("nonce", nonce);
        //    form.AddField("signature", sign);
        //    form.AddField("deviceId", SystemInfo.deviceUniqueIdentifier);

        //    using UnityWebRequest www = UnityWebRequest.Post(url, form);

        //    yield return www.SendWebRequest();

        //    if (www.result == UnityWebRequest.Result.ConnectionError)
        //    {
        //        Debug.LogError("UserDataManager.IEverifySignature(): Error verifying signature: " + www.error);
        //    }
        //    else
        //    {
        //        string data = www.downloadHandler.text;

        //        if (data.StartsWith("<!DOCTYPE html>"))
        //        {

        //            // The error msg could likely be in the title tag.
        //            int titleStart = data.IndexOf("<title>") + 7;
        //            int titleEnd = data.IndexOf("</title>");
        //            string error = data.Substring(titleStart, titleEnd - titleStart);

        //            UserLoginManager.instance.LoginEmailErrorTxt.text = "Error: " + error;
        //            Debug.LogError("UserDataManager.IEverifySignature(): Error: " + error);
        //            Debug.LogError("UserDataManager.IEverifySignature(): Verify Signature data: " + data);
        //            yield break;
        //        }

        //        loginRoot = JsonConvert.DeserializeObject<LoginResponseRoot>(data);
        //        if (loginRoot.success)
        //        {
        //            pwalletAddress = walletAddress = loginRoot.data.user.walletAddress;
        //            authorizationToken = AUTH_TOKEN = loginRoot.data.token;
        //            GetUserDetailsByWallet();
        //        }
        //        else
        //        {
        //            if (loginRoot.msg.Contains("You are already logged in from multiple devices. Logout from all devices first"))
        //            {
        //                StartCoroutine(IEverifySignatureDestroyDevice(sign, nonce));
        //            }
        //            else
        //            {
        //                if (loginRoot.msg.Contains("You are already logged in from another device"))
        //                {
        //                    StartCoroutine(IEverifySignatureDestroyDevice(sign, nonce));
        //                }
        //            }
        //            Web3AuthCustom.Instance.logout();
        //            Debug.LogError("verifySignature data: " + data);
        //        }
        //        Web3AuthCustom.Instance.logout();
        //        Debug.Log("UserDataManager.IEverifySignature(): Verify Signature data: " + data);
        //    }
        //}
        //public IEnumerator IEverifySignatureDestroyDevice(string sign, string nonce)
        //{
        //    yield return new WaitForSeconds(.5f);
        //    string url = UserLoginManager.currentAPI + UserLoginManager.VerifySignatureDestroDevices;
        //    Debug.LogError(url);
        //    Debug.LogError("sign: " + sign);
        //    Debug.LogError("nonce: " + nonce);
        //    Debug.LogError("deviceId: " + SystemInfo.deviceUniqueIdentifier);
        //    WWWForm form = new WWWForm();
        //    form.AddField("nonce", nonce);
        //    form.AddField("signature", sign);
        //    form.AddField("deviceId", SystemInfo.deviceUniqueIdentifier);
        //    using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        //    {
        //        yield return www.SendWebRequest();
        //        if (www.isNetworkError || www.isHttpError)
        //        {
        //            Debug.LogError("Errorin verifySignature API: " + www.error);
        //        }
        //        else
        //        {
        //            string data = www.downloadHandler.text;
        //            Debug.LogError("verifySignature data: " + data);
        //            loginResponseDestroyDevice = JsonConvert.DeserializeObject<LoginResponseRoot>(data);
        //            if (loginResponseDestroyDevice.success)
        //            {
        //                pwalletAddress = walletAddress = loginResponseDestroyDevice.data.user.walletAddress;
        //                authorizationToken = AUTH_TOKEN = loginResponseDestroyDevice.data.token;
        //                Web3AuthCustom.Instance.logout();
        //                GetUserDetailsByWallet();
        //            }
        //        }
        //    }
        //}

        //public void HitRefreshTokenAPI()
        //{
        //    StartCoroutine(IERefreshToken());
        //}
        //public IEnumerator IERefreshToken()
        //{
        //    WWWForm form = new WWWForm();
        //    form.AddField("deviceId", deviceId);

        //    //UnityWebRequest request = UnityWebRequest.Post(UserRegisterationManager.currentAPI + REFRESHTOKEN_URL, form);
        //    UnityWebRequest request = UnityWebRequest.Post(REFRESHTOKEN_URL, form);

        //    request.SetRequestHeader("Authorization", authorizationToken);

        //    yield return request.SendWebRequest();

        //    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        //    {
        //        Debug.LogError("Error: " + request.error);
        //    }
        //    else
        //    {
        //        string responseData = request.downloadHandler.text;
        //        Debug.Log("Response: " + responseData);
        //        refreshTokenRoot = JsonConvert.DeserializeObject<RefreshTokenRoot>(responseData);
        //        if (!string.IsNullOrEmpty(refreshTokenRoot.data.token))
        //        {
        //            alreadyLogin = true;
        //            UserStatics.user = user = refreshTokenRoot.data.user;
        //            UserStatics.walletAddress = walletAddress = user.walletAddress;
        //            PlayerPrefs.SetString("LoginToken", refreshTokenRoot.data.token);
        //            SceneManager.LoadSceneAsync("AuthenticationKit", LoadSceneMode.Additive);
        //        }
        //    }
        //}
        //public bool LogOut()
        //{
        //    //StartCoroutine(IELogOut());

        //    //Task<bool> logoutTask = LogOutAsync();
        //    //return logoutTask.Wait(2000); // Blocks the thread until logout finishes, timeout after 10 seconds
        //}

        //public IEnumerator IELogOut()
        //{
        //    yield return new WaitForSeconds(2f);
        //    string url = UserLoginManager.currentAPI + UserLoginManager.LogOut;
        //    Debug.Log("UserDataManager.LogOut(): url: " + url + ", deviceID: " + SystemInfo.deviceUniqueIdentifier);

        //    WWWForm form = new();
        //    form.AddField("deviceId", SystemInfo.deviceUniqueIdentifier);

        //    using UnityWebRequest request = UnityWebRequest.Post(url, form);
        //    request.SetRequestHeader("Authorization", AUTH_TOKEN);
        //    yield return request.SendWebRequest();


        //    if (request.result == UnityWebRequest.Result.ConnectionError)
        //    {
        //        Debug.LogError("UserDataManager.LogOut(): Error logging out: " + request.error);
        //    }
        //    else
        //    {
        //        string data = request.downloadHandler.text;

        //        if (data.StartsWith("<!DOCTYPE html>"))
        //        {
        //            Debug.LogError("UserDataManager.LogOut(): Possible IP Block Issue");
        //            UserLoginManager.instance.LoginEmailErrorTxt.text = "UserDataManager.LogOut(): Possible IP Block Issue";
        //            yield break;
        //        }

        //        Debug.Log("UserDataManager.LogOut(): Logout response data: " + data);
        //        //LogOutResponse logoutResponse = JsonConvert.DeserializeObject<LogOutResponse>(data);
        //        yield return new WaitForSeconds(0.5f);
        //    }
        //}


        //public async static Task<bool> LogOutAsync()
        //{
        //    string url = UserLoginManager.currentAPI + UserLoginManager.LogOut;
        //    Debug.Log("UserDataManager.LogOut(): url: " + url + ", deviceID: " + SystemInfo.deviceUniqueIdentifier);

        //    WWWForm form = new();
        //    form.AddField("deviceId", SystemInfo.deviceUniqueIdentifier);

        //    using UnityWebRequest request = UnityWebRequest.Post(url, form);
        //    request.SetRequestHeader("Authorization", AUTH_TOKEN);
        //    await request.SendWebRequest();


        //    if (request.result == UnityWebRequest.Result.ConnectionError)
        //    {
        //        Debug.LogError("UserDataManager.LogOut(): Error logging out: " + request.error);
        //        return false;
        //    }
        //    else
        //    {
        //        string data = request.downloadHandler.text;

        //        if (data.StartsWith("<!DOCTYPE html>"))
        //        {
        //            Debug.LogError("UserDataManager.LogOut(): Possible IP Block Issue");
        //            UserLoginManager.instance.LoginEmailErrorTxt.text = "UserDataManager.LogOut(): Possible IP Block Issue";
        //            return false;
        //        }

        //        Debug.Log("UserDataManager.LogOut(): Logout response data: " + data);
        //        LogOutResponse logoutResponse = JsonConvert.DeserializeObject<LogOutResponse>(data);

        //        if (logoutResponse.success)
        //        {
        //            Debug.Log("UserDataManager.LogOut(): Logged out successfully");
        //            return true;
        //        }

        //        Debug.LogError("UserDataManager.LogOut(): Error: " + logoutResponse.msg);
        //        return false;
        //    }
        //}

        //[System.Serializable]
        //public class LogOutResponse
        //{
        //    public bool success;
        //    public object data;
        //    public string msg;
        //}
    }

    //[System.Serializable]
    //public class RefreshTokenData
    //{
    //    public string token;
    //    public object xanaliaToken;
    //    public string encryptedId;
    //    public User user = new User();
    //    public bool isAdmin;
    //}

    //[System.Serializable]
    //public class RefreshTokenRoot
    //{
    //    public bool success;
    //    public RefreshTokenData data = new RefreshTokenData();
    //    public string msg;
    //}

    //[System.Serializable]
    //public class NonceData
    //{
    //    public string nonce;
    //}

    //[System.Serializable]
    //public class NonceRoot
    //{
    //    public bool success;
    //    public NonceData data = new NonceData();
    //    public string msg;
    //}
}