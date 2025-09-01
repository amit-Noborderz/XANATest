using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
public class XanaVoiceChat : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public GameObject micOnBtn;
    public GameObject micOnBtnPotrait;
    public GameObject micOffBtn;
    public GameObject micOffBtnPotrait;
    public Sprite micOnSprite;
    public Sprite micOffSprite;

    private PunVoiceClient _punVoiceClient;
    public Recorder recorder;
    public Speaker speaker;

    private Button micBtn;

    private bool canTalk;
    private bool useMic;

    public UnityAction MicToggleOff, MicToggleOn;
    public static XanaVoiceChat instance;

    [Header("Mic Toast to instatiate")]
    public GameObject mictoast;
    public Transform placetoload;
    public string MicroPhoneDevice;
    public int index;
    private byte currentGroup;
    [Header("Toast Message UI")]
    public RectTransform VoiceChatToastPanel;
    public TextMeshProUGUI StereoDescriptionText;
    public TextMeshProUGUI AppSettingsDescriptionText;
    public Button AppSettingsBtn;
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void OnEnable()
    {
        BuilderEventManager.AfterPlayerInstantiated += CheckMicPermission;
        // Added by Waqas Ahmad
        if (instance != this && instance.recorder != null)
        {
            //if (instance.recorder.TransmitEnabled)
            //{
            //    Invoke("TurnOnMic", 0.25f);

            //}
            //else
            //{
            //    Invoke("TurnOffMic", 0.25f);

            //}

            instance = this;
            if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                StartCoroutine(instance.SetMic());
            }
        }
    }
    private void OnDisable()
    {
        BuilderEventManager.AfterPlayerInstantiated -= CheckMicPermission;
        if (_punVoiceClient != null)
            _punVoiceClient.Client.StateChanged -= VoiceClientStateChanged;
    }

    private void CheckMicPermission()
    {
        if (!ScreenOrientationManager._instance.isPotrait)
        {
            // There is two instance of this script
            // one used for Landscape & one for Portrait
            // Already Called For Landscape no need to call again.
            if (Application.isEditor)
            {
                SetMicByBtn();
                //PermissionPopusSystem.Instance.onCloseAction += SetMicByBtn;
                //PermissionPopusSystem.Instance.textType = PermissionPopusSystem.TextType.Mic;
                //PermissionPopusSystem.Instance.OpenPermissionScreen();
            }
            else
            {
#if UNITY_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                {
                    PermissionPopusSystem.Instance.onCloseAction += SetMicByBtn;
                    PermissionPopusSystem.Instance.textType = PermissionPopusSystem.TextType.Mic;
                    PermissionPopusSystem.Instance.OpenPermissionScreen();
                }
                else
                {
                    StartCoroutine(SetMic());
                }
#elif UNITY_IOS
                if(!Application.HasUserAuthorization(UserAuthorization.Microphone) && PlayerPrefs.GetInt("MicPermission", 0) == 0){
                      PermissionPopusSystem.Instance.onCloseAction += SetMicByBtn;
                    PermissionPopusSystem.Instance.textType = PermissionPopusSystem.TextType.Mic;
                    PermissionPopusSystem.Instance.OpenPermissionScreen();
                }
                else
                {
                    StartCoroutine(SetMic());
                }
#endif
            }
        }
    }

    public void SetMicByBtn()
    {
        if (ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            return;
        }
        PermissionPopusSystem.Instance.onCloseAction -= SetMicByBtn;
#if UNITY_IOS
    Application.RequestUserAuthorization(UserAuthorization.Microphone);
#endif
        if (this != null)
        {
            StartCoroutine(SetMic());
        }
        else
        {
            Debug.LogWarning("XanaVoiceChat instance has been destroyed, cannot start SetMic coroutine.");
        }
    }

    private IEnumerator SetMic()
    {
#if UNITY_IOS
        PlayerPrefs.SetInt("MicPermission", 1);
#endif

        //Adding delay because of loading screen stuck issue in rotation by getting permission popup. // Sohaib
        //yield return new WaitForSeconds(2f);

        //Debug.Log("Xana VoiceChat Start");
        recorder = GameObject.FindObjectOfType<Recorder>();
        _punVoiceClient = recorder.GetComponent<PunVoiceClient>();
        _punVoiceClient.Client.StateChanged += this.VoiceClientStateChanged;

        if (!ScreenOrientationManager._instance.isPotrait)
        {
            // There is two instance of this script
            // one used for Landscape & one for Portrait
            // Already Called For Landscape no need to call again.
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
        }
        //No Need to wait here if mic permission is not granted
        //while (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        //{
        //    yield return new WaitForSeconds(1f);
        //}

        if (recorder != null)
        {
            TurnOffMic();
            ConstantsHolder.xanaConstants.mic = 0;
        }

        if (ConstantsHolder.xanaConstants.pushToTalk)
        {
            micOffBtn.AddComponent<PushToTalk>();
            micOffBtnPotrait.AddComponent<PushToTalk>();
            TurnOffMic();
        }
        else
        {
            MicToggleOff = TurnOnMic;
            MicToggleOn = TurnOffMic;

            micOffBtn.GetComponent<Button>().onClick.AddListener(MicToggleOff);
            micOffBtnPotrait.GetComponent<Button>().onClick.AddListener(MicToggleOff);
            micOnBtn.GetComponent<Button>().onClick.AddListener(MicToggleOn);
            micOnBtnPotrait.GetComponent<Button>().onClick.AddListener(MicToggleOn);

        }
        yield return null;

    }
    public void TurnOnMic()
    {
        if(CheckUserAccessForMic())
        {
            Debug.Log("User can Unmute thier mic");
        }
        else
        {

            return;
        }

        if (Application.isEditor)
        {
            OpenToast(true);
        }
        else
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Debug.Log("Permission not granted");
                OpenToast();
                return;
            }
            else
            {
                OpenToast(true);
            }
#elif UNITY_IOS
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Debug.Log("Permission not granted");
                OpenToast();
                return;
            }else{
                OpenToast(true);
            }
#endif
        }

        if (ConstantsHolder.xanaConstants.mic == 0) // to confrim is correct value or not 
            ConstantsHolder.xanaConstants.mic = PlayerPrefs.GetInt("micSound");
        //if (ConstantsHolder.xanaConstants.mic == 0)
        //{
        //    GameObject go = Instantiate(mictoast, placetoload);
        //    Destroy(go, 1.5f);
        //    return;
        //}
        micOffBtn.SetActive(false);
        micOffBtnPotrait.SetActive(false);
        micOnBtn.SetActive(true);
        micOnBtnPotrait.SetActive(true);
        if (recorder != null)
        {
            recorder.TransmitEnabled = true;
            recorder.RecordingEnabled = true;
            // recorder.enabled = true;

        }
        ConstantsHolder.xanaConstants.PlayMic();
        //_punVoiceClient.enabled = true;
        //if (_punVoiceClient.ClientState == Photon.Realtime.ClientState.PeerCreated
        //             || _punVoiceClient.ClientState == Photon.Realtime.ClientState.Disconnected)
        //{
        //    _punVoiceClient.ConnectAndJoinRoom();
        //}

        VoiceClientStateChanged(ClientState.Joining, ClientState.Joined); // manually calling this method to force audio to speaker
    }

    public void TurnOffMic()
    {
        micOffBtn.SetActive(true);
        micOffBtnPotrait.SetActive(true);
        micOnBtn.SetActive(false);
        micOnBtnPotrait.SetActive(false);
        if (recorder != null)
        {
            recorder.TransmitEnabled = false;
            recorder.RecordingEnabled = false;
        }
        ConstantsHolder.xanaConstants.StopMic();
        //if (_punVoiceClient.ClientState == Photon.Realtime.ClientState.Joined)
        //{
        //    _punVoiceClient.Disconnect();
        //}
        // _punVoiceClient.enabled = false;
        //  recorder.enabled = false;
    }

    bool CheckUserAccessForMic()
    {
        if (ConstantsHolder.xanaConstants.UserCanUnmute)
            return true;
        else
            return false;

    }

    //Overriding methods for push to talk 
    public async void PushToTalk(bool canTalk)
    {
        if (canTalk)
        {
            micOffBtn.transform.GetChild(0).gameObject.SetActive(false);
            micOffBtnPotrait.transform.GetChild(0).gameObject.SetActive(false);
            if (recorder != null)
                recorder.TransmitEnabled = true;

        }
        else
        {
            micOffBtn.transform.GetChild(0).gameObject.SetActive(true);
            micOffBtnPotrait.transform.GetChild(0).gameObject.SetActive(true);
            while (recorder.IsCurrentlyTransmitting)
            {
                await Task.Delay(1000);
            }
            if (recorder != null)
                recorder.TransmitEnabled = false;

        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {

        base.OnDisconnected(cause);
        if (ConstantsHolder.xanaConstants.mic == 1 && !ConstantsHolder.xanaConstants.pushToTalk)
        {
            print(" OnDisconnected voice calling mic on");

            TurnOnMic();
        }
        else
        {
            TurnOffMic();
        }
    }

    public override void OnConnected()
    {
        base.OnConnected();
#if UNITY_IOS
        if ((Device.generation.ToString()).IndexOf("iPhone") > -1)//for iphones only
        { 
            iPhoneSpeaker.ForceToSpeaker();
        }
#endif
        if (ConstantsHolder.xanaConstants.mic == 1 && !ConstantsHolder.xanaConstants.pushToTalk)
        {
            print(" OnConnected voice calling mic on");
            TurnOnMic();

        }
        else
        {
            TurnOffMic();
        }
       
    }

    void ShowVoiceChatDialogBox()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, "Please turn on \"Voice\" from the settings", 0);
                toastObject.Call("show");
            }));
        }
#endif
    }

    private async void VoiceClientStateChanged(ClientState fromState, ClientState toState)
    {
        // print("!! fromState" + fromState);
        // print("!! toState" + toState);
        if (fromState == ClientState.Joining && toState == ClientState.Joined)
        {
            //   print("!!!!!!!!  FROCE CALL");
            await Task.Delay(2000);
            if (IsVoiceChatLoaded()) {
                if(!XanaPrivateChat.IsPrivateChatActive)
                    SetVoiceGroup(1);
            }
            // Handle state changes if needed
#if UNITY_IOS
        if ((Device.generation.ToString()).IndexOf("iPhone") > -1)
        {
            // For iPhones only
            Debug.Log("Forcing audio to speaker...");
            iPhoneSpeaker.ForceToSpeaker();
        }
#endif
        }
    }
    private bool IsVoiceChatLoaded()
    { 
      return recorder != null && _punVoiceClient != null && _punVoiceClient.Client.State == ClientState.Joined; 
    }
    public void SetVoiceGroup(byte newGroup)
    {
        byte oldGroup = currentGroup;
        currentGroup = newGroup;

        // Set the recorder's interest group to the new group
        recorder.InterestGroup = newGroup;

        // Change the groups: unsubscribe from the old group and subscribe to the new group
        if (PunVoiceClient.Instance.Client.InRoom)
            ChangeGroups(new byte[] { oldGroup }, new byte[] { newGroup });
        else
            Debug.LogError("Not connected to Game Server. Cannot change groups.");
    }

    private void ChangeGroups(byte[] groupsToLeave, byte[] groupsToJoin)
    {
        if (PunVoiceClient.Instance.Client.InRoom)
        {
            PunVoiceClient.Instance.Client.OpChangeGroups(groupsToLeave, groupsToJoin);
        }
        else
        {
            Debug.LogError("Operation ChangeGroups not allowed because the client is not connected to the Game Server.");
        }
    }
    private void OpenToast(bool isStereoToast = false)
    {
        if (isStereoToast)
        {
            if (ConstantsHolder.xanaConstants.ShowStereoToast)
            {
                string keyToLocalize = TextLocalization.GetLocaliseTextByKey("Stereo sound will be disabled when voice chat is enabled");
                StereoDescriptionText.text = keyToLocalize;
                StereoDescriptionText.gameObject.SetActive(true);
                AppSettingsDescriptionText.gameObject.SetActive(false);
                AppSettingsBtn.gameObject.SetActive(false);
                VoiceChatToastPanel.gameObject.SetActive(true);
                VoiceChatToastPanel.DOAnchorPos(new Vector2(VoiceChatToastPanel.anchoredPosition.x, -113f), 0.5f).SetEase(Ease.InOutSine);
                ConstantsHolder.xanaConstants.ShowStereoToast = false;
            }
        }
        else
        {
            string keyToLocalize = TextLocalization.GetLocaliseTextByKey("Microphone permission is required. Please allow it in the app settings.");
            AppSettingsDescriptionText.text = keyToLocalize;
            AppSettingsDescriptionText.gameObject.SetActive(true);
            StereoDescriptionText.gameObject.SetActive(false);
            AppSettingsBtn.gameObject.SetActive(true);
            AppSettingsBtn.onClick.AddListener(OpenAppSettings);
            VoiceChatToastPanel.gameObject.SetActive(true);
            VoiceChatToastPanel.DOAnchorPos(new Vector2(VoiceChatToastPanel.anchoredPosition.x, -113f), 0.5f).SetEase(Ease.InOutSine);
        }
    }
    void OpenAppSettings()
    {
        // Open app settings (works for Android and iOS)
#if UNITY_ANDROID
        try
        {
            using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS"))
            {
                string packageName = Application.identifier;
                using (AndroidJavaObject uri = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", "package:" + packageName))
                {
                    intent.Call<AndroidJavaObject>("setData", uri);
                }

                using (AndroidJavaObject unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    unityActivity.Call("startActivity", intent);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to open app settings: " + e.Message);
        }
#elif UNITY_IOS
        Application.OpenURL("app-settings:");
#else
        Debug.LogError("Opening app settings is not supported on this platform.");
#endif
        CloseToast();
    }
    public void CloseToast()
    {
        if (VoiceChatToastPanel.gameObject.activeInHierarchy)
        {
            VoiceChatToastPanel.DOAnchorPos(new Vector2(VoiceChatToastPanel.anchoredPosition.x, 45f), 0.5f).SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    VoiceChatToastPanel.gameObject.SetActive(false);
                    AppSettingsBtn.onClick.RemoveAllListeners();
                    //DescriptionText.text = "";
                });
        }
    }
}
