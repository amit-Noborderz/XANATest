using Paroxe.PdfRenderer;
using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PresentationMiniController : MonoBehaviourPunCallbacks
{
    public GameObject MiniControllerParent;
    public GameObject PDFViewerParent;
    public GameObject VideoPlayerParent;
    public GameObject ImageParent;
    public Slider VideoSlider;
    public Button playButton;
    public Button PauseButton;
    public TMPro.TextMeshProUGUI CurrVideoTime;
    public TMPro.TextMeshProUGUI TotalVideoTime;

    public PDFViewer PDFViewer;
    public GameObject YoutubeVideoPlayer;
    public GameObject AwsVideoPlayer;

    private double videoTime = 0f;
    private bool isVideoPlaying = false;
    private VideoType PlayingVideoType;
    private float VideoFrameRate = 30;
    private void OnEnable()
    {
        PDFViewer.m_Internal.ScrollRect.onValueChanged.AddListener(OnScrollUpdate);
        PDFViewer.OnZoomChanged += PDFViewer_OnZoomChanged;
        BuilderEventManager.StopMediaSharing += StopSharingMultiDisplay;
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PDFViewer.m_Internal.ScrollRect.onValueChanged.RemoveListener(OnScrollUpdate);
        PDFViewer.OnZoomChanged -= PDFViewer_OnZoomChanged;
        BuilderEventManager.StopMediaSharing -= StopSharingMultiDisplay;
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void Update()
    {
        // Sync video time across rooms
        if (CheckIsVideoplaying())
        {
            float _currFrame = GetVideoCurrentTime();
            if (_currFrame - videoTime > 2f)
            {
                SendVideoState(FilePicker.MediaType.Video, _currFrame, isVideoPlaying, !isVideoPlaying);
            }
            VideoSliderUpdate(_currFrame, VideoFrameRate);
        }
    }


    public async void Init(string _mediaUrl, FilePicker.MediaType _mediaType)
    {
        MiniControllerParent.SetActive(true);
        if (_mediaType == FilePicker.MediaType.Video)
        {
            VideoPlayerParent.SetActive(true);
            PlayVideo(false, "Aws", _mediaUrl);
        }
        else if (_mediaType == FilePicker.MediaType.PDF)
        {
            PDFViewerParent.SetActive(true);
            PDFViewer.LoadDocumentFromWeb(_mediaUrl);
        }
        else if (_mediaType == FilePicker.MediaType.Image)
        {
            ImageParent.SetActive(true);
            LoadImage(_mediaUrl);
        }
    }

    public void StopSharing()
    {
        MiniControllerParent.SetActive(false);
        PDFViewerParent.SetActive(false);
        VideoPlayerParent.SetActive(false);
        ImageParent.SetActive(false);
        PlayingVideoType = VideoType.none;
        isVideoPlaying = false;
        Destroy(ImageParent.GetComponent<RawImage>().texture);
        StopSharingMultiDisplay();
    }

    void StopSharingMultiDisplay()
    {
        GameObject VideoDisplay = GameObject.FindGameObjectWithTag("VideoDisplay");
        if (VideoDisplay != null)
        {
            VideoDisplay.GetComponent<MeshRenderer>().material.mainTexture = Texture2D.whiteTexture;
        }
    }

    #region ImageControl

    void LoadImage(string _Mediaurl)
    {
        StartCoroutine(DownloadImageCoroutine(_Mediaurl, ImageParent.GetComponent<RawImage>()));
    }

    private IEnumerator DownloadImageCoroutine(string url, RawImage _image)
    {
        Debug.LogError(url);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        request.SendWebRequest();
        while (!request.isDone)
        {
            yield return null;
        }
        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            _image.texture = texture; // Apply the texture to the RawImage component
        }
        else
        {
            Debug.LogError("Image download failed: " + request.error);
        }
    }

    #endregion

    #region PDFController
    int PDFSync = 0;
    public async override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogError("here ");
        base.OnPlayerEnteredRoom(newPlayer);
        PDFSync = 0;
        while(PDFSync<3)
        {
            await Task.Delay(2000);
            PDFSync++;
            OnScrollUpdate(new Vector2());
        }
    }

    void OnScrollUpdate(Vector2 vector2)
    {
        float curreScroll = PDFViewer.m_Internal.ScrollRect.verticalNormalizedPosition;
        SendPDFState(FilePicker.MediaType.PDF, 0, true, true, curreScroll, 0);
    }

    private void PDFViewer_OnZoomChanged(PDFViewer sender, float oldZoom, float newZoom)
    {
        float curreScroll = PDFViewer.m_Internal.ScrollRect.verticalNormalizedPosition;
        float ZoomFactor = ((newZoom - oldZoom) / oldZoom) * 100;
        SendPDFState(FilePicker.MediaType.PDF, 0, true, true, curreScroll, ZoomFactor);

    }

    void SendPDFState(FilePicker.MediaType _mediaType, float _currentVideoTime, bool _videoPlay, bool _videoPause, float _pageNumber, float _zoomValue)
    {
        MediaPresentationManager.SendMasterDataSocket?.Invoke(_mediaType, _currentVideoTime, _videoPlay, _videoPause, _pageNumber, _zoomValue);
    }

    #endregion


    #region VideoController 
    //Video Syncing Methods
    void SendVideoState(FilePicker.MediaType _mediaType, float _currentVideoTime, bool _videoPlay, bool _videoPause)
    {
        videoTime = _currentVideoTime;
        MediaPresentationManager.SendMasterDataSocket?.Invoke(_mediaType, _currentVideoTime, _videoPlay, _videoPause, 0, 0);
    }

    public void PlayVideo()
    {
        if (!isVideoPlaying)
        {
            PauseButton.gameObject.SetActive(true);
            playButton.gameObject.SetActive(false);
            float currTime = PlayPauseVideo(true);
            isVideoPlaying = true;
            SendVideoState(FilePicker.MediaType.Video, currTime, isVideoPlaying, !isVideoPlaying);
        }
    }

    public void PauseVideo()
    {
        if (isVideoPlaying)
        {
            PauseButton.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);
            float currTime = PlayPauseVideo(false);
            isVideoPlaying = false;
            SendVideoState(FilePicker.MediaType.Video, currTime, isVideoPlaying, !isVideoPlaying);
        }
    }

    void SetVideoControllerUI(float _videoduration, float _frameRate)
    {
        PauseButton.gameObject.SetActive(true);
        playButton.gameObject.SetActive(false);
        VideoFrameRate = _frameRate;
        VideoSlider.maxValue = _videoduration / _frameRate;
        TotalVideoTime.text = FramesToTime((int)_videoduration, _frameRate);
    }

    void VideoSliderUpdate(float _videoCurrduration, float _frameRate)
    {
        VideoSlider.value = _videoCurrduration;
        CurrVideoTime.text = TimeToString((int)_videoCurrduration);
    }

    public static string FramesToTime(int totalFrames, double frameRate = 30.0)
    {
        // Calculate total seconds
        double totalSeconds = totalFrames / frameRate;

        // Calculate time components
        int hours = (int)(totalSeconds / 3600);
        int minutes = (int)((totalSeconds % 3600) / 60);
        int seconds = (int)(totalSeconds % 60);
        //int remainingFrames = totalFrames % (int)frameRate;

        // Return formatted time string
        if (hours > 0)
            return $"{hours:D2}:{minutes:D2}";
        else
            return $"{minutes:D2}:{seconds:D2}";
    }

    public static string TimeToString(int _currTime)
    {
        int hours = (int)(_currTime / 3600);
        int minutes = (int)((_currTime % 3600) / 60);
        int seconds = (int)(_currTime % 60);
        //int remainingFrames = totalFrames % (int)frameRate;

        // Return formatted time string
        if (hours > 0)
            return $"{hours:D2}:{minutes:D2}";
        else
            return $"{minutes:D2}:{seconds:D2}";
    }


    async void PlayVideo(bool _IsYoutubeUrl, string _VideoType, string _VideoUrl)
    {
        if (_IsYoutubeUrl)
        {
            YoutubeVideoPlayer.SetActive(true);
            AwsVideoPlayer.SetActive(false);
            if (_VideoType == "Live")
            {
                string _StreamableUrl = await GetLiveVideoUrl(_VideoUrl);
                YoutubeVideoPlayer.GetComponent<MediaPlayer>().MediaPath.Path = _StreamableUrl;
                YoutubeVideoPlayer.GetComponent<MediaPlayer>().OpenMedia(YoutubeVideoPlayer.GetComponent<MediaPlayer>().MediaPath, true);
                PlayingVideoType = VideoType.Live;
            }
            else
            {
                string _StreamableUrl = await GetPrerecordedVideoUrl(_VideoUrl);
                YoutubeVideoPlayer.GetComponent<MediaPlayer>().MediaPath.Path = _StreamableUrl;
                YoutubeVideoPlayer.GetComponent<MediaPlayer>().Loop = true;
                YoutubeVideoPlayer.GetComponent<MediaPlayer>().OpenMedia(YoutubeVideoPlayer.GetComponent<MediaPlayer>().MediaPath, true);
                PlayingVideoType = VideoType.Prerecorded;
            }
        }
        else
        {
            YoutubeVideoPlayer.SetActive(false);
            AwsVideoPlayer.SetActive(true);
            AwsVideoPlayer.GetComponent<VideoPlayer>().url = _VideoUrl;
            AwsVideoPlayer.GetComponent<VideoPlayer>().isLooping = true;
            AwsVideoPlayer.GetComponent<VideoPlayer>().Play();
            PlayingVideoType = VideoType.AWS;
            AwsVideoPlayer.GetComponent<VideoPlayer>().prepareCompleted += PresentationMiniController_prepareCompleted;
        }
        isVideoPlaying = true;
    }

    private void PresentationMiniController_prepareCompleted(VideoPlayer source)
    {
        SetVideoControllerUI(source.frameCount, source.frameRate);
    }

    float PlayPauseVideo(bool Play)
    {
        if (PlayingVideoType == VideoType.Live)
        {
            if (Play)
                YoutubeVideoPlayer.GetComponent<MediaPlayer>().Play();
            else
                YoutubeVideoPlayer.GetComponent<MediaPlayer>().Pause();
            return (float)YoutubeVideoPlayer.GetComponent<MediaPlayer>().Control.GetCurrentTime();
        }
        else if (PlayingVideoType == VideoType.Prerecorded)
        {
            if (Play)
                YoutubeVideoPlayer.GetComponent<MediaPlayer>().Play();
            else
                YoutubeVideoPlayer.GetComponent<MediaPlayer>().Pause();
            return (float)YoutubeVideoPlayer.GetComponent<MediaPlayer>().Control.GetCurrentTime();
        }
        else if (PlayingVideoType == VideoType.AWS)
        {
            if (Play)
                AwsVideoPlayer.GetComponent<VideoPlayer>().Play();
            else
                AwsVideoPlayer.GetComponent<VideoPlayer>().Pause();
            return (float)AwsVideoPlayer.GetComponent<VideoPlayer>().time;
        }
        else
        {
            Debug.LogError("Video type not found.");
            return 0;
        }
    }

    bool CheckIsVideoplaying()
    {
        if (PlayingVideoType == VideoType.Live)
        {
            return YoutubeVideoPlayer.GetComponent<MediaPlayer>().Control.IsPlaying();
        }
        else if (PlayingVideoType == VideoType.Prerecorded)
        {
            return YoutubeVideoPlayer.GetComponent<MediaPlayer>().Control.IsPlaying();
        }
        else if (PlayingVideoType == VideoType.AWS)
        {
            return AwsVideoPlayer.GetComponent<VideoPlayer>().isPlaying;
        }
        else
        {
            return false;
        }
    }

    float GetVideoCurrentTime()
    {
        if (PlayingVideoType == VideoType.Live)
        {
            return (float)YoutubeVideoPlayer.GetComponent<MediaPlayer>().Control.GetCurrentTime();
        }
        else if (PlayingVideoType == VideoType.Prerecorded)
        {
            return (float)YoutubeVideoPlayer.GetComponent<MediaPlayer>().Control.GetCurrentTime();
        }
        else if (PlayingVideoType == VideoType.AWS)
        {
            return (float)AwsVideoPlayer.GetComponent<VideoPlayer>().time;
        }
        else
        {
            Debug.LogError("Video type not found.");
            return 0;
        }
    }

    public async Task<string> GetLiveVideoUrl(string LiveYoutubeUrl)
    {
        int Counter = 0;
        RequestUrl requestUrl = new RequestUrl();
        requestUrl.url = LiveYoutubeUrl;
        string json = JsonUtility.ToJson(requestUrl);
    TryAgain:
        using (UnityWebRequest request = UnityWebRequest.Post(ConstantsGod.LIVEVIDEOAPI, "POST"))
        {
            byte[] jsonToSend = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);

            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Counter++;
                if (Counter < 5)
                    goto TryAgain;
            }
            else
            {
                LiveVideoResponse response = new LiveVideoResponse();
                response = JsonUtility.FromJson<LiveVideoResponse>(request.downloadHandler.text);
                return response.streamable_url;
            }



            request.Dispose();
        }
        return string.Empty;
    }


    public async Task<string> GetPrerecordedVideoUrl(string YoutubeUrl)
    {
        int Counter = 0;
        RequestUrl requestUrl = new RequestUrl();
        requestUrl.url = YoutubeUrl;
        string json = JsonUtility.ToJson(requestUrl);
    TryAgain:
        using (UnityWebRequest request = UnityWebRequest.Post(ConstantsGod.PRERECORDEDVIDEOAPI, "POST"))
        {
            byte[] jsonToSend = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);

            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Counter++;
                if (Counter < 5)
                    goto TryAgain;
            }
            else
            {
                PrerecordedVideoResponse response = new PrerecordedVideoResponse();
                response = JsonUtility.FromJson<PrerecordedVideoResponse>(request.downloadHandler.text);
                return response.video_url;
            }
            request.Dispose();
        }
        return string.Empty;
    }

    #endregion

    [System.Serializable]
    public class RequestUrl
    {
        public string url;
    }

    [System.Serializable]
    public class LiveVideoResponse
    {
        public string streamable_url;
    }

    [System.Serializable]
    public class PrerecordedVideoResponse
    {
        public string video_url;
    }


    [System.Serializable]
    public partial class StreamResponse
    {
        public bool success;
        public string msg;
        public IncomingData data;
    }

    [System.Serializable]
    public partial class IncomingData
    {
        public long id;
        public string link;
        public bool isLive;
        public bool isYoutubeURL;
        public string quality;
        public object createdAt;
        public object updatedAt;
        public bool isPlaying;
    }

    public enum VideoType
    {
        none,
        Prerecorded,
        Live,
        AWS
    }
}
