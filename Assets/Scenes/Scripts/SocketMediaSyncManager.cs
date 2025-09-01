using System;
using UnityEngine;
using BestHTTP.SocketIO3;
using BestHTTP.SocketIO3.Events;
using System.Threading.Tasks;


// Data structures for socket communication
[Serializable]
public class MediaData
{
    public string environmentName;
    public string mediaUrl;
    public string mediaType;

    public MediaData(string envName, string url, string type)
    {
        environmentName = envName;
        mediaUrl = url;
        mediaType = type;
    }
}

[Serializable]
public class MediaSyncData
{
    public string environmentName;
    public string mediaType;
    public float currentVideoTime;
    public bool videoPlay;
    public bool videoPause;
    public float pageNumber;
    public float zoomValue;

    public MediaSyncData(string _environmentName, string type, float time, bool play, bool pause)
    {
        environmentName = _environmentName;
        mediaType = type;
        currentVideoTime = time;
        videoPlay = play;
        videoPause = pause;
    }

    public MediaSyncData(string _environmentName, string type, float _pageNumber = 1, float _zoomValue = 100)
    {
        environmentName = _environmentName;
        mediaType = type;
        pageNumber = _pageNumber;
        zoomValue = _zoomValue;
    }
}

[Serializable]
public class GetMediaRequest
{
    public string environmentName;

    public GetMediaRequest(string envName)
    {
        environmentName = envName;
    }
}

public class SocketMediaSyncManager : MonoBehaviour
{
    [Header("Socket Configuration")]
    string socketTestnet = "https://api-test.xana.net";
    string socketMainnet = "https://chat-prod.xana.net/";
    [Header("Media Settings")]
    private string currentEnvironment = "xyz";

    // Socket client
    public SocketManager Manager;

    // Events for Unity components to subscribe to
    public static event Action<MediaData> OnMediaDataReceived;
    public static event Action<MediaSyncData> OnSyncDataReceived;
    public static event Action OnSocketConnected;
    public static event Action OnSocketDisconnected;
    public static event Action OnInternetDisconnect;

    // Current media state
    private MediaSyncData currentSyncData;
    private string CurrentMediaUrl = "abc";

    public bool IsConnected { get; private set; }
    private bool wasConnected = true;

    private void Start()
    {
        InitializeSocket();
        StartCoroutine(CheckNetworkStatus());
    }

    private void OnDisable()
    {
        Manager.Close();
    }


    void InitializeSocket()
    {
        Manager = new SocketManager(new Uri((socketTestnet)));
        Manager.Socket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        Manager.Socket.On<CustomError>(SocketIOEventTypes.Error, OnError);
        Manager.Socket.On<CustomError>(SocketIOEventTypes.Disconnect, OnSocketDisconnect);

        // Custom Method
        Manager.Socket.On<string>("receiveSync", OnReceiveSyncData);
        Manager.Socket.On<string>("getEnvironmentInfo", OnMediaDataResponse);

    }

    void OnConnected(ConnectResponse resp)
    {
        OnSocketConnected?.Invoke();
        Debug.Log("Socket Connected");
    }
    void OnError(CustomError args)
    {
        Debug.Log("<color=red>Socket Error: " + args.message + "</color>");
    }
    void Onresult(CustomError args)
    {
        //Debug.Log("<color=red>" + string.Format("Error: {0}", args.ToString()) + "</color>");
    }
    void OnSocketDisconnect(CustomError args)
    {
        Debug.Log("<color=yellow>Socket Disconnected: " + args.message + "</color>");
    }

    private void OnReceiveSyncData(string response)
    {
        MediaSyncData mediaSyncData = JsonUtility.FromJson<MediaSyncData>(response);
        try
        {
            currentSyncData = mediaSyncData;

            //Debug.LogError($"Received sync data - Type: {mediaSyncData.mediaType}, Time: {mediaSyncData.currentVideoTime}, Play: {mediaSyncData.videoPlay}");

            // Invoke on main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnSyncDataReceived?.Invoke(mediaSyncData);
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing sync data: {ex.Message}");
        }
    }

    private void OnMediaDataResponse(string response)
    {
        MediaData mediaData = JsonUtility.FromJson<MediaData>(response);
        try
        {

            //Debug.LogError($"Received media data - Environment: {mediaData.environmentName}, URL: {mediaData.mediaUrl}, Type: {mediaData.mediaType}");

            //Invoke on main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (CurrentMediaUrl != mediaData.mediaUrl)
                {
                    CurrentMediaUrl = mediaData.mediaUrl;
                    OnMediaDataReceived?.Invoke(mediaData);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing media data: {ex.Message}");
        }
    }

    public async Task SetMediaData(string environmentName, string mediaUrl, string mediaType)
    {
        if (!Manager.Socket.IsOpen)
        {
            Debug.LogWarning("Socket not connected. Cannot send media data.");
            return;
        }

        var mediaData = new MediaData(environmentName, mediaUrl, mediaType);
        try
        {
            Manager.Socket.Emit("setMediaData", mediaData);

            await Task.Delay(2000);  // later we will replace this with another socket or getMediaData will emit from backend side after uplaoding data

            Manager.Socket.Emit("getMediaData", environmentName);

            //Debug.LogError($"Sent media data - Environment: {environmentName}, URL: {mediaUrl}, Type: {mediaType}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send media data: {ex.Message}");
        }
    }

    public async Task GetMediaData(string environmentName)
    {
        if (!Manager.Socket.IsOpen)
        {
            Debug.LogWarning("Socket not connected. Cannot request media data.");
            return;
        }

        try
        {
            Manager.Socket.Emit("getMediaData", environmentName);

            //Debug.LogError($"Requested media data for environment: {environmentName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to request media data: {ex.Message}");
        }
    }


    public async Task SyncMedia(string mediaType, float currentVideoTime, bool videoPlay, bool videoPause, float _pageNumber, float _zoomValue)
    {
        if (!Manager.Socket.IsOpen)
        {
            Debug.LogWarning("Socket not connected. Cannot sync media.");
            return;
        }
        MediaSyncData syncData;
        if (mediaType == FilePicker.MediaType.Video.ToString())
            syncData = new MediaSyncData(ConstantsHolder.xanaConstants.EnviornmentName, mediaType, currentVideoTime, videoPlay, videoPause);
        else
            syncData = new MediaSyncData(ConstantsHolder.xanaConstants.EnviornmentName, mediaType, _pageNumber, _zoomValue);

        currentSyncData = syncData;

        try
        {
            Manager.Socket.Emit("syncMedia", syncData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to sync media: {ex.Message}");
        }
    }

    public async Task PlayVideo(float currentTime)
    {
        await SyncMedia("video", currentTime, true, false, 0, 0);
    }

    public async Task PauseVideo(float currentTime)
    {
        await SyncMedia("video", currentTime, false, true, 0, 0);
    }

    public MediaSyncData CurrentSyncData => currentSyncData;
    public string CurrentEnvironment => currentEnvironment;


    #region HandleSocketDisconnection

    private System.Collections.IEnumerator CheckNetworkStatus()
    {
        while (true)
        {
            bool currentlyConnected = Application.internetReachability != NetworkReachability.NotReachable;

            if (currentlyConnected != wasConnected)
            {
                if (currentlyConnected)
                {
                    OnInternetReconnected();
                }
                else
                {
                    OnInternetDisconnected();
                }
                wasConnected = currentlyConnected;
            }

            IsConnected = currentlyConnected;
            yield return new WaitForSeconds(1f); // Check every second
        }
    }

    private void OnInternetReconnected()
    {
        Debug.Log("Internet reconnected!");
        // Trigger socket reconnection
        Manager.Socket.Off(); // Remove existing event handlers to avoid duplicates
        InitializeSocket();
    }

    private void OnInternetDisconnected()
    {
        Debug.Log("Internet disconnected!");
        OnInternetDisconnect?.Invoke();
        Manager.Socket.Off(); // Remove existing event handlers to avoid duplicates
    }
    #endregion
}