using TMPro;
using System;
using SimpleJSON;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using Photon.Pun.Demo.PunBasics;
using static StayTimeTrackerForSummit;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class DomeMinimapDataHolder : MonoBehaviour
{
    [SerializeField]
    private StayTimeTrackerForSummit _stayTimeTrackerForSummit;
    public Sprite HighlightedSprite;
    public List<DomeDataForMap> MapDomes;
    public List<DomeDataForMap> MapDomes_portrait;
    public List<TextMeshProUGUI> VistedCount;

    public GameObject ConfirmationPopup;
    public GameObject ConfirmationPopup_Portrait;

    public static Action<OnTriggerSceneSwitch> OnInitDome;
    public static Action<int, string> OnSetDomeId;

    public static Action RedirectToRequireDome;


    private Dictionary<int, Transform> _allInitDomes = new Dictionary<int, Transform>();
    private Transform _playerTransform;
    private int _clickedDomeID;
    private string _clickedDomeArea;
    private int _totalDomeCount = 128;


    private void OnEnable()
    {
        OnInitDome += UpdateDomeList;
        OnSetDomeId += SetDomeID;
        RedirectToRequireDome += ReturnFromEventRedirectToEventDome;

        SummitSceneReloaded();
    }
    private void OnDisable()
    {
        OnInitDome -= UpdateDomeList;
        OnSetDomeId -= SetDomeID;
        RedirectToRequireDome -= ReturnFromEventRedirectToEventDome;
    }

    public void SummitSceneReloaded()
    {
        _allInitDomes.Clear();
        if (ConstantsHolder.xanaConstants.EnviornmentName.Equals("SaudiExpo"))
        {
            GetVisitedDomeData();
        }
    }
    void UpdateDomeList(OnTriggerSceneSwitch domeObj)
    {
        if (!_allInitDomes.ContainsKey(domeObj.DomeId))
            _allInitDomes.Add(domeObj.DomeId, domeObj.transform);
        else
            _allInitDomes[domeObj.DomeId] = domeObj.transform;
    }
    public async Task GetVisitedDomeData()
    {
        string url = ConstantsGod.API_BASEURL + ConstantsGod.GETVISITDOMES;
        string result = await GetAsyncRequest(url);

        //Debug.Log("VisitedWorld: Result: " + result);
        if (string.IsNullOrEmpty(result))
        {
            //Debug.Log("<color=red>Error fetching dome data. Might be the user is Guest</color>");
            return;
        }
        var jsonNode = JSON.Parse(result);
        var domeVisits = jsonNode["domeVisits"];
        HashSet<int> _VisitedDomeIDs = domeVisits.Children.Select(d => d["domeId"].AsInt).ToHashSet();

        string visitedText = $"({_VisitedDomeIDs.Count}/{_totalDomeCount})";
        VistedCount.ForEach(_text => _text.text = $"({_VisitedDomeIDs.Count}/{_totalDomeCount})");

        // Combine MapDomes and MapDomes_portrait into one collection for processing
        var allDomes = MapDomes.Concat(MapDomes_portrait);
        foreach (var item in allDomes)
        {
            if (_VisitedDomeIDs.Contains(item.domeId))
            {
                item.MyImage = item.MyImage ?? item.GetComponent<UnityEngine.UI.Image>();
                item.MyImage.sprite = HighlightedSprite;
            }
        }
    }
    private async Task<string> GetAsyncRequest(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", ConstantsGod.AUTH_TOKEN);
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                //Debug.Log("Get Dome Visit : "+ www.error);
                return null;
            }
            else
            {
                return www.downloadHandler.text;
            }
        }
    }
    public void OnClickTeleportPlayerDomePosition(int domeId)
    {
        if (_playerTransform == null && GameplayEntityLoader.instance && GameplayEntityLoader.instance.mainController)
            _playerTransform = GameplayEntityLoader.instance.mainController.transform;
        _clickedDomeID = domeId;
        ConfirmationPanelHandling(true);
    }
    void TeleportPlayerToSelectedDome(int _domeId, Transform playerTransform)
    {
        if (_allInitDomes.TryGetValue(_domeId, out Transform domeTransform))
        {
            MutiplayerController.instance.Ontriggered("Default");

            // Attempt to find "Player Spawner" or default to first child if not found
            Transform domePos = domeTransform.Find("Player Spawner") ?? domeTransform.GetChild(0);
            ConstantsHolder.isTeleporting = true;
            Vector3 NewPos = CheckForValidPlayerPos(domePos.position);
            playerTransform.position = NewPos;
            playerTransform.rotation = Quaternion.Euler(0f, domePos.rotation.eulerAngles.y, 0f);
            Invoke("SetTeleportingFalse", 3f);


            if(ReferencesForGamePlay.instance != null)
            {
                AvatarController ac = ReferencesForGamePlay.instance.m_34player?.GetComponent<AvatarController>();
                ac.AvatarAndClothParent.SetActive(true);
                ac.EnableClothMesh();
            }
        }
        else
        {
            Debug.Log($"Dome with ID {_domeId} not found.");
        }
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
                    PlayerPos = new Vector3(PlayerPos.x, hit.point.y, PlayerPos.z);
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


    void SetTeleportingFalse()
    {
        ConstantsHolder.isTeleporting = false;
        Invoke("EnableBgMusicForIOS", 5f);
    }


    public void ConfirmationPanelHandling(bool status)
    {
        ConfirmationPopup.SetActive(status);
        ConfirmationPopup_Portrait.SetActive(status);
    }
    public void OnClickYesBtn()
    {
        ConfirmationPanelHandling(false);
        ReferencesForGamePlay.instance.FullScreenMapStatus(false);
        if (ActionManager.IsAnimRunning)
        {
            ActionManager.StopActionAnimation?.Invoke();

            //  EmoteAnimationHandler.Instance.StopAnimation();
            //  EmoteAnimationHandler.Instance.StopAllCoroutines();
        }

        if (_clickedDomeID == 0) // No Dome is selected
            return;

        TeleportPlayerToSelectedDome(_clickedDomeID, _playerTransform);
        CallAnalyticsFromMinimapTeleport();
    }

    void EnableBgMusicForIOS()
    {
        if (ConstantsHolder.xanaConstants.mic != 0)
        {
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
    void CallAnalyticsFromMinimapTeleport()
    {
        if (Enum.GetNames(typeof(SummitAreaTrigger)).Any(name => _clickedDomeArea.Contains(name)))
        {
            _clickedDomeArea = Enum.GetNames(typeof(SummitAreaTrigger)).FirstOrDefault(name => _clickedDomeArea.Contains(name));
            if (_stayTimeTrackerForSummit != null)
            {
                if (_clickedDomeArea != _stayTimeTrackerForSummit.SummitAreaName)
                {
                    _stayTimeTrackerForSummit.SummitAreaName = _clickedDomeArea;
                    string eventName = "XS_TV_" + _stayTimeTrackerForSummit.SummitAreaName;
                    GlobalConstants.SendFirebaseEventForSummit(eventName);
                    _stayTimeTrackerForSummit.IsTrackingTimeForExteriorArea = true;
                    _stayTimeTrackerForSummit.StartTrackingTime();
                }
            }
        }
    }
    public void SetDomeID(int DomeId, string areaName)
    {
        _clickedDomeID = DomeId;
        _clickedDomeArea = areaName;
        if (_playerTransform == null)
            _playerTransform = GameplayEntityLoader.instance.mainController.transform;
    }


    static bool callOnetime = true;
    private void ReturnFromEventRedirectToEventDome()
    {
        if (!callOnetime)
            return;

        callOnetime = false;

        var domes = SummitSingletonClass.Instance.XANASummitDataContainer.summitData.domes;
        var occurringDome = domes.FirstOrDefault(dome => dome.worldId == ConstantsHolder.xanaConstants.saudiEventDomeId); // Where the Current Event is occuring

        if (occurringDome != null)
        {
            Debug.Log($"EventDome --- : {occurringDome.world} -- {occurringDome.worldId}");

            var targetDomeObject = XanaWorldDownloader.AllDomes
                .FirstOrDefault(domeObject => domeObject.GetComponentInChildren<OnTriggerSceneSwitch>().DomeId == occurringDome.id);

            if (targetDomeObject != null)
            {
                Debug.Log($"EventRedirect --- Dome ID : {occurringDome.id} -- {ConstantsHolder.xanaConstants.saudiEventDomeId}");

                if (_playerTransform == null && GameplayEntityLoader.instance?.mainController != null)
                    _playerTransform = GameplayEntityLoader.instance.mainController.transform;

                ConstantsHolder.xanaConstants.saudiEventDomeId = -1; // Resetting the Event Dome ID after teleporting
                TeleportPlayerToSelectedDome(occurringDome.id, _playerTransform);
            }
        }
    }
}