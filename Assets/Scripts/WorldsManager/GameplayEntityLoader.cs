using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using Cinemachine;
using UnityEditor;
using WebSocketSharp;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Photon.Realtime;
using UnityEngine.ResourceManagement.ResourceProviders;
using System;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Rendering.Universal;
using Photon.Pun.Demo.PunBasics;
using Photon.Voice.PUN;
using PhysicsCharacterController;
using System.Threading.Tasks;
#if UNITY_IOS
using UnityEngine.iOS;
#endif


public class GameplayEntityLoader : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public StayTimeTrackerForSummit StayTimeTrackerForSummit;
    public bool isAlreadySpawned;
    public Camera MiniMapCamera;
    [Header("singleton object")]
    public static GameplayEntityLoader instance;
    public bool IsJoinSummitWorld = false;
    public GameObject mainPlayer;
    public GameObject mainController;
    private GameObject mainControllerRefHolder;
    private GameObject YoutubeStreamPlayer;
    public GameObject PenguinPlayer;
    public GameObject DashButton;
    public ActionManager ActionEmoteSystem;

    public CinemachineFreeLook PlayerCamera;
    public CinemachineFreeLook playerCameraCharacterRender;
    public Camera environmentCameraRender;
    public Camera firstPersonCamera;
    [HideInInspector]
    private Transform updatedSpawnpoint;
    private Transform _spawnTransform;
    [HideInInspector]
    public Vector3 spawnPoint;
    public GameObject currentEnvironment;
    public bool isEnvLoaded = false;

    private float fallOffset = 10f;
    public bool setLightOnce = false;

    [HideInInspector]
    public GameObject player;
    System.DateTime eventUnivStartDateTime, eventLocalStartDateTime, eventlocalEndDateTime;

    [HideInInspector]
    public GameObject leftJoyStick;

    [HideInInspector]
    public float joyStickMovementRange;

    public LayerMask layerMask;

    public string addressableSceneName;

    [SerializeField] Button HomeBtn;

    public double eventRemainingTime;

    public HomeSceneLoader _uiReferences;
    public bool ClothsLoaded = false;
    //string OrdinaryUTCdateOfSystem = "2023-08-10T14:45:00.000Z";
    //DateTime OrdinarySystemDateTime, localENDDateTime, univStartDateTime, univENDDateTime;

    //Bool for BuilderSpawn point available or not
    bool BuilderSpawnPoint = false;

    #region XANA PARTY WORLD
    [Header("XANA Party")]
    public GameObject PositionResetButton;
    [SerializeField] GameObject XanaWorldController;
    [SerializeField] GameObject XanaPartyController;
    [SerializeField] public CameraManager XanaPartyCamera;
    [SerializeField] InputReader XanaPartyInput;
    [SerializeField] PenguinLookPointTracker penguinLook;
    public ReferenceForPenguinAvatar referenceForPenguin;
    #endregion
    [SerializeField] RaffleTicketHandler _raffleTickets;

    [Header("SaudiExpo Performer AI")]
    public GameObject[] AIAvatarPrefab;

    public XANASummitDataContainer XanaSummitDataContainerObject;
    public DownloadPopupHandler DownloadPopupHandlerInstance;

    public GameObject OldPlayer;
    public bool isLocalPlayer = false;
    private void Awake()
    {
        instance = this;
        setLightOnce = false;
        mainControllerRefHolder = mainController;
        if (ConstantsHolder.xanaConstants.isJoinigXanaPartyGame && ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            PositionResetButton.SetActive(true);
            Invoke(nameof(LoadFile), 1f);
        }
    }


    private void OnDestroy()
    {
        Physics.autoSimulation = true;
        Resources.UnloadUnusedAssets();
        GC.SuppressFinalize(this);
        GC.Collect(0);
    }

    private void Start()
    {
        if (XanaEventDetails.eventDetails.DataIsInitialized)
        {
            StartEventTimer();
        }
        Input.multiTouchEnabled = true;
        for (int i = 0; i < PlayerSelfieController.Instance.OnFeatures.Length; i++)
        {
            if (PlayerSelfieController.Instance.OnFeatures[i] != null)
            {
                if (PlayerSelfieController.Instance.OnFeatures[i].name == "LeftJoyStick")
                {
                    leftJoyStick = PlayerSelfieController.Instance.OnFeatures[i];
                    break;
                }
            }
        }

        GameObject _updatedSpawnPoint = new GameObject();
        updatedSpawnpoint = _updatedSpawnPoint.transform;
        SceneManager.MoveGameObjectToScene(updatedSpawnpoint.gameObject, SceneManager.GetSceneByName("GamePlayScene"));
        BuilderSpawnPoint = false;

        if (ConstantsHolder.xanaConstants.isSaudiEvent)
        {
            MiniMapCamera.gameObject.SetActive(false);
        }

        // Reset Camera 
        MiniMapCamera.orthographicSize = 30;
        if (ConstantsHolder.xanaConstants.EnviornmentName.Contains("SaudiExpo"))
        {
            Debug.LogError("force map open");
            // Zoom Out map Camera
            MiniMapCamera.orthographicSize = 45;
            ForcedMapOpenForSummitScene();
        }
        ConstantsHolder.xanaConstants.isGoingForHomeScene = false;

        //ForcedMapOpenForSummitScene();
    }

    void OnEnable()
    {
        BuilderEventManager.AfterWorldInstantiated += ResetPlayerAfterInstantiation;
        GamePlayButtonEvents.OnExitButtonXANASummit += ResetOnBackFromSummit;
    }


    private void OnDisable()
    {
        BuilderEventManager.AfterWorldInstantiated -= ResetPlayerAfterInstantiation;
        GamePlayButtonEvents.OnExitButtonXANASummit -= ResetOnBackFromSummit;
    }
    SummitPlayerRPC _SummitPlayerRPC;
    public void ForcedMapOpenForSummitScene()
    {
        if (ReferencesForGamePlay.instance.m_34player == null && !ConstantsHolder.xanaConstants)
            return;

        try
        {
            _SummitPlayerRPC = ReferencesForGamePlay.instance.m_34player.GetComponent<SummitPlayerRPC>();
        }
        catch (Exception ex)
        {
            Debug.LogError("Player not found");
            return;
        }


        if (ConstantsHolder.xanaConstants.EnviornmentName == "SaudiExpo" && !_SummitPlayerRPC.isInsideCAr && !_SummitPlayerRPC.isInsideWheel)
        {
            //if (ConstantsHolder.xanaConstants.isJoiningOsakaFristTime)
            //{
            //    ReferencesForGamePlay.instance.FullScreenMapStatus(true);
            //}
            ReferencesForGamePlay.instance.SumitMapStatus(true);

            ConstantsHolder.xanaConstants.isJoiningOsakaFristTime = false;

            ReferencesForGamePlay.instance.minimap.SetActive(true);
            PlayerPrefs.SetInt("minimap", 1);
            ConstantsHolder.xanaConstants.minimap = PlayerPrefs.GetInt("minimap");

            //XanaChatSystem.instance.chatDialogBox.SetActive(false);
            XanaChatSystem.instance.OpenCloseChatDialog(false);
        }
        else
        {
            ReferencesForGamePlay.instance.FullScreenMapStatus(false);
            ReferencesForGamePlay.instance.minimap.SetActive(false);
            PlayerPrefs.SetInt("minimap", 0);
            ConstantsHolder.xanaConstants.minimap = PlayerPrefs.GetInt("minimap");
        }
    }
    public void ForcedMapCloseForSummitScene()
    {
        if (ConstantsHolder.xanaConstants.EnviornmentName == "SaudiExpo")
        {
            ReferencesForGamePlay.instance.minimap.SetActive(false);
            PlayerPrefs.SetInt("minimap", 0);
            ConstantsHolder.xanaConstants.minimap = 0;
            ReferencesForGamePlay.instance.SumitMapStatus(false);
        }
    }
    public void StartEventTimer()
    {
        eventUnivStartDateTime = DateTime.Parse(XanaEventDetails.eventDetails.startTime);
        eventLocalStartDateTime = eventUnivStartDateTime.ToLocalTime();
        eventlocalEndDateTime = eventLocalStartDateTime.Add(TimeSpan.FromSeconds(XanaEventDetails.eventDetails.duration));

        //eventRemainingTime = eventTimeInSeconds;
        InvokeRepeating("CalculateEventTime", 0, 1);
    }

    public void CalculateEventTime()
    {
        //univStartDateTime = DateTime.Parse(OrdinaryUTCdateOfSystem);
        //OrdinarySystemDateTime = univStartDateTime.ToLocalTime();
        int _eventEndSystemDateTimediff = (int)(eventlocalEndDateTime - System.DateTime.Now).TotalMinutes;

        //print("===================DIFFEND : " + _eventEndSystemDateTimediff);

        if (_eventEndSystemDateTimediff <= 0)
        {
            //print("Event Ended");
            _uiReferences.EventEndedPanel.SetActive(true);
            CancelInvoke("CalculateEventTime");
        }
    }

    public IEnumerator VoidCalculation()
    {
        while (true)
        {
            if (CheckVoid())
            {
                //Debug.Log("Resetting Position");
                ResetPlayerPosition();
            }
            yield return new WaitForSeconds(1f);
        }
    }


    public void LoadFile()
    {
        if (WorldItemView.m_EnvName == "RooftopParty")
        {
            if (PlayerPrefs.GetString("LoggedInMail") == "mm7732002@gmail.com" || PlayerPrefs.GetString("LoggedInMail") == "xanamainnet@gmail.com" || PlayerPrefs.GetString("LoggedInMail") == "i@noborderz.com")
            {
                ConstantsHolder.isPenguin = false;
            }
            else
            {
                ConstantsHolder.isPenguin = true;
            }
        }

        if (currentEnvironment == null)
        {
            if (ConstantsHolder.xanaConstants.isBuilderScene)
                StartCoroutine(WaitForMapDownload());

            else
            {
                Debug.LogError(ConstantsHolder.xanaConstants.EnviornmentName);
                LoadEnvironment(ConstantsHolder.xanaConstants.EnviornmentName);
                //CharacterLightCulling();  //amit-05-05-2023 commented this line as it excuted before env instantidated and env light is not culled. called this method on another right place
            }
        }
        else
        {
            Debug.LogError("Here.......");
            LoadingHandler.CompleteSlider?.Invoke();
            StartCoroutine(SpawnPlayer());
        }

        PlayerCamera.gameObject.SetActive(true);
        environmentCameraRender.gameObject.SetActive(true);
        PlayerSelfieController.Instance.DisableSelfieFromStart();
    }

    void InstantiateYoutubePlayer()
    {
        if (YoutubeStreamPlayer == null)
        {
            if (WorldItemView.m_EnvName.Contains("DJ Event"))
            {
                YoutubeStreamPlayer = Instantiate(Resources.Load("DJEventData/YoutubeVideoPlayer") as GameObject);
                YoutubeStreamPlayer.transform.localPosition = new Vector3(0f, 0f, 10f);
                YoutubeStreamPlayer.transform.localScale = new Vector3(1f, 1f, 1f);

                YoutubeStreamPlayer.SetActive(false);
                if (YoutubeStreamPlayer)
                {
                    YoutubeStreamPlayer.SetActive(true);
                }
            }
            if (WorldItemView.m_EnvName.Contains("XANA Festival Stage") && !WorldItemView.m_EnvName.Contains("Dubai"))
            {
                YoutubeStreamPlayer = Instantiate(Resources.Load("XANAFestivalStageData/YoutubeVideoPlayer1") as GameObject);
                YoutubeStreamPlayer.transform.localPosition = new Vector3(0f, 0f, 10f);
                YoutubeStreamPlayer.transform.localScale = new Vector3(1f, 1f, 1f);

                YoutubeStreamPlayer.SetActive(false);
                if (YoutubeStreamPlayer)
                {
                    YoutubeStreamPlayer.SetActive(true);
                }
            }

            //if (WorldItemView.m_EnvName.Contains("Xana Festival") || WorldItemView.m_EnvName.Contains("NFTDuel Tournament"))
            //{
            //    YoutubeStreamPlayer = Instantiate(Resources.Load("MyBeach/XanaFestivalPlayer") as GameObject);
            //    YoutubeStreamPlayer.transform.localPosition = new Vector3(0f, 0f, 10f);
            //    YoutubeStreamPlayer.transform.localScale = new Vector3(1f, 1f, 1f);

            //    YoutubeStreamPlayer.SetActive(false);
            //    if (YoutubeStreamPlayer)
            //    {
            //        YoutubeStreamPlayer.SetActive(true);
            //    }
            //}
            if (WorldItemView.m_EnvName.Contains("XANA Lobby"))
            {
                YoutubeStreamPlayer = Instantiate(Resources.Load("XanaLobby/XanaLobbyPlayer") as GameObject);
                YoutubeStreamPlayer.SetActive(false);
                if (YoutubeStreamPlayer)
                {
                    YoutubeStreamPlayer.SetActive(true);
                }
            }
        }
    }


    void CharacterLightCulling()
    {
        if ((!WorldItemView.m_EnvName.Contains("Xana Festival") || !WorldItemView.m_EnvName.Contains("NFTDuel Tournament")) && !ConstantsHolder.xanaConstants.isBuilderScene)
        {
            //riken
            Light[] directionalLightList = FindObjectsOfType<Light>();
            for (int i = 0; i < directionalLightList.Length; i++)
            {
                if (directionalLightList[i].type == LightType.Directional && directionalLightList[i].gameObject.tag != "CharacterLight")
                {
                    directionalLightList[i].cullingMask = layerMask;
                }
            }
        }

        //.......
    }

    private void LoadLightSettings(string mEnvName)
    {
        string path = "Environment Data/" + mEnvName + " Data/LightingData/LightingData";
        if (!mEnvName.IsNullOrEmpty())
        {
            EnvironmentProperties EnvProp = Resources.Load<EnvironmentProperties>(path);
            if (EnvProp)
            {

                EnvProp.ApplyLightSettings();
            }
            else
            {
                //Debug.LogWarning("No Environment Light Properties Found");
            }
        }
        else
        {
            //Debug.LogWarning("No Environment Name Found");
        }
    }

    bool CheckVoid()
    {
        if (!ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            if (mainController == null)
                return false;
            if (mainController?.transform.position.y < (updatedSpawnpoint.transform.position.y - fallOffset))
            {
                RaycastHit hit;
                if (Physics.Raycast(mainController.transform.position, mainController.transform.TransformDirection(Vector3.down), out hit, 1000))
                {
                    updatedSpawnpoint.transform.localPosition = new Vector3(spawnPoint.x, hit.transform.localPosition.y, spawnPoint.z);
                    return false;
                }
                else
                {
                    updatedSpawnpoint.localPosition = spawnPoint;
                    return true;
                }
            }
        }
        else
        {
            if (XanaPartyCamera.characterManager != null)
            {
                if (BuilderData.mapData != null && BuilderData.mapData.data.worldType == 2)
                {
                    if (XanaPartyCamera.characterManager.transform.position.y < 0.75f)
                    {
                        string displayMessage = TextLocalization.GetLocaliseTextByKey("YOU WERE ELIMINATED!");
                        BuilderEventManager.OnDisplayMessageCollisionEnter?.Invoke(displayMessage, 5, true);
                        return true;
                    }
                }
                if (XanaPartyCamera.characterManager.transform.position.y < (updatedSpawnpoint.transform.position.y - fallOffset))
                {
                    RaycastHit hit;
                    if (Physics.Raycast(XanaPartyCamera.characterManager.transform.position, XanaPartyCamera.characterManager.transform.TransformDirection(Vector3.down), out hit, 1000))
                    {
                        updatedSpawnpoint.transform.localPosition = new Vector3(spawnPoint.x, hit.transform.localPosition.y, spawnPoint.z);
                        return false;
                    }
                    else
                    {
                        updatedSpawnpoint.localPosition = spawnPoint;
                        return true;
                    }
                }
            }
        }
        return false;

    }
    public void SetSpawnPosition()
    {


    }

    public void SetPlayer()
    {


        AvatarSpawnerOnDisconnect.Instance.currentDummyPlayer = null;
        SpawnPlayerSection();

    }
    public async void SpawnPlayerSection()  // Created this for summit
    {
        spawnPoint = player.transform.position;
        OldPlayer = player;
        ClothsLoaded = false;
        Debug.Log("player shoud be destroyed");
        InstantiatePlayerAvatarSector(new Vector3(0, -1000, 0));  // instantiate player below ground to avoid glitter;

        while (ClothsLoaded == false)
        {
            await Task.Delay(1000);
        }
        Quaternion rotation = OldPlayer.transform.localRotation;

        Destroy(OldPlayer);
        MutiplayerController.instance.DestroyPlayerDelay();
        player.transform.parent = mainController.transform;
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = rotation;

        ReferencesForGamePlay.instance.m_34player = player;
        if (player.GetComponent<SummitAnalyticsTrigger>() == null)
            player.AddComponent<SummitAnalyticsTrigger>();
        //  SetAxis();
        mainPlayer.SetActive(true);
        if (player.GetComponent<StepsManager>())
        {
            player.GetComponent<StepsManager>().isplayer = true;
        }
        //GetComponent<PostProcessManager>().SetPostProcessing();

        //change youtube player instantiation code because while env is in loading and youtube started playing video




        XanaWorldDownloader.initialPlayerPos = mainController.transform.localPosition;



        // Firebase Event for Join World
        /* Debug.Log("Player Spawn Completed --  Join World");
         GlobalConstants.SendFirebaseEvent(GlobalConstants.FirebaseTrigger.Join_World.ToString());
         UserAnalyticsHandler.onUpdateWorldRelatedStats?.Invoke(true, false, false, false);*/
        /// <summary>
        /// Load NPC fake chat system
        /// </summary>
        //ActivateNpcChat();

        await new WaitForSeconds(1);
        var controller = GameplayEntityLoader.instance.mainController.GetComponent<PlayerController>();
        if (controller.isFirstPerson)
        {
            controller.DisablePlayerOnFPS();
        }

    }
    public void SpawnLocalPlayer()
    {
        if (isLocalPlayer) // Avoid multiple avatar instantiation
        {
            return;
        }
        isLocalPlayer = true;
        if (ConstantsHolder.isPenguin || ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            DashButton.SetActive(false);
            XanaWorldController.SetActive(false);
            XanaPartyController.SetActive(true);
            GameObject go1 = Instantiate(ReferencesForGamePlay.instance.m_34player) as GameObject;
            player = go1;
            player.SetActive(true);
            PenguinPlayer = player;
            mainController = player;
            if (player != null)
            {
                if (SceneManager.GetActiveScene().name == "Builder" && ConstantsHolder.xanaConstants.isXanaPartyWorld)
                {
                    SituationChangerSkyboxScript.instance.builderMapDownload.XANAPartyLoading.SetActive(false);
                }
                StartCoroutine(SetXanaPartyControllers(player));
            }
            ReferencesForGamePlay.instance.m_34player = player;
            player.GetComponent<AudioListener>().enabled = true;
            player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            return;
        }
        else
        {
            DashButton.SetActive(true);
        }
        GameObject go = Instantiate(ReferencesForGamePlay.instance.m_34player) as GameObject;
        player = go;
        if (player.GetComponent<SummitPlayerRPC>().isInsideCAr)
        {
            mainController.transform.parent = mainPlayer.transform;
            ConstantsHolder.DisableFppRotation = false;
            _uiReferences.Onfreecam.interactable = true;
            _uiReferences.OffFreecam.interactable = true;
            mainController.GetComponent<CharacterController>().enabled = true;
            mainController.GetComponent<PlayerController>().enabled = true;
            player.transform.parent = mainController.transform;
            CarNavigationManager.CarNavigationInstance.DisableExitCanvas();
            SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(true, false);
            PlayerCamera.GetComponent<CinemachineCollider>().m_MinimumDistanceFromTarget = 0.02f; //Set to Default on Car Exit
            PlayerCameraController.instance.DisableCameraRecenter();

        }
        else if (player.GetComponent<SummitPlayerRPC>().isInsideWheel)
        {
            mainController.transform.parent = mainPlayer.transform;
            ConstantsHolder.DisableFppRotation = false;
            _uiReferences.Onfreecam.interactable = true;
            _uiReferences.OffFreecam.interactable = true;
            MutiplayerController.instance.disableSector = false;
            mainController.GetComponent<CharacterController>().enabled = true;
            mainController.GetComponent<PlayerController>().enabled = true;
            player.transform.parent = mainController.transform;
            CarNavigationManager.CarNavigationInstance.DisableExitCanvas();
            SummitCarUIHandler.SummitCarUIHandlerInstance.UpdateUIelement(true, true);
            mainController.transform.position = GiantWheelManager.Instance.Exit.position;
            GiantWheelManager.Instance.WheelCar.SetActive(true);
            GiantWheelManager.Instance.CarAdded = false;
            // Restore the old LOD bias
            QualitySettings.lodBias = player.GetComponent<SummitPlayerRPC>()._oldLodBias;
        }
        else
        {
            player.transform.parent = mainController.transform;
            player.transform.localPosition = ReferencesForGamePlay.instance.m_34player.transform.localPosition;
            player.transform.localRotation = ReferencesForGamePlay.instance.m_34player.transform.localRotation;
        }
        player.SetActive(true);
        player.GetComponent<Animator>().enabled = true;
        player.GetComponent<EyesBlinking>().enabled = true;
        StartCoroutine(player.GetComponent<EyesBlinking>().BlinkingStartRoutine());
        ReferencesForGamePlay.instance.m_34player = player;
        ReferencesForGamePlay.instance.playerControllerNew.animator = player.GetComponent<Animator>();
        if (CarNavigationManager.CarNavigationInstance != null && CarNavigationManager.CarNavigationInstance.Cars.Count > 0)
            CarNavigationManager.CarNavigationInstance.Cars.Clear();
        if (player.GetComponent<PlayerSitting>().isSitting)
        {
            mainController.GetComponent<PlayerController>().isMovementAllowed = true;
            mainController.GetComponent<CharacterController>().enabled = true;
            GamePlayUIHandler.inst.sitButton.gameObject.SetActive(false);
        }
    }
    async void WaitForAvatarClothes()
    {
        GameObject avatarClothParent = player.GetComponent<AvatarController>().AvatarAndClothParent;
        while (ClothsLoaded == false && !avatarClothParent.activeInHierarchy)
        {
            await Task.Delay(1000);
        }
    }
    public IEnumerator SpawnPlayer()
    {
        if (!ConstantsHolder.xanaConstants.isFromXanaLobby)
        {
            LoadingHandler.Instance.UpdateLoadingStatusText("Joining World...");
        }

        if (ReferencesForGamePlay.instance.m_34player == null)
        {
            if (WorldItemView.m_EnvName == "SaudiExpo")
            {
                SetPlayerCameraAngle();
            }
            // Code by Hardik 9 Aug 2024
            if (!SceneManager.GetActiveScene().name.Contains("Museum"))
            {
                // Add a small random offset to the initial spawn point to reduce chances of collision
                spawnPoint = new Vector3(spawnPoint.x + UnityEngine.Random.Range(-0.5f, 0.5f), spawnPoint.y + 0.5f, spawnPoint.z + UnityEngine.Random.Range(-0.5f, 0.5f));

                RaycastHit hit;
                bool validSpawnPointFound = false;
                int maxAttempts = 10; // Limit the number of attempts to prevent an infinite loop
                int attempts = 0;

                while (!validSpawnPointFound && attempts < maxAttempts)
                {
                    attempts++;

                    // Cast a ray downwards from the spawn point to detect collisions
                    if (Physics.Raycast(spawnPoint, -transform.up, out hit, 2000))
                    {
                        // Check if the hit object is a player or other non-walkable surface
                        if (hit.collider.gameObject.CompareTag("PhotonLocalPlayer") || hit.collider.gameObject.layer == LayerMask.NameToLayer("NoPostProcessing"))
                        {
                            // Adjust spawn point slightly if occupied
                            spawnPoint = new Vector3(
                                spawnPoint.x + UnityEngine.Random.Range(-3f, 3f),
                                spawnPoint.y,
                                spawnPoint.z + UnityEngine.Random.Range(-3f, 3f)
                            );
                        }
                        else
                        {
                            // Valid spawn point found
                            spawnPoint = new Vector3(spawnPoint.x, hit.point.y, spawnPoint.z);
                            validSpawnPointFound = true;
                        }
                    }
                }

                if (!validSpawnPointFound)
                {
                    Debug.LogWarning("Failed to find a valid spawn point after multiple attempts.");
                }

                SetPlayerCameraAngle();
            }
            // Set main player and controller positions
            mainPlayer.transform.position = Vector3.zero;

            if (mainController == null)
                mainController = mainControllerRefHolder;

            mainController.transform.position = spawnPoint + new Vector3(0, 0.1f, 0);

            // Optional: Adjust camera position based on new player position
            Vector3 newPos = spawnPoint + new Vector3(500, 500f, 500);
            InstantiatePlayerAvatar(newPos);

            ReferencesForGamePlay.instance.m_34player = player;
            SetAxis();
            mainPlayer.SetActive(true);
        }
        else
        {
            spawnPoint = ReferencesForGamePlay.instance.m_34player.transform.position;
            OldPlayer = player;
            ClothsLoaded = false;
            InstantiatePlayerAvatar(spawnPoint);
            ReferencesForGamePlay.instance.m_34player = player;
            Quaternion rotation = OldPlayer.transform.localRotation;
            player.transform.localRotation = rotation;
            if (!ConstantsHolder.isPenguin)
                WaitForAvatarClothes();
            // Check and update the sector name
            if (ConstantsHolder.xanaConstants.EnviornmentName == "SaudiExpo" && OldPlayer.GetComponent<SummitPlayerRPC>().isInsideWheel)
            {
                string sectorName = PhotonNetwork.CurrentRoom.CustomProperties["Sector"] as string;
                if (sectorName == "Wheel")
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Sector", "Default" } });
                    MutiplayerController.instance.SectorName = "Default";
                    Debug.Log("Sector name changed from 'Wheel' to 'Default'.");
                }
                //OldPlayer.GetComponent<SummitPlayerRPC>().isInsideWheel = false;
            }
            Destroy(OldPlayer);
            isLocalPlayer = false;
            ReferencesForGamePlay.instance.playerControllerNew.animator = player.GetComponent<Animator>();

        }
        if (AvatarSpawnerOnDisconnect.Instance.IsRjoining)
        {
            AvatarSpawnerOnDisconnect.Instance.EnableDisableUIElements(true);
            if (!ConstantsHolder.isPenguin && mainController.GetComponent<SwimmingController>().isInPool)
            {
                DashButton.SetActive(false);
                mainController.GetComponent<SwimmingController>().SetSwimJumpState(false);
                mainController.GetComponent<SwimmingController>().StartSwimming();
            }
            AvatarSpawnerOnDisconnect.Instance.DisableConnectionToast();
        }
        if (player.GetComponent<StepsManager>())
        {
            player.GetComponent<StepsManager>().isplayer = true;
        }

        GetComponent<PostProcessManager>().SetPostProcessing();

        // Change YouTube player instantiation code because while env is in loading and YouTube started playing video
        InstantiateYoutubePlayer();

        SetAddressableSceneActive();
        CharacterLightCulling();

        if (!ConstantsHolder.xanaConstants.isCameraMan && LoadingHandler.Instance.isFirstTime)
        {
            LoadingHandler.Instance.HideLoading();
        }

        if (WorldItemView.m_EnvName != "JJ MUSEUM" && player.GetComponent<PhotonView>().IsMine)
        {
            if (!ConstantsHolder.xanaConstants.isCameraMan)
                LoadingHandler.Instance.StartCoroutine(LoadingHandler.Instance.TeleportFader(FadeAction.Out));
        }
        else
        {
            if (JjMusuem.Instance)
                JjMusuem.Instance.SetPlayerPos(ConstantsHolder.xanaConstants.mussuemEntry);
            else
            {
                if (!ConstantsHolder.xanaConstants.isCameraMan)
                    LoadingHandler.Instance.StartCoroutine(LoadingHandler.Instance.TeleportFader(FadeAction.Out));
            }
        }

        if (WorldItemView.m_EnvName.Contains("XANA_DUNE"))
        {
            ReferencesForGamePlay.instance.MainPlayerParent.GetComponent<XanaDuneControllerHandler>().AddComponentOn34();
        }

        if (ConstantsHolder.xanaConstants.JjWorldSceneChange)
        {
            ConstantsHolder.xanaConstants.hasWorldTransitionedInternally = true;
        }

        ConstantsHolder.xanaConstants.JjWorldSceneChange = false;
        updatedSpawnpoint.transform.localPosition = spawnPoint;

        if (ConstantsHolder.xanaConstants.EnviornmentName.Contains("XANA Lobby"))
        {
            ConstantsHolder.xanaConstants.isFromXanaLobby = false;
        }

        ConstantsHolder.xanaConstants.isFromTottoriWorld = false;

        StartCoroutine(VoidCalculation());
        LightCullingScene();

        if (ConstantsHolder.xanaConstants.isCameraMan)
        {
            ReferencesForGamePlay.instance.randerCamera.gameObject.SetActive(false);
            ReferencesForGamePlay.instance.FirstPersonCam.gameObject.SetActive(false);
            ConstantsHolder.xanaConstants.StopMic();
            XanaVoiceChat.instance.TurnOffMic();
        }

        LoadingHandler.Instance.manualRoomController.HideRoomList();

        if (!ConstantsHolder.xanaConstants.isCameraMan)
            LoadingHandler.Instance.HideLoading();

        // Join Room Activate Chat
        if (XanaEventDetails.eventDetails.DataIsInitialized)
        {
            string worldId = 0.ToString();
            if (XanaEventDetails.eventDetails.environmentId != 0)
            {
                ConstantsHolder.xanaConstants.MuseumID = "" + XanaEventDetails.eventDetails.environmentId;
            }
            else
            {
                ConstantsHolder.xanaConstants.MuseumID = "" + XanaEventDetails.eventDetails.museumId;
            }
        }

        ChatSocketManager.onJoinRoom?.Invoke(ConstantsHolder.xanaConstants.MuseumID);

        if (ConstantsHolder.xanaConstants.isCameraMan)
        {
            if (StreamingCamera.instance)
            {
                StreamingCamera.instance.TriggerStreamCam();
            }
            else
            {
                _uiReferences.LoadMain(false);
            }
        }

        if (ConstantsHolder.isPenguin)
            XanaWorldDownloader.initialPlayerPos = player.transform.localPosition;
        else
            XanaWorldDownloader.initialPlayerPos = mainController.transform.localPosition;

        BuilderEventManager.AfterPlayerInstantiated?.Invoke();

        // Firebase Event for Join World
        GlobalConstants.SendFirebaseEvent(GlobalConstants.FirebaseTrigger.Join_World.ToString());

        if (ConstantsHolder.xanaConstants.EnviornmentName.Contains("SaudiExpo"))
        {
            ReferencesForGamePlay.instance.m_34player.AddComponent<SummitAnalyticsTrigger>();
            if (StayTimeTrackerForSummit != null)
            {
                string eventName = "XS_TV_" + StayTimeTrackerForSummit.SummitAreaName;
                GlobalConstants.SendFirebaseEventForSummit(eventName);
                StayTimeTrackerForSummit.IsTrackingTimeForExteriorArea = true;
                StayTimeTrackerForSummit.StartTrackingTime();
            }
        }

        UserAnalyticsHandler.onUpdateWorldRelatedStats?.Invoke(true, false, false, false);
        yield return null;
        /// <summary>
        /// Load NPC fake chat system
        /// </summary>
        //ActivateNpcChat();
        //Debug.Log("this is called..........................");
        //yield return new WaitForSeconds(1);
        //var controller = GameplayEntityLoader.instance.mainController.GetComponent<PlayerController>();
        //if(controller.isFirstPerson)
        //{
        //   controller.DisablePlayerOnFPS();
        //}
    }

    void SetPlayerCameraAngle()
    {
        if (ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            StartCoroutine(setPlayerCamAngle(-0.830f, 0.5572f));
            return;
        }
        if (WorldItemView.m_EnvName.Contains("DJ Event") || WorldItemView.m_EnvName.Contains("XANA Festival Stage"))
        {
            //mainPlayer.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            mainController.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else if (WorldItemView.m_EnvName.Contains("Koto") || WorldItemView.m_EnvName.Contains("Tottori") || WorldItemView.m_EnvName.Contains("DEEMO") || WorldItemView.m_EnvName.Contains("XANA Lobby"))
        {
            // mainPlayer.transform.rotation = Quaternion.Euler(0f, 180f, 0);
            mainController.transform.rotation = Quaternion.Euler(0f, 180f, 0);
            //Invoke(nameof(SetKotoAngle), 0.5f);
            if (WorldItemView.m_EnvName.Contains("XANA Lobby"))
            {
                StartCoroutine(setPlayerCamAngle(-0.830f, 0.5572f));
            }
            else if (WorldItemView.m_EnvName.Contains("Daisen"))
            {
                StartCoroutine(setPlayerCamAngle(0, 0.5f));
            }
            else
            {
                StartCoroutine(setPlayerCamAngle(0, 0.75f));
            }
        }
        else if (WorldItemView.m_EnvName.Contains("Genesis"))
        {
            StartCoroutine(setPlayerCamAngle(0, 0.75f));
        }
        else if (WorldItemView.m_EnvName.Contains("ZONE X Musuem") || WorldItemView.m_EnvName.Contains("FIVE ELEMENTS"))
        {
            StartCoroutine(setPlayerCamAngle(-30.0f, 0.5f));
        }
        else if (WorldItemView.m_EnvName.Contains("ZONE-X"))
        {
            StartCoroutine(setPlayerCamAngle(0f, 00.5f));
        }
        if (WorldItemView.m_EnvName.Contains("JJ MUSEUM") || WorldItemView.m_EnvName.Contains("FIVE ELEMENTS"))
        {
            PlayerCamera.m_Lens.NearClipPlane = 0.05f;
        }
        if (WorldItemView.m_EnvName.Contains("D_Infinity_Labo"))     // D +  Infinity Labo
        {            
            // added by AR for ToyotaHome world
            // mainPlayer.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            mainController.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            StartCoroutine(setPlayerCamAngle(0f, 00.5f));
        }
        if (WorldItemView.m_EnvName.Contains("XANA_DUNE"))
        {
            //mainPlayer.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            mainController.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            StartCoroutine(setPlayerCamAngle(0f, 0.5f));
        }
        if (WorldItemView.m_EnvName == "TOTTORI METAVERSE")
        {
            // mainPlayer.transform.rotation = _spawnTransform.rotation;
            mainController.transform.rotation = _spawnTransform.rotation;
            StartCoroutine(setPlayerCamAngle(180f, 0.5f));
        }
        if (WorldItemView.m_EnvName.Contains("JJTest"))
        {
           // mainPlayer.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            mainController.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            StartCoroutine(setPlayerCamAngle(0f, 0.5f));
        }
        if (WorldItemView.m_EnvName == "SaudiExpo")
        {
            //mainPlayer.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
            mainController.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
            StartCoroutine(setPlayerCamAngle(0f, 0.6f));
        }
        if (WorldItemView.m_EnvName == "Government Venue")
        {
            //mainPlayer.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            StartCoroutine(setPlayerCamAngle(0f, 0.8f));
        }
    }

    public void SetPlayerPos()
    {
        mainController.transform.position = spawnPoint + new Vector3(0, 0.1f, 0);
    }

    void InstantiatePlayerAvatar(Vector3 pos)
    {
        if (ScreenOrientationManager._instance != null && ScreenOrientationManager._instance.isPotrait)
        {
            ScreenOrientationManager._instance.MyOrientationChangeCode(DeviceOrientation.LandscapeLeft);
        }


        if (ConstantsHolder.isPenguin || ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            DashButton.SetActive(false);
            XanaWorldController.SetActive(false);
            XanaPartyController.SetActive(true);
            player = PhotonNetwork.Instantiate("XanaPenguin", spawnPoint, Quaternion.identity, 0);
            PenguinPlayer = player;
            mainController = player;
            if (player != null)
            {
                if (SceneManager.GetActiveScene().name == "Builder" && ConstantsHolder.xanaConstants.isXanaPartyWorld)
                {
                    SituationChangerSkyboxScript.instance.builderMapDownload.XANAPartyLoading.SetActive(false);
                }
                StartCoroutine(SetXanaPartyControllers(player));
            }
            return;
        }
        else
        {
            DashButton.SetActive(true);
        }

        XanaPartyController.SetActive(false);
        XanaWorldController.SetActive(true);
        mainController = mainControllerRefHolder;
        if (ConstantsHolder.isFixedHumanoid)
        {
            InstantiatePlayerForFixedHumanoid();
            return;
        }

        if (SaveCharacterProperties.instance?.SaveItemList.gender == AvatarGender.Male.ToString())
        {
            player = PhotonNetwork.Instantiate("XanaAvatar2.0_Male", spawnPoint, Quaternion.identity, 0);    // Instantiate Male Avatar
            player.transform.parent = mainController.transform;
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            player.GetComponent<AvatarController>().SetAvatarClothDefault(player.gameObject, "Male");        // Set Default Cloth to avoid naked avatar
        }
        else if (SaveCharacterProperties.instance?.SaveItemList.gender == AvatarGender.Female.ToString())
        {
            player = PhotonNetwork.Instantiate("XanaAvatar2.0_Female", spawnPoint, Quaternion.identity, 0);  // Instantiate Female Avatar
            player.transform.parent = mainController.transform;
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            player.GetComponent<AvatarController>().SetAvatarClothDefault(player.gameObject, "Female");      // Set Default Cloth to avoid naked avatar
        }
        else if (SaveCharacterProperties.instance?.SaveItemList.gender == AvatarGender.VTuber_Female.ToString())
        {
            player = PhotonNetwork.Instantiate("Vtuber_Avatar_Female_World", spawnPoint, Quaternion.identity, 0);  // Instantiate Female Avatar
            player.transform.parent = mainController.transform;
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            player.GetComponent<AvatarController>().SetAvatarClothDefault(player.gameObject, "");      // Set Default Cloth to avoid naked avatar
        }
        else if (SaveCharacterProperties.instance?.SaveItemList.gender == AvatarGender.VTuber_Male.ToString())
        {
            player = PhotonNetwork.Instantiate("Vtuber_Avatar_Male_World", spawnPoint, Quaternion.identity, 0);  // Instantiate Female Avatar
            player.transform.parent = mainController.transform;
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            player.GetComponent<AvatarController>().SetAvatarClothDefault(player.gameObject, "");      // Set Default Cloth to avoid naked avatar
        }
        else
        {
            player = PhotonNetwork.Instantiate("XanaAvatar2.0_Male", spawnPoint, Quaternion.identity, 0);    // Instantiate Male Avatar
            player.transform.parent = mainController.transform;
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            player.GetComponent<AvatarController>().SetAvatarClothDefault(player.gameObject, "Male");
        }
    }

    void InstantiatePlayerAvatarSector(Vector3 pos)
    {
        if (ConstantsHolder.isPenguin || ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            DashButton.SetActive(false);
            XanaWorldController.SetActive(false);
            XanaPartyController.SetActive(true);
            player = PhotonNetwork.Instantiate("XanaPenguin", spawnPoint, Quaternion.identity, 0);
            PenguinPlayer = player;
            mainController = player;
            if (player != null)
            {
                if (SceneManager.GetActiveScene().name == "Builder" && ConstantsHolder.xanaConstants.isXanaPartyWorld)
                {
                    SituationChangerSkyboxScript.instance.builderMapDownload.XANAPartyLoading.SetActive(false);
                }
                StartCoroutine(SetXanaPartyControllers(player));
            }
            return;
        }
        else
        {
            DashButton.SetActive(true);
        }
        XanaPartyController.SetActive(false);
        XanaWorldController.SetActive(true);
        mainController = mainControllerRefHolder;
        if (ConstantsHolder.isFixedHumanoid)
        {
            InstantiatePlayerForFixedHumanoid();
            return;
        }

        if (SaveCharacterProperties.instance?.SaveItemList.gender == AvatarGender.Male.ToString())
        {
            player = PhotonNetwork.Instantiate("XanaAvatar2.0_Male", pos, Quaternion.identity, 0);    // Instantiate Male Avatar
        }
        else if (SaveCharacterProperties.instance?.SaveItemList.gender == AvatarGender.Female.ToString())
        {
            player = PhotonNetwork.Instantiate("XanaAvatar2.0_Female", pos, Quaternion.identity, 0);  // Instantiate Female Avatar
        }
        else if (SaveCharacterProperties.instance?.SaveItemList.gender == AvatarGender.VTuber_Female.ToString())
        {
            player = PhotonNetwork.Instantiate("Vtuber_Avatar_Female_World", pos, Quaternion.identity, 0);  // Instantiate VT Female Avatar
        }
        else if (SaveCharacterProperties.instance?.SaveItemList.gender == AvatarGender.VTuber_Male.ToString())
        {
            player = PhotonNetwork.Instantiate("Vtuber_Avatar_Male_World", pos, Quaternion.identity, 0);  // Instantiate VT Male Avatar
        }
        else
        {
            player = PhotonNetwork.Instantiate("XanaAvatar2.0_Male", pos, Quaternion.identity, 0);    // Instantiate Male Avatar
        }
    }

    void InstantiatePlayerForFixedHumanoid()
    {
        Debug.Log("Fixed Humanoid: " + ConstantsHolder.AvatarIndex);
        if (ConstantsHolder.AvatarIndex < 10)
        {
            player = PhotonNetwork.Instantiate("XanaAvatar2.0_Male", spawnPoint, Quaternion.identity, 0);    // Instantiate Male Avatar
            player.transform.parent = mainController.transform;
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            player.GetComponent<AvatarController>().SetAvatarClothDefault(player.gameObject, "Male");        // Set Default Cloth to avoid naked avatar
        }
        else
        {
            player = PhotonNetwork.Instantiate("XanaAvatar2.0_Female", spawnPoint, Quaternion.identity, 0);  // Instantiate Female Avatar
            player.transform.parent = mainController.transform;
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            player.GetComponent<AvatarController>().SetAvatarClothDefault(player.gameObject, "Female");      // Set Default Cloth to avoid naked avatar
        }

    }

    void ActivateNpcChat()
    {
        GameObject npcChatSystem = Resources.Load("NpcChatSystem") as GameObject;
        Instantiate(npcChatSystem);
        //Debug.Log("<color=red> NPC Chat Object Loaded </color>");
    }

    [SerializeField] int autoSwitchTime;
    public IEnumerator BackToMainmenuforAutoSwtiching()
    {
        print("AUTO BACK CALL");
        yield return new WaitForSecondsRealtime(30);
        LoadingHandler.Instance.streamingLoading.UpdateLoadingText(false);
        LoadingHandler.Instance.StartCoroutine(LoadingHandler.Instance.TeleportFader(FadeAction.In));
        ConstantsHolder.xanaConstants.JjWorldSceneChange = true;
        _uiReferences.LoadMain(false);
    }


    public IEnumerator SpawnPlayerForBuilderScene()
    {
        LoadingHandler.Instance.UpdateLoadingStatusText("Joining World...");
        yield return new WaitForSeconds(0.1f);
        if (ReferencesForGamePlay.instance.m_34player == null)
        {
            spawnPoint = new Vector3(spawnPoint.x, spawnPoint.y + 2, spawnPoint.z);

            RaycastHit hit;
            CheckAgain:
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(spawnPoint, -transform.up, out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject.tag == "PhotonLocalPlayer" || hit.collider.gameObject.tag == "Player" || hit.collider.gameObject.layer == LayerMask.NameToLayer("NoPostProcessing"))
                {
                    if (BuilderData.StartFinishPoints.Count > 0 && BuilderData.mapData.data.worldType != 0)
                    {
                        StartFinishPointData startFinishPoint = BuilderData.StartFinishPoints.Find(x => x.IsStartPoint);
                        StartPoint sp = startFinishPoint.SpawnObject.GetComponent<StartPoint>();
                        BuilderData.StartPointID = startFinishPoint.ItemID;
                        spawnPoint = sp.SpawnPoints[UnityEngine.Random.Range(0, sp.SpawnPoints.Count)].transform.position;
                    }
                    else
                    {
                        spawnPoint = new Vector3(spawnPoint.x + UnityEngine.Random.Range(-1f, 1f), spawnPoint.y, spawnPoint.z + UnityEngine.Random.Range(-1f, 1f));
                    }
                    goto CheckAgain;
                } //else if()

                else if (hit.collider.gameObject.GetComponent<NPCRandomMovement>())
                {
                    spawnPoint = new Vector3(spawnPoint.x + UnityEngine.Random.Range(-2, 2), spawnPoint.y, spawnPoint.z + UnityEngine.Random.Range(-2, 2));
                    goto CheckAgain;
                }

                spawnPoint = new Vector3(spawnPoint.x, hit.point.y, spawnPoint.z);
            }

            mainPlayer.transform.position = new Vector3(0, 0, 0);
            if (mainController)
            {
                mainController.transform.position = spawnPoint + new Vector3(0, 0.1f, 0);
            }

            Vector3 newPos = spawnPoint + new Vector3(500, 500f, 500);

            InstantiatePlayerAvatar(newPos);
        }
        else
        {
            spawnPoint = ReferencesForGamePlay.instance.m_34player.transform.position;
            OldPlayer = player;
            ClothsLoaded = false;
            InstantiatePlayerAvatar(spawnPoint);
        }
        while (player == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (ConstantsHolder.xanaConstants.isBuilderScene && !ConstantsHolder.xanaConstants.isXanaPartyWorld && !ConstantsHolder.xanaConstants.isBuilderGame)
        {
            player.transform.localScale = Vector3.one * 1.153f;
            Rigidbody playerRB = player.AddComponent<Rigidbody>();
            playerRB.mass = 60;
            playerRB.isKinematic = true;
            playerRB.useGravity = true;
            playerRB.constraints = RigidbodyConstraints.FreezeRotation;
            GamificationComponentData.instance.PlayerRigidBody = playerRB;
            player.AddComponent<KeyValues>();
            GamificationComponentData.instance.spawnPointPosition = mainController.transform.position;
            GamificationComponentData.instance.buildingDetect = player.AddComponent<BuildingDetect>();
            //player.GetComponent<CapsuleCollider>().isTrigger = false;
            //player.GetComponent<CapsuleCollider>().enabled = false;
            TimeStats.playerCanvas = Instantiate(GamificationComponentData.instance.playerCanvas);
            GamificationComponentData.instance.playerControllerNew = mainPlayer.GetComponentInChildren<PlayerController>();
            player.AddComponent<EnvironmentChecker>();
            if (GamificationComponentData.instance.raycast == null)
                GamificationComponentData.instance.raycast = new GameObject("Raycasst");
            GamificationComponentData.instance.raycast.transform.SetParent(GamificationComponentData.instance.playerControllerNew.transform);
            GamificationComponentData.instance.raycast.transform.localPosition = Vector3.up * 1.683f;
            GamificationComponentData.instance.raycast.transform.localScale = Vector3.one * 0.37f;
            if (GamificationComponentData.instance.worldCameraEnable)
                BuilderEventManager.EnableWorldCanvasCamera?.Invoke();
            GamificationComponentData.instance.avatarController = player.GetComponent<AvatarController>();
            GamificationComponentData.instance.charcterBodyParts = player.GetComponent<CharacterBodyParts>();
            GamificationComponentData.instance.ikMuseum = player.GetComponent<IKMuseum>();

            //Post Process enable for Builder Scene
            firstPersonCamera.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            environmentCameraRender.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            Camera freeCam = this.GetComponent<PostProcessManager>().freeCam;
            freeCam.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            if (player.GetComponent<StepsManager>())
            {
                player.GetComponent<StepsManager>().isplayer = true;
            }
            //set Far & Near value same as builder for flickering assets testing
            firstPersonCamera.nearClipPlane = 0.03f;
            environmentCameraRender.nearClipPlane = 0.03f;
            freeCam.nearClipPlane = 0.03f;
            firstPersonCamera.farClipPlane = 1000;
            environmentCameraRender.farClipPlane = 1000;
            freeCam.farClipPlane = 1000;
            BuilderEventManager.ApplySkyoxSettings?.Invoke();
            //Rejoin world after internet connection stable
            if (GamificationComponentData.instance.isBuilderWorldPlayerSetup)
            {
                ReferencesForGamePlay.instance.playerControllerNew.StopBuilderComponent();
                SituationChangerSkyboxScript.instance.builderMapDownload.UpdateScene();
                BuilderEventManager.ChangeCameraHeight?.Invoke(false);
            }

            SituationChangerSkyboxScript.instance.builderMapDownload.PlayerSetup();
        }
        else
        {
            BuilderEventManager.ApplySkyoxSettings?.Invoke();
        }
        if ((WorldItemView.m_EnvName != "JJ MUSEUM") && player.GetComponent<PhotonView>().IsMine)
        {
            LoadingHandler.Instance.StartCoroutine(LoadingHandler.Instance.TeleportFader(FadeAction.Out));
        }
        if (ReferencesForGamePlay.instance.m_34player == null)
        {
            ReferencesForGamePlay.instance.m_34player = player;
            SetAxis();
            mainPlayer.SetActive(true);
            AvatarSpawnerOnDisconnect.Instance.EnableDisableUIElements(true);
        }
        else
        {
            ReferencesForGamePlay.instance.m_34player = player;
            Quaternion rotation = OldPlayer.transform.localRotation;
            player.transform.localRotation = rotation;
            WaitForAvatarClothes();
            Destroy(OldPlayer);
            isLocalPlayer = false;
            AvatarSpawnerOnDisconnect.Instance.EnableDisableUIElements(true);
            ReferencesForGamePlay.instance.playerControllerNew.animator = player.GetComponent<Animator>();
        }
        if (AvatarSpawnerOnDisconnect.Instance.IsRjoining)
        {
            AvatarSpawnerOnDisconnect.Instance.DisableConnectionToast();
        }
        SetAddressableSceneActive();
        updatedSpawnpoint.localPosition = spawnPoint;
        StartCoroutine(VoidCalculation());
        LightCullingScene();


        if (!ConstantsHolder.xanaConstants.isCameraMan)
        {
            LoadingHandler.Instance.HideLoading();
            // LoadingHandler.Instance.UpdateLoadingSlider(0, true);
        }
        if ((WorldItemView.m_EnvName != "JJ MUSEUM") && player.GetComponent<PhotonView>().IsMine)
        {
            if (!ConstantsHolder.xanaConstants.isCameraMan)
                LoadingHandler.Instance.StartCoroutine(LoadingHandler.Instance.TeleportFader(FadeAction.Out));
        }
        else
        {
            JjMusuem.Instance.SetPlayerPos(ConstantsHolder.xanaConstants.mussuemEntry);
        }
        ConstantsHolder.xanaConstants.JjWorldSceneChange = false;

        //while (!GamificationComponentData.instance.isSkyLoaded)
        //    yield return new WaitForSeconds(0.5f);
        BuilderEventManager.AfterPlayerInstantiated?.Invoke();


        isEnvLoaded = true;
        if (!ConstantsHolder.xanaConstants.isBackFromWorld)
        {
            LoadingHandler.Instance.HideLoading();

        }

        UserAnalyticsHandler.onUpdateWorldRelatedStats?.Invoke(true, false, false, false);
        ChatSocketManager.onJoinRoom?.Invoke(ConstantsHolder.xanaConstants.builderMapID.ToString());

        Debug.Log("Player Spawn Completed --  Join World");
        GlobalConstants.SendFirebaseEvent(GlobalConstants.FirebaseTrigger.Join_World.ToString());

        //ActivateNpcChat();
    }

    public IEnumerator setPlayerCamAngle(float xValue, float yValue)
    {
        yield return new WaitForSeconds(0.1f);
        if (ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            CinemachineFreeLook cam = XanaPartyCamera.GetComponentInChildren<CinemachineFreeLook>();
            cam.m_XAxis.Value = xValue;
            cam.m_YAxis.Value = yValue;
        }
        else
        {
            PlayerCamera.m_XAxis.Value = xValue;
            PlayerCamera.m_YAxis.Value = yValue;
        }
    }

    //void SetKotoAngle()
    //{
    //    PlayerCamera.m_XAxis.Value = 0f;
    //    PlayerCamera.m_YAxis.Value = 0.75f;
    //}
    public void SetAxis()
    {
        CinemachineFreeLook cam = PlayerCamera.GetComponent<CinemachineFreeLook>();
        if (cam)
        {
            if (ConstantsHolder.xanaConstants.EnviornmentName == "XANALIA NFTART AWARD 2021")
            {
                cam.Follow = mainController.transform;
                cam.m_XAxis.Value = 0;
                cam.m_YAxis.Value = 0.5f;
            }
            else
            {

                cam.Follow = mainController.transform;
                cam.m_XAxis.Value = 180;
                cam.m_YAxis.Value = 0.5f;
            }

            if (ConstantsHolder.xanaConstants.EnviornmentName == "DJ Event" || ConstantsHolder.xanaConstants.EnviornmentName == "Xana Festival" || ConstantsHolder.xanaConstants.EnviornmentName == "NFTDuel Tournament")
            {
                cam.Follow = mainController.transform;
                cam.m_XAxis.Value = 0;
                cam.m_YAxis.Value = 0.5f;
            }
            else
            {

                cam.Follow = mainController.transform;
                cam.m_XAxis.Value = 173;
                cam.m_YAxis.Value = 0.5f;
            }


        }

        CinemachineFreeLook cam2 = playerCameraCharacterRender.GetComponent<CinemachineFreeLook>();
        if (cam2)
        {

            if (ConstantsHolder.xanaConstants.EnviornmentName == "XANALIA NFTART AWARD 2021")
            {
                cam2.Follow = mainController.transform;
                cam2.m_XAxis.Value = 0;
                cam2.m_YAxis.Value = 0.5f;
            }

            else
            {

                cam2.Follow = mainController.transform;
                cam2.m_XAxis.Value = 180;
                cam2.m_YAxis.Value = 0.5f;
            }
            if (ConstantsHolder.xanaConstants.EnviornmentName == "DJ Event" || ConstantsHolder.xanaConstants.EnviornmentName == "Xana Festival" || ConstantsHolder.xanaConstants.EnviornmentName == "NFTDuel Tournament")
            {
                cam2.Follow = mainController.transform;
                cam2.m_XAxis.Value = 0;
                cam2.m_YAxis.Value = 0.5f;
            }
            else
            {

                cam2.Follow = mainController.transform;
                cam2.m_XAxis.Value = 173;
                cam2.m_YAxis.Value = 0.5f;
            }

        }
    }

    public async void ResetPlayerPosition()
    {
        //Stop selfi functionality when respawn after fall down
        if (!ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            PlayerSelfieController.Instance.DisableSelfieFeature();
        }
        if (ConstantsHolder.xanaConstants.isBuilderScene)
        {
            //Player respawn at spawn point after jump down from world
            if (!ConstantsHolder.xanaConstants.isXanaPartyWorld)
                mainController.transform.localPosition = await AvoidAvatarMergeInBuilderScene();
            else if (XanaPartyCamera.characterManager != null)
                XanaPartyCamera.characterManager.transform.localPosition = await AvoidAvatarMergeInBuilderScene();
        }
        else
        {
            if (!ConstantsHolder.isPenguin)
                mainController.GetComponent<PlayerController>().gravityVector.y = 0;
            GameObject spawnPointObject = GameObject.FindGameObjectWithTag("SpawnPoint");
            if (spawnPointObject != null)
            {
                spawnPoint = spawnPointObject.transform.position;
            }
            mainController.transform.localPosition = spawnPoint;
        }
        if (IdolVillaRooms.instance != null)
        {
            IdolVillaRooms.instance.ResetVilla();
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        UserAnalyticsHandler.onUpdateWorldRelatedStats?.Invoke(false, false, false, true);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        //Debug.Log("Instantiating Photon Complete");

        ResetPlayerPosition();
    }
    /*******************************************************************new code */

    string environmentLabel;
    public async void LoadEnvironment(string label)
    {
        environmentLabel = label;
        if (label == "SaudiExpo")
        {
            XanaWorldDownloader.downloadSize = Addressables.GetDownloadSizeAsync(environmentLabel).WaitForCompletion();
            XanaWorldDownloader.downloadSize += (71 * 1024 * 1024);
            if (!DownloadPopupHandler.AlwaysAllowDownload && !XanaWorldDownloader.CheckForVisitedWorlds(ConstantsHolder.xanaConstants.EnviornmentName))
            {
                LoadingHandler.StopLoader = true;
                if (!XanaWorldDownloader.DownloadedWorldNames.Contains(ConstantsHolder.xanaConstants.EnviornmentName))
                    XanaWorldDownloader.DownloadedWorldNames.Add(ConstantsHolder.xanaConstants.EnviornmentName);
                bool permission = await DownloadPopupHandlerInstance.ShowDialogAsync();
                LoadingHandler.StopLoader = false;
                LoadingHandler.CompleteSlider?.Invoke();
                if (!permission)
                {
                    return;
                }
            }
            else
            {
                LoadingHandler.StopLoader = false;

                LoadingHandler.CompleteSlider?.Invoke();
            }
        }
        else
        {
            if (label != "BreakingDownGame")
                LoadingHandler.CompleteSlider?.Invoke();
        }
        StartCoroutine(DownloadAssets());
    }

    IEnumerator WaitForMapDownload()
    {
        while (!BuilderAssetDownloader.isSpawnDownloaded)
        {
            yield return new WaitForSeconds(0.1f);
        }

        SetupEnvirnmentForBuidlerScene();
    }

    void SetupEnvirnmentForBuidlerScene()
    {
        if (ConstantsHolder.xanaConstants.orientationchanged && ConstantsHolder.xanaConstants.JjWorldSceneChange)
        {
            ScreenOrientationManager._instance.MyOrientationChangeCode(DeviceOrientation.Portrait);
        }

        LoadingHandler.Instance.UpdateLoadingStatusText("Getting World Ready....");

        Transform tempSpawnPoint = GetSpawnPoint();
        if (tempSpawnPoint == null)
        {
            tempSpawnPoint = CreateDefaultSpawnPoint();
        }

        spawnPoint = tempSpawnPoint.position;
        BuilderAssetDownloader.initialPlayerPos = tempSpawnPoint.localPosition;
        LoadingHandler.CompleteSlider?.Invoke();

        if (tempSpawnPoint)
        {
            StartCoroutine(XanaEventDetails.eventDetails.DataIsInitialized ? SpawnPlayer() : SpawnPlayerForBuilderScene());
        }
    }

    private Transform GetSpawnPoint()
    {
        if (BuilderData.StartFinishPoints.Count > 0 && BuilderData.mapData.data.worldType != 0)
        {
            StartFinishPointData startFinishPoint = BuilderData.StartFinishPoints.Find(x => x.IsStartPoint);
            StartPoint sp = startFinishPoint.SpawnObject.GetComponent<StartPoint>();
            BuilderData.StartPointID = startFinishPoint.ItemID;
            return sp.SpawnPoints[UnityEngine.Random.Range(0, sp.SpawnPoints.Count)].transform;
        }

        if (BuilderData.spawnPoint.Count == 1)
        {
            BuilderSpawnPoint = true;
            return BuilderData.spawnPoint[0].spawnObject.transform;
        }

        if (BuilderData.spawnPoint.Count > 1)
        {
            foreach (SpawnPointData g in BuilderData.spawnPoint)
            {
                if (g.IsActive)
                {
                    BuilderSpawnPoint = true;
                    return g.spawnObject.transform;
                }
            }
        }

        return null;
    }

    private Transform CreateDefaultSpawnPoint()
    {
        GameObject newObject = new GameObject("SpawningPoint");
        newObject.transform.position = new Vector3(0, 2500, 0);

        RaycastHit hit;
        if (Physics.Raycast(newObject.transform.position, Vector3.down, out hit, 3000))
        {
            newObject.transform.position = new Vector3(0, hit.point.y, 0);
        }
        else
        {
            newObject.transform.position = new Vector3(0, 100, 0);
        }

        return newObject.transform;
    }



    IEnumerator DownloadAssets()
    {
        if (isEnvLoaded)
        {
            UnloadUnusedAssetsAndRespawnPlayer();
            yield break;
        }

        CleanEnvironmentLabel();

        yield return new WaitUntil(() => ConstantsHolder.isAddressableCatalogDownload);
        Debug.LogError(environmentLabel);
        AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(environmentLabel, LoadSceneMode.Additive, false);
        if (!ConstantsHolder.xanaConstants.isFromXanaLobby)
        {
            LoadingHandler.Instance.UpdateLoadingStatusText("Loading World");
        }

        yield return new WaitUntil(() => handle.IsDone);

        addressableSceneName = environmentLabel;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            AddressableDownloader.bundleAsyncOperationHandle.Add(handle);
            if (environmentLabel == "BreakingDownGame")
                LoadingHandler.CompleteSlider?.Invoke();
            yield return handle.Result.ActivateAsync();
            if (environmentLabel != "BreakingDownGame")
            {
                // Wait for the spawn point to be available in the scene
                GameObject spawnPointObject = null;
                float timeout = 10f; // seconds
                float timer = 0f;
                while (spawnPointObject == null && timer < timeout)
                {
                    spawnPointObject = GameObject.FindGameObjectWithTag("SpawnPoint");
                    if (spawnPointObject == null)
                    {
                        yield return new WaitForSeconds(1f); // wait 1 second
                        timer += 1f;
                    }
                    else
                    {
                        _spawnTransform = spawnPointObject.transform;
                        spawnPoint = spawnPointObject.transform.position;
                        break;
                    }
                }
                if (spawnPointObject == null)
                {
                    Debug.LogError("SpawnPoint not found after scene activation timeout.");

                    // Unload current incomplete addressable scene and assets
                    if (handle.IsValid())
                    {
                        Addressables.UnloadSceneAsync(handle, true);
                    }
                    AssetBundle.UnloadAllAssetBundles(false);
                    Resources.UnloadUnusedAssets();

                    // Optionally, clear any cached handles or data related to this environment
                    AddressableDownloader.bundleAsyncOperationHandle.Remove(handle);

                    // Retry loading the environment
                    StartCoroutine(DownloadAssets());
                    yield break;
                }
            }
            DownloadCompleted();
        }
        else
        {
            HandleDownloadError();
        }
    }

    private void CleanEnvironmentLabel()
    {
        if (environmentLabel.Contains(" : "))
        {
            environmentLabel = environmentLabel.Replace(" : ", string.Empty);
        }
    }

    private void UnloadUnusedAssetsAndRespawnPlayer()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        Resources.UnloadUnusedAssets();
        RespawnPlayer();
    }

    private void HandleDownloadError()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        Resources.UnloadUnusedAssets();
        HomeBtn.onClick.Invoke();
    }


    private void DownloadCompleted()
    {
        if (ConstantsHolder.xanaConstants.EnviornmentName != "BreakingDownGame")
        {
            isEnvLoaded = true;
            StartCoroutine(SpawnPlayerWithWait());
        }
        else
        {
            BuilderEventManager.AfterPlayerInstantiated?.Invoke();
            SetAddressableSceneActive();
        }

    }

    IEnumerator SpawnPlayerWithWait()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        Resources.UnloadUnusedAssets();

        //Transform temp = null;

        // Finding spawn point right after Addressable scene loading in DownloadAssets()
        //// Attempt to find the spawn point
        //while (temp == null)
        //{
        //    GameObject spawnPointObject = GameObject.FindGameObjectWithTag("SpawnPoint");
        //    if (spawnPointObject != null)
        //    {
        //        temp = spawnPointObject.transform;
        //    }
        //    else
        //    {
        //        temp = new GameObject("SpawnPoint").transform;
        //    }
        //}

        //_spawnTransform = temp;
        //spawnPoint = temp.position;

        if (spawnPoint != null)
        {
            StartCoroutine(SpawnPlayer());
        }

        yield return null;
    }

    void RespawnPlayer()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        Resources.UnloadUnusedAssets();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("GamePlayScene"));
        StartCoroutine(SpawnPlayerWithWait());
    }

    async void ResetPlayerAfterInstantiation()
    {
        if (ConstantsHolder.xanaConstants.isBuilderGame)
        {
            return;
        }

        if (!BuilderAssetDownloader.isPostLoading)
        {
            return;
        }

        // Resetting player position for builder game
        if (BuilderData.StartFinishPoints.Count > 0 && BuilderData.mapData.data.worldType != 0)
        {
            SetSpawnPointForStartFinishPoints();
        }
        else
        {
            SetSpawnPointForBuilderData();
        }

        if (!ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            mainController.transform.localPosition = await AvoidAvatarMergeInBuilderScene();
        }
        else if (XanaPartyCamera.characterManager != null && !IsPlayerOnStartPoint())
        {
            XanaPartyCamera.characterManager.transform.localPosition = await AvoidAvatarMergeInBuilderScene();
        }
    }

    private void SetSpawnPointForStartFinishPoints()
    {
        if (!IsPlayerOnStartPoint())
        {
            BuilderSpawnPoint = true;
            StartFinishPointData startFinishPoint = BuilderData.StartFinishPoints.Find(x => x.IsStartPoint);
            StartPoint sp = startFinishPoint.SpawnObject.GetComponent<StartPoint>();
            BuilderData.StartPointID = startFinishPoint.ItemID;
            spawnPoint = sp.SpawnPoints[UnityEngine.Random.Range(0, sp.SpawnPoints.Count)].transform.position;
        }
    }

    private void SetSpawnPointForBuilderData()
    {
        if (BuilderData.spawnPoint.Count == 1)
        {
            BuilderSpawnPoint = true;
            spawnPoint = BuilderData.spawnPoint[0].spawnObject.transform.localPosition;
        }
        else if (BuilderData.spawnPoint.Count > 1)
        {
            foreach (SpawnPointData g in BuilderData.spawnPoint)
            {
                if (g.IsActive)
                {
                    BuilderSpawnPoint = true;
                    spawnPoint = g.spawnObject.transform.localPosition;
                    break;
                }
            }
        }
    }


    async Task<Vector3> AvoidAvatarMergeInBuilderScene()
    {
        Vector3 spawnPoint = this.spawnPoint;
        spawnPoint.y += BuilderSpawnPoint ? 2 : 1000;

        RaycastHit hit;

        while (true)
        {
            if (Physics.Raycast(spawnPoint, -transform.up, out hit, Mathf.Infinity))
            {
                if (IsPlayerOrNoPostProcessingLayer(hit))
                {
                    PhotonView pv = hit.collider.GetComponent<PhotonView>();
                    if (pv == null || !pv.IsMine)
                    {
                        spawnPoint = AdjustSpawnPointForBuilderData(spawnPoint);
                        continue;
                    }
                }
                else if (hit.collider.gameObject.GetComponent<NPCRandomMovement>())
                {
                    spawnPoint = AdjustSpawnPointForNPC(spawnPoint);
                    continue;
                }
                spawnPoint = new Vector3(spawnPoint.x, hit.point.y, spawnPoint.z);
                this.spawnPoint = spawnPoint;
                break;
            }

            // Await Task.Delay to yield control and allow other updates.
            await Task.Delay(10);
        }
        return spawnPoint;
    }

    private bool IsPlayerOrNoPostProcessingLayer(RaycastHit hit)
    {
        return hit.collider.gameObject.CompareTag("PhotonLocalPlayer") ||
               hit.collider.gameObject.CompareTag("Player") ||
               hit.collider.gameObject.layer == LayerMask.NameToLayer("NoPostProcessing");
    }

    private Vector3 AdjustSpawnPointForBuilderData(Vector3 spawnPoint)
    {
        if (BuilderData.StartFinishPoints.Count > 0 && BuilderData.mapData.data.worldType != 0)
        {
            if (!IsPlayerOnStartPoint())
            {
                StartFinishPointData startFinishPoint = BuilderData.StartFinishPoints.Find(x => x.IsStartPoint);
                StartPoint sp = startFinishPoint.SpawnObject.GetComponent<StartPoint>();
                BuilderData.StartPointID = startFinishPoint.ItemID;
                spawnPoint = sp.SpawnPoints[UnityEngine.Random.Range(0, sp.SpawnPoints.Count)].transform.position;
            }
        }
        else
        {
            spawnPoint = new Vector3(spawnPoint.x + UnityEngine.Random.Range(-1f, 1f), spawnPoint.y, spawnPoint.z + UnityEngine.Random.Range(-1f, 1f));
        }
        return spawnPoint;
    }

    private Vector3 AdjustSpawnPointForNPC(Vector3 spawnPoint)
    {
        return new Vector3(spawnPoint.x + UnityEngine.Random.Range(-2, 2), spawnPoint.y, spawnPoint.z + UnityEngine.Random.Range(-2, 2));
    }

    bool IsPlayerOnStartPoint()
    {
        Transform playerTransform = PenguinPlayer.transform;
        float raycastDistance = 2f;

        if (Physics.Raycast(playerTransform.position, -playerTransform.up, out RaycastHit hit, raycastDistance))
        {
            Debug.DrawRay(playerTransform.position, -playerTransform.up * raycastDistance, Color.green);
            StartPoint startPoint = hit.collider.GetComponentInParent<StartPoint>();
            if (startPoint != null)
            {
                Debug.Log("Player is on a StartPoint object.");
                return true;
            }
        }
        Debug.Log("Player is not on a StartPoint object.");
        return false;
    }


    public void SetAddressableSceneActive()
    {
        string temp = addressableSceneName;
        if (!string.IsNullOrEmpty(temp) && temp.Contains(" Astroboy x Tottori Metaverse Museum"))
        {
            temp = "Astroboy x Tottori Metaverse Museum";
        }

        if (!string.IsNullOrEmpty(temp))
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(temp));
            return;
        }

        if (ConstantsHolder.xanaConstants.isBuilderScene)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Builder"));
        }
        else
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(ConstantsHolder.xanaConstants.EnviornmentName));
        }
    }

    void LightCullingScene()
    {
        if (GetComponent<PostProcessManager>().CheckPostProcessEnable())
        {
            Light[] sceneLights = GameObject.FindObjectsOfType<Light>();
            foreach (var light in sceneLights)
            {
                if (light.name.Contains("Character"))
                {
                    light.cullingMask = LayerMask.GetMask("NoPostProcessing");
                }
                else if (light.name.Contains("Directional Light"))
                {
                    light.cullingMask = ConstantsHolder.xanaConstants.EnviornmentName.Contains("FIVE ELEMENTS")
                        ? LayerMask.GetMask("Water")
                        : LayerMask.GetMask("Default", "TransparentFX", "RenderTexture", "Character", "Head", "Body", "Plane", "Room", "AvaterSelection", "MiniMap", "ZoomUI", "Arrow", "CameraColliderIgnore", "PostProcessing", "PictureInteractable", "Particles", "NFTDisplayPanel", "NoRenderOnFPS", "Hair_Light");
                }
            }
        }
        else
        {
            CharacterLightCulling();
        }
    }


    //penguin mehtods 
    IEnumerator SetXanaPartyControllers(GameObject player)
    {
        CharacterManager characterManager = player.GetComponent<CharacterManager>();
        XanaPartyCamera.characterManager = characterManager;
        characterManager.input = XanaPartyInput;
        characterManager.characterCamera = XanaPartyCamera.GetComponentInChildren<Camera>().gameObject;

        XanaPartyCamera.thirdPersonCamera.Follow = player.transform;
        XanaPartyCamera.thirdPersonCamera.LookAt = player.transform;
        characterManager.enabled = true;
        XanaPartyCamera.SetCamera();
        XanaPartyCamera.SetDebug();
        XanaPartyCamera.thirdPersonCamera.GetComponent<XANAPartyCameraController>().SetReference(player, characterManager.headPoint.gameObject);

        yield return new WaitForSeconds(0.1f);

        if (GamificationComponentData.instance != null)
        {
            GamificationComponentData.instance.PlayerRigidBody = player.GetComponent<Rigidbody>();
            GamificationComponentData.instance.PlayerRigidBody.constraints = RigidbodyConstraints.None;
            GamificationComponentData.instance.PlayerRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        referenceForPenguin.ActiveXanaUIData(false);

        if (ConstantsHolder.xanaConstants.isXanaPartyWorld && !isLocalPlayer)
        {
            ReferencesForGamePlay.instance.SetGameplayForPenpenz(false);

            if (BuilderAssetDownloader.Instance != null)
            {
                BuilderAssetDownloader.Instance.WaitingForOtherPlayer.SetActive(!ConstantsHolder.xanaConstants.isJoinigXanaPartyGame);
            }
            else
            {
                ReferencesForGamePlay.instance.XANAPartyWaitingPanel.SetActive(!ConstantsHolder.xanaConstants.isJoinigXanaPartyGame);
            }

            player.GetComponent<PartyTimerManager>().enabled = true;
            player.GetComponent<XANAPartyMulitplayer>().enabled = true;
        }
        else
        {
            if (BuilderAssetDownloader.Instance != null)
            {
                BuilderAssetDownloader.Instance.WaitingForOtherPlayer.SetActive(false);
            }
            else
            {
                ReferencesForGamePlay.instance.XANAPartyWaitingPanel.SetActive(false);
            }

            player.GetComponent<PartyTimerManager>().enabled = false;
            player.GetComponent<XANAPartyMulitplayer>().enabled = false;
        }

        if (ConstantsHolder.xanaConstants.isXanaPartyWorld && ConstantsHolder.xanaConstants.isJoinigXanaPartyGame && GamificationComponentData.instance != null && !GamificationComponentData.instance.isRaceStarted && ReferencesForGamePlay.instance != null)
        {
            ReferencesForGamePlay.instance.IsLevelPropertyUpdatedOnlevelLoad = false;
            ReferencesForGamePlay.instance.CheckActivePlayerInCurrentLevel();
        }
    }

    public void ResetOnBackFromSummit()
    {
        if (YoutubeStreamPlayer)
            Destroy(YoutubeStreamPlayer);

        mainController = mainControllerRefHolder;

        ReferencesForGamePlay.instance.XANAPartyCounterPanel.SetActive(false);
        if (BuilderAssetDownloader.Instance != null)
        {
            BuilderAssetDownloader.Instance.WaitingForOtherPlayer.SetActive(false);
        }
        else
        {
            ReferencesForGamePlay.instance.XANAPartyWaitingPanel.SetActive(false);
        }
        IsJoinSummitWorld = false;

        ConstantsHolder.isFixedHumanoid = false;
        ConstantsHolder.isPenguin = false;
        ConstantsHolder.xanaConstants.isXanaPartyWorld = false;
        ConstantsHolder.xanaConstants.isBuilderGame = false;
        ConstantsHolder.xanaConstants.isJoinigXanaPartyGame = false;
    }

    public void AssignRaffleTickets(int domeID)
    {
        _raffleTickets.UpdateData(domeID);
    }
}