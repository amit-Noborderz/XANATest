using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Firebase.DynamicLinks;
using UnityEditor;
using Firebase;
using Firebase.Extensions;
using SimpleJSON;

public class DynamicEventManager : Singleton<DynamicEventManager>
{
    #region Variables
    private string EnvironmentURl = "/world/get-world-custom-data/";
    private string EventArguments;
    bool FirstTimeopen = true;
    bool Debugging = true;
    bool isOnAppLunached;
    static DynamicEventManager Instance;
    bool isDomeLinkJoinned = false;
    #endregion

    #region Unity Functions

    void OnEnable()
    {
        BuilderEventManager.AfterPlayerInstantiated += TriggerDomeLoading;
        //BuilderEventManager.AfterWorldOffcialWorldsInatantiated += TriggerDomeLoading;
        Application.deepLinkActivated += OnDeepLinkActivated;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
        SaveCharacterProperties.NeedToShowSplash = 1;
        print("Set need to show 1 in dynamic event manager");
        XanaEventDetails.eventDetails = new XanaEventDetails();
        XanaEventDetails.eventDetails.DataIsInitialized = false;
    }

    private void OnDestroy()
    {
        BuilderEventManager.AfterWorldInstantiated -= TriggerDomeLoading;
        BuilderEventManager.AfterWorldOffcialWorldsInatantiated -= TriggerDomeLoading;
        Application.deepLinkActivated -= OnDeepLinkActivated;
    }

    private void Start()
    {
        ConstantsHolder.xanaConstants.isFirebaseInit = false;
        Debug.Log("DynamicEventManager: Start called");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            Debug.Log("DynamicEventManager: Initialized");
            ConstantsHolder.xanaConstants.isFirebaseInit = true;
            DynamicLinks.DynamicLinkReceived += OnDynamicLink;
        });
    }

    void OnDynamicLink(object sender, EventArgs args)
    {
        var dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;
        if (dynamicLinkEventArgs != null)
        {
            Debug.LogFormat("Received dynamic link {0}", dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString);
            ProcessDeepLink(dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString);
        }
        else
        {
            Debug.LogError("Received dynamic link event args are null");
        }
    }

    void OnDeepLinkActivated(string url)
    {
        Debug.Log($"Deep link activated: {url}");
        ProcessDeepLink(url);
    }

    void ProcessDeepLink(string link)
    {
        Debug.LogError("Processing deep link: " + link);

        if (!string.IsNullOrEmpty(link))
        {
            if (link.Contains("ENV"))
            {
                Debug.LogError("Detected ENV in URL");
                ConstantsHolder.xanaConstants.isSummitDeepLink = true;
                ConstantsHolder.xanaConstants.isJoiningXANADeeplink = false;
                StartCoroutine(FetchDomeId(link));
            }
            else if (link.Contains("Join"))
            {
                Debug.Log("Detected Join in URL");
                ConstantsHolder.xanaConstants.isSummitDeepLink = false;
                ConstantsHolder.xanaConstants.isJoiningXANADeeplink = true;
                Debug.LogError("isJoiningXANADeeplink set to true");
                PlayerPrefs.SetInt("FirstTimeappOpen", 0);
                XANADeeplink(link);
            }
        }
        else
        {
            Debug.LogError("Received empty deep link URL");
        }
    }

    public void XANADeeplink(string deeplinkUrl)
    {
        Debug.Log($"XANADeeplink called with URL: {deeplinkUrl}");
        StartCoroutine(ValidateLoginthenDeeplink(deeplinkUrl));
    }

    IEnumerator ValidateLoginthenDeeplink(string deeplinkUrl)
    {
#if UNITY_IOS
        while ((((!ConstantsHolder.loggedIn || (!ConstantsHolder.xanaConstants.LoggedInAsGuest)) &&
               (PlayerPrefs.GetString("PlayerName") == "") || string.IsNullOrEmpty(ConstantsHolder.userName)) && PlayerPrefs.GetInt("FirstTimeappOpen") == 0))
        {
            Debug.Log("Waiting for login on IOS : loggedIn : " + ConstantsHolder.loggedIn + " :  LoggedInAsGuest " +
            ConstantsHolder.xanaConstants.LoggedInAsGuest + " : PlayerName  " + PlayerPrefs.GetString("PlayerName") + " : FirstTimeappOpen : " + PlayerPrefs.GetInt("FirstTimeappOpen"));
            yield return new WaitForEndOfFrame();
        }
#endif

#if UNITY_ANDROID
        while ((((!ConstantsHolder.loggedIn || (!ConstantsHolder.xanaConstants.LoggedInAsGuest)) &&
            (PlayerPrefs.GetString("PlayerName") == "") || string.IsNullOrEmpty(ConstantsHolder.userName))))
        {
            yield return new WaitForEndOfFrame();
        }
#endif
        string decodedUrl = UnityWebRequest.UnEscapeURL(deeplinkUrl);
        Debug.Log($"Decoded URL: {decodedUrl}");

        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    var intent = activity.Call<AndroidJavaObject>("getIntent");
                    intent.Call("removeExtra", "com.google.firebase.dynamiclinks.DYNAMIC_LINK_DATA");
                    intent.Call("removeExtra", "com.google.android.gms.appinvite.REFERRAL_BUNDLE");
                    Debug.Log("Removed extras from Android intent");
                }
            }
        }


        yield return null;
        LoadingHandler.Instance.characterLoading.gameObject.SetActive(true);

#if UNITY_ANDROID
        string joinKey = "Join=";
        int startIndex = decodedUrl.IndexOf(joinKey) + joinKey.Length;
        int endIndex = decodedUrl.IndexOf("&", startIndex);

        if (startIndex != -1)
        {
            if (endIndex != -1)
            {
                EventArguments = decodedUrl.Substring(startIndex, endIndex - startIndex);
            }
            else
            {
                EventArguments = decodedUrl.Substring(startIndex);
            }
            Debug.Log($"EventArguments set to: {EventArguments}");
            if (FirstTimeopen)
            {
                FirstTimeopen = false;
                Debug.Log($"Invoking deep link environment with arguments: {EventArguments}");
                InvokeDeepLinkEnvironment(EventArguments);
            }
        }
#endif

#if UNITY_IOS
        if (decodedUrl.Contains("Join"))
        {
            int envIndex = decodedUrl.IndexOf("Join");
            int ampersandIndex = decodedUrl.IndexOf("&", envIndex);

            if (envIndex != -1)
            {
                string envSubstring;
                if (ampersandIndex != -1)
                {
                    envSubstring = decodedUrl.Substring(envIndex + 4, ampersandIndex - envIndex - 4);
                }
                else
                {
                    envSubstring = decodedUrl.Substring(envIndex + 4);
                }
                envSubstring = envSubstring.Replace("=", "");
                if (FirstTimeopen)
                {
                    EventArguments = envSubstring;
                    FirstTimeopen = false;
                    Debug.Log($"Invoking deep link environment with arguments: {EventArguments}");
                    InvokeDeepLinkEnvironment(EventArguments);
                }
            }
        }
#endif
    }

    public void InvokeDeepLinkEnvironment(string environmentIDf)
    {
        if (string.IsNullOrEmpty(EventArguments))
        {
            return;
        }
        if (ConstantsHolder.xanaConstants.isSummitDeepLink)
        {
            Debug.Log("isSummitDeepLink : " + ConstantsHolder.xanaConstants.isSummitDeepLink);
            MainSceneEventHandler.OpenLandingScene?.Invoke();
            return;
        }
        else
        {
            StartCoroutine(HitGetEnvironmentJson(ConstantsGod.API_BASEURL + EnvironmentURl + environmentIDf, environmentIDf));
        }
    }

    IEnumerator HitGetEnvironmentJson(string url, string envId)
    {
        if (ConstantsHolder.xanaConstants.isSummitDeepLink)
        {
            yield return null;
        }


        string urlEvent = ConstantsGod.API_BASEURL + ConstantsGod.GetDomeEvent;

        string eventId = "";

        using (UnityWebRequest www = UnityWebRequest.Get(urlEvent))
        {
            www.SetRequestHeader("Authorization", ConstantsGod.AUTH_TOKEN);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("API Error: " + www.error);
            }
            else
            {
                Debug.Log("API Response: " + www.downloadHandler.text);
                string json = www.downloadHandler.text;
                var node = JSON.Parse(json);

                if (node != null && node["data"] != null && node["data"]["dome_id"] != null)
                {
                    eventId = node["data"]["dome_id"].Value;

                    if (eventId.Equals(envId)) // Yes, the event is running, but the dynamic link is not for the event world
                    {
                        ConstantsHolder.xanaConstants.isSaudiEvent = true;
                        ConstantsHolder.xanaConstants.saudiEventDomeId = int.Parse(envId);
                    }
                }
                else
                {
                    Debug.LogWarning("dome_id not found in API response.");
                }
            }
        }


        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", ConstantsGod.AUTH_TOKEN);
            yield return request.SendWebRequest();
            EnvironmentDetails environmentDetails = JsonUtility.FromJson<EnvironmentDetails>(request.downloadHandler.text);
            Debug.Log("World Data : " + request.downloadHandler.text);
            ConstantsHolder.xanaConstants.MuseumID = envId;

            if (!request.isHttpError && !request.isNetworkError)
            {
                if (request.error == null)
                {
                    if (environmentDetails.success == true)
                    {
                        Debug.Log("Environment details successfully retrieved");
                        ConstantsHolder.xanaConstants.MuseumID = envId;
                        yield return new WaitForSeconds(4f);
                        PlayerPrefs.SetInt("PlayerDeepLinkOpened", 1);
                        bool isBuilderScene = false;
                        bool isMuseumScene = false;

                        if (environmentDetails.data.entityType == WorldType.MUSEUM.ToString())
                            isMuseumScene = true;
                        else if (environmentDetails.data.entityType == WorldType.USER_WORLD.ToString())
                        {
                            isBuilderScene = true;
                            isMuseumScene = true;
                        }
                        if (name == "Xana Festival")
                            ConstantsHolder.userLimit = 10;
                        else
                            ConstantsHolder.userLimit = environmentDetails.data.user_limit;

                        ConstantsHolder.xanaConstants.builderMapID = int.Parse(envId);
                        ConstantsHolder.xanaConstants.IsMuseum = isMuseumScene;
                        ConstantsHolder.xanaConstants.isBuilderScene = isBuilderScene;
                        ConstantsHolder.xanaConstants.UserMicEnable = environmentDetails.data.userMicEnable;
                        ConstantsHolder.xanaConstants.isXanaPartyWorld = false;
                        ConstantsHolder.isFromXANASummit = false;

                        LoadingHandler.Instance.loadingCanvas.alpha = 1;
                        WorldItemView.m_EnvName = environmentDetails.data.name;
                        SaveCharacterProperties.NeedToShowSplash = 2;
                        LoadingHandler.Instance.characterLoading.gameObject.SetActive(false);
                        PlayerPrefs.SetInt("FirstTimeappOpen", 1);
                        if (isBuilderScene)
                            WorldManager.instance.JoinBuilderWorld();
                        else
                            WorldManager.instance.JoinEvent();
                    }
                }
            }
            else
            {
                Debug.LogError($"Error retrieving environment details: {request.error}");

                if (request.Equals(UnityWebRequest.Result.ConnectionError))
                {
                    Debug.LogWarning("Connection error, retrying...");
                    yield return StartCoroutine(HitGetEnvironmentJson(url, envId));
                }
                else
                {
                    if (request.error != null)
                    {
                        if (environmentDetails.success == false)
                        {
                            Debug.LogWarning("Environment details retrieval failed, retrying...");
                            yield return StartCoroutine(HitGetEnvironmentJson(url, envId));
                        }
                    }
                }
            }
            request.Dispose();
        }
    }

    IEnumerator FetchDomeId(string deeplinkUrl)
    {
        Debug.Log("~~ deeplinkUrl" + deeplinkUrl);
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    var intent = activity.Call<AndroidJavaObject>("getIntent");
                    intent.Call("removeExtra", "com.google.firebase.dynamiclinks.DYNAMIC_LINK_DATA");
                    intent.Call("removeExtra", "com.google.android.gms.appinvite.REFERRAL_BUNDLE");
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

#if UNITY_ANDROID
        string[] urlBreakDown = deeplinkUrl.Split('=');
        foreach (string word in urlBreakDown)
        {
            if (urlBreakDown[1] == word)
            {
                if (word.Contains("ENV"))
                {
                    EventArguments = word.Replace("ENV", "");
                    if (FirstTimeopen)
                    {
                        FirstTimeopen = false;
                    }
                }
            }
        }
#endif

#if UNITY_IOS
        if (deeplinkUrl.Contains("ENV"))
        {
            Debug.Log("is ENV " + deeplinkUrl);
            int envIndex = deeplinkUrl.IndexOf("ENV");
            int ampersandIndex = deeplinkUrl.IndexOf("&", envIndex);

            if (envIndex != -1)
            {
                string envSubstring;
                if (ampersandIndex != -1)
                {
                    envSubstring = deeplinkUrl.Substring(envIndex + 3, ampersandIndex - envIndex - 3);
                }
                else
                {
                    envSubstring = deeplinkUrl.Substring(envIndex + 3);
                }
                if (FirstTimeopen)
                {
                    EventArguments = envSubstring;
                    Debug.Log("EventArguments : " + EventArguments);
                    FirstTimeopen = false;
                }
            }
        }
#endif

        yield return new WaitForSeconds(0.1f);
        if (int.TryParse(EventArguments, out int domeId))
        {
            ConstantsHolder.domeId = domeId;
            Debug.Log("~~DOME ID : " + ConstantsHolder.domeId);
        }
        else
        {
            Debug.LogError("Failed to parse dome ID from EventArguments: " + EventArguments);
        }
    }

    void TriggerDomeLoading()
    {
        if (string.IsNullOrEmpty(EventArguments))
        {
            return;
        }

        if (!isDomeLinkJoinned)
        {
            Debug.Log("Starting TriggerSceneLoading coroutine");
            LoadingHandler.Instance.characterLoading.SetActive(true);
            StartCoroutine(TriggerSceneLoading());
        }
        else
        {
            Debug.Log("Dome link already joined, skipping TriggerSceneLoading");
        }
    }

    IEnumerator TriggerSceneLoading()
    {
        Debug.Log("TriggerSceneLoading started, waiting for 5 seconds");

        while (LoadingHandler.Instance.loadingPanel.activeInHierarchy)
        {
            Debug.Log("Loading panel is active, waiting for 1 second");
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Loading panel is no longer active, proceeding with scene loading");
        SaveCharacterProperties.NeedToShowSplash = 2;
        LoadingHandler.Instance.loadingCanvas.alpha = 1;
        Debug.Log("Invoking LoadNewScene with domeId: " + ConstantsHolder.domeId);
        BuilderEventManager.LoadNewScene?.Invoke(ConstantsHolder.domeId, new Vector3(74.0876f, 0.870f, -169.146f),false);
        LoadingHandler.Instance.characterLoading.SetActive(false);
        LoadingHandler.Instance.EnterDome();
        isDomeLinkJoinned = true;
        Debug.Log("Dome link joined, setting isDomeLinkJoinned to true");
        yield return new WaitForEndOfFrame();
        ReferencesForGamePlay.instance.FullScreenMapStatus(false);
        Debug.Log("FullScreenMapStatus set to false");
    }

    #endregion
}

#if UNITY_EDITOR

[CustomEditor(typeof(DynamicEventManager))]
public class EditorTestDeeplinking : Editor
{
    public string DeepLink = "";

    public override void OnInspectorGUI()
    {
        DeepLink = EditorGUILayout.TextField("DeepLink", DeepLink);

        if (GUILayout.Button("Enter World"))
        {
            if (!DeepLink.IsNullOrEmpty())
            {
                if (DeepLink.Contains("ENV"))
                {
                    ConstantsHolder.xanaConstants.isSummitDeepLink = true;
                    ConstantsHolder.xanaConstants.isJoiningXANADeeplink = false;
                    StartEditorCoroutine(() => FetchDomeIdSimulated(DeepLink));
                }
                else if (DeepLink.Contains("Join"))
                {
                    ConstantsHolder.xanaConstants.isSummitDeepLink = false;
                    ConstantsHolder.xanaConstants.isJoiningXANADeeplink = true;
                }
                Debug.Log($"EditorTestDeeplinking: {DeepLink}");
                DynamicEventManager.Instance.XANADeeplink(DeepLink);
            }
        }
    }

    private void StartEditorCoroutine(System.Func<IEnumerator> coroutineMethod)
    {
        IEnumerator coroutine = coroutineMethod.Invoke();
        EditorApplication.update += EditorCoroutineHandler;

        void EditorCoroutineHandler()
        {
            if (!coroutine.MoveNext())
            {
                EditorApplication.update -= EditorCoroutineHandler;
            }
        }
    }

    private IEnumerator FetchDomeIdSimulated(string deeplinkUrl)
    {
        Debug.Log("Starting FetchDomeId simulation in Editor");
        yield return null;

        string[] urlBreakDown = deeplinkUrl.Split('&');
        foreach (string segment in urlBreakDown)
        {
            if (segment.Contains("ENV"))
            {
                string numberPart = segment.Split(new string[] { "ENV" }, StringSplitOptions.None)[1];
                int endIndex = numberPart.IndexOf('&');
                if (endIndex != -1)
                {
                    numberPart = numberPart.Substring(0, endIndex);
                }
                Debug.Log($"Detected number after ENV: {numberPart}");
                if (int.TryParse(numberPart, out int domeId))
                {
                    ConstantsHolder.domeId = domeId;
                    Debug.Log($"Simulated dome ID fetch complete. Dome ID: {ConstantsHolder.domeId}");
                }
                else
                {
                    Debug.LogError("Failed to parse dome ID");
                }
                break;
            }
        }
    }
}
#endif