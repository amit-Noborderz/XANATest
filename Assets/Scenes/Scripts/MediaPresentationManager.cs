using Photon.Pun;
using System;
using UnityEngine;
using static XANASummitDataContainer;

public class MediaPresentationManager : MonoBehaviour
{
    public int MediaIndex = 0;
    public bool MultiDisplayVideo = false;
    public Transform VideoPlayerParent;
    [Header("PDF Syncing")]
    public Transform PdfViewerParent;
    [Header("Image Syncing")]
    public Transform ImageParent;

    public GameObject StopSharingPopup;
    public XANASummitDataContainer XANASummitDataContainer;

    private GameObject NetworkMediaPlayer;
    private string MediaUrl;
    private FilePicker.MediaType MediaType;

    public static Action<FilePicker.MediaType, float, bool, bool, float, float> SendMasterDataSocket;
    public static Action<FilePicker.MediaType, float, bool, bool, float, float> SyncAcrossAllPlayers;
    public static Action<string, FilePicker.MediaType> LoadMediaAcrossAllPlayers;

    public PresentationMiniController PresentationMiniControllerInstance;
    private void OnEnable()
    {
        //BuilderEventManager.AfterPlayerInstantiated += CheckForScreenSharing;
    }

    private void OnDisable()
    {
        //BuilderEventManager.AfterPlayerInstantiated -= CheckForScreenSharing;
    }


    [SerializeField] private SocketMediaSyncManager syncManager;

    void Start()
    {
        // Subscribe to events
        SocketMediaSyncManager.OnMediaDataReceived += HandleMediaDataReceived;
        SocketMediaSyncManager.OnSyncDataReceived += HandleSyncDataReceived;
        SocketMediaSyncManager.OnSocketConnected += HandleSocketConnected;
        SocketMediaSyncManager.OnInternetDisconnect += StopSharing;
        SendMasterDataSocket += SendMasterData;
    }

    private void HandleMediaDataReceived(MediaData mediaData)
    {
        Debug.Log($"Handling media data: {mediaData.mediaUrl}");
        LoadMediaAcrossAllPlayers?.Invoke(mediaData.mediaUrl, GetMediaType(mediaData.mediaType));
        // Load your media here based on mediaData.mediaUrl and mediaData.mediaType
    }

    private void HandleSyncDataReceived(MediaSyncData syncData)
    {
        SyncDataAcrossAllPlayers(syncData.mediaType, syncData.currentVideoTime, syncData.videoPlay, syncData.videoPause, syncData.pageNumber, syncData.zoomValue);
        Debug.Log($"Handling sync data: Time={syncData.currentVideoTime}, Playing={syncData.videoPlay}");
        // Sync your media player here
    }

    public async void HandleSocketConnected()
    {
        Debug.LogError("On Conennected Get Media..");
        // Example: Request media data for current environment
        await syncManager.GetMediaData(ConstantsHolder.xanaConstants.EnviornmentName);
    }

    private async void SetMediaSocket(string _envName, string _mediaUrl, string _mediaType)
    {
        // Example: Set media data
        await syncManager.SetMediaData(_envName, _mediaUrl, _mediaType);
    }

    // Example methods you can call from UI or other components
    public async void SendMasterData(FilePicker.MediaType mediaType, float currentVideoTime, bool videoPlay, bool videoPause, float _pageNumber, float _zoomValue)
    {
        Debug.LogError("Sending master data ");
        await syncManager.SyncMedia(mediaType.ToString(), currentVideoTime, videoPlay, videoPause, _pageNumber, _zoomValue);
    }

    public async void OnPlayButtonClicked()
    {
        await syncManager.PlayVideo(GetCurrentVideoTime());
    }

    public async void OnPauseButtonClicked()
    {
        await syncManager.PauseVideo(GetCurrentVideoTime());
    }

    private float GetCurrentVideoTime()
    {
        // Return current video time from your media player
        return 123.5f; // Example value
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        SocketMediaSyncManager.OnMediaDataReceived -= HandleMediaDataReceived;
        SocketMediaSyncManager.OnSyncDataReceived -= HandleSyncDataReceived;
        SocketMediaSyncManager.OnSocketConnected -= HandleSocketConnected;
        SocketMediaSyncManager.OnInternetDisconnect -= StopSharing;
        SendMasterDataSocket -= SendMasterData;

        StopSharing();
    }





    public void Init(string _mediaUrl, FilePicker.MediaType _mediaType)
    {
        MediaUrl = _mediaUrl;
        MediaType = _mediaType;
        CheckForScreenSharing(MediaUrl, MediaType);
        PresentationMiniControllerInstance.Init(MediaUrl, MediaType);
    }

    void CheckForScreenSharing()
    {
        XANASummitDataContainer.DomeGeneralData domeGeneralData = XANASummitDataContainer.GetDomeData(ConstantsHolder.domeId);
        if (domeGeneralData != null)
        {
            if (domeGeneralData.domes_media_v2 != null && domeGeneralData.domes_media_v2.mediajson.Length > 0)
            {
                if (domeGeneralData.domes_media_v2.mediajson[MediaIndex].mediaType == "Video")
                {
                    InstantiateVideoPlayerOnNetwork();
                }
                else if (domeGeneralData.domes_media_v2.mediajson[MediaIndex].mediaType == "PDF")
                {
                    InstantiatePdfViewerOnNetwork();
                }
                else if (domeGeneralData.domes_media_v2.mediajson[MediaIndex].mediaType == "Image")
                {
                    InstantiateImageOverNetwork();
                }
            }
        }
    }

    void CheckForScreenSharing(string _mediaUrl, FilePicker.MediaType _mediaType)
    {
        if (_mediaType.ToString() == "Video")
        {
            InstantiateVideoPlayerOnNetwork();
        }
        else if (_mediaType.ToString() == "PDF")
        {
            InstantiatePdfViewerOnNetwork();
        }
        else if (_mediaType.ToString() == "Image")
        {
            InstantiateImageOverNetwork();
        }

        SetMediaSocket(ConstantsHolder.xanaConstants.EnviornmentName, _mediaUrl, _mediaType.ToString());
    }

    void InstantiateVideoPlayerOnNetwork()
    {
        if (MultiDisplayVideo)
        {
            NetworkMediaPlayer = PhotonNetwork.Instantiate("MultiplayerSharing/Multiplayer MultiDisplay Video", VideoPlayerParent.position, VideoPlayerParent.rotation);
            bool DisplayCheck = NetworkMediaPlayer.GetComponent<FindDisplayForMedia>().FindDisplayForVideo();

            if (DisplayCheck)
                Debug.Log("Display Found for Video Player");
            else
                return;
        }
        else
        {
            NetworkMediaPlayer = PhotonNetwork.Instantiate("MultiplayerSharing/Multiplayer Video Player", VideoPlayerParent.position, VideoPlayerParent.rotation);
            NetworkMediaPlayer.transform.localScale = VideoPlayerParent.transform.localScale;
        }

        //NetworkMediaPlayer.transform.SetParent(VideoPlayerParent, false);
        NetworkMediaPlayer.GetComponent<MultiplayerVideoManager>().MediaIndex = MediaIndex;
        //NetworkMediaPlayer.GetComponent<MultiplayerVideoManager>().LoadNetworkVideoUrl(MediaUrl, MediaType);
        //NetworkMediaPlayer.GetComponent<MultiplayerVideoManager>().TriggerRPC(MediaUrl,MediaType);
    }

    void InstantiatePdfViewerOnNetwork()
    {
        NetworkMediaPlayer = PhotonNetwork.Instantiate("MultiplayerSharing/Multiplayer PDF Viewer", PdfViewerParent.position, PdfViewerParent.rotation);
        NetworkMediaPlayer.transform.localScale = PdfViewerParent.transform.localScale;
        //NetworkMediaPlayer.transform.SetParent(PdfViewerParent, false);
        NetworkMediaPlayer.GetComponent<PDFSyncMultiplayer>().MediaIndex = MediaIndex;
        //NetworkMediaPlayer.GetComponent<PDFSyncMultiplayer>().LoadPDFUrl(MediaUrl, MediaType);
        //NetworkMediaPlayer.GetComponent<PDFSyncMultiplayer>().TriggerRPC(MediaUrl, MediaType);
    }

    void InstantiateImageOverNetwork()
    {
        NetworkMediaPlayer = PhotonNetwork.Instantiate("MultiplayerSharing/Multiplayer Image Viewer", ImageParent.position, ImageParent.rotation);
        NetworkMediaPlayer.transform.localScale = ImageParent.transform.localScale;
        //NetworkMediaPlayer.transform.SetParent(ImageParent, false);
        NetworkMediaPlayer.GetComponent<ImageSyncMultiplayer>().MediaIndex = MediaIndex;
        //NetworkMediaPlayer.GetComponent<ImageSyncMultiplayer>().LoadImageUrl(MediaUrl, MediaType);
        //NetworkMediaPlayer.GetComponent<ImageSyncMultiplayer>().TriggerRPC(MediaUrl,MediaType);
    }

    void SyncDataAcrossAllPlayers(string _mediaType, float _currentTime, bool _videoPlay, bool _videoPause, float _pageNumber, float _zoomValue)
    {
        SyncAcrossAllPlayers?.Invoke(GetMediaType(_mediaType), _currentTime, _videoPlay, _videoPause, _pageNumber, _zoomValue);
    }


    public void StartSharing()
    {
        if (PhotonNetwork.IsMasterClient && NetworkMediaPlayer != null)
        {
            CheckForScreenSharing();
        }
    }

    public void StopSharing()
    {
        //if (PhotonNetwork.IsMasterClient)
        //{
        if (NetworkMediaPlayer != null)
            PhotonNetwork.Destroy(NetworkMediaPlayer);

        PresentationMiniControllerInstance.StopSharing();

        StopSharingPopup.SetActive(true);
        //} 
    }

    FilePicker.MediaType GetMediaType(string _mediaType)
    {
        if (_mediaType == "Video" || _mediaType == "video")
            return FilePicker.MediaType.Video;
        else if (_mediaType == "Image" || _mediaType == "Image")
            return FilePicker.MediaType.Image;
        else if (_mediaType == "Pdf" || _mediaType == "PDF")
            return FilePicker.MediaType.PDF;
        else
            return FilePicker.MediaType.PPT;
    }
}

