/*using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Text;
using UnityEngine.Events;
namespace BD
{

public class AccountManager : MonoBehaviour
{
    // Inspector Fields
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject loginButton;
    [SerializeField] private GameObject guestLoginButton;
    [SerializeField] private TextMeshProUGUI errorText;

    [SerializeField] private GameObject userInfoPanel;
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private GameObject logoutButton;


    //[SerializeField] private GameObject PhotonPanel;

    private readonly string testEmail = "gigroffagufri-7515@yopmail.com";
    private readonly string testPassword = "12345678Aa!";

    public static string BD_Login_API = "https://breaking-down-stg.xana.net/api/auth/sign-in";
    public static string BD_GuestLogin_API = "https://breaking-down-stg.xana.net/api/auth/login-as-guest";
    public static string BD_Logout_API = "https://breaking-down-stg.xana.net/api/auth/logout";
    public static string BD_GetAllUserGlobalRanking_API = "https://breaking-down-stg.xana.net/api/users/get-users-global-rank";

    // user data
    public static int userID;
    public static string userName;
    public static string AUTH_TOKEN;
    public static bool IsLoggedIn = false;

    // Events
    public static UnityAction OnLoggedIn;
    public static UnityAction OnLoggedOut;

    private void Start()
    {
        emailInputField.text = testEmail;
        passwordInputField.text = testPassword;

        errorText.gameObject.SetActive(false);


        if (IsLoggedIn)
        {
            OnLoggedIn?.Invoke();
            loginPanel.SetActive(false);
            userInfoPanel.SetActive(true);
            userNameText.text = $"Logged In As: <b>{userName}</b>";
        }
        else
        {
            // Load login data from PlayerPrefs
            if (PlayerPrefs.HasKey("AUTH_TOKEN"))
            {
                AUTH_TOKEN = PlayerPrefs.GetString("AUTH_TOKEN");
                var userData = JsonUtility.FromJson<UserData>(PlayerPrefs.GetString("UserData"));
                userID = int.Parse(userData.id);
                userName = userData.name;
                IsLoggedIn = true;
                OnLoggedIn?.Invoke();

                loginPanel.SetActive(false);
                userInfoPanel.SetActive(true);
                userNameText.text = $"Logged In As: <b>{userName}</b>";

                Debug.Log(AUTH_TOKEN);
            }
            else
            {
                loginPanel.SetActive(true);
                userInfoPanel.SetActive(false);
            }
        }
    }

    class LeagueData
    {
        public int leagueID;
    }


    #region Login
    public void SubmitLoginCredentials()
    {
        string loginEmail = emailInputField.text;
        string loginPassword = passwordInputField.text;

        errorText.gameObject.SetActive(false);

        if (loginEmail == "" || loginPassword == "")
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "Email Or Password should not be empty";
            Debug.LogError("Accounts: Email Or Password should not be empty");
            return;
        }
        else if (loginEmail.Contains(" "))
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "Email should not contain spaces";
            Debug.LogError("Accounts: Invalid email address");
            return;
        }

        string url = BD_Login_API;

        loginEmail = loginEmail.Trim();

        MyClassOfLoginJson myObject = new MyClassOfLoginJson();
        loginEmail = loginEmail.Trim();
        loginPassword = loginPassword.Trim();
        string bodyJson;

        if (IsValidEmail(loginEmail))
        {

            loginEmail = loginEmail.ToLower();
            bodyJson = JsonUtility.ToJson(myObject.GetdataFromClass(loginEmail, "", loginPassword, UniqueDeviceID()));
        }
        else
        {
            bodyJson = JsonUtility.ToJson(myObject.GetdataFromClass("", loginEmail, loginPassword));
        }
        //print("Start Json " + bodyJson);
        StartCoroutine(LoginUserWithNewT(url, bodyJson));
    }

    
    IEnumerator LoginUserWithNewT(string url, string Jsondata)
    {
        loginButton.SetActive(false); // to prevent multiple clicks
        guestLoginButton.SetActive(false);

        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(Jsondata);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SendWebRequest();
        while (!request.isDone)
        {
            yield return null;
        }

        ClassWithToken myObject1 = ClassWithToken.CreateFromJSON(request.downloadHandler.text);

        if (myObject1 == null)
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "Server Error (IP blocked?)";
            Debug.LogError("Accounts: " + request.downloadHandler.text);

            loginButton.SetActive(true);
            guestLoginButton.SetActive(true);
            yield break;
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            //if (request.error == null)
            {
                if (myObject1.success)
                {
                    AUTH_TOKEN = myObject1.data.token;
                    Debug.Log(AUTH_TOKEN);
                    userID = int.Parse(myObject1.data.user.id);
                    Debug.Log("User ID: " + userID);
                    userName = myObject1.data.user.name;
                    IsLoggedIn = true;
                    OnLoggedIn?.Invoke();

                    Debug.Log("Accounts: Logged in as: " + userName);
                    loginPanel.SetActive(false);

                    userInfoPanel.SetActive(true);
                    userNameText.text = $"Logged In As: <b>{userName}</b>";

                    // save AUTH_TOKEN and myObject1.data.user data as a JSON in PlayerPrefs
                    PlayerPrefs.SetString("AUTH_TOKEN", AUTH_TOKEN);
                    PlayerPrefs.SetString("UserData", JsonUtility.ToJson(myObject1.data.user));
                }
                else
                {
                    errorText.gameObject.SetActive(true);
                    errorText.text = myObject1.msg;
                    Debug.LogError("Accounts: " + myObject1.msg);

                    loginButton.SetActive(true);
                    guestLoginButton.SetActive(true);
                }
            }
        }
        else
        {
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                errorText.gameObject.SetActive(true);
                errorText.text = "Connection Error";
                Debug.LogError("Accounts: Connection Error");
            }
            else
            {
                errorText.gameObject.SetActive(true);
                errorText.text = request.error.ToString();
                Debug.LogError("Accounts: " + request.error.ToString());
            }

            loginButton.SetActive(true);
            guestLoginButton.SetActive(true);
        }
    }

    //   AppID = uniqueID();
    private string UniqueDeviceID()
    {
        if (PlayerPrefs.GetString("DeviceID") == "")
        {
            int z1 = UnityEngine.Random.Range(0, 1000);
            int z2 = UnityEngine.Random.Range(0, 1000);
            string uid = z1.ToString() + z2.ToString();
            PlayerPrefs.SetString("DeviceID", uid);
            PlayerPrefs.Save();
            Debug.Log("DeviceID: " + PlayerPrefs.GetString("DeviceID"));
            return PlayerPrefs.GetString("DeviceID");
        }
        else
        {
            Debug.Log("DeviceID: " + PlayerPrefs.GetString("DeviceID"));
            return PlayerPrefs.GetString("DeviceID");
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public void LoginAsGuest()
    {
        StartCoroutine(LoginGuest(BD_GuestLogin_API));
    }

    [System.Obsolete]
    IEnumerator LoginGuest(string url)
    {
        loginButton.SetActive(false);
        guestLoginButton.SetActive(false);

        using UnityWebRequest www = UnityWebRequest.Post(url, "POST");
        var operation = www.SendWebRequest();
        while (!operation.isDone)
        {
            yield return null;
        }

        ClassWithToken myObject1 = new ClassWithToken();
        myObject1 = ClassWithToken.CreateFromJSON(www.downloadHandler.text);

        if (!www.isHttpError && !www.isNetworkError)
        {
            if (www.error == null)
            {
                if (myObject1.success)
                {
                    AUTH_TOKEN = myObject1.data.token;
                    Debug.Log(AUTH_TOKEN);
                    userID = int.Parse(myObject1.data.user.id);
                    Debug.Log("User ID: " + userID);
                    userName = myObject1.data.user.name;
                    IsLoggedIn = true;
                    OnLoggedIn?.Invoke();

                    this.gameObject.SetActive(false);
                    //PhotonPanel.SetActive(true);
                }
            }
        }
        else
        {
            if (www.isNetworkError)
            {
                Debug.LogError("Connection Error");
            }
            else
            {
                Debug.LogError("Error");
            }

            loginButton.SetActive(true);
            guestLoginButton.SetActive(true);
        }
    }

    [Serializable]
    public class LoginClass
    {
        //public LoginClass LoginObject;
        public string id;
        public string iat;
        public int exp;
        public LoginClass Load(string savedData)
        {
            LoginClass LoginObject = new LoginClass();
            LoginObject = JsonUtility.FromJson<LoginClass>(savedData);
            return LoginObject;
        }
    }

    [Serializable]
    public class MyClassOfLoginJson
    {
        public string email;
        public string phoneNumber;
        public string password;
        public string deviceId;
        public MyClassOfLoginJson GetdataFromClass(string L_eml, string L_phonenbr, string passwrd, string _Deviceid = "")
        {
            MyClassOfLoginJson myObj = new MyClassOfLoginJson();
            myObj.email = L_eml;
            myObj.phoneNumber = L_phonenbr;
            myObj.password = passwrd;
            myObj.deviceId = _Deviceid;
            return myObj;
        }

        public MyClassOfLoginJson CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<MyClassOfLoginJson>(jsonString);
        }

    }

    [System.Serializable]
    public class UserData
    {
        public string id;
        public string name;
        public string email;
        public string phoneNumber;
        public string coins;
        public string walletAddress;
    }

    [System.Serializable]
    public class JustToken
    {
        public string token;
        public string encryptedId;
        public string xanaliaToken;
        public UserData user;
        public bool isAdmin;
        public static JustToken CreateFromJSON(string jsonString)
        {
            print("Person " + jsonString);
            return JsonUtility.FromJson<JustToken>(jsonString);
        }
    }

    [System.Serializable]
    public class ClassWithToken
    {
        public static ClassWithToken _UserData;
        public bool success;
        public JustToken data;
        public string msg;
        public static ClassWithToken CreateFromJSON(string jsonString)
        {
            try
            {
                return JsonUtility.FromJson<ClassWithToken>(jsonString);
            }
            catch (Exception e)
            {
                Debug.LogError("Error: " + e.Message);
                return null;
            }
        }
    }
    #endregion

    #region Logout

    public void Logout()
    {
        // Clear login data from PlayerPrefs
        PlayerPrefs.DeleteKey("AUTH_TOKEN");
        PlayerPrefs.DeleteKey("UserData");

        StartCoroutine(LogoutUser(BD_Logout_API));
    }

    IEnumerator LogoutUser(string url)
    {
        logoutButton.SetActive(false);

        using UnityWebRequest request = UnityWebRequest.Post(url, "POST");
        request.SetRequestHeader("Authorization", "Bearer " + AUTH_TOKEN);


        // send deviceId in the body
        WWWForm form = new WWWForm();
        form.AddField("deviceId", UniqueDeviceID());
        request.uploadHandler = new UploadHandlerRaw(form.data);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.error == null)
            {
                IsLoggedIn = false;
                OnLoggedOut?.Invoke();
                userName = "";
                userID = -1;
                AUTH_TOKEN = "";

                loginPanel.SetActive(true);
                userInfoPanel.SetActive(false);
                logoutButton.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Account: Error Logging Out. " + request.error);
            logoutButton.SetActive(true);
        }
    }


    #endregion
}
}
*/