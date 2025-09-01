using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class DomeTapVideoManager : MonoBehaviour
{
    public int VideoIndex = 0;
    public GameObject VideoPlayerCanvas;
    public GameObject YoutubeVideoPlayer;
    public GameObject AwsVideoPlayer;
    public bool CheckForAutoRefresh;

    private int RefreshTime = 10000;
    private string VideoUrl;
    private XANASummitDataContainer XANASummitDataContainer;

    private StreamResponse _response;

    public DisplayUGUI displayUGUI;
    public GameObject VideoPlayerLoader;
    public GameObject YouTubeUrlLoader;

    //private void OnEnable()
    //{
    //    if (ConstantsHolder.isFromXANASummit)
    //    {
    //        if (SummitSingletonClass.Instance)
    //            XANASummitDataContainer = SummitSingletonClass.Instance.XANASummitDataContainer;
    //        GetVideo(ConstantsHolder.domeId);
    //        CheckForUrlRefresh(ConstantsHolder.domeId);
    //    }
    //    else
    //    {
    //        StartCoroutine(GetVideoLinkDirect());
    //    }

    //}

    //IEnumerator GetVideoLinkDirect()
    //{
    //    string incominglink = "Demo";
    //    checkAgain:
    //    using (UnityWebRequest www = UnityWebRequest.Get(ConstantsGod.API_BASEURL + ConstantsGod.YOUTUBEVIDEOBYSCENE + WorldItemView.m_EnvName))
    //    {
    //        www.timeout = 10;
    //        www.SendWebRequest();
    //        while (!www.isDone)
    //        {
    //            yield return null;
    //        }
    //        if (www.isHttpError || www.isNetworkError)
    //        {
    //            _response = null;
    //        }
    //        else
    //        {
    //            _response = JsonUtility.FromJson<StreamResponse>(www.downloadHandler.text.Trim());
    //            if (_response != null)
    //            {
    //                if (incominglink == _response.data.link)
    //                    goto WaitHere;

    //                incominglink = _response.data.link;
    //                if (!string.IsNullOrEmpty(incominglink))
    //                {
    //                    string videoType;
    //                    if (_response.data.isLive)
    //                        videoType = "Live";
    //                    else
    //                        videoType = "Prerecorded";

    //                    PlayVideo(_response.data.isYoutubeURL, videoType, incominglink);
    //                }

    //                WaitHere:
    //                yield return new WaitForSeconds(5f);
    //                goto checkAgain;
    //            }
    //        }
    //    }
    //}


    //void GetVideo(int DomeId)
    //{
    //    XANASummitDataContainer.DomeGeneralData _DomeGeneralData = new XANASummitDataContainer.DomeGeneralData();
    //    _DomeGeneralData = XANASummitDataContainer.GetDomeData(DomeId);

    //    if (_DomeGeneralData != null)
    //    {
    //        VideoUrl = _DomeGeneralData.domes_media_v2.mediajson[0].mediaUrl;
    //        PlayVideo(_DomeGeneralData.domes_media_v2.mediajson[0].isYoutubeUrl, _DomeGeneralData.domes_media_v2.mediajson[0].videoUrlType, _DomeGeneralData.domes_media_v2.mediajson[0].mediaUrl);
    //    }
    //}

    //async void CheckForUrlRefresh(int DomeId)
    //{
    //    XANASummitDataContainer.SingleDomeData _DomeGeneralData = new XANASummitDataContainer.SingleDomeData();
    //    while (CheckForAutoRefresh)
    //    {
    //        await Task.Delay(RefreshTime);
    //        _DomeGeneralData = await XANASummitDataContainer.GetSingleDomeData(DomeId);

    //        if (VideoUrl != _DomeGeneralData.dome.mediajson[0].mediaUrl)
    //        {
    //            VideoUrl = _DomeGeneralData.dome.mediajson[0].mediaUrl;
    //            PlayVideo(_DomeGeneralData.dome.mediajson[0].isYoutubeUrl, _DomeGeneralData.dome.mediajson[0].videoUrlType, _DomeGeneralData.dome.mediajson[0].mediaUrl);
    //        }
    //    }

    //}

    async public void PlayVideo(bool _IsYoutubeUrl, string _VideoType, string _VideoUrl)
    {
        if (_IsYoutubeUrl)
        {
            if (YouTubeUrlLoader != null)
                YouTubeUrlLoader.SetActive(true);

            if (displayUGUI != null)
                displayUGUI.enabled = false;

            YoutubeVideoPlayer.SetActive(true);
            VideoPlayerCanvas.SetActive(true);
            string _StreamableUrl = "";
            if (_VideoType == "Live")
            {
                 _StreamableUrl = await GetLiveVideoUrl(_VideoUrl);
            }
            else
            {
                 _StreamableUrl = await GetPrerecordedVideoUrl(_VideoUrl);
            }

            var mediaPlayer = YoutubeVideoPlayer.GetComponent<MediaPlayer>();

            mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
            mediaPlayer.Events.AddListener(OnMediaPlayerEvent);

            mediaPlayer.MediaPath.Path = _StreamableUrl;
            mediaPlayer.OpenMedia(mediaPlayer.MediaPath, true);
        }
        else
        {
            VideoPlayerCanvas.SetActive(true);
            if(VideoPlayerLoader!= null)
                VideoPlayerLoader.SetActive(true);
            AwsVideoPlayer.SetActive(true);
            var videoPlayer = AwsVideoPlayer.GetComponent<VideoPlayer>();
            videoPlayer.url = _VideoUrl;

            videoPlayer.prepareCompleted -= OnAwsVideoPrepared;
            videoPlayer.prepareCompleted += OnAwsVideoPrepared;

            videoPlayer.errorReceived -= OnAwsVideoError;
            videoPlayer.errorReceived += OnAwsVideoError;

            videoPlayer.Prepare();

        }
    }

    private void OnAwsVideoPrepared(VideoPlayer vp)
    {
        if (VideoPlayerLoader != null)
            VideoPlayerLoader.SetActive(false);
        vp.Play();
    }
    private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType evtType, ErrorCode errorCode)
    {
        if (evtType == MediaPlayerEvent.EventType.FirstFrameReady)
        {
            if (YouTubeUrlLoader != null)
                YouTubeUrlLoader.SetActive(false);
            if (displayUGUI != null)
                displayUGUI.enabled = true;
        }
    }



    private void OnAwsVideoError(VideoPlayer vp, string msg)
    {
        Debug.LogError("AWS video error: " + msg);
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

    public void CloseVideoPanel()
    {
        VideoPlayerCanvas.SetActive(false);
        YoutubeVideoPlayer.SetActive(false);
        if (GamePlayUIHandler.inst)
        {
            GamePlayUIHandler.inst.gamePlayUIParent.SetActive(true);
        }
        AwsVideoPlayer.GetComponent<VideoPlayer>().targetTexture.Release();
        AwsVideoPlayer.SetActive(false);
        if (VideoPlayerLoader != null)
            VideoPlayerLoader.SetActive(false);
        if (YouTubeUrlLoader != null)
            YouTubeUrlLoader.SetActive(false);
        if (displayUGUI != null)
            displayUGUI.enabled = false;
        SummitDomeImageHandler.VideoIsClosed?.Invoke();
    }

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

}
