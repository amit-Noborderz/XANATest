using Crosstales;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using SuperStar.Helpers;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static XANASummitDataContainer;

public class XANASummitSceneLoading : MonoBehaviour
{
    [SerializeField]
    private StayTimeTrackerForSummit _stayTimeTrackerForSummit;
    public static Vector3 playerPos;
    public static Vector3 playerRot;
    public static Vector3 playerScale;
    public static Action<bool> OnJoinSubItem; // Car, GiantWheel, Planets

    public MutiplayerController multiplayerController;

    public GameplayEntityLoader gameplayEntityLoader;

    public SubWorldsHandler SubWorldsHandlerInstance;

    public XANASummitDataContainer dataContainer;

    [SerializeField]
    private DomeMinimapDataHolder _domeMiniMap;

    public SummitDomePAAIController DomePerformerAvatarHandler;

    public delegate void SetPlayerOnSubworldBack();
    public static event SetPlayerOnSubworldBack setPlayerPositionDelegate;

    int prevstate;

    private void OnEnable()
    {
        setPlayerPositionDelegate = null;
        BuilderEventManager.LoadNewScene += LoadingFromDome;
        BuilderEventManager.LoadSceneByName += LoadingSceneByIDOrName;
        BuilderEventManager.LoadSummitScene += LoadDomesData;
        BuilderEventManager.AfterPlayerInstantiated += SetPlayerTransform;
        GamePlayButtonEvents.OnExitButtonXANASummit += LoadingXANASummitOnBack;
        OnJoinSubItem += SummitMiniMapStatusOnSceneChange;


        if (LoadingHandler.Instance.nftLoadingScreen.activeInHierarchy || LoadingHandler.Instance.LoadingScreenSummit.activeInHierarchy)
        {
            LoadingHandler.Instance.nftLoadingScreen.SetActive(false);
            LoadingHandler.Instance.LoadingScreenSummit.SetActive(false);
        }
    }


    private void OnDisable()
    {
        BuilderEventManager.LoadNewScene -= LoadingFromDome;
        BuilderEventManager.LoadSceneByName -= LoadingSceneByIDOrName;
        BuilderEventManager.LoadSummitScene -= LoadDomesData;
        BuilderEventManager.AfterPlayerInstantiated -= SetPlayerTransform;
        GamePlayButtonEvents.OnExitButtonXANASummit -= LoadingXANASummitOnBack;
        OnJoinSubItem -= SummitMiniMapStatusOnSceneChange;
    }



    async void LoadDomesData()
    {
        dataContainer.GetAllDomesData();
        dataContainer.GetAllSubDomeData();
        dataContainer.GetDomeInstruction();
        dataContainer.GetDomeAccessList();
    }


    void SummitMiniMapStatusOnSceneChange(bool makeActive)
    {
        GameplayEntityLoader.instance.IsJoinSummitWorld = !makeActive;
        if (!makeActive)
        {
            prevstate = ConstantsHolder.xanaConstants.minimap;
            ConstantsHolder.xanaConstants.minimap = 0;
        }
        else { ConstantsHolder.xanaConstants.minimap = prevstate; }

        if (makeActive && ConstantsHolder.xanaConstants.minimap == 1)
        {
            ReferencesForGamePlay.instance.minimap.SetActive(true);
            ReferencesForGamePlay.instance.SumitMapStatus(true);
        }
        else
        {
            ReferencesForGamePlay.instance.SumitMapStatus(false);
            ReferencesForGamePlay.instance.minimap.SetActive(false);
        }
    }

    async void LoadingFromDome(int domeId, Vector3 playerPos, bool IsSubWorld)
    {
        XANASummitDataContainer.DomeGeneralData domeGeneralData = new XANASummitDataContainer.DomeGeneralData();
        if (IsSubWorld)
            domeGeneralData = dataContainer.GetSubDomeData(domeId, ConstantsHolder.SubDomeId);
        else
            domeGeneralData = GetDomeData(domeId);
        int worldId = domeGeneralData.worldType == true ? domeGeneralData.builderWorldId : domeGeneralData.worldId;
        ConstantsHolder.visitorCount = await dataContainer.GetVisitorCount(worldId.ToString());
        if (string.IsNullOrEmpty(domeGeneralData.world))
        {
            return;
        }


        if (domeGeneralData.is_penpenz && ConstantsHolder.xanaConstants.LoggedInAsGuest)
        {
            GamePlayUIHandler.inst.SignInPopupForGuestUser.SetActive(true);
            return;
        }

        LoadingHandler.Instance.LoadInstructionData(dataContainer.GetDomeInstruction(domeGeneralData.id));

        if (IsSubWorld)
            ConstantsHolder.HaveSubWorlds = false;
        else
            ConstantsHolder.HaveSubWorlds = dataContainer.DomeHasSubWorld(domeGeneralData.id);
        if (ConstantsHolder.HaveSubWorlds)
        {
            ConstantsHolder.domeId = domeId;
            LoadingHandler.Instance.DisableDomeLoading();
            bool Success = await SubWorldsHandlerInstance.CreateSubWorldList(domeGeneralData, playerPos);
            if (Success)
                return;
        }
        else
        {
            #region WaitingForPLayerApproval
            LoadingHandler.Instance.showApprovaldomeloading(domeGeneralData);
            SubWorldsHandlerInstance.OnEnteredIntoWorld();
            while (LoadingHandler.Instance.WaitForInput)
            {
                await Task.Delay(1000);
            }
            if (!LoadingHandler.Instance.enter)
            {
                ConstantsHolder.DiasableMultiPartPhoton = false;
                return;
            }

            LoadingHandler.Instance.enter = false;

            #endregion
            if (ConstantsHolder.MultiSectionPhoton)
            {
                ConstantsHolder.DiasableMultiPartPhoton = true;
            }
        }

        SummitMiniMapStatusOnSceneChange(false);
        //StartCoroutine(LoadingHandler.Instance.FadeIn());
        //  LoadingHandler.Instance.ShowVideoLoading();
        ReferencesForGamePlay.instance.ChangeExitBtnImage(false);

        Vector3[] currentPlayerPos = GetPlayerPosition(playerPos);


        string sceneTobeUnload = WorldItemView.m_EnvName;

        XANASummitDataContainer.StackInfoWorld subWorldInfo = new XANASummitDataContainer.StackInfoWorld();
        subWorldInfo.id = ConstantsHolder.xanaConstants.MuseumID;
        subWorldInfo.name = sceneTobeUnload;
        subWorldInfo.isBuilderWorld = ConstantsHolder.xanaConstants.isBuilderScene;
        subWorldInfo.user_limit = ConstantsHolder.userLimit;
        subWorldInfo.domeId = ConstantsHolder.domeId;
        subWorldInfo.thumbnail = ConstantsHolder.Thumbnail;
        subWorldInfo.description = ConstantsHolder.description;
        subWorldInfo.haveSubWorlds = ConstantsHolder.HaveSubWorlds;
        subWorldInfo.isFromSummitWorld = ConstantsHolder.isFromXANASummit;
        subWorldInfo.playerTrasnform = currentPlayerPos;
        subWorldInfo.domeType = ConstantsHolder.DomeType;
        subWorldInfo.domeCategory = ConstantsHolder.DomeCategory;
        subWorldInfo.creator = ConstantsHolder.CreatorName;
        XANASummitDataContainer.LoadedScenesInfo.Push(subWorldInfo);


        ConstantsHolder.isFromXANASummit = true;
        ConstantsHolder.domeId = domeId;

        WorldItemView.m_EnvName = domeGeneralData.world;
        ConstantsHolder.xanaConstants.EnviornmentName = domeGeneralData.world;
        gameplayEntityLoader.addressableSceneName = domeGeneralData.world;
        ConstantsHolder.userLimit = domeGeneralData.maxPlayer;
        ConstantsHolder.isPenguin = domeGeneralData.IsPenguin;
        if (domeGeneralData.isBuilderGame)
        {
            ConstantsHolder.xanaConstants.isXanaPartyWorld = false;
            ConstantsHolder.xanaConstants.isBuilderGame = true;
            ConstantsHolder.XanaPartyMaxPlayers = 1;
        }
        else
            ConstantsHolder.xanaConstants.isXanaPartyWorld = domeGeneralData.is_penpenz;
        // ConstantsHolder.xanaConstants.isXanaPartyWorld = domeGeneralData.is_penpenz;
        ConstantsHolder.isFixedHumanoid = domeGeneralData.Ishumanoid;
        ConstantsHolder.Thumbnail = domeGeneralData.domes_media_v2.thumbnail;
        if (LocalizationManager.forceJapanese || GameManager.currentLanguage == "ja")
            ConstantsHolder.description = domeGeneralData.jpDescription;
        else
            ConstantsHolder.description = domeGeneralData.description;
        // ConstantsHolder.AvatarIndex = domeGeneralData.AvatarIndex;
        if (domeGeneralData.worldType)
            ConstantsHolder.xanaConstants.MuseumID = domeGeneralData.builderWorldId.ToString();
        else
            ConstantsHolder.xanaConstants.MuseumID = domeGeneralData.worldId.ToString();


        (bool IsSpecialAvatar, string SpecialAvatarJson) = DomeAccessHandler.CheckPlayerForSpecialAvtar(dataContainer.GetDomeAccessData(ConstantsHolder.domeId));



        if (IsSpecialAvatar)
        {
            ConstantsHolder.isFixedHumanoid = true;
            XANASummitDataContainer.FixedAvatarJson = SpecialAvatarJson;
            ConstantsHolder.xanaConstants.SetPlayerProperties(SpecialAvatarJson);
        }
        else if (domeGeneralData.Ishumanoid)
        {
            XANASummitDataContainer.FixedAvatarJson = domeGeneralData.Avatarjson;
            ConstantsHolder.xanaConstants.SetPlayerProperties(XANASummitDataContainer.FixedAvatarJson);
        }
        else
        {
            ConstantsHolder.xanaConstants.SetPlayerProperties();
        }
        gameplayEntityLoader.currentEnvironment = null;
        multiplayerController.singlePlayerInstance = domeGeneralData.experienceType != "double";
        multiplayerController.isConnecting = false;
        gameplayEntityLoader.isEnvLoaded = false;
        gameplayEntityLoader.isAlreadySpawned = true;
        ReferencesForGamePlay.instance.ChangeExitBtnImage(false);
        if (!ConstantsHolder.HaveSubWorlds)
            LoadingHandler.Instance.startLoading();
        XanaWorldDownloader.ResetAll();
        BuilderEventManager.ResetSummit?.Invoke();
        while (MutiplayerController.instance.isShifting || !PhotonNetwork.InRoom)
        {
            await Task.Delay(1000);
        }
        multiplayerController.Disconnect();
        multiplayerController.playerobjects.Clear();

        ReferencesForGamePlay.instance.m_34player.transform.localScale = new Vector3(0, 0, 0);

        await UnloadScene(sceneTobeUnload);

        await HomeSceneLoader.ReleaseUnsedMemory();

        if (domeGeneralData.worldType)
            LoadBuilderSceneLoading(domeGeneralData.builderWorldId);
        else
        {
            ConstantsHolder.xanaConstants.LastLobbyName = "SaudiExpo-" + ConstantsHolder.domeId + "-" + domeGeneralData.world;
            multiplayerController.Connect("SaudiExpo-" + ConstantsHolder.domeId + "-" + domeGeneralData.world);
        }

        DomePerformerAvatarHandler.InitPerformerAvatarNPC();
        if (XanaPrivateChat.IsPrivateChatActive)
        {
            XanaChatSystem.instance.xanaPrivateChat.LeavePrivateChatBtn();
        }
        // Summit Analytics Part
        if (_stayTimeTrackerForSummit != null)
        {
            if (_stayTimeTrackerForSummit.IsTrackingTimeForExteriorArea)
            {
                _stayTimeTrackerForSummit.StopTrackingTime();
                //_stayTimeTrackerForSummit.CalculateAndLogStayTime();
                _stayTimeTrackerForSummit.IsTrackingTimeForExteriorArea = false;
            }
            _stayTimeTrackerForSummit.DomeId = domeId;
            _stayTimeTrackerForSummit.IsBuilderWorld = domeGeneralData.worldType;
            string eventName;
            if (domeGeneralData.worldType)
            {
                _stayTimeTrackerForSummit.DomeWorldId = domeGeneralData.builderWorldId;
                eventName = "TV_Dome_" + domeId + "_BW_" + domeGeneralData.builderWorldId;
            }
            else
            {
                _stayTimeTrackerForSummit.DomeWorldId = domeGeneralData.worldId;
                eventName = "TV_Dome_" + domeId + "_XW_" + domeGeneralData.worldId;
            }
            GlobalConstants.SendFirebaseEventForSummit(eventName);
            _stayTimeTrackerForSummit.StartTrackingTime();
        }
        // For Single Dome
        string World = "";
        if (domeGeneralData.worldType)
            World = "USER";
        else if (ConstantsHolder.xanaConstants.IsMuseum)
            World = "MUSEUM";
        else
            World = "ENVIRONMENT";
        if (domeGeneralData.worldType)
        {
            UserAnalyticsHandler.onGetWorldId?.Invoke(domeGeneralData.builderWorldId, World);
        }
        else
        {
            UserAnalyticsHandler.onGetWorldId?.Invoke(domeGeneralData.worldId, World);
        }
        if (ReferencesForGamePlay.instance.playerControllerNew.isFirstPerson)
        {
            GamePlayUIHandler.inst.OnSwitchCameraClick();
        }
        //GameplayEntityLoader.instance.ForcedMapOpenForSummitScene();

        if (ActionManager.IsAnimRunning)
        {
            ActionManager.StopActionAnimation?.Invoke();

            //  EmoteAnimationHandler.Instance.StopAnimation();
            //  EmoteAnimationHandler.Instance.StopAllCoroutines();
        }
        GameplayEntityLoader.instance.AssignRaffleTickets(domeId);
    }



    public async void LoadingSceneByIDOrName(string worldId, Vector3 playerPos)
    {
        if (string.IsNullOrEmpty(worldId))
        {
            Debug.LogError("null World");
            return;
        }

        //StartCoroutine(LoadingHandler.Instance.FadeIn());
        //LoadingHandler.Instance.ShowVideoLoading();
        //SummitMiniMapStatusOnSceneChange(false);
        Vector3[] currentPlayerPos = GetPlayerPosition(playerPos);

        string sceneToBeUnload = WorldItemView.m_EnvName;

        ConstantsHolder.visitorCount = await dataContainer.GetVisitorCount(worldId);
        SingleWorldInfo worldInfo = await GetSingleWorldData(worldId);

        #region WaitingForPLayerApproval
        LoadingHandler.Instance.showApprovaldomeloading(worldInfo);
        SubWorldsHandlerInstance.OnEnteredIntoWorld();
        while (LoadingHandler.Instance.WaitForInput)
        {
            await Task.Delay(1000);
        }
        if (!LoadingHandler.Instance.enter) { ConstantsHolder.DiasableMultiPartPhoton = false; return; }

        LoadingHandler.Instance.enter = false;

        #endregion

        SummitMiniMapStatusOnSceneChange(false);

        if (ConstantsHolder.MultiSectionPhoton)
        {
            ConstantsHolder.DiasableMultiPartPhoton = true;
        }
        XANASummitDataContainer.StackInfoWorld subWorldInfo = new XANASummitDataContainer.StackInfoWorld();
        subWorldInfo.id = ConstantsHolder.xanaConstants.MuseumID;
        subWorldInfo.name = sceneToBeUnload;
        subWorldInfo.isBuilderWorld = ConstantsHolder.xanaConstants.isBuilderScene;
        subWorldInfo.user_limit = ConstantsHolder.userLimit;
        subWorldInfo.domeId = ConstantsHolder.domeId;
        subWorldInfo.haveSubWorlds = ConstantsHolder.HaveSubWorlds;
        subWorldInfo.isFromSummitWorld = ConstantsHolder.isFromXANASummit;
        subWorldInfo.thumbnail = ConstantsHolder.Thumbnail;
        subWorldInfo.description = ConstantsHolder.description;
        subWorldInfo.playerTrasnform = currentPlayerPos;
        subWorldInfo.creator = ConstantsHolder.CreatorName;
        subWorldInfo.domeType = ConstantsHolder.DomeType;
        subWorldInfo.domeCategory=ConstantsHolder.DomeCategory;
        XANASummitDataContainer.LoadedScenesInfo.Push(subWorldInfo);

        ConstantsHolder.isFromXANASummit = true;
        WorldItemView.m_EnvName = worldInfo.data.name;
        ConstantsHolder.xanaConstants.EnviornmentName = worldInfo.data.name;
        gameplayEntityLoader.addressableSceneName = worldInfo.data.name;
        ConstantsHolder.userLimit = worldInfo.data.user_limit;
        ConstantsHolder.xanaConstants.MuseumID = worldInfo.data.id;
        //ConstantsHolder.HaveSubWorlds = false;
        ConstantsHolder.Thumbnail = worldInfo.data.thumbnail;
        ConstantsHolder.description = worldInfo.data.description;
        ConstantsHolder.xanaConstants.isBuilderScene = worldInfo.data.entityType == "USER_WORLD" ? true : false;
        gameplayEntityLoader.currentEnvironment = null;
        multiplayerController.isConnecting = false;
        gameplayEntityLoader.isEnvLoaded = false;
        gameplayEntityLoader.isAlreadySpawned = true;
        LoadingHandler.Instance.startLoading();
        ReferencesForGamePlay.instance.m_34player.transform.localScale = new Vector3(0, 0, 0);
        while (MutiplayerController.instance.isShifting || !PhotonNetwork.InRoom)
        {
            await Task.Delay(1000);
        }
        multiplayerController.Disconnect();

        XanaWorldDownloader.ResetAll();
        BuilderEventManager.ResetSummit?.Invoke();


        multiplayerController.playerobjects.Clear();

        await UnloadScene(sceneToBeUnload);

        await HomeSceneLoader.ReleaseUnsedMemory();

        if (ConstantsHolder.xanaConstants.isBuilderScene)
            LoadBuilderSceneLoading(int.Parse(worldInfo.data.id));
        else
        {
            ConstantsHolder.xanaConstants.LastLobbyName = "SaudiExpo-" + ConstantsHolder.domeId + "-" + worldInfo.data.name;
            multiplayerController.Connect("SaudiExpo-" + ConstantsHolder.domeId + "-" + worldInfo.data.name);
        }
        DomePerformerAvatarHandler.InitPerformerAvatarNPC();
        if (ReferencesForGamePlay.instance.playerControllerNew.isFirstPerson)
        {
            GamePlayUIHandler.inst.OnSwitchCameraClick();
        }
        if (XanaPrivateChat.IsPrivateChatActive)
        {
            XanaChatSystem.instance.xanaPrivateChat.LeavePrivateChatBtn();
        }
        if (SubWorldsHandlerInstance.IsEnteringInSubWorld)
        {
            SubWorldsHandlerInstance.IsEnteringInSubWorld = false;
            SubWorldsHandlerInstance.CallAnalyticsForSubWorlds();
        }
        else
        {
            if (_stayTimeTrackerForSummit != null)
            {
                if (_stayTimeTrackerForSummit.IsTrackingTimeForExteriorArea)
                {
                    _stayTimeTrackerForSummit.StopTrackingTime();
                    //_stayTimeTrackerForSummit.CalculateAndLogStayTime();
                    _stayTimeTrackerForSummit.IsTrackingTimeForExteriorArea = false;
                }
                _stayTimeTrackerForSummit.DomeId = subWorldInfo.domeId;
                _stayTimeTrackerForSummit.IsBuilderWorld = ConstantsHolder.xanaConstants.isBuilderScene;
                string eventName;
                if (ConstantsHolder.xanaConstants.isBuilderScene)
                {
                    _stayTimeTrackerForSummit.DomeWorldId = int.Parse(worldInfo.data.id);
                    eventName = "TV_Dome_" + subWorldInfo.domeId + "_BW_" + worldInfo.data.id;
                }
                else
                {
                    _stayTimeTrackerForSummit.DomeWorldId = int.Parse(worldInfo.data.id);
                    eventName = "TV_Dome_" + subWorldInfo.domeId + "_XW_" + worldInfo.data.id;
                }
                GlobalConstants.SendFirebaseEventForSummit(eventName);
                _stayTimeTrackerForSummit.StartTrackingTime();
            }
        }
        //For Subworlds
        string World = "";
        if (ConstantsHolder.xanaConstants.isBuilderScene)
            World = "USER";
        else if (ConstantsHolder.xanaConstants.IsMuseum)
            World = "MUSEUM";
        else
            World = "ENVIRONMENT";

        UserAnalyticsHandler.onGetWorldId?.Invoke(int.Parse(worldInfo.data.id), World);
        //GameplayEntityLoader.instance.ForcedMapOpenForSummitScene();
        if (ActionManager.IsAnimRunning)
        {
            ActionManager.StopActionAnimation?.Invoke();

            //  EmoteAnimationHandler.Instance.StopAnimation();
            //  EmoteAnimationHandler.Instance.StopAllCoroutines();
        }

    }
    async Task UnloadScene(string sceneName)
    {
        if (SceneManager.GetSceneByName("Builder").isLoaded)
        {
            await SceneManager.UnloadSceneAsync("Builder");
        }
        else
        {
            await SceneManager.UnloadSceneAsync(sceneName);
        }
    }


    async void LoadBuilderSceneLoading(int builderMapId)
    {
        //Debug.Log("Loading builder Scene...");
        ConstantsHolder.xanaConstants.builderMapID = builderMapId;
        ConstantsHolder.xanaConstants.isBuilderScene = true;
        gameplayEntityLoader.addressableSceneName = null;

        AsyncOperation handle = await LoadingHandler.Instance.LoadSceneByIndex("Builder", true, LoadSceneMode.Additive);
        // handle = SceneManager.LoadSceneAsync("Builder", LoadSceneMode.Additive);


        handle.completed += Handle_completed;
    }

    private void Handle_completed(AsyncOperation obj)
    {
        obj.allowSceneActivation = true;
    }

    async void LoadingXANASummitOnBack()
    {
        if (ConstantsHolder.isFromXANASummit == false)
            return;

        if (ConstantsHolder.xanaConstants.isSaudiEvent)
        {
            ConstantsHolder.xanaConstants.isSaudiEvent = false;
            GetSummitData();
            return;
        }

        if (XanaPrivateChat.IsPrivateChatActive)
        {
            XanaChatSystem.instance.xanaPrivateChat.LeavePrivateChatBtn();
        }
        if (_stayTimeTrackerForSummit != null)
        {
            if (_stayTimeTrackerForSummit.IsTrackingTime)
            {
                _stayTimeTrackerForSummit.StopTrackingTime();
                //_stayTimeTrackerForSummit.CalculateAndLogStayTime();
                _stayTimeTrackerForSummit.IsTrackingTimeForExteriorArea = true;
            }
        }
        setPlayerPositionDelegate += SetPlayerOnback;

        //StartCoroutine(LoadingHandler.Instance.FadeIn());
        // LoadingHandler.Instance.ShowVideoLoading();
        XANASummitDataContainer.StackInfoWorld subWorldInfo = new XANASummitDataContainer.StackInfoWorld();
        subWorldInfo = XANASummitDataContainer.LoadedScenesInfo.Pop();


        ConstantsHolder.visitorCount = await dataContainer.GetVisitorCount(subWorldInfo.id);
        LoadingHandler.Instance.showDomeLoading(subWorldInfo);

        if (GamePlayUIHandler.inst.LeaderboardPanel.activeInHierarchy)
        {
            GamePlayUIHandler.inst.LeaderboardPanel.SetActive(false);
        }

        playerPos = subWorldInfo.playerTrasnform[0];
        playerRot = subWorldInfo.playerTrasnform[1];
        playerScale = subWorldInfo.playerTrasnform[2];

        string sceneToBeUnload = WorldItemView.m_EnvName;
        //string sceneName = "SaudiExpo";
        WorldItemView.m_EnvName = subWorldInfo.name;
        ConstantsHolder.xanaConstants.EnviornmentName = subWorldInfo.name;
        ConstantsHolder.userLimit = subWorldInfo.user_limit;
        ConstantsHolder.xanaConstants.isBuilderScene = subWorldInfo.isBuilderWorld;
        ConstantsHolder.xanaConstants.MuseumID = subWorldInfo.id;
        ConstantsHolder.isFromXANASummit = subWorldInfo.isFromSummitWorld;
        ConstantsHolder.HaveSubWorlds = subWorldInfo.haveSubWorlds;
        ConstantsHolder.description = subWorldInfo.description;
        ConstantsHolder.Thumbnail = subWorldInfo.thumbnail;
        ConstantsHolder.CreatorName = subWorldInfo.creator;
        ConstantsHolder.DomeType = subWorldInfo.domeType;
        ConstantsHolder.DomeCategory = subWorldInfo.domeCategory;
        ConstantsHolder.isPenguin = false;
        ConstantsHolder.isFixedHumanoid = false;
        ConstantsHolder.domeId = subWorldInfo.domeId;
        ConstantsHolder.xanaConstants.SetPlayerProperties();
        gameplayEntityLoader.currentEnvironment = null;
        multiplayerController.isConnecting = false;
        gameplayEntityLoader.isEnvLoaded = false;
        gameplayEntityLoader.isAlreadySpawned = true;
        multiplayerController.Disconnect();

        XanaWorldDownloader.ResetAll();
        BuilderEventManager.ResetSummit?.Invoke();

        multiplayerController.playerobjects.Clear();

        await UnloadScene(sceneToBeUnload);

        await HomeSceneLoader.ReleaseUnsedMemory();

        if (subWorldInfo.isBuilderWorld)
            LoadBuilderSceneLoading(int.Parse(subWorldInfo.id));
        else if (subWorldInfo.name == "SaudiExpo")
            multiplayerController.Connect(subWorldInfo.name + "-" + ConstantsHolder.xanaConstants.MuseumID);
        else
            multiplayerController.Connect("SaudiExpo-" + subWorldInfo.domeId + "-" + subWorldInfo.name);

        //"SaudiExpo-" + ConstantsHolder.domeId + "-" + domeGeneralData.world

        ConstantsHolder.DiasableMultiPartPhoton = false;

        ReferencesForGamePlay.instance.SetGameplayForPenpenz(true);

        ReferenceForPenguinAvatar referenceForPenguin = GameplayEntityLoader.instance.referenceForPenguin;
        referenceForPenguin.ActiveXanaUIData(true);


        if (GamePlayUIHandler.inst.isHideButton)
        {
            ReferencesForGamePlay.instance.hiddenButtonDisable();
        }
        if (GamePlayUIHandler.inst.isFreeCam)
        {
            ReferencesForGamePlay.instance.playerControllerNew.FreeFloatToggleButton(false);
            ReferencesForGamePlay.instance.hiddenButtonEnable();
        }

        // Map Working
        _domeMiniMap.SummitSceneReloaded();
        //SummitMiniMapStatusOnSceneChange(true);

        ConstantsHolder.xanaConstants.comingFrom = ConstantsHolder.ComingFrom.None;
        if (ReferencesForGamePlay.instance.playerControllerNew.isFirstPerson)
        {
            GamePlayUIHandler.inst.OnSwitchCameraClick();
        }
        //GameplayEntityLoader.instance.ForcedMapOpenForSummitScene();
        if (subWorldInfo.name != "SaudiExpo")
        {
            _stayTimeTrackerForSummit.IsTrackingTimeForExteriorArea = false;
            _stayTimeTrackerForSummit.DomeId = ConstantsHolder.domeId;
            _stayTimeTrackerForSummit.IsBuilderWorld = ConstantsHolder.xanaConstants.isBuilderScene;
            string eventName;
            if (ConstantsHolder.xanaConstants.isBuilderScene)
            {
                _stayTimeTrackerForSummit.DomeWorldId = int.Parse(subWorldInfo.id);
                eventName = "TV_Dome_" + ConstantsHolder.domeId + "_BW_" + subWorldInfo.id;
            }
            else
            {
                _stayTimeTrackerForSummit.DomeWorldId = int.Parse(subWorldInfo.id);
                eventName = "TV_Dome_" + ConstantsHolder.domeId + "_XW_" + subWorldInfo.id;
            }
            GlobalConstants.SendFirebaseEventForSummit(eventName);
            _stayTimeTrackerForSummit.StartTrackingTime();
        }
        string World = "";
        if (ConstantsHolder.xanaConstants.isBuilderScene)
            World = "USER";
        else if (ConstantsHolder.xanaConstants.IsMuseum)
            World = "MUSEUM";
        else
            World = "ENVIRONMENT";
        UserAnalyticsHandler.onGetWorldId?.Invoke(int.Parse(subWorldInfo.id), World);
        if (ActionManager.IsAnimRunning)
        {
            ActionManager.StopActionAnimation?.Invoke();

            //  EmoteAnimationHandler.Instance.StopAnimation();
            //  EmoteAnimationHandler.Instance.StopAllCoroutines();
        }
        //
    }
    async void GetSummitData()
    {
        ConstantsHolder.xanaConstants.isSaudiEvent = false;
        string WorldId;
        try
        {
            if (APIBasepointManager.instance.IsXanaLive)
                WorldId = "7722";    //Mainnet
            else
                WorldId = "3865";
        }
        catch (Exception e)
        {
            WorldId = "7722";
        }


        SingleWorldInfo singleWorldInfo = await GetSingleWorldData(WorldId);

        ConstantsHolder.isPenguin = false;
        ConstantsHolder.xanaConstants.openLandingSceneDirectly = false;
        ConstantsHolder.IsXSummitApp = true;
        ConstantsHolder.xanaConstants.isBuilderScene = false;


        XANASummitDataContainer.StackInfoWorld subWorldInfo = new XANASummitDataContainer.StackInfoWorld();
        subWorldInfo.id = singleWorldInfo.data.id;
        subWorldInfo.name = singleWorldInfo.data.name;
        subWorldInfo.isBuilderWorld = false;
        subWorldInfo.user_limit = singleWorldInfo.data.user_limit;
        subWorldInfo.domeId = 0;
        subWorldInfo.haveSubWorlds = false;
        subWorldInfo.isFromSummitWorld = false;
        subWorldInfo.thumbnail = singleWorldInfo.data.thumbnail;
        subWorldInfo.description = singleWorldInfo.data.description;
        subWorldInfo.playerTrasnform = new Vector3[] { new Vector3(1.58f,1.06f,-64f), Vector3.zero, Vector3.one };// Get this from MAP
        subWorldInfo.creator = singleWorldInfo.data.creator;
        subWorldInfo.domeType = "";
        subWorldInfo.domeCategory = "";

        XANASummitDataContainer.LoadedScenesInfo.Push(subWorldInfo);
        LoadingXANASummitOnBack();
    }



    XANASummitDataContainer.DomeGeneralData GetDomeData(int domeId)
    {
        XANASummitDataContainer.DomeGeneralData domeGeneralData = new XANASummitDataContainer.DomeGeneralData();
        for (int i = 0; i < dataContainer.summitData.domes.Count; i++)
        {
            if (dataContainer.summitData.domes[i].id == domeId)
            {
                domeGeneralData.id = dataContainer.summitData.domes[i].id;

                if (dataContainer.summitData.domes[i].world.Contains("D + Infinity Labo") || dataContainer.summitData.domes[i].world.Contains("D +  Infinity Labo"))
                {
                    dataContainer.summitData.domes[i].world = "D_Infinity_Labo";
                }

                domeGeneralData.world = dataContainer.summitData.domes[i].world;
                domeGeneralData.worldType = dataContainer.summitData.domes[i].worldType;
                domeGeneralData.experienceType = dataContainer.summitData.domes[i].experienceType;
                domeGeneralData.builderWorldId = dataContainer.summitData.domes[i].builderWorldId;
                domeGeneralData.worldId = dataContainer.summitData.domes[i].worldId;
                domeGeneralData.maxPlayer = dataContainer.summitData.domes[i].maxPlayer;
                domeGeneralData.IsPenguin = dataContainer.summitData.domes[i].IsPenguin;
                domeGeneralData.Ishumanoid = dataContainer.summitData.domes[i].Ishumanoid;
                domeGeneralData.Avatarjson = dataContainer.summitData.domes[i].Avatarjson;
                domeGeneralData.AvatarIndex = dataContainer.summitData.domes[i].AvatarIndex;
                domeGeneralData.name = dataContainer.summitData.domes[i].name;
                domeGeneralData.creatorName = dataContainer.summitData.domes[i].creatorName;
                domeGeneralData.description = dataContainer.summitData.domes[i].description;
                //domeGeneralData.isSubWorld = dataContainer.summitData.domes[i].isSubWorld;
                MediaInformation mediaInformation = new MediaInformation();
                mediaInformation.thumbnail = dataContainer.summitData.domes[i].domes_media_v2.thumbnail;
                mediaInformation.companyLogo = dataContainer.summitData.domes[i].domes_media_v2.companyLogo;
                domeGeneralData.domes_media_v2 = mediaInformation;
                //domeGeneralData.SubWorlds = dataContainer.summitData.domes[i].SubWorlds;
                domeGeneralData.domeCategory = dataContainer.summitData.domes[i].domeCategory;
                domeGeneralData.domeType = dataContainer.summitData.domes[i].domeType;
                domeGeneralData.isBuilderGame = dataContainer.summitData.domes[i].isBuilderGame;
                domeGeneralData.is_penpenz = dataContainer.summitData.domes[i].is_penpenz;
                domeGeneralData.creatorName = dataContainer.summitData.domes[i].creatorName;
                domeGeneralData.jpCreatorName = dataContainer.summitData.domes[i].jpCreatorName;
                domeGeneralData.jpDescription = dataContainer.summitData.domes[i].jpDescription;
                domeGeneralData.jpWorldName = dataContainer.summitData.domes[i].jpWorldName;
                //domeGeneralData.instruction = dataContainer.summitData.domes[i].instruction;

            }
        }
        return domeGeneralData;
        //return new[] { string.Empty, "0", "0" };
    }

    Vector3[] GetPlayerPosition(Vector3 _playerPos)
    {
        playerPos = _playerPos;
        playerRot = GameplayEntityLoader.instance.mainController.transform.rotation.eulerAngles;
        playerScale = GameplayEntityLoader.instance.mainController.transform.localScale;

        return new[] { playerPos, playerRot, playerScale };
    }

    void SetPlayerTransform()
    {
        //if (ConstantsHolder.isFromXANASummit == false)
        //    return;

        setPlayerPositionDelegate?.Invoke();

        //StartCoroutine(LoadingHandler.Instance.FadeOut());
        LoadingHandler.Instance.DisableVideoLoading();
        LoadingHandler.Instance.DisableDomeLoading();
    }

    void SetPlayerOnback()
    {
        playerPos = CheckForValidPlayerPos(playerPos);
        GameplayEntityLoader.instance.mainController.transform.position = playerPos;
        GameplayEntityLoader.instance.mainController.transform.rotation = playerRot.CTQuaternion();
        GameplayEntityLoader.instance.mainController.transform.localScale = playerScale;
        XanaWorldDownloader.initialPlayerPos = playerPos;
        if (WorldItemView.m_EnvName == "SaudiExpo")
        {
            ConstantsHolder.isFromXANASummit = false;
            ReferencesForGamePlay.instance.ChangeExitBtnImage(true);
        }
        setPlayerPositionDelegate = null;
    }

    Vector3 CheckForValidPlayerPos(Vector3 PlayerPos)
    {
        RaycastHit hit;
        bool validSpawnPointFound = false;
        int maxAttempts = 3; // Limit the number of attempts to prevent an infinite loop
        int attempts = 0;

        while (!validSpawnPointFound && attempts < maxAttempts)
        {
            attempts++;

            // Cast a ray downwards from the spawn point to detect collisions
            if (Physics.Raycast(PlayerPos, -transform.up, out hit, 2000))
            {
                // Check if the hit object is a player or other non-walkable surface
                if (hit.collider.gameObject.CompareTag("PhotonLocalPlayer") || hit.collider.gameObject.layer == LayerMask.NameToLayer("NoPostProcessing"))
                {
                    // Adjust spawn point slightly if occupied
                    PlayerPos = new Vector3(
                        PlayerPos.x + UnityEngine.Random.Range(-.5f, .5f),
                        PlayerPos.y,
                        PlayerPos.z + UnityEngine.Random.Range(-.5f, .5f)
                    );
                }
                else
                {
                    // Valid spawn point found 
                    PlayerPos = new Vector3(PlayerPos.x, hit.point.y + 0.5f, PlayerPos.z);
                    validSpawnPointFound = true;
                }
            }
        }

        if (!validSpawnPointFound)
        {
            Debug.LogWarning("Failed to find a valid spawn point after multiple attempts.");
        }

        return PlayerPos;
    }

    async Task<SingleWorldInfo> GetSingleWorldData(string WorldID)
    {
        string url;
        url = ConstantsGod.API_BASEURL + ConstantsGod.SINGLEWORLDINFO + WorldID;

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", ConstantsGod.AUTH_TOKEN);
            await www.SendWebRequest();
            if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
            {
                www.Dispose();
                return null;
            }
            else
            {
                SingleWorldInfo worldInfo = new SingleWorldInfo();
                worldInfo = JsonUtility.FromJson<SingleWorldInfo>(www.downloadHandler.text);
                www.Dispose();
                return worldInfo;
            }

        }
    }

    [System.Serializable]
    public class SingleWorldInfo
    {
        public bool success;
        public DataClassSingle data;
    }
    [System.Serializable]
    public class DataClassSingle
    {
        public string id;
        public string name;
        public int user_limit;
        public string thumbnail;
        public string banner;
        public string description;
        public string creator;
        public string entityType;
        public UserDetails user;
        public bool userMicEnable =false;
    }
    [System.Serializable]
    public class UserDetails
    {
        public string name;

    }


}
