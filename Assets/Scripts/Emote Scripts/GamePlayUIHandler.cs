using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;
using Unity.Mathematics;

public class GamePlayUIHandler : MonoBehaviour
{
    static GamePlayUIHandler _inst;
    public static GamePlayUIHandler inst
    {
        get
        {
            if (_inst == null) _inst = FindObjectOfType<GamePlayUIHandler>();
            return _inst;
        }
    }

    public PlayerController ref_PlayerControllerNew;

    [Header("GamePlay ui")]
    public GameObject gamePlayUIParent;

    public GameObject actionsContainer;
    public Transform actionToggleImg;
    public ActionSelectionPanelHandler ActionSelectionPanel;
    public GameObject EmoteReactionPanelMainScreen;
    public GameObject FavoriteSelectionPanel;
    public GameObject AnimationBtnClose;
    public GameObject ReactionBtnClose;
    public Button rotateOrientationLand;
    public GameObject FightBtn;

    [Header("FPS Button Reference")]
    public GameObject fPSButton;
    public Button fPSOnButton;
    public Button fPSOffButton;

    public bool stopCurrentPlayingAnim = false;

    public LoadEmoteAnimations ref_LoadEmoteAnimations;

    [HideInInspector]
    public bool isHideButton = false;
    [HideInInspector]
    public bool isFreeCam = false;
    public GameObject portraitJoystick;
    public GameObject jumpBtn;
    public GameObject JumpUI;
    public GameObject ChatSystem;
    //Summit related UI References
    public EmailEntryUIController SummitCXOEmailAuthUIHandle;

    public GameObject JJPortalPopup;
    public GameObject currentPortalObject;
    public TextMeshProUGUI JJPortalPopupText;
    public string[] JJPortalPopupTextData;
    public MainCharactorControllerValue mainCharactorControllerValue = new MainCharactorControllerValue();


    [Header("Tap to info Data")]
    private int _maxTriesToFetchUserDetails = 3;
    public int ClickedPlayerActrNumber = -1;
    public string tapInfo_UserAvatar;
    public GameObject tapInfoPanel;
    public TextMeshProUGUI tapInfo_Name;
    public TextMeshProUGUI tapInfo_Organization;
    public TextMeshProUGUI tapInfo_Profession;
    public TextMeshProUGUI tapInfo_Industry;
    public TextMeshProUGUI tapInfo_Email;
    public TextMeshProUGUI tapInfo_Contact;
    public TextMeshProUGUI tapInfo_Join;
    public TextMeshProUGUI tapInfo_Hobbies;
    public TextMeshProUGUI tapInfo_Status;
    public GameObject tapInfo_GuestPanel;
    public GameObject tapInfo_UserPanel;
    public GameObject tapInfo_InviteBtn;
    public GameObject tapInfo_LoadingPanel;
    // sitting feature 
    public Button sitButton;         // Reference to the UI button for sitting
    public float NearestDistance = math.INFINITY;
    public class MainCharactorControllerValue
    {
        public float slopeLimit;
        public float stepOffcet;
        public float skinWidth;
        public float minMoveDistance;
        public Vector3 center;
        public float radius;
        public float height;
    }

    #region XANA PARTY WORLD
    [Header("Penpenz Leaderboard")]
    public TextMeshProUGUI MyRankText;
    public TextMeshProUGUI MyPointsText;
    public GameObject LeaderboardPanel;
    public GameObject PlayerLeaderboardStatsContainer;
    public GameObject PlayerLeaderboardStatsPrefab;
    public GameObject MoveToLobbyBtn;
    public GameObject SignInPopupForGuestUser;
    #endregion

    private void Start()
    {
        if (rotateOrientationLand)
            rotateOrientationLand.onClick.AddListener(ChangeOrientation);
        ref_PlayerControllerNew = ReferencesForGamePlay.instance.MainPlayerParent.GetComponent<PlayerController>();
        GetMainCharactorControllerValueHandler();

        CheckTheStatusFromServer();
    }

    public void GetMainCharactorControllerValueHandler()
    {
        //mainCharactorControllerValue.slopeLimit = ref_PlayerControllerNew.characterController.slopeLimit;
        //mainCharactorControllerValue.stepOffcet = ref_PlayerControllerNew.characterController.stepOffset;
        //mainCharactorControllerValue.skinWidth = ref_PlayerControllerNew.characterController.skinWidth;
        //mainCharactorControllerValue.minMoveDistance = ref_PlayerControllerNew.characterController.minMoveDistance;
        mainCharactorControllerValue.center = new Vector3(0, 0.86f, 0);
        mainCharactorControllerValue.radius = 0.2f;
        //mainCharactorControllerValue.height = ref_PlayerControllerNew.characterController.height;

    }

    public void SetMainCharactorControllerValueHandler()
    {
        //ref_PlayerControllerNew.characterController.slopeLimit = mainCharactorControllerValue.slopeLimit;
        //ref_PlayerControllerNew.characterController.stepOffset = mainCharactorControllerValue.stepOffcet;
        //ref_PlayerControllerNew.characterController.skinWidth = mainCharactorControllerValue.skinWidth;
        //ref_PlayerControllerNew.characterController.minMoveDistance = mainCharactorControllerValue.minMoveDistance;
        ref_PlayerControllerNew.characterController.center = mainCharactorControllerValue.center;
        ref_PlayerControllerNew.GetComponent<CharacterController>().radius = mainCharactorControllerValue.radius;
        //ref_PlayerControllerNew.characterController.height = mainCharactorControllerValue.height;

    }

    private void OnEnable()
    {
        if (_inst != this)
            _inst = this;

        ConstantsHolder.xanaConstants.EnableSignInPanelByDefault = false;
        LoadingHandler.Instance.EnterWheel += ((bo) => { if (bo) CloseEmoteAndReactionPanelOnWheel(); });
        fPSOnButton.onClick.AddListener(OnSwitchCameraClick);
        fPSOffButton.onClick.AddListener(OnSwitchCameraClick);
    }

    private void OnDisable()
    {
        LoadingHandler.Instance.EnterWheel -= ((bo) => { CloseEmoteAndReactionPanelOnWheel(); });
        fPSOnButton.onClick.RemoveAllListeners();
        fPSOffButton.onClick.RemoveAllListeners();
    }
    void ChangeOrientation()
    {
        ScreenOrientationManager._instance.ChangeOrientation_editor();
    }

    public void OnGotoAnotherWorldClick()
    {
        GamePlayButtonEvents.inst.OnGotoAnotherWorldClick();
    }

    public void OnWordrobeClick()
    {
        GamePlayButtonEvents.inst.OnWordrobeClick();
    }

    public void OnHelpButtonClick(bool isOn)
    {
        if (PlayerController.isJoystickDragging && ReferencesForGamePlay.instance.playerControllerNew)
            ReferencesForGamePlay.instance.playerControllerNew.restJoyStick();
        if (ConstantsHolder.IsSummitDomeWorld)
        {
            LoadingHandler.Instance.InstructionIntoWorld(isOn);
        }
        else
        {
            gamePlayUIParent.SetActive(!isOn);//rik.......
            JumpUI.SetActive(!isOn);
            ChatSystem.SetActive(!isOn);
            GamePlayButtonEvents.inst.UpdateHelpObjects(isOn);
        }
    }

    public void HelpScreen(bool isOn) // Display help screen if no instruction is available
    {
        gamePlayUIParent.SetActive(!isOn);
        JumpUI.SetActive(!isOn);
        ChatSystem.SetActive(!isOn);
        GamePlayButtonEvents.inst.UpdateHelpObjects(isOn);
    }


    public void OnSettingButtonClick()
    {
        if (PlayerController.isJoystickDragging && ReferencesForGamePlay.instance.playerControllerNew)
            ReferencesForGamePlay.instance.playerControllerNew.restJoyStick();
        GamePlayButtonEvents.inst.OnSettingButtonClick();
    }

    public void OnExitButtonClick()
    {
        ConstantsHolder.xanaConstants.LastLobbyName = "";
        if (ConstantsHolder.isFromXANASummit)
        {
            ConstantsHolder.IsSummitDomeWorld = false;
        }

        if (ReferencesForGamePlay.instance != null && ReferencesForGamePlay.instance.XANAPartyCounterPanel.activeInHierarchy)
            ReferencesForGamePlay.instance.XANAPartyCounterPanel.SetActive(false);

        GamePlayButtonEvents.inst.OnExitButtonClick();
        SetMainCharactorControllerValueHandler();
        ConstantsHolder.xanaConstants.haveSubDomeEnabled = false;
    }

    public void OnPeopeClick()
    {
        GamePlayButtonEvents.inst.OnPeopeClick();
    }

    public void OnAnnouncementClick()
    {
        GamePlayButtonEvents.inst.OnAnnouncementClick();
    }

    public void OnInviteClick()
    {
        GamePlayButtonEvents.inst.OnInviteClick();
    }

    public void EnableJJPortalPopup(GameObject obj, int indexForText)
    {
        if (LoadingHandler.Instance != null)
        {
            LoadingHandler.Instance.ResetLoadingValues();
        }
        JJPortalPopupText.text = JJPortalPopupTextData[indexForText].ToString();
        currentPortalObject = obj;
        JJPortalPopup.SetActive(true);
    }

    public void MoveFromPortal()
    {
        JJPortalPopup.SetActive(false);
        ref_PlayerControllerNew.m_IsMovementActive = true;
        if (currentPortalObject.GetComponent<PlayerPortal>())
            currentPortalObject.GetComponent<PlayerPortal>().RedirectToWorld();
        else if (currentPortalObject.GetComponent<JjWorldChanger>())
            currentPortalObject.GetComponent<JjWorldChanger>().RedirectToWorld();
        else if (currentPortalObject.GetComponent<MeetingRoomTeleport>())
            currentPortalObject.GetComponent<MeetingRoomTeleport>().RedirectToWorld();
    }

    public void ClosePortalPopup()
    {
        JJPortalPopup.SetActive(false);
        ref_PlayerControllerNew.m_IsMovementActive = true;
    }

    public void OnSwitchCameraClick()
    {
        if (!UserPassManager.Instance.CheckSpecificItem("fp_camera"))
        {
            print("Please Upgrade to Premium account");
            return;
        }
        else
        {
            print("Horayyy you have Access");
        }

        GamePlayButtonEvents.inst.OnSwitchCameraClick();
    }

    public void OnChangehighlightedFPSbutton(bool isSelected)
    {
        //fPSButton.GetComponent<Image>().enabled = isSelected;
        if (isSelected)
        {
            fPSOffButton.gameObject.SetActive(false);
            fPSOnButton.gameObject.SetActive(true);
        }
        else
        {
            fPSOffButton.gameObject.SetActive(true);
            fPSOnButton.gameObject.SetActive(false);
        }
    }

    public void OnSelfiBtnClick()
    {
        GamePlayButtonEvents.inst.OnSelfieClick();
    }

    public void OnOpenAnimationPanel()
    {
        ref_LoadEmoteAnimations.OpenAnimationSelectionPanel();
        Debug.Log("call hua times 3===" + GamePlayButtonEvents.inst.selectionPanelOpen);
        GamePlayButtonEvents.inst.selectionPanelOpen = true;
        GamePlayButtonEvents.inst.OpenAllAnims();
    }

    public void CloseEmoteAndReactionPanelOnWheel()
    {
        CloseAnimationButtonClick();
        CloseReactionButtonClick();
    }

    public void CloseAnimationButtonClick()
    {
        try
        {
            if (!ActionManager.IsAnimRunning)
                AnimationBtnClose.SetActive(false);
            EmoteReactionPanelMainScreen.SetActive(false);
            FavoriteSelectionPanel.SetActive(false);
        }
        catch (System.Exception)
        {
            print("Some error thrown here");
        }


    }

    public void CloseReactionButtonClick()
    {
        try
        {
            if (ActionManager.IsAnimRunning)
                AnimationBtnClose.SetActive(true);
            ReactionBtnClose.SetActive(false);
            EmoteReactionPanelMainScreen.SetActive(false);
            FavoriteSelectionPanel.SetActive(false);
        }
        catch (System.Exception)
        {
            print("ReactionBtnClose is not null");
        }

    }

    public void CloseEmoteSelectionPanel()
    {
        BuilderEventManager.UIToggle?.Invoke(false);

        EmoteAnimationHandler.Instance.isEmoteActive = false;      // AH working

        if (ActionManager.IsAnimRunning)                            // AH working
        {
            ActionManager.StopActionAnimation?.Invoke();

            // EmoteAnimationHandler.Instance.StopAnimation();
        }

        ref_LoadEmoteAnimations.CloseAnimationSelectionPanel();
        GamePlayButtonEvents.inst.CloseEmoteSelectionPanel();

        if (ReactionFilterManager.Instance && ReactionFilterManager.Instance.gameObject.activeInHierarchy)            // AH working
            ReactionFilterManager.Instance.HideReactionPanel();


        ReactScreen.Instance.HideEmoteScreen();
        // GamePlayButtonEvents.inst.CloseEmoteSelectionPanel();
        GamePlayButtonEvents.inst.selectionPanelOpen = false;

    }

    public void OnJumpBtnUp()
    {
        GamePlayButtonEvents.inst.OnJumpBtnUp();
    }

    public void OnJumpBtnDown()
    {
        GamePlayButtonEvents.inst.OnJumpBtnDown();
    }

    bool isActionShowing;
    public void OnActionsToggleClicked()
    {
        if (ScreenOrientationManager._instance.isPotrait)
        {
            if (ScreenOrientationManager._instance.joystickInitPosY == 0)
                ScreenOrientationManager._instance.joystickInitPosY = portraitJoystick.transform.localPosition.y;
        }
        if (!UserPassManager.Instance.CheckSpecificItem("env_actions"))
        {
            print("Please Upgrade to Premium account");
            return;
        }
        else
        {
            print("Horayyy you have Access");
        }

        isActionShowing = !isActionShowing;
        actionsContainer.SetActive(isActionShowing);
        Vector3 rot = new Vector3(0f, 0f, (isActionShowing) ? 0f : 180f);
        actionToggleImg.rotation = Quaternion.Euler(rot);
        if (jumpBtn)
            jumpBtn.transform.DOLocalMoveX((isActionShowing) ? 277f : 372.6f, 0.1f);
    }
    public void EnableJJPortalPopup(GameObject obj)
    {
        currentPortalObject = obj;
        JJPortalPopup.SetActive(true);
    }

    #region XANA PARTY WORLD
    public void MoveToLobbyBtnClick()
    {
        XANAPartyManager.Instance.GetComponent<PenpenzLpManager>().RaceStartWithPlayers = 0;
        XANAPartyManager.Instance.GetComponent<PenpenzLpManager>().PlayerIDs.Clear();
        XANAPartyManager.Instance.GetComponent<PenpenzLpManager>().WinnerPlayerIds.Clear();
        XANAPartyManager.Instance.GetComponent<PenpenzLpManager>().RaceFinishTime.Clear();
        XANAPartyManager.Instance.GetComponent<PenpenzLpManager>().isLeaderboardShown = false;
        XANAPartyManager.Instance.GetComponent<PenpenzLpManager>().ResetGame();
        ConstantsHolder.xanaConstants.isXanaPartyWorld = false;
        ConstantsHolder.xanaConstants.isBuilderGame = false;
        ConstantsHolder.xanaConstants.isJoinigXanaPartyGame = false;
        ConstantsHolder.xanaConstants.LastLobbyName = "";
        //StartCoroutine(GameplayEntityLoader.instance.PenguinPlayer.GetComponent<XANAPartyMulitplayer>().MoveToLobby());
        //LeaderboardPanel.SetActive(false);
        //ReferencesForGamePlay.instance.SetGameplayForPenpenz(true);
        GamePlayButtonEvents.inst.OnExitButtonClick();
    }

    public void OnSignInBtnClick()
    {
        LoadingHandler.Instance.ShowLoading();
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        ConstantsHolder.xanaConstants.EnableSignInPanelByDefault = true;
        GameplayEntityLoader.instance._uiReferences.LoadMain(false);
        SignInPopupForGuestUser.SetActive(false);
    }
    #endregion

    #region Tap to Info

    public void ShowInfoPanel(string _otherUserId, string _userName)
    {
        Debug.Log("Show Info Panel");
        PlayerCameraController.instance.isReturn = true;
        if (String.IsNullOrEmpty(_otherUserId) || _otherUserId.Contains("guest", StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("Id is not Set yet");
            tapInfo_Name.text = _userName;
            tapInfo_GuestPanel.SetActive(true);
            tapInfo_UserPanel.SetActive(false);
            tapInfo_InviteBtn.SetActive(false);
            tapInfo_LoadingPanel.SetActive(false);

            tapInfoPanel.SetActive(true);
            //PlayerCameraController.instance.isReturn = true;
            return;
        }
        else
        {
            tapInfo_Name.text = _userName;
            tapInfo_GuestPanel.SetActive(false);
            tapInfo_UserPanel.SetActive(false);
            tapInfo_LoadingPanel.SetActive(true);
            tapInfoPanel.SetActive(true);
        }
        XanaChatSystem.instance.xanaPrivateChat.SetReceiverId(_otherUserId);
        CheckTheStatusFromServer();
        StartCoroutine(GetUserDetails(_otherUserId, _userName));
    }
    IEnumerator GetUserDetails(string UserId, string clickedObjName)
    {
        Debug.Log("Here At User Details");
        string url = ConstantsGod.API_BASEURL + ConstantsGod.GetUserDetailsAPI + "?userId=" + UserId;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", ConstantsGod.AUTH_TOKEN);

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            UserData requireData = new UserData();
            if (request.result != UnityWebRequest.Result.ConnectionError && request.result == UnityWebRequest.Result.Success)
            {
                UserDetailsRoot root = JsonUtility.FromJson<UserDetailsRoot>(request.downloadHandler.text);
                requireData = root.data;

                if (request.error == null)
                {
                    if (root.success)
                    {
                        Debug.Log("API Success to Load ");
                        tapInfo_GuestPanel.SetActive(false);
                        tapInfo_UserPanel.SetActive(true);
                        tapInfo_LoadingPanel.SetActive(false);

                        tapInfoPanel.SetActive(true);
                        tapInfo_Name.text = requireData.name;

                        //PlayerCameraController.instance.isReturn = true;

                        tapInfo_Organization.text = requireData.userProfile.organizationName;
                        tapInfo_Profession.text = requireData.userProfile.job;
                        tapInfo_Industry.text = requireData.userProfile.industry;
                        tapInfo_Email.text = requireData.userProfile.secondaryEmail;
                        tapInfo_Contact.text = requireData.userProfile.contactNo;
                        tapInfo_Join.text = requireData.userProfile.reasonToJoin;
                        tapInfo_Hobbies.text = string.Join(", ", requireData.tags);
                        tapInfo_Status.text = SNS_APIManager.DecodedString(requireData.userProfile.bio);
                        tapInfo_UserAvatar = requireData.avatar;

                    }
                }
                _maxTriesToFetchUserDetails = 3;
            }
            else
            {
                Debug.Log("API Fail to Load " + request.downloadHandler.text);

                tapInfo_Name.text = clickedObjName;
                GetUserDetailsFailed(UserId, clickedObjName);
                //tapInfo_GuestPanel.SetActive(false);
                //tapInfo_UserPanel.SetActive(true);
                //tapInfo_LoadingPanel.SetActive(false);
                //tapInfoPanel.SetActive(true);
            }
        }
    }
    void GetUserDetailsFailed(string _otherUserId, string _userName)
    {
        if (_maxTriesToFetchUserDetails > 0)
        {
            _maxTriesToFetchUserDetails--;
            StartCoroutine(GetUserDetails(_otherUserId, _userName));
        }
    }
    void CheckTheStatusFromServer()
    {
        var xanaConstants = ConstantsHolder.xanaConstants;

        var componentsWithConstants = new (GameObject component, bool constant)[]
        {
            (tapInfo_Organization.transform.parent.gameObject, xanaConstants.tapToKnow_organization),
            (tapInfo_Profession.transform.parent.gameObject, xanaConstants.tapToKnow_profession),
            (tapInfo_Industry.transform.parent.gameObject, xanaConstants.tapToKnow_industry),
            (tapInfo_Email.transform.parent.gameObject, xanaConstants.tapToKnow_email),
            (tapInfo_Contact.transform.parent.gameObject, xanaConstants.tapToKnow_contact),
            (tapInfo_Join.transform.parent.gameObject, xanaConstants.tapToKnow_join),
            (tapInfo_Hobbies.transform.parent.gameObject, xanaConstants.tapToKnow_hobbies),
            (tapInfo_Status.transform.parent.gameObject, xanaConstants.tapToKnow_status)
        };

        foreach (var item in componentsWithConstants)
        {
            item.component.SetActive(item.constant);
        }
    }
    public void CloseInfoPanel()
    {
        tapInfoPanel.SetActive(false);

        PlayerCameraController.instance.isReturn = false;
        tapInfo_UserPanel.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        ClickedPlayerActrNumber = -1;
        SetDefaultValue_TapToInfo();
    }
    void SetDefaultValue_TapToInfo()
    {
        tapInfo_Name.text = "";
        tapInfo_Organization.text = "";
        tapInfo_Profession.text = "";
        tapInfo_Industry.text = "";
        tapInfo_Email.text = "";
        tapInfo_Contact.text = "";
        tapInfo_Join.text = "";
        tapInfo_Hobbies.text = "";
        tapInfo_Status.text = "";
    }
    #endregion


}
[Serializable]
public class UserProfile
{
    public string bio;
    public string job;
    public string organizationName;
    public string industry;
    public string contactNo;
    public string secondaryEmail;
    public string reasonToJoin;
}

[Serializable]
public class UserData
{
    public int id;
    public string name;
    public string avatar;
    public List<string> tags;
    public UserProfile userProfile;
}

[Serializable]
public class UserDetailsRoot
{
    public bool success;
    public UserData data;
    public string msg;
}