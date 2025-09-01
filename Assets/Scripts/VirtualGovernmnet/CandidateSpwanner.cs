using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CandidateSpwanner : MonoBehaviourPunCallbacks
{
    public static CandidateSpwanner Instance { get; private set; }
    public static List<GameObject> CandidateObjectReference ;
    string addressableKey = "CandidateManager"; // Set this to your Addressable key
    public static CandidatesManager candidatesManager;
    public static int candidateCount = 14;
    float tempOldBGMVoice;
    private int lastMasterActorNumber = -1;

    //public List<GameObject> TempCandidates = new List<GameObject>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep this object across scenes
        }
        else
        {
            Destroy(gameObject);
        }
        // Ensure the CandidatesManager is not null at the start
        CandidateObjectReference = new List<GameObject>();
        CandidateObjectReference.Clear();
    }
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        GamePlayButtonEvents.OnExitButtonXANASummit += OnExitButtonXANASummitHandler;
        SummitSingletonClass.Instance.GetComponent<SummitBGMSoundManager>().audioSource.mute = false;
        SummitSingletonClass.Instance.GetComponent<SummitBGMSoundManager>().StartBGMSound();
        // Store the current BGM_VOLUME value
        tempOldBGMVoice = PlayerPrefs.GetFloat(ConstantsGod.BGM_VOLUME); // Default to 1.0f if not set
        PlayerPrefs.SetFloat(ConstantsGod.BGM_VOLUME, 8.0f); // Set BGM_VOLUME to 10
        SoundSettings.soundManagerSettings.bgmSlider.value = 8;
        Application.runInBackground = false;

    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        GamePlayButtonEvents.OnExitButtonXANASummit -= OnExitButtonXANASummitHandler;
        GamePlayUIHandler.inst.FightBtn.SetActive(false);
        Application.runInBackground = true;

    }

    IEnumerator Start()
    {
        while (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InRoom)
        {
          //  Debug.Log("[CanidateSpwanner] Waiting for Photon to connect and join a room...");
            yield return null;
        }

        StartCoroutine(WaitForPhotonConnectionAndSpawn());
    }

    IEnumerator WaitForPhotonConnectionAndSpawn()
    {
        while (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InRoom)
        {
            yield return null;
        }

       // Debug.Log("[CandidateSpwanner] Photon is connected and ready. Proceeding to spawn CandidatesManager.");

        if (/*PhotonNetwork.IsMasterClient*/ true && candidatesManager == null)
        {
            StartCoroutine(SpawnCanidatesManager());
        }
        else
        {
            //// Check if CandidatesManager is already spawned
            //if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CanidatesManagerSpawned") &&
            //    (bool)PhotonNetwork.CurrentRoom.CustomProperties["CanidatesManagerSpawned"])
            //{
            //    Debug.Log("[CandidateSpwanner] CandidatesManager already spawned. Instantiating locally for non-master client.");
            //    InstantiateCandidatesManagerLocally();
            //}
        }
    }

    // Method to instantiate CandidatesManager locally for non-master clients
    //void InstantiateCandidatesManagerLocally()
    //{
    //    if (candidatesManager == null)
    //    {
    //        print("Spwaning Candidates Manager Locally");
    //        GameObject managerObject = PhotonNetwork.Instantiate(addressableKey, Vector3.zero, Quaternion.identity);
    //        candidatesManager = managerObject.GetComponent<CandidatesManager>();
    //        Debug.Log("[CandidateSpwanner] CandidatesManager instantiated locally for non-master client. : "+ WorldItemView.m_EnvName);
    //        SceneManager.MoveGameObjectToScene(managerObject, SceneManager.GetSceneByName(WorldItemView.m_EnvName));
    //        Debug.Log("[CandidateSpwanner] CandidatesManager instantiated locally for non-master client.");
    //    }
    //}

    IEnumerator SpawnCanidatesManager()
    {
        // Check if already spawned
        //if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CanidatesManagerSpawned") &&
        //    (bool)PhotonNetwork.CurrentRoom.CustomProperties["CanidatesManagerSpawned"])
        //{
        //    yield break;
        //}

        // Allocate a unique PhotonView ID for the object
        int viewID = PhotonNetwork.AllocateViewID(true);
       // Debug.Log("[CandidateSpwanner] Allocated PhotonView ID: " + viewID);
        ReferencesForGamePlay.instance.m_34player.GetComponent<SummitPlayerRPC>().SpwanCanidateManager(viewID, addressableKey);

        // Update room property to indicate that CanidatesManager has been spawned
        //ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable
        //{
        //    { "CanidatesManagerSpawned", true }
        //};
        //PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        // Wait for the manager to be instantiated
        yield return new WaitUntil(() => candidatesManager != null);

        // Wait until avatars count equals CandidateSpwanner.candidateCount - 1
        //Debug.Log("[CandidateSpwanner] Waiting for avatars count to equal CandidateSpwanner.candidateCount - 1...");
        if(candidatesManager == null)
        yield return new WaitUntil(() => candidatesManager.avatars.Count == CandidateSpwanner.candidateCount - 1);

        // Ensure all candidates are set as children of the manager
        candidatesManager.SetAvatars();
    }

    //public override void OnMasterClientSwitched(Player newMasterClient)
    //{
    //    //if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CanidatesManagerSpawned") ||
    //    //    !(bool)PhotonNetwork.CurrentRoom.CustomProperties["CanidatesManagerSpawned"])
    //    //{
    //    //    if (PhotonNetwork.IsMasterClient)
    //    //    {
    //    //       // Debug.Log("[CanidateSpwanner] CanidatesManager not yet spawned. New MasterClient will spawn it.");
    //    //        StartCoroutine(SpawnCanidatesManager());
    //    //    }
    //    //}
    //}
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //Debug.Log($"[CandidateSpwanner] Master client switched to: {newMasterClient.NickName} (ActorNumber: {newMasterClient.ActorNumber})");
        // Track the last master
        if (newMasterClient != null)
        {
            lastMasterActorNumber = newMasterClient.ActorNumber;
        }
    }
    public override void OnJoinedRoom()
    {
        //Debug.Log("[CandidateSpwanner] Rejoined room. Checking CandidatesManager...");

        DestroyAllChildrenOfAssetParentStatic();
        CandidateObjectReference.Clear();
        XanaWorldDownloader.ResetAll();
        XanaWorldDownloader.assetParentStatic.GetComponent<XanaWorldInfoHolder>().ReDownloadMap();
        if (candidatesManager == null && PhotonNetwork.CurrentRoom.PlayerCount <= 1)
        {
          //  Debug.Log("[CandidateSpwanner] Respawning CandidatesManager after rejoining room.");
            StartCoroutine(SpawnCanidatesManager());
        }

        //// Transfer TempCandidates to CandidateObjectReference
        //TransferTempCandidatesToReference();

        //// Use CandidateObjectReference to set up the CandidatesManager
        //if (CandidateObjectReference.Count > 0 && candidatesManager != null)
        //{
        //    Debug.Log("[CandidateSpwanner] Using CandidateObjectReference to set up CandidatesManager.");
        //    candidatesManager.SetAvatars();
        //}
    }

    private void DestroyAllChildrenOfAssetParentStatic()
    {
        if (XanaWorldDownloader.assetParentStatic != null)
        {
            foreach (Transform child in XanaWorldDownloader.assetParentStatic)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("XanaWorldDownloader.assetParentStatic is null. No children to destroy.");
        }
    }
    private void OnApplicationPause(bool pause)
    {
        HandleAppPauseOrMinimize(pause);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        //Debug.Log("[CandidateSpwanner] Application focus changed. Has focus: " + hasFocus);
        // If the app loses focus, treat as minimized
        HandleAppPauseOrMinimize(!hasFocus);
    }

    private void HandleAppPauseOrMinimize(bool pausedOrMinimized)
    {
       // Debug.Log("[CandidateSpwanner] Application paused or minimized: " + pausedOrMinimized);
        if (pausedOrMinimized && PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
        {
         //   Debug.Log("[CandidateSpwanner] Master client is paused or minimized. Transferring master client.");
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playerCount > 1)
            {
             //   Debug.Log("[CandidateSpwanner] More than one player in the room. Transferring master client.");
                // Find the next player (not self) to transfer master client
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    if (!player.IsMasterClient)
                    {
                        PhotonNetwork.SetMasterClient(player);
                     //   Debug.LogError("[CandidateSpwanner] Master client transferred due to app unfocus/minimize.");
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError("[CandidateSpwanner] Master client is exiting due to app unfocus/minimize with only one player in the room.");
                PhotonNetwork.LeaveRoom();
                GamePlayButtonEvents.OnExitButtonXANASummit.Invoke();
            }
        }
    }
    private void OnExitButtonXANASummitHandler()
    {
        if (candidatesManager != null)
        {
            if (candidatesManager.audioSource != null)
            {
                candidatesManager.audioSource.Stop();
            }
            CandidateObjectReference.Clear();
            //Debug.Log("[CandidateSpwanner] Destroying CandidatesManager locally for exit button click. " + CandidateObjectReference.Count);

            // Destroy the GameObject before nullifying the reference
            DestroyImmediate(candidatesManager.gameObject);

            candidatesManager = null;

            // Restore the previous BGM_VOLUME value
            PlayerPrefs.SetFloat(ConstantsGod.BGM_VOLUME, tempOldBGMVoice);
            SoundSettings.soundManagerSettings.bgmSlider.value = tempOldBGMVoice;
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Debug.Log($"[CandidateSpwanner] Player left: {otherPlayer.NickName} (ActorNumber: {otherPlayer.ActorNumber})");

        // If the left player was the master, or the current master is inactive, assign a new master
        Player currentMaster = PhotonNetwork.MasterClient;

        // Track the last master
        if (otherPlayer.IsMasterClient)
        {
            lastMasterActorNumber = otherPlayer.ActorNumber;
            //Debug.Log($"[CandidateSpwanner] Last master was: {lastMasterActorNumber}");
        }
        else if (currentMaster != null && currentMaster.IsInactive)
        {
            lastMasterActorNumber = currentMaster.ActorNumber;
            //Debug.Log($"[CandidateSpwanner] Last master (inactive) was: {lastMasterActorNumber}");
        }

        // Check if the left player was the master or the master is inactive
        if (otherPlayer.IsMasterClient || (currentMaster != null && currentMaster.IsInactive))
        {
            Player newMaster = GetNextActiveMasterCandidate();
            if (newMaster != null && newMaster == PhotonNetwork.LocalPlayer && !PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                //Debug.Log("[CandidateSpwanner] Local player is now the new master client due to player left or master inactivity.");
            }
        }
    }
    /// <summary>
    /// Finds the next eligible active player to be master, excluding the last master.
    /// </summary>
    private Player GetNextActiveMasterCandidate()
    {
        Player candidate = null;
        int lowestActorNumber = int.MaxValue;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.IsInactive && player.ActorNumber < lowestActorNumber && player.ActorNumber != lastMasterActorNumber)
            {
                lowestActorNumber = player.ActorNumber;
                candidate = player;
                //Debug.Log($"[CandidateSpwanner] Found new master candidate: {candidate.NickName} (ActorNumber: {candidate.ActorNumber})");
            }
        }
        return candidate;
    }
}