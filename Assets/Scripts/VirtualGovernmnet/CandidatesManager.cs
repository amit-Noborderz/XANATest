using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;

public class CandidatesManager : MonoBehaviourPunCallbacks
{

    #region Serializable Classes 
    [System.Serializable]
    public class Avatar
    {
        public int id;
        public GameObject avatarObject;
        public GameObject CameraObj; // Optional: Camera object for the avatar, can be used for specific camera views
    }

    [Serializable]
    public class DialougeWrapper
    {

        public List<DialogueData> Dailogues = new List<DialogueData>(); // List of dialogues for the avatar
        public string topic;
    }
    [System.Serializable]
    public class DialogueData
    {
        public int AvatarIndex;
        public string voiceUrl; // URL to fetch the mp3 for this dialogue, set in the inspector
        public string dialogueText; // Optional: Text for the dialogue, can be used for subtitles or UI display
    }

    [Serializable]
    private class IndexResponse
    {
        public string currentIndex;
    }

    [Serializable]
    private class RootResponse
    {
        public IndexResponse response;
    }
    #endregion
    public List<DialougeWrapper> DialogueContainer = new List<DialougeWrapper>();

    public List<Avatar> avatars = new List<Avatar>(14);

    public AudioSource audioSource; // Assign in inspector or create at runtime

    private int currentAvatarIndex;
    private double currentAudioStartTime = 0;
    private Coroutine sequenceCoroutine;

    private Dictionary<string, AudioClip> preloadedAudioClips = new Dictionary<string, AudioClip>();
    private const int MaxConcurrentPreloads = 5; // Limit concurrent audio preloads
    const string AvatarIndexKey = "AvatarIndex";
    const string AudioStartTimeKey = "AudioStartTime";
    const string WrapperIndexKey = "WrapperIndex";
    const string VGS3Path= "https://cdn.xana.net/xanaprod/VG%20Voices/"; // Path to the CSV file
    public Material VideoScreenMaterial; // Assign in the inspector or dynamically
    public Material OffScreenMaterial; // Assign in the inspector or dynamically
    [HideInInspector]
    public MeshRenderer screenMesh;
    public GameObject overlayScreen; // Assign in the inspector or dynamically
    private RenderTexture sharedRenderTexture;
    bool managerInitialized = true;
    bool audioIsPreloaded;
    private bool isSequenceRunning = false; // Flag to track if the sequence is running
    private bool hasPlayedResumedClip = false;
    private int currentWrapperIndex = 0;
    private bool isApiDataFetched = false;
    private bool isWaitingForResume = false;
    private string lastPlayedVoiceUrl = "";
    private double lastPlayTime = -1;
    private Coroutine currentVoiceRoutine = null;
    private string currentPlayingVoiceUrl = "";
    private bool isVoiceLocked = false;

    // Add this field to control the sequence loop
    private bool shouldRunSequence = false;
    private void Awake()
    {
        PhotonView photonView = GetComponent<PhotonView>();
        if (photonView != null && !photonView.IsRoomView)
        {
            photonView.ViewID = PhotonNetwork.AllocateViewID(true);
        }

        sharedRenderTexture = new RenderTexture(640, 360, 16, RenderTextureFormat.RGB565)
        {
            antiAliasing = 1,
            useMipMap = false,
            filterMode = FilterMode.Bilinear
        };
        sharedRenderTexture.Create();

        audioSource.volume = PlayerPrefs.GetFloat(ConstantsGod.MIC);
    }
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        BuilderEventManager.AfterWorldOffcialWorldsInatantiated += SetAvatars;
        SoundSettings.soundManagerSettings.UserSlider.onValueChanged.AddListener((float vol) =>
        {
            SetCanididatesVoice(vol);
        });
    }

    //public bool updateRecord = false;
    //private void OnValidate()
    //{
    //    if (updateRecord)
    //    {
    //        updateRecord = false;

    //        for (int i = 0; i < 113; i++)
    //        {
    //            Dailogues[i].topic = "選択的夫婦別姓";
    //        }


    //        //string filePath = "Assets/MyData.csv";
    //        //if (!File.Exists(filePath))
    //        //{
    //        //    Debug.LogError("CSV file not found: " + filePath);
    //        //    return;
    //        //}

    //        //var lines = File.ReadAllLines(filePath).Skip(1); // Skip header
    //        ////Dailogues.Clear();

    //        //foreach (var line in lines)
    //        //{
    //        //    var values = line.Split(',');
    //        //    //if (values.Length < 4) continue;

    //        //    DailogueData data = new DailogueData
    //        //    {
    //        //        AvatarIndex = int.Parse(values[6]),
    //        //        voiceMp3Url = "https://cdn.xana.net/xanaprod/VG+Voices/Dialogue+2/" + Path.GetFileNameWithoutExtension(values[5]) + ".ogg",
    //        //        dialogueText = values[3],
    //        //        topic = "消費税引き下げ"
    //        //    };
    //        //    Dailogues.Add(data);
    //        //}
    //        //Debug.Log("Loaded " + Dailogues.Count + " dialogues from CSV.");

    //    }
    //}

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        BuilderEventManager.AfterWorldOffcialWorldsInatantiated -= SetAvatars;
        SoundSettings.soundManagerSettings.UserSlider.onValueChanged.RemoveListener((float vol) =>
        {
            SetCanididatesVoice(vol);
        });

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
        if (currentVoiceRoutine != null)
        {
            StopCoroutine(currentVoiceRoutine);
            currentVoiceRoutine = null;
        }
        isVoiceLocked = false;
        currentPlayingVoiceUrl = "";
        audioSource.Stop();

        //Debug.Log("[CandidatesManager] OnDisable called. Coroutines stopped.");
    }

    private async void Start()
    {
        DialogueContainer.Clear();
        StartCoroutine(FetchVGDebiteCsvCoroutine());
        //await PreloadAudioBatchAsync(DialogueContainer[0]);
        //if (PhotonNetwork.IsMasterClient && Dailogues.Count > 0)
        //{
        //    StartSequence();
        //}
    }

    // Add this coroutine to your CandidatesManager class
    public IEnumerator FetchVGDebiteCsvCoroutine()
    {
        // Step 1: Call the API to get Api_Output
        string apiUrl = ConstantsGod.API_BASEURL + ConstantsGod.GetVGDebiteIndex;
        using (UnityWebRequest apiRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return apiRequest.SendWebRequest();

            if (apiRequest.result != UnityWebRequest.Result.Success)
            {
                //Debug.LogError($"[FetchVGDebiteCsvCoroutine] API call failed: {apiRequest.error}");
                yield break;
            }

            IndexResponse apiOutput = JsonUtility.FromJson <IndexResponse> (apiRequest.downloadHandler.text);
             
            //Debug.Log($"[FetchVGDebiteCsvCoroutine] API call succeeded. Output: {apiOutput.currentIndex}");
            // Optionally, parse apiOutput if it's JSON or needs processing
            // For now, assuming apiOutput is the required string for the CSV path

            // Step 2: Build the CSV URL
            string csvUrl = $"{VGS3Path}{apiOutput.currentIndex}/Sheet.csv";
            //string csvUrl = $"{VGS3Path}1/Sheet.csv";
            //Debug.Log("!! csvUrl" + csvUrl);
            // Step 3: Download the CSV
            using (UnityWebRequest csvRequest = UnityWebRequest.Get(csvUrl))
            {
                yield return csvRequest.SendWebRequest();

                if (csvRequest.result != UnityWebRequest.Result.Success)
                {
                    //Debug.LogError($"[FetchVGDebiteCsvCoroutine] CSV download failed: {csvRequest.error}");
                    yield break;
                }

                string csvContent = csvRequest.downloadHandler.text;
                var wrapper = ParseCsvToWrapper(csvContent, apiOutput.currentIndex);
                if (wrapper.Dailogues.Count == 0)
                {
                    //Debug.LogWarning("[FetchVGDebiteCsvCoroutine] No dialogues found in the CSV content.");
                    yield break;
                }
                else
                {
                    DialogueContainer.Add(wrapper);
                    isApiDataFetched = true;
                    PreloadFirstWrapperAudio();
                    //Debug.Log("[FetchVGDebiteCsvCoroutine] CSV content downloaded successfully." + csvContent);
                }
               
                // Process csvContent as needed
            }
        }
    }

    public DialougeWrapper ParseCsvToWrapper(string csvContent, string apiIndex)
    {
        var wrapper = new DialougeWrapper();
        var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
            return wrapper; // No data

        // Parse each data row (skip header)
        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');
            if (cols.Length > 7)
            {
                string fileName = Path.GetFileNameWithoutExtension(cols[5].Trim()); // F2 and onwards
                string voiceUrl = $"{VGS3Path}{apiIndex}/{fileName}.ogg";
                int avatarIndex = 0;
                int.TryParse(cols[6].Trim(), out avatarIndex); // G2 and onwards

                var dialogue = new DialogueData
                {
                    dialogueText = cols[3].Trim(),
                    voiceUrl = voiceUrl,
                    AvatarIndex = avatarIndex
                };
                wrapper.Dailogues.Add(dialogue);

                // Set topic for this row (H2 and onwards)
                wrapper.topic = cols[7].Trim();
            }
        }

        return wrapper;
    }


    private async void PreloadFirstWrapperAudio()
    {
        if (DialogueContainer.Count > 0)
            await PreloadAudioBatchAsync(DialogueContainer[0]);
    }
    private async Task PreloadAudioBatchAsync(DialougeWrapper targetWrapper)
    {
        if (targetWrapper == null)
        {
            //Debug.LogError("[PreloadAudioBatchAsync] Target wrapper is null. Cannot preload audio.");
            return;
        }

       // Debug.Log($"[PreloadAudioBatchAsync] Preloading audio for wrapper with topic: {targetWrapper.topic}");

        List<Task> preloadTasks = new List<Task>();

        foreach (var dialogue in targetWrapper.Dailogues)
        {
            if (!string.IsNullOrEmpty(dialogue.voiceUrl))
            {
                preloadTasks.Add(PreloadAudioAsync(dialogue.voiceUrl));
                if (preloadTasks.Count >= MaxConcurrentPreloads)
                {
                    await Task.WhenAll(preloadTasks);
                    preloadTasks.Clear();
                }
            }
        }

        if (preloadTasks.Count > 0)
        {
            await Task.WhenAll(preloadTasks);
        }

        //Debug.Log($"[PreloadAudioBatchAsync] All audio files for wrapper '{targetWrapper.topic}' have been preloaded.");

        // Set currentAvatarIndex to the first avatar index of the target wrapper
        if (targetWrapper.Dailogues.Count > 0)
        {
            currentAvatarIndex = targetWrapper.Dailogues[0].AvatarIndex;
            //Debug.Log($"[PreloadAudioBatchAsync] currentAvatarIndex set to: {currentAvatarIndex}");
        }
        else
        {
            //Debug.LogWarning($"[PreloadAudioBatchAsync] Target wrapper '{targetWrapper.topic}' has no dialogues.");
        }
    }

    private async Task PreloadAudioAsync(string url)
    {
        if (preloadedAudioClips.ContainsKey(url))
        {
            return;
        }

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS))
        {
            var operation = www.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                preloadedAudioClips[url] = clip;
                //Debug.Log($"[PreloadAudioAsync] Successfully preloaded audio for URL: {url}");
            }
            else
            {
                //Debug.LogError($"[PreloadAudioAsync] Failed to preload audio for URL: {url}. Error: {www.error}");
            }
        }
    }
   
    private IEnumerator PreloadAudioBatch()
    {
        foreach (var wrapper in DialogueContainer)
        {
            for (int i = 0; i < wrapper.Dailogues.Count; i += MaxConcurrentPreloads)
            {
                List<Coroutine> preloadCoroutines = new List<Coroutine>();

                for (int j = i; j < i + MaxConcurrentPreloads && j < wrapper.Dailogues.Count; j++)
                {
                    var dialogue = wrapper.Dailogues[j];
                    if (!string.IsNullOrEmpty(dialogue.voiceUrl))
                    {
                        preloadCoroutines.Add(StartCoroutine(PreloadAudio(dialogue.voiceUrl)));
                    }
                }

                foreach (var coroutine in preloadCoroutines)
                {
                    yield return coroutine;
                }
            }
        }

        audioIsPreloaded = true;
        //Debug.Log("[PreloadAudioBatch] All audio files have been preloaded.");
        if (PhotonNetwork.IsMasterClient && DialogueContainer.Count > 0)
        {
            StartSequence();
        }
    }

    private IEnumerator PreloadAudio(string url)
    {
        if (preloadedAudioClips.ContainsKey(url))
            yield break;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS))
        {
            var request = www.SendWebRequest();
            float timer = 0f;
            float timeout = 10f; // seconds

            while (!request.isDone)
            {
                timer += Time.deltaTime;
                if (timer > timeout)
                {
                    Debug.LogError($"[PreloadAudio] Timeout for URL: {url}");
                    yield break;
                }
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                preloadedAudioClips[url] = clip;
            }
            else
            {
                Debug.LogError($"[PreloadAudio] Failed to preload audio for URL: {url}. Error: {www.error}");
            }
        }
    }

    void StartSequence()
    {
        //Debug.Log("[StartSequence] Validating data...");

        if (DialogueContainer == null || DialogueContainer.Count == 0)
        {
            //Debug.LogError("[StartSequence] DialogueContainer is null or empty. Cannot start sequence.");
            return;
        }

        if (avatars == null || avatars.Count == 0)
        {
            //Debug.LogError("[StartSequence] Avatars list is null or empty. Cannot start sequence.");
            return;
        }

        if (isSequenceRunning)
        {
            //Debug.LogWarning("[StartSequence] Sequence is already running. Skipping.");
            return;
        }
        hasPlayedResumedClip = false;
        //Debug.Log("[StartSequence] Data validated. Starting sequence.");
        if (sequenceCoroutine != null)
        {
            //Debug.Log("[CandidatesManager] Stopping existing PlayAvatarsSequence...");
            StopCoroutine(sequenceCoroutine);
        }

        isSequenceRunning = true; // Set the flag to true
        shouldRunSequence = true; // Start the loop
        sequenceCoroutine = StartCoroutine(PlayAvatarsSequence());
    }

    public void StopSequence()
    {
        shouldRunSequence = false;
        isSequenceRunning = false;
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
        //Debug.Log("[CandidatesManager] Sequence stopped.");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //Debug.Log("[CandidatesManager] MasterClient switched.");
        PhotonView photonView = GetComponent<PhotonView>();

        if (photonView != null && newMasterClient != null)
        {
            photonView.TransferOwnership(newMasterClient);
            //Debug.Log($"[CandidatesManager] Ownership transferred to new MasterClient: {newMasterClient.ActorNumber}");
        }

        // Don't resend RPC  just wait and continue after it finishes
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(AvatarIndexKey, out object avatarIndexObj) &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(AudioStartTimeKey, out object audioStartTimeObj) &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(WrapperIndexKey, out object wrapperIndexObj))
            {
                currentAvatarIndex = (int)avatarIndexObj;
                currentAudioStartTime = (double)audioStartTimeObj;
                int currentWrapperIndex = (int)wrapperIndexObj;

                //Debug.Log($"[CandidatesManager] Will resume after voice finishes: AvatarIndex: {currentAvatarIndex}, StartTime: {currentAudioStartTime}, WrapperIndex: {currentWrapperIndex}");

                StartCoroutine(ResumeSequenceAfterClip(currentWrapperIndex));
            }
    }
    IEnumerator ResumeSequenceAfterClip(int wrapperIndex)
    {
        if (wrapperIndex < 0 || wrapperIndex >= DialogueContainer.Count)
        {
            //Debug.LogWarning("[ResumeSequenceAfterClip] Invalid wrapperIndex. Aborting resume.");
            yield break;
        }

        if (isWaitingForResume || hasPlayedResumedClip)
        {
            //Debug.Log("[ResumeSequenceAfterClip] Already waiting or resumed. Skipping.");
            yield break;
        }

        isWaitingForResume = true;

        var wrapper = DialogueContainer[wrapperIndex];
        var dialogue = wrapper.Dailogues.Find(d => d.AvatarIndex == currentAvatarIndex);

        if (dialogue != null && preloadedAudioClips.TryGetValue(dialogue.voiceUrl, out AudioClip clip))
        {
            double elapsed = PhotonNetwork.Time - currentAudioStartTime;

            if (elapsed >= clip.length - 0.1f)
            {
                //Debug.Log("[ResumeSequenceAfterClip] Audio already completed. Skipping resume.");
                isWaitingForResume = false;
                StartSequence();
                yield break;
            }

            float remaining = Mathf.Max(0f, clip.length - (float)elapsed);
            //Debug.Log($"[ResumeSequenceAfterClip] Waiting {remaining}s to continue sequence");
            yield return new WaitForSeconds(remaining + 0.2f); // small buffer

            if (PhotonNetwork.IsMasterClient && !isSequenceRunning)
            {
                hasPlayedResumedClip = true;
                isWaitingForResume = false;

                // Advance to next dialogue
                int currentIndex = wrapper.Dailogues.IndexOf(dialogue);
                int nextIndex = (currentIndex + 1) % wrapper.Dailogues.Count;
                currentAvatarIndex = wrapper.Dailogues[nextIndex].AvatarIndex;

                // Sync to room
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { AvatarIndexKey, currentAvatarIndex },
                { AudioStartTimeKey, PhotonNetwork.Time },
                { WrapperIndexKey, wrapperIndex }
            };
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);

                //Debug.Log("[ResumeSequenceAfterClip] Resuming sequence...");
                StartSequence();
            }
            else
            {
                isWaitingForResume = false;
            }
        }
        else
        {
            isWaitingForResume = false;
        }
    }

    public override void OnJoinedRoom()
    {
       // Debug.Log("[CandidatesManager] Player joined or rejoined the room.");
        PhotonNetwork.RemoveBufferedRPCs(photonView.ViewID, nameof(RPC_PlayAvatarVoice));

        // Ensure avatars are set up
        if (avatars.Count == 0)
        {
          //  Debug.Log("[CandidatesManager] Avatars list is empty. Attempting to set avatars.");
            SetAvatars();
        }

        // If this client is the master client, start the sequence
        if (PhotonNetwork.IsMasterClient && DialogueContainer.Count > 0)
        {
            //Debug.Log("[CandidatesManager] Restarting PlayAvatarsSequence as MasterClient.");
           // Debug.Log("[Start Sequence] Call from OnJoinedRoom");
            StartSequence();
        }
        else
        {
            // For non-master clients, check if the room properties contain the current state
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(AvatarIndexKey, out object avatarIndexObj) &&
                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(AudioStartTimeKey, out object audioStartTimeObj) &&
                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(WrapperIndexKey, out object wrapperIndexObj))
                {
                    int avatarIndex = (int)avatarIndexObj;
                    double audioStartTime = (double)audioStartTimeObj;
                    int wrapperIndex = (int)wrapperIndexObj;

                    if (wrapperIndex >= 0 && wrapperIndex < DialogueContainer.Count &&
                        avatarIndex >= 0 && avatarIndex < avatars.Count)
                    {
                        var wrapper = DialogueContainer[wrapperIndex];
                        var dialogue = wrapper.Dailogues.Find(d => d.AvatarIndex == avatarIndex);

                        if (dialogue != null && !string.IsNullOrEmpty(dialogue.voiceUrl))
                        {
                            double elapsed = PhotonNetwork.Time - audioStartTime;

                            if (preloadedAudioClips.TryGetValue(dialogue.voiceUrl, out AudioClip clip) && elapsed < (clip.length - 1f))
                        {
                                //Debug.Log($"[OnJoinedRoom] Replaying voice clip for AvatarIndex {avatarIndex} (elapsed: {elapsed}s)");

                                photonView.RPC(nameof(RPC_PlayAvatarVoice), RpcTarget.Others, avatarIndex, dialogue.voiceUrl, audioStartTime,
                                    avatars[avatarIndex - 1].avatarObject.GetComponent<Candidate>().CanidateNameJP, dialogue.dialogueText, wrapper.topic);
                            }
                            else
                            {
                               // Debug.LogWarning("[OnJoinedRoom] Audio already finished or not preloaded.");
                            }
                        }
                        else
                        {
                            //Debug.LogWarning("[OnJoinedRoom] No matching dialogue found.");
                        }
                    }
                }

        }
    }

    IEnumerator PlayAvatarsSequence()
    {
        float delayDuration = 1.0f;
        //Debug.Log("[PlayAvatarsSequence] Starting sequence...");

        foreach (var wrapper in DialogueContainer)
        {
           // Debug.Log($"[PlayAvatarsSequence] Processing wrapper with topic: {wrapper.topic}");

            foreach (var dialogue in wrapper.Dailogues)
            {
                if (!shouldRunSequence) break;

              //  Debug.Log($"[PlayAvatarsSequence] Processing dialogue for AvatarIndex: {dialogue.AvatarIndex}");

                if (currentAvatarIndex != dialogue.AvatarIndex)
                {
                   // Debug.Log($"[PlayAvatarsSequence] Skipping dialogue. CurrentAvatarIndex: {currentAvatarIndex}, Dialogue AvatarIndex: {dialogue.AvatarIndex}");
                    continue;
                }
                else
                {
                    var nextDialogueIndex = wrapper.Dailogues.IndexOf(dialogue) + 1;
                    if (nextDialogueIndex < wrapper.Dailogues.Count)
                    {
                        currentAvatarIndex = wrapper.Dailogues[nextDialogueIndex].AvatarIndex;
                    }
                    else
                    {
                        currentAvatarIndex = wrapper.Dailogues[0].AvatarIndex;
                    }
                  //  Debug.Log($"[PlayAvatarsSequence] Updated currentAvatarIndex to: {currentAvatarIndex}");
                }

                if (dialogue == null || string.IsNullOrEmpty(dialogue.voiceUrl) || dialogue.AvatarIndex < 0 || dialogue.AvatarIndex >= avatars.Count)
                {
                 //   Debug.LogWarning($"[PlayAvatarsSequence] Invalid dialogue data. Skipping. Dialogue: {dialogue}");
                    continue;
                }

                Avatar avatar = avatars[dialogue.AvatarIndex];

                if (avatar == null || avatar.avatarObject == null)
                {
                  //  Debug.LogWarning($"[PlayAvatarsSequence] Avatar at index {dialogue.AvatarIndex} is null or invalid. Skipping.");
                    continue;
                }

                if (!preloadedAudioClips.TryGetValue(dialogue.voiceUrl, out AudioClip clip))
                {
                  //  Debug.LogWarning($"[PlayAvatarsSequence] Audio not preloaded for URL: {dialogue.voiceUrl}. Skipping.");
                    continue;
                }

                if (audioSource == null)
                {
                 //   Debug.LogError("[PlayAvatarsSequence] AudioSource is null. Cannot play audio.");
                    yield break;
                }

                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { AvatarIndexKey, dialogue.AvatarIndex },
                { AudioStartTimeKey, PhotonNetwork.Time },
                { WrapperIndexKey, DialogueContainer.IndexOf(wrapper) }
            };

                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                currentAudioStartTime = PhotonNetwork.Time;
               // Debug.Log($"[PlayAvatarsSequence] Set room properties: AvatarIndex = {dialogue.AvatarIndex}, AudioStartTime = {PhotonNetwork.Time}");

              //  Debug.Log($"[PlayAvatarsSequence] Syncing play for AvatarIndex: {dialogue.AvatarIndex}, Name: {avatars[dialogue.AvatarIndex].avatarObject.GetComponent<Candidate>().CanidateNameJP}");
                photonView.RPC(nameof(RPC_PlayAvatarVoice), RpcTarget.All, dialogue.AvatarIndex, dialogue.voiceUrl, PhotonNetwork.Time, avatars[dialogue.AvatarIndex - 1].avatarObject.GetComponent<Candidate>().CanidateNameJP, dialogue.dialogueText, wrapper.topic);

              //  Debug.Log($"[PlayAvatarsSequence] Playing audio for AvatarIndex: {dialogue.AvatarIndex}, URL: {dialogue.voiceUrl}");

              //  Debug.Log($"[PlayAvatarsSequence] Waiting for audio to finish. Clip length: {clip.length}");
                yield return new WaitForSeconds(clip.length);

              //  Debug.Log($"[PlayAvatarsSequence] Adding delay of {delayDuration} seconds before the next dialogue.");
                yield return new WaitForSeconds(delayDuration);
            }
            if (!shouldRunSequence) break;
        }

        //Debug.Log("[PlayAvatarsSequence] Sequence completed.");
        isSequenceRunning = false;
        shouldRunSequence = false;
    }

    [PunRPC]
    void RPC_PlayAvatarVoice(int avatarIndex, string voiceMp3Url, double startTime, string name, string des, string topic)
    {
        double elapsed = PhotonNetwork.Time - startTime;

        if (!preloadedAudioClips.TryGetValue(voiceMp3Url, out AudioClip clip))
        {
            //Debug.LogWarning($"[RPC_PlayAvatarVoice] Clip not preloaded: {voiceMp3Url}");
            return;
        }

        if (elapsed >= clip.length)
        {
            //Debug.LogWarning("[RPC_PlayAvatarVoice] Skipped: audio already completed");
            return;
        }

        // Stop previous
        if (currentVoiceRoutine != null)
        {
            StopCoroutine(currentVoiceRoutine);
            currentVoiceRoutine = null;
        }

        StartCoroutine(WaitUntilManagerInitializedFalse(() =>
        {
            SetActiveCandidateCamera(avatarIndex);
            UpdateOverlayScreenMaterial(true, name, des, topic);
            currentVoiceRoutine = StartCoroutine(PlayAvatarVoiceRoutine(avatarIndex, voiceMp3Url, startTime));
        }));
    }


    IEnumerator WaitUntilManagerInitializedFalse(System.Action onComplete)
    {
        // Wait until managerInitialized becomes true
        while (managerInitialized)
        {
            //Debug.Log("[WaitUntilManagerInitializedFalse] Waiting for managerInitialized to become true...");
            yield return null; // Wait for the next frame
        }

        // Execute the action once the condition is met
        onComplete?.Invoke();
    }
    IEnumerator PlayAvatarVoiceRoutine(int avatarIndex, string voiceMp3Url, double startTime)
    {
        if (avatarIndex < 0 || avatarIndex >= avatars.Count)
            yield break;

        //Debug.Log($"[PlayAvatarVoiceRoutine] Requested voice: {voiceMp3Url}, StartTime: {startTime}");

        if (isVoiceLocked && currentPlayingVoiceUrl == voiceMp3Url)
        {
           // Debug.Log("[PlayAvatarVoiceRoutine] Voice already playing with lock. Skipping.");
            yield break;
        }

        // Lock voice
        isVoiceLocked = true;
        currentPlayingVoiceUrl = voiceMp3Url;

        // Stop any ongoing playback
        if (audioSource.isPlaying)
            audioSource.Stop();

        Animator animator = null;
        for (int i = 0; i < avatars.Count; i++)
        {
            if (avatarIndex == avatars[i].id)
            {
                animator = avatars[i].avatarObject.GetComponent<Animator>();
            }
        }

        if (animator != null)
            animator.SetBool("IsSpeaking", true);

        if (!preloadedAudioClips.TryGetValue(voiceMp3Url, out AudioClip clip))
        {
            while (!preloadedAudioClips.TryGetValue(voiceMp3Url, out clip))
                yield return null;
        }

        float offset = Mathf.Clamp((float)(PhotonNetwork.Time - startTime), 0, clip.length);
        audioSource.clip = clip;
        audioSource.time = offset;
        audioSource.Play();

        //Debug.Log($"[PlayAvatarVoiceRoutine] Playing clip: {voiceMp3Url} with offset: {offset}");

        yield return new WaitForSeconds(clip.length - offset);

        if (animator != null)
            animator.SetBool("IsSpeaking", false);

        // Unlock
        isVoiceLocked = false;
        currentPlayingVoiceUrl = "";
    }


    public void SetAvatars()
    {
        StartCoroutine(WaitForCandidatesToBeReady());
    }

    private IEnumerator WaitForCandidatesToBeReady()
    {
        print("CandidateObjectReference.Count : " + CandidateSpwanner.CandidateObjectReference.Count + " CandidateSpwanner.candidateCount : " + CandidateSpwanner.candidateCount);
        // Wait until the count of CandidateObjectReference matches candidateCount
        //yield return new WaitUntil(() => CandidateSpwanner.CandidateObjectReference.Count == CandidateSpwanner.candidateCount);
        //yield return new WaitUntil(() => isApiDataFetched); // wait till API data is fetched
        float timeout = 10f; // seconds
        float timer = 0f;
        while (CandidateSpwanner.CandidateObjectReference.Count != CandidateSpwanner.candidateCount)
        {
            if (timer > timeout)
            {
                Debug.LogError("Timeout waiting for candidates to be ready!");
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;
        while (!isApiDataFetched)
        {
            if (timer > timeout)
            {
                Debug.LogError("Timeout waiting for API data to be fetched!");
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        avatars.Clear();

        foreach (var candidateObj in CandidateSpwanner.CandidateObjectReference)
        {
            if (candidateObj == null) continue;

            candidateObj.transform.SetParent(this.transform);
            candidateObj.GetComponent<PhotonAnimatorView>().enabled = true;

            Candidate candidate = candidateObj.GetComponent<Candidate>();
            if (candidate == null) continue;

            Avatar avatar = new Avatar
            {
                id = candidate.CanidateId,
                avatarObject = candidateObj,
                CameraObj = candidate.canididateCamera
            };
            avatars.Add(avatar);
        }

        managerInitialized = false;
        avatars.Sort((a, b) => a.id.CompareTo(b.id));

        //Debug.Log("[WaitForCandidatesToBeReady] Avatars set. Starting audio preload.");
        StartCoroutine(PreloadAudioBatch());
    }

    public void SetCanididatesVoice(float val)
    {
        audioSource.volume = val;
    }

    public void SetActiveCandidateCamera(int speakingCandidateIndex)
    {
        for (int i = 0; i < avatars.Count; i++)
        {
            if (avatars[i].CameraObj != null)
            {
                bool isActive = avatars[i].id == speakingCandidateIndex;
                avatars[i].CameraObj.SetActive(isActive);

                if (isActive)
                {
                    // Assign the shared RenderTexture to the camera
                    Camera camera = avatars[i].CameraObj.GetComponent<Camera>();
                    if (camera != null)
                    {
                        camera.targetTexture = sharedRenderTexture;
                    }
                    else
                    {
                        Debug.LogError($"[SetActiveCandidateCamera] Camera component not found on {avatars[i].CameraObj.name}");
                        continue;
                    }

                    // Assign the shared RenderTexture to the screen material
                    if (VirtualGovernmentScreen.Instance != null && VirtualGovernmentScreen.Instance.screenMesh != null)
                    {
                        VirtualGovernmentScreen.Instance.screenMesh.GetComponent<MeshRenderer>().material.mainTexture = sharedRenderTexture;
                        //Debug.Log($"[SetActiveCandidateCamera] Shared RenderTexture assigned to VideoScreenMaterial for avatar {i}");
                    }
                    else
                    {
                      //  Debug.LogError("[SetActiveCandidateCamera] VideoScreenMaterial or VirtualGovernmentScreen.Instance is null.");
                    }
                }
            }
        }
    }

    public void UpdateOverlayScreenMaterial(bool isScreenActive, string name, string description, string topic)
    {
       // Debug.Log($"[UpdateOverlayScreenMaterial] Called with isScreenActive: {isScreenActive}");

        foreach (var avatar in avatars)
        {
            if (avatar.avatarObject == null)
            {
                //Debug.Log("[UpdateOverlayScreenMaterial] Skipping avatar with null avatarObject.");
                continue;
            }

            if (VirtualGovernmentScreen.Instance != null && VirtualGovernmentScreen.Instance.screenMesh != null)
            {
               // Debug.Log($"[UpdateOverlayScreenMaterial] Instance is not null");

                // Change the material color to white or black based on isScreenActive
                Material screenMaterial = VirtualGovernmentScreen.Instance.screenMesh.GetComponent<MeshRenderer>().material;
                if (screenMaterial != null)
                {
                    screenMaterial.color = Color.white;
                    //screenMaterial.color = isScreenActive ? Color.white : Color.black;
                   // Debug.Log($"[UpdateOverlayScreenMaterial] Material color set to {(isScreenActive ? "white" : "black")}.");
                }
                else
                {
                   // Debug.LogError("[UpdateOverlayScreenMaterial] Screen material is null.");
                }

                if (VirtualGovernmentScreen.Instance.overlayObject != null)
                {
                    //VirtualGovernmentScreen.Instance.overlayObject.SetActive(isScreenActive);
                    VirtualGovernmentScreen.Instance.overlayObject.SetActive(true); // making breaking new always on.
                    VirtualGovernmentScreen.Instance.UpdateTextOnOverlay(name, description, topic);
                   // Debug.Log($"[UpdateOverlayScreenMaterial] Overlay object set to active: {isScreenActive}");
                }
                else
                {
                   // Debug.Log("[UpdateOverlayScreenMaterial] Overlay object is null.");
                }
            }
            else
            {
              //  Debug.Log("[UpdateOverlayScreenMaterial] VirtualGovernmentScreen.Instance or screenMesh is null.");
            }
        }
    }
    private void OnDestroy()
    {
        if (currentVoiceRoutine != null)
        {
            StopCoroutine(currentVoiceRoutine);
            currentVoiceRoutine = null;
        }
        isVoiceLocked = false;
        currentPlayingVoiceUrl = "";
        audioSource.Stop();
        // Clean up the RenderTexture
        if (sharedRenderTexture != null)
        {
            sharedRenderTexture.Release();
            sharedRenderTexture = null;
        }

        // Stop any running coroutines
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
       
        preloadedAudioClips.Clear();

        Debug.Log("[CandidatesManager] OnDestroy called. Resources released.");
    }
    
}
