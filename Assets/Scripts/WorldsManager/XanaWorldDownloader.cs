using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class XanaWorldDownloader : MonoBehaviour
{

    //booleans 
    public static bool stopDownloading;
    public static bool downloadIsGoingOn;
    public static bool dataArranged;
    public static bool dataSorted;
    public static bool isSpawnDownloaded;
    public static bool isDefaultPriorityObjectDownloaded;
    public static bool isfailedObjectsDownloaded;
    public static bool isLowPriorityDownloaded;

    public Transform assetParent;
    public TMPro.TextMeshProUGUI assetDownloadingText;
    public TMPro.TextMeshProUGUI assetDownloadingTextPotrait;

    public static Vector3 initialPlayerPos;
    public static Vector3 currPlayerPosition;
    public static Transform assetParentStatic;
    private static int totalAssetCount;
    public static int downloadedTillNow = 0;
    private static HashSet<string> uniqueDownloadKeys = new HashSet<string>();
    public static List<string> DownloadedWorldNames = new List<string>();
    public static long downloadSize;

    [Header("Short Interval sorting element count")]
    public static int shortSortingCount = 100;
    public static int timeshortSorting = 10;
    [Header("Sorting full list again")]
    public static int timeFullSorting = 100;

    public static List<DownloadQueueData> downloadDataQueue = new List<DownloadQueueData>();
    public static List<DownloadQueueData> preLoadObjects = new List<DownloadQueueData>();
    public static List<DownloadQueueData> postLoadObjects = new List<DownloadQueueData>();
    public static List<DownloadQueueData> downloadFailed = new List<DownloadQueueData>();
    public static Dictionary<string, ObjectsInfo> xanaWorldDataDictionary = new Dictionary<string, ObjectsInfo>();
    public static Dictionary<string, GameObject> prefabObjectPool = new Dictionary<string, GameObject>();
    public static List<GameObject> AllDomes = new List<GameObject>();

    public static SceneData xanaSceneData = new SceneData();

    private string response;

    private float unloadDistance = 50;

    public static XanaWorldDownloader xanaWorldDownloader;
    public DownloadPopupHandler DownloadPopupHandlerInstance;

    private static CancellationTokenSource cts;

    private void Start()
    {
        xanaWorldDownloader = this;
    }

    void GetDataFromAPI(string worldID, bool islocal)
    {
        if (islocal)
        {
            response = worldID;
            xanaSceneData = JsonUtility.FromJson<SceneData>(worldID);
        }
        else
        {
            StartCoroutine(DownloadSceneData(worldID));
        }
    }

    IEnumerator DownloadSceneData(string worldID)
    {
        string _url = ConstantsGod.API_BASEURL + ConstantsGod.GETXANAOFFICIALWORLDBYID + worldID;
        using (UnityWebRequest www = UnityWebRequest.Get(_url))
        {
            www.SetRequestHeader("Authorization", ConstantsGod.AUTH_TOKEN);
            www.SendWebRequest();
            while (!www.isDone)
            {
                yield return null;
            }
            if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
            {
                response = www.downloadHandler.text;
            }
            else
            {
                response = www.downloadHandler.text;
                xanaSceneData = JsonUtility.FromJson<SceneData>(www.downloadHandler.text);
            }
        }
    }

    private void OnEnable()
    {
//#if UNITY_EDITOR
//        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
//#endif
        if (assetParent)
            assetParentStatic = assetParent;
        if (!ConstantsHolder.xanaConstants.isBuilderScene || ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            BuilderEventManager.XanaMapDataDownloaded += PostLoadingBuilderAssets;
            ScreenOrientationManager.switchOrientation += OnOrientationChange;
        }
    }

    private void OnDisable()
    {
        BuilderEventManager.XanaMapDataDownloaded -= PostLoadingBuilderAssets;
        ScreenOrientationManager.switchOrientation -= OnOrientationChange;

//#if UNITY_EDITOR
//        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
//#endif
    }

    async void PostLoadingBuilderAssets(string worldData)
    {
        isOneTimeLoad = true;
        cts = new CancellationTokenSource();
        GetDataFromAPI(worldData, true);
        ArrangeData();
        while (!dataArranged)
        {
            await Task.Yield();
        }
        if (!DownloadPopupHandler.AlwaysAllowDownload && !CheckForVisitedWorlds(ConstantsHolder.xanaConstants.EnviornmentName))
        {
            if (!DownloadedWorldNames.Contains(ConstantsHolder.xanaConstants.EnviornmentName))
                DownloadedWorldNames.Add(ConstantsHolder.xanaConstants.EnviornmentName);
            bool permission = await DownloadPopupHandlerInstance.ShowDialogAsync();
            if (!permission)
                return;
        }

        StartCoroutine(DownloadObjects(preLoadObjects, Priority.High));
        while (!isSpawnDownloaded)
        {
            await Task.Yield();
        }
        if (totalAssetCount > downloadedTillNow)
        {
            EnableDownloadingText();
        }
        
        try
        {
            await StartDownloadingAssets(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("<color=red>task Canceled</color>");
        }
    }


    void LoadAddressableSceneAfterDownload()
    {
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
    }

    public static void ArrangeData()
    {
        try
        {
            for (int i = 0; i < xanaSceneData.SceneObjects.Count; i++)
            {
                DownloadQueueData temp = new DownloadQueueData();
                temp.ItemID = xanaSceneData.SceneObjects[i].addressableKey;
                if (!uniqueDownloadKeys.Contains(xanaSceneData.SceneObjects[i].addressableKey) && !XanaWorldDownloader.CheckForVisitedWorlds(ConstantsHolder.xanaConstants.EnviornmentName))
                {
                    //Debug.Log("<color=red>Calculate Download Size</color>");
                    uniqueDownloadKeys.Add(xanaSceneData.SceneObjects[i].addressableKey);
                    downloadSize += Addressables.GetDownloadSizeAsync(xanaSceneData.SceneObjects[i].addressableKey).WaitForCompletion();
                }
                temp.DcitionaryKey = i.ToString();
                temp.Position = xanaSceneData.SceneObjects[i].position;
                temp.Rotation = xanaSceneData.SceneObjects[i].rotation;
                temp.Scale = xanaSceneData.SceneObjects[i].scale;

                if (!xanaWorldDataDictionary.ContainsKey(i.ToString()))
                {
                    xanaWorldDataDictionary.Add(i.ToString(), xanaSceneData.SceneObjects[i]);
                    if (xanaSceneData.SceneObjects[i].priority == Priority.High)
                    {
                        preLoadObjects.Add(temp);
                    }
                    else if (xanaSceneData.SceneObjects[i].priority == Priority.Low)
                    {
                        postLoadObjects.Add(temp);
                    }
                    else
                    {
                        downloadDataQueue.Add(temp);
                        totalAssetCount++;
                    }
                }
            }
            uniqueDownloadKeys.Clear();
            dataArranged = true;
        }
        catch (Exception e)
        {
            Debug.Log("<color=red>An error occurred: " + e.Message + "</color>");
        }
    }

    //Sorting data on start and after long Interval
    public static void SortingQueueData(Vector3 playerPos)
    {
        List<Tuple<DownloadQueueData, float>> distances = new List<Tuple<DownloadQueueData, float>>();
        foreach (DownloadQueueData position in downloadDataQueue)
        {
            float distance = Vector3.Distance(playerPos, position.Position);
            distances.Add(new Tuple<DownloadQueueData, float>(position, distance));
        }
        // Sort the list based on distances in ascending order
        distances.Sort((a, b) => a.Item2.CompareTo(b.Item2));

        downloadDataQueue.Clear();
        // Now, distances contains the vector positions sorted by proximity to the player position
        foreach (var item in distances)
        {
            ////Debug.LogError(item.Item1.ItemID+"---"+item.Item2);
            downloadDataQueue.Add(item.Item1);
            // //Debug.Log("Position: " + item.Item1 + ", Distance: " + item.Item2);
        }
        dataSorted = true;
    }

    static void SortingDataShortInterval(Vector3 playerPos)
    {
        List<Tuple<DownloadQueueData, float>> distances = new List<Tuple<DownloadQueueData, float>>();

        if (shortSortingCount > downloadDataQueue.Count)
            shortSortingCount = downloadDataQueue.Count;

        for (int i = 0; i < shortSortingCount; i++)
        {
            float distance = Vector3.Distance(playerPos, downloadDataQueue[i].Position);
            distances.Add(new Tuple<DownloadQueueData, float>(downloadDataQueue[i], distance));
        }

        // Sort the list based on distances in ascending order
        distances.Sort((a, b) => a.Item2.CompareTo(b.Item2));

        downloadDataQueue.RemoveRange(0, distances.Count);
        int x = 0;
        // Now, distances contains the vector positions sorted by proximity to the player position
        foreach (var item in distances)
        {
            downloadDataQueue.Insert(x, item.Item1);
            x++;
            ////Debug.Log("Position: " + item.Item1 + ", Distance: " + item.Item2);
        }
    }

    public async Task StartDownloadingAssets(CancellationToken token)
    {
        await DownloadAssetsFromSortedListAsync(token);
    }

    private async Task WaitUntil(Func<bool> condition, CancellationToken token)
    {
        while (!condition() /*&& !token.IsCancellationRequested*/)
        {
            await Task.Yield();
        }
    }
    private async Task CheckForInternetConnection()
    {
        if (Application.internetReachability.Equals(NetworkReachability.NotReachable))
        {
            // Disable Asset downloading UI
            assetDownloadingText.transform.parent.gameObject.SetActive(false);
            assetDownloadingTextPotrait.transform.parent.gameObject.SetActive(false);
            // Wait until internet is restored and player is spawned in room
            while (Application.internetReachability.Equals(NetworkReachability.NotReachable) || GameplayEntityLoader.instance.isLocalPlayer)
            {
                await Task.Delay(1000);
            }
            // Enable Asset downloading UI
            assetDownloadingText.transform.parent.gameObject.SetActive(true);
            assetDownloadingTextPotrait.transform.parent.gameObject.SetActive(true);
        }
    }
    private async Task DownloadAssetsFromSortedListAsync(CancellationToken token)
    {
        const int maxRetryCount = 5;
        const float retryDelay = 0.1f;
        const float endOfFrameDelay = 0.05f;
        const int maxConcurrentDownloads = 5; // Adjust this value based on your requirements

        HashSet<string> downloadedKeys = new HashSet<string>();
        List<Task> downloadTasks = new List<Task>();

        while (downloadDataQueue.Count > 0 && !stopDownloading)
        {
            while (downloadTasks.Count < maxConcurrentDownloads && downloadDataQueue.Count > 0)
            {
                var downloadData = downloadDataQueue[0];
                downloadDataQueue.RemoveAt(0);
                downloadTasks.Add(DownloadAndInstantiateAssetAsync(downloadData, maxRetryCount, retryDelay, token, downloadedKeys));
            }

            await Task.WhenAny(downloadTasks);
            downloadTasks.RemoveAll(task => task.IsCompleted);

            await Task.Delay(TimeSpan.FromSeconds(endOfFrameDelay));
        }

        await Task.WhenAll(downloadTasks);

        if (totalAssetCount == downloadedTillNow)
            LoadingFlagUpdate(Priority.defaultPriority);

        // Call the events after all assets are spawned
        if (isOneTimeLoad)
        {
            isOneTimeLoad = false;
            BuilderEventManager.AfterWorldOffcialWorldsInatantiated?.Invoke();
            ReferencesForGamePlay.instance.SumitMapStatus(true);
            GameplayEntityLoader.instance.ForcedMapOpenForSummitScene();

        }
    }
    bool isOneTimeLoad= true;
    private async Task DownloadAndInstantiateAssetAsync(DownloadQueueData downloadData, int maxRetryCount, float retryDelay, CancellationToken token, HashSet<string> downloadedKeys)
    {
        string downloadKey = downloadData.ItemID;
        string dicKey = downloadData.DcitionaryKey;
        AsyncOperationHandle<GameObject> _async = default;
        int _assetDownloadTryCount = 0;

        while (_assetDownloadTryCount < maxRetryCount)
        {
            // Wait for internet before each attempt
            await CheckForInternetConnection();

            _async = Addressables.LoadAssetAsync<GameObject>(downloadKey);
            await _async.Task;

            if (_async.Status == AsyncOperationStatus.Succeeded && _async.Result != null)
            {
                AddressableDownloader.bundleAsyncOperationHandle.Add(_async);
                InstantiateAsset(_async.Result, xanaWorldDataDictionary[dicKey], dicKey);

                if (downloadedKeys.Add(downloadKey))
                {
                    Interlocked.Increment(ref downloadedTillNow);
                }

                DisplayDownloadedAssetText();
                return;
            }
            else
            {
                // Release and clear everything before retry
                Addressables.ClearDependencyCacheAsync(downloadKey);
                Addressables.ReleaseInstance(_async);
                Addressables.Release(_async);

                // If internet is lost, wait for it to return and retry without incrementing retry count
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    await CheckForInternetConnection();
                    continue;
                }
                _assetDownloadTryCount++;
                await Task.Delay(TimeSpan.FromSeconds(retryDelay));
            }
        }

        downloadFailed.Add(downloadData);
    }

    private IEnumerator DownloadAssetsFromSortedListCoroutine(TaskCompletionSource<bool> tcs, CancellationToken token)
    {
        yield return DownloadAssetsFromSortedList();
        tcs.SetResult(true);
    }

    //private async Task DownloadObjectsAsync(List<DownloadQueueData> downloadQueues, Priority priority, CancellationToken token)
    //{
    //    var downloadTask = new TaskCompletionSource<bool>();
    //    StartCoroutine(DownloadObjectsCoroutine(downloadTask, downloadQueues, priority, token));
    //    await downloadTask.Task;
    //}
    private async Task DownloadObjectsAsync(List<DownloadQueueData> downloadQueues, Priority priority, CancellationToken token)
    {
        List<Task> downloadTasks = new List<Task>();

        foreach (var queueData in downloadQueues)
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            // Check if the object is already in the pool
            if (prefabObjectPool.TryGetValue(queueData.ItemID, out GameObject pooledObject))
            {
                InstantiateAsset(pooledObject, queueData, true);
            }
            else
            {
                // Batch the addressable loading operations
                downloadTasks.Add(LoadAndInstantiateAsset(queueData));
            }
        }

        // Wait for all batched tasks to complete
        await Task.WhenAll(downloadTasks);
    }

    private async Task LoadAndInstantiateAsset(DownloadQueueData queueData)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(queueData.ItemID);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedObject = handle.Result;
            AddObjectInPool(queueData.ItemID, loadedObject);
            InstantiateAsset(loadedObject, queueData, false);
        }
        else
        {
            Debug.LogError($"Failed to load asset: {queueData.ItemID}");
        }
    }

    private void InstantiateAsset(GameObject prefab, DownloadQueueData queueData, bool alreadyInstantiated)
    {
        GameObject instance;
        if (alreadyInstantiated)
        {
            instance = prefab;
            instance.SetActive(true);
        }
        else
        {
            instance = GameObject.Instantiate(prefab, queueData.Position, queueData.Rotation, assetParent);
            instance.transform.localScale = queueData.Scale;
        }

        // Apply additional properties if needed
        instance.name = queueData.ItemID;
        instance.SetActive(queueData.IsActive);
    }

    //private void AddObjectInPool(string objectKey, GameObject prefabObject)
    //{
    //    if (!prefabObjectPool.ContainsKey(objectKey))
    //    {
    //        prefabObjectPool[objectKey] = prefabObject;
    //    }
    //}

    private IEnumerator DownloadObjectsCoroutine(TaskCompletionSource<bool> tcs, List<DownloadQueueData> downloadQueues, Priority priority, CancellationToken token)
    {
        yield return DownloadObjects(downloadQueues, priority);
        tcs.SetResult(true);
    }

    private async Task DownloadFailedItemAsync(CancellationToken token)
    {
        var downloadTask = new TaskCompletionSource<bool>();
        StartCoroutine(DownloadFailedItemCoroutine(downloadTask, token));
        await downloadTask.Task;
    }

    private IEnumerator DownloadFailedItemCoroutine(TaskCompletionSource<bool> tcs, CancellationToken token)
    {
        yield return DownloadFailedItem();
        tcs.SetResult(true);
    }

    IEnumerator DownloadAssetsFromSortedList()
    {
        const int maxRetryCount = 5;
        const float retryDelay = 0.1f;
        const float endOfFrameDelay = 0.05f;

        while (downloadDataQueue.Count > 0 && !stopDownloading)
        {
            downloadIsGoingOn = true;
            var downloadData = downloadDataQueue[0];
            string downloadKey = downloadData.ItemID;
            string dicKey = downloadData.DcitionaryKey;
            AsyncOperationHandle<GameObject> _async = default;
            int _assetDownloadTryCount = 0;

            while (_assetDownloadTryCount < maxRetryCount)
            {
                _async = Addressables.LoadAssetAsync<GameObject>(downloadKey);
                yield return _async;

                if (_async.Status == AsyncOperationStatus.Succeeded && _async.Result != null)
                {
                    AddressableDownloader.bundleAsyncOperationHandle.Add(_async);
                    InstantiateAsset(_async.Result, xanaWorldDataDictionary[dicKey], dicKey);
                    break;
                }
                else
                {
                    Addressables.ClearDependencyCacheAsync(downloadKey);
                    Addressables.ReleaseInstance(_async);
                    Addressables.Release(_async);
                    _assetDownloadTryCount++;
                    yield return new WaitForSeconds(retryDelay);
                }
            }

            if (_async.Status == AsyncOperationStatus.Succeeded && _async.Result != null)
            {
                downloadDataQueue.RemoveAt(0);
                DisplayDownloadedAssetText();
            }
            else
            {
                downloadFailed.Add(downloadDataQueue[0]);
                downloadDataQueue.RemoveAt(0);
            }

            downloadIsGoingOn = false;
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(endOfFrameDelay);
        }

        if (totalAssetCount == downloadedTillNow)
            LoadingFlagUpdate(Priority.defaultPriority);
    }

    IEnumerator DownloadFailedItem()
    {
        while (downloadFailed.Count > 0)
        {
            string downloadKey = downloadFailed[0].ItemID;
            string dicKey = downloadFailed[0].DcitionaryKey;
            AsyncOperationHandle<GameObject> _async = default;
            int _assetDownloadTryCount = 0;

            while (_assetDownloadTryCount < 5)
            {
                _async = Addressables.LoadAssetAsync<GameObject>(downloadKey);
                yield return _async;

                if (_async.Status == AsyncOperationStatus.Succeeded && _async.Result != null)
                {
                    AddressableDownloader.bundleAsyncOperationHandle.Add(_async);
                    InstantiateAsset(_async.Result, xanaWorldDataDictionary[dicKey], dicKey);
                    break;
                }
                else
                {
                    Addressables.ClearDependencyCacheAsync(downloadKey);
                    Addressables.ReleaseInstance(_async);
                    Addressables.Release(_async);
                    _assetDownloadTryCount++;
                    yield return new WaitForSeconds(0.1f);
                }
            }

            if (_async.Status == AsyncOperationStatus.Succeeded && _async.Result != null)
            {
                DisplayDownloadedAssetText();
            }
            else
            {
                // Log error or handle the failed download case
                // Debug.LogError("Download Failed......");
            }

            downloadFailed.RemoveAt(0);
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.01f);
        }

        isfailedObjectsDownloaded = true;

        if (totalAssetCount == downloadedTillNow)
        {
            BuilderEventManager.AfterWorldOffcialWorldsInatantiated?.Invoke();

            // Force Enable Map When all Data is Download

            //GameplayEntityLoader.instance.ForcedMapOpenForSummitScene();

        }
    }

    public IEnumerator DownloadObjects(List<DownloadQueueData> downloadQueues, Priority objectLoadingPriority)
    {
        for (int i = 0; i < downloadQueues.Count; i++)
        {
            string downloadKey = downloadQueues[i].ItemID;
            string dicKey = downloadQueues[i].DcitionaryKey;
            AsyncOperationHandle<GameObject> _async;
            //GameObject objectfromPool = GetObjectFromPool(downloadKey);
            //if (objectfromPool)
            //{
            //    InstantiateAsset(objectfromPool, xanaWorldDataDictionary[dicKey], true);
            //    yield break;
            //}
            //else
            int _assetDownloadTryCount = 0;
        LoadAssetAgain:
            _async = Addressables.LoadAssetAsync<GameObject>(downloadKey);
            while (!_async.IsDone)
            {
                yield return null;
            }
            if (_async.IsValid() && _async.Result != null)
            {
                
            }
            else
            {
                Addressables.ClearDependencyCacheAsync(downloadKey);
                Addressables.ReleaseInstance(_async);
                Addressables.Release(_async);
                _assetDownloadTryCount++;
                if (_assetDownloadTryCount < 5)
                {
                    yield return new WaitForSeconds(.1f);
                    goto LoadAssetAgain;
                }
            }
            if (_async.Status == AsyncOperationStatus.Succeeded && _async.Result != null)
            {
                AddressableDownloader.bundleAsyncOperationHandle.Add(_async);
                InstantiateAsset(_async.Result, xanaWorldDataDictionary[dicKey], dicKey);
            }
            else
            {
                //Debug.LogError(_async.Status);
            }
            yield return new WaitForSeconds(0.01f);
        }
       
        LoadingFlagUpdate(objectLoadingPriority);
    }

    void LoadingFlagUpdate(Priority flag)
    {
        switch (flag)
        {
            case Priority.defaultPriority:
                isDefaultPriorityObjectDownloaded = true;
                break;
            case Priority.High:
                isSpawnDownloaded = true;
                break;
            case Priority.Low:
                isLowPriorityDownloaded = true;
                break;
        }
    }

    void EnableDownloadingText()
    {
        assetDownloadingText.transform.parent.gameObject.SetActive(true);
        assetDownloadingTextPotrait.transform.parent.gameObject.SetActive(false);
    }

    void DisplayDownloadedAssetText()
    {
        ++downloadedTillNow;
        switch (GameManager.currentLanguage)
        {

            case "en":
                assetDownloadingText.text = "World will be ready once loading is complete... " + (downloadedTillNow) + "/" + (totalAssetCount);
                assetDownloadingTextPotrait.text = "World will be ready once loading is complete... " + (downloadedTillNow) + "/" + (totalAssetCount);
                if (downloadedTillNow >= totalAssetCount)
                {
                    assetDownloadingText.text = "Loading Completed.... " + downloadedTillNow + "/" + (totalAssetCount);
                    assetDownloadingTextPotrait.text = "Loading Completed.... " + downloadedTillNow + "/" + (totalAssetCount);

                    assetDownloadingText.transform.parent.gameObject.SetActive(false);
                    assetDownloadingTextPotrait.transform.parent.gameObject.SetActive(false);

                    // All Assets are loaded
                    if (ConstantsHolder.xanaConstants.saudiEventDomeId > 0 && !ConstantsHolder.xanaConstants.isSaudiEvent) DomeMinimapDataHolder.RedirectToRequireDome?.Invoke();
                }
                break;
            case "ja":
                assetDownloadingText.text = "ロード完了後、ワールドの準備が整います... " + (downloadedTillNow) + "/" + (totalAssetCount);
                assetDownloadingTextPotrait.text = "ロード完了後、ワールドの準備が整います... " + (downloadedTillNow) + "/" + (totalAssetCount);
                if (downloadedTillNow >= totalAssetCount)
                {
                    assetDownloadingText.text = "読み込み完了.... " + downloadedTillNow + "/" + (totalAssetCount);
                    assetDownloadingTextPotrait.text = "読み込み完了.... " + downloadedTillNow + "/" + (totalAssetCount);
                    assetDownloadingText.transform.parent.gameObject.SetActive(false);
                    assetDownloadingTextPotrait.transform.parent.gameObject.SetActive(false);
                }
                break;
            default:
                assetDownloadingText.text = "World will be ready once loading is complete... " + (downloadedTillNow) + "/" + (totalAssetCount);
                assetDownloadingTextPotrait.text = "World will be ready once loading is complete... " + (downloadedTillNow) + "/" + (totalAssetCount);
                if (downloadedTillNow >= totalAssetCount)
                {
                    assetDownloadingText.text = "Loading Completed.... " + downloadedTillNow + "/" + (totalAssetCount);
                    assetDownloadingTextPotrait.text = "Loading Completed.... " + downloadedTillNow + "/" + (totalAssetCount);
                    assetDownloadingText.transform.parent.gameObject.SetActive(false);
                    assetDownloadingTextPotrait.transform.parent.gameObject.SetActive(false);
                }
                break;
        }
    }

    void ResetDisplayDownloadText()
    {
        if (!assetDownloadingText)
        {
            Debug.Log("<color=red> Textmesh is Destroyed </color>");
            return;
        }
        assetDownloadingText.text = string.Empty;
        assetDownloadingTextPotrait.text = string.Empty;
        assetDownloadingText.transform.parent.gameObject.SetActive(false);
        assetDownloadingTextPotrait.transform.parent.gameObject.SetActive(false);
    }

    GameObject GetObjectFromPool(string objectKey)
    {
        if (prefabObjectPool.ContainsKey(objectKey))
        {
            if (CheckAvailableForReuse(objectKey, prefabObjectPool[objectKey]))
                return prefabObjectPool[objectKey];
        }
        return null;
    }

    static void AddObjectInPool(string objectKey, GameObject prefabObject)
    {
        if (prefabObjectPool.ContainsKey(objectKey))
        {
            return;
        }
        else
        {
            prefabObjectPool.Add(objectKey, prefabObject);
        }
    }

    void RemoveItemFromPool(string objectKey)
    {
        Addressables.Release(objectKey);
        prefabObjectPool.Remove(objectKey);
    }

    bool CheckAvailableForReuse(string key, GameObject poolObject)
    {
        ObjectsInfo objectsInfo = new ObjectsInfo();
        if (xanaWorldDataDictionary.TryGetValue(key, out objectsInfo))
        {
            float distance = Vector3.Distance(poolObject.transform.position, objectsInfo.position);
            if (distance > unloadDistance)
                return true;
        }
        return false;
    }
    private static void InstantiateAsset(GameObject objectTobeInstantiate, ObjectsInfo _itemData, string downloadKey)
    {
        var newObj = Instantiate(objectTobeInstantiate, _itemData.position, _itemData.rotation, assetParentStatic);
        newObj.transform.localScale = _itemData.scale;
        newObj.name = _itemData.name;
        newObj.SetActive(_itemData.isActive);
        ApplyLightmapData(_itemData.lightmapData, newObj);

        if (ConstantsHolder.DomeHeaderInfo)
        {
            AssignDomeId(newObj, _itemData);
            SetSubworldIndex(newObj, _itemData);
        }

        // Uncomment if needed
        // AddObjectInPool(downloadKey, newObj);
        // if (ConstantsHolder.HaveSubWorlds && _itemData.addressableKey.Contains("TLP"))
        // {
        //     XANASummitDataContainer.SceneTeleportingObjects.Add(objectTobeInstantiate);
        // }
    }

    private static void InstantiateAsset(GameObject ObjectFromPool, ObjectsInfo _itemData, bool alreadyInstantiated)
    {
        ObjectFromPool.transform.localScale = _itemData.scale;
        ObjectFromPool.name = _itemData.name;
        ObjectFromPool.SetActive(_itemData.isActive);
        ApplyLightmapData(_itemData.lightmapData, ObjectFromPool);
    }

    private static void ApplyLightmapData(LightmapData[] lightmapData, GameObject prefab)
    {
        try
        {
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].lightmapIndex = lightmapData[i].lightmapIndex;
                renderers[i].lightmapScaleOffset = lightmapData[i].lightmapScaleOffset;
            }
        }
        catch (Exception e)
        {
            //Debug.LogError("Error while applying lightmap data :- " + e.Message);
        }

    }

    static void AssignDomeId(GameObject DomeObject, ObjectsInfo _itemData)
    {
        if (_itemData.summitDomeInfo.domeIndex != 0)
        {
            var sceneSwitchComponent = DomeObject.GetComponentInChildren<OnTriggerSceneSwitch>();
            if (sceneSwitchComponent != null)
            {
                sceneSwitchComponent.DomeId = _itemData.summitDomeInfo.domeIndex;
                var textMeshProComponent = sceneSwitchComponent.textMeshPro.GetComponent<TMPro.TextMeshPro>();
                if (textMeshProComponent == null)
                {
                    textMeshProComponent = sceneSwitchComponent.textMeshPro.AddComponent<TMPro.TextMeshPro>();
                }
                textMeshProComponent.text = _itemData.summitDomeInfo.domeIndex.ToString();
                textMeshProComponent.alignment = TMPro.TextAlignmentOptions.Center;
                textMeshProComponent.fontSize = 50;
                textMeshProComponent.color = new Color32(45,255,0,255);
                var shaderApplyComponent = DomeObject.GetComponent<SummitDomeShaderApply>();
                if (shaderApplyComponent != null)
                {
                    AllDomes.Add(DomeObject);
                    shaderApplyComponent.DomeId = _itemData.summitDomeInfo.domeIndex;
                }
            }
        }

        #region DomeMiniMap

        var xanaConstants = ConstantsHolder.xanaConstants;

        if (xanaConstants.EnviornmentName.Equals("SaudiExpo", StringComparison.Ordinal))
        {
            var sceneSwitchComponent = DomeObject.GetComponentInChildren<OnTriggerSceneSwitch>();
            if (sceneSwitchComponent != null)
            {
                DomeMinimapDataHolder.OnInitDome?.Invoke(sceneSwitchComponent);
            }
        }
        #endregion
    }

    static void SetSubworldIndex(GameObject objectTobeInstantiate, ObjectsInfo _itemData)
    {
        if (ConstantsHolder.HaveSubWorlds && _itemData.subWorldComponent)
        {
            var subWorldIndexComponent = objectTobeInstantiate.GetComponent<SummitSubWorldIndex>();
            if (subWorldIndexComponent != null)
            {
                subWorldIndexComponent.SubworldIndex = _itemData.subWorldIndex;
                XANASummitDataContainer.SceneTeleportingObjects.Add(objectTobeInstantiate);
            }
        }
    }

    IEnumerator CheckForUnloading()
    {
        CheckingAgain:
        yield return new WaitForSecondsRealtime(timeshortSorting);
        currPlayerPosition = GameplayEntityLoader.instance.mainController.transform.localPosition;
        yield return new WaitForEndOfFrame();
        while (downloadIsGoingOn)
        {
            yield return null;
        }
        SortingDataShortInterval(currPlayerPosition);
        yield return new WaitForEndOfFrame();
        stopDownloading = false;
        StartCoroutine(DownloadAssetsFromSortedList());
        if (downloadDataQueue.Count > 0)
            goto CheckingAgain;
        else
        {
            stopDownloading = true;
            BuilderEventManager.AfterWorldInstantiated?.Invoke();
            //CheckPlacementOfAllObjects();
        }
    }

    IEnumerator CheckShortIntervalSorting()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(timeshortSorting);
            stopDownloading = true;
            currPlayerPosition = GameplayEntityLoader.instance.mainController.transform.localPosition;
            yield return new WaitForEndOfFrame();

            while (downloadIsGoingOn)
            {
                yield return null;
            }

            SortingDataShortInterval(currPlayerPosition);
            yield return new WaitForEndOfFrame();
            stopDownloading = false;
            StartCoroutine(DownloadAssetsFromSortedList());

            if (downloadDataQueue.Count == 0)
            {
                stopDownloading = true;
                BuilderEventManager.AfterWorldInstantiated?.Invoke();
                //CheckPlacementOfAllObjects();
                yield break;
            }
        }
    }

    IEnumerator CheckLongIntervalSorting()
    {
        CheckingAgain:
        yield return new WaitForSecondsRealtime(timeFullSorting);
        StopCoroutine(CheckShortIntervalSorting());
        stopDownloading = true;
        currPlayerPosition = GameplayEntityLoader.instance.mainController.transform.localPosition;
        yield return new WaitForEndOfFrame();
        while (downloadIsGoingOn)
        {
            yield return null;
        }
        SortingQueueData(currPlayerPosition);
        yield return new WaitForEndOfFrame();
        stopDownloading = false;
        StartCoroutine(DownloadAssetsFromSortedList());
        if (downloadDataQueue.Count > 0)
        {
            StartCoroutine(CheckShortIntervalSorting());
            goto CheckingAgain;
        }
        else
        {
            stopDownloading = true;
            BuilderEventManager.AfterWorldInstantiated?.Invoke();
            //CheckPlacementOfAllObjects();
        }

    }

    //bool posChecking = true;
    //public void CheckPlacementOfAllObjects()
    //{
    //    if (posChecking)
    //    {
    //        foreach (Transform t in assetParent)
    //        {
    //            ItemData _itemData;
    //            builderDataDictionary.TryGetValue(t.name, out _itemData);
    //            t.transform.localPosition = _itemData.Position;
    //            t.transform.rotation = _itemData.Rotation;
    //        }
    //        posChecking = false;
    //    }
    //}

    public static bool CheckForVisitedWorlds(string envName)
    {
        for (int i = 0; i < DownloadedWorldNames.Count; i++)
        {
            if (DownloadedWorldNames[i] == envName)
            {
                return true;
            }
        }
        return false;
    }

    void OnOrientationChange(bool IsPortrait)
    {
        if (totalAssetCount != downloadedTillNow)
        {
            if (IsPortrait)
            {
                assetDownloadingText.transform.parent.gameObject.SetActive(false);
                assetDownloadingTextPotrait.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                assetDownloadingText.transform.parent.gameObject.SetActive(true);
                assetDownloadingTextPotrait.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    public static void ResetAll()
    {
        stopDownloading = true;

        downloadDataQueue.Clear();
        xanaWorldDataDictionary.Clear();
        preLoadObjects.Clear();
        postLoadObjects.Clear();
        downloadFailed.Clear();
        if (xanaSceneData != null && xanaSceneData.SceneObjects != null)
            xanaSceneData.SceneObjects.Clear();
        BuilderData.mapData = null;
        BuilderData.spawnPoint.Clear();
        AllDomes.Clear();
        XANASummitDataContainer.SceneTeleportingObjects.Clear();
        downloadedTillNow = 0;
        totalAssetCount = 0;
        dataArranged = false;
        dataSorted = false;
        stopDownloading = false;
        isDefaultPriorityObjectDownloaded = false;
        isfailedObjectsDownloaded = false;
        isSpawnDownloaded = false;

        if (cts != null)
            cts.Cancel();
        xanaWorldDownloader.ResetDisplayDownloadText();
        xanaWorldDownloader.StopAllCoroutines();
        LoadingHandler.Instance.HideLoading();
        //AssetBundle.UnloadAllAssetBundles(false);
        //Caching.ClearCache();
        //Addressables.CleanBundleCache();
        //Resources.UnloadUnusedAssets();

    }
#if UNITY_EDITOR

    private void CancelAllTasks()
    {
        // Cancel the CancellationTokenSource
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }

        // Stop all coroutines
        StopAllCoroutines();

        // Reset static flags and data
        ResetAll();
    }
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // Cancel tasks when exiting play mode
            CancelAllTasks();
        }
    }
#endif
}
[System.Serializable]
public class SceneData
{
    public List<ObjectsInfo> SceneObjects = new List<ObjectsInfo>();
}

[System.Serializable]
public class ObjectsInfo
{
    public string addressableKey;
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public Bounds objectBound;
    public Priority priority;
    public string tagName;
    public int layerIndex;
    public bool isActive;
    public bool subWorldComponent;
    public int subWorldIndex;
    public LightmapData[] lightmapData;
    public SummitDomeInfo summitDomeInfo;
}
[System.Serializable]
public class LightmapData
{
    public int lightmapIndex;
    public Vector4 lightmapScaleOffset;
    // Add more fields as needed to store relevant data
}

[System.Serializable]
public class SummitDomeInfo
{
    public int domeIndex;
}

public enum Priority
{
    defaultPriority = 0,
    High = 1,
    Low = 2
}