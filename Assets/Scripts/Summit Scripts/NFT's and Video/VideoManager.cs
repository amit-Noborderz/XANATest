using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public int VideoIndex = 0;
    public GameObject YoutubeVideoPlayer;
    public GameObject AwsVideoPlayer;
    public bool CheckForAutoRefresh;

    private int RefreshTime = 25000;
    private string VideoUrl;
    private XANASummitDataContainer XANASummitDataContainer;

    private StreamResponse _response; 
    public AudioSource ksaBgm;  // Saudi Event BGM
    private void Awake()
    {
        SetUpAudioSourceAWSPlayer();
    }

    private void OnEnable()
    {
        if (SummitSingletonClass.Instance)
            XANASummitDataContainer = SummitSingletonClass.Instance.XANASummitDataContainer;

        if (ConstantsHolder.isFromXANASummit && ConstantsHolder.IsSubWorld)
        {
            GetSubWorldVideo(ConstantsHolder.domeId, ConstantsHolder.SubDomeId);
        }
        else if (ConstantsHolder.isFromXANASummit)
        {
            GetVideo(ConstantsHolder.domeId);
            CheckForUrlRefresh(ConstantsHolder.domeId);
        }
        else if (WorldItemView.m_EnvName == "SaudiExpo")
        {
            BuilderEventManager.AfterPlayerInstantiated += SetVideoInExternalSpace;
        }
        else
        {
            StartCoroutine(GetVideoLinkDirect());
        }
    }

    private void OnDisable()
    {
        BuilderEventManager.AfterPlayerInstantiated -= SetVideoInExternalSpace;
    }

    void SetUpAudioSourceAWSPlayer()
    {
        if(AwsVideoPlayer!= null && AwsVideoPlayer.GetComponent<VideoPlayer>()!=null)
        {
            if(AwsVideoPlayer.GetComponent<AudioSource>() != null)
            {
                AwsVideoPlayer.GetComponent<VideoPlayer>().audioOutputMode = VideoAudioOutputMode.AudioSource;
                AwsVideoPlayer.GetComponent<VideoPlayer>().controlledAudioTrackCount = 1;
                AwsVideoPlayer.GetComponent<VideoPlayer>().SetTargetAudioSource(0, AwsVideoPlayer.GetComponent<AudioSource>());
                AwsVideoPlayer.GetComponent<VideoPlayer>().EnableAudioTrack(0, true);
            }
            else
            {
                AwsVideoPlayer.AddComponent<AudioSource>(); 
                AwsVideoPlayer.GetComponent<VideoPlayer>().audioOutputMode = VideoAudioOutputMode.AudioSource;
                AwsVideoPlayer.GetComponent<VideoPlayer>().controlledAudioTrackCount = 1;
                AwsVideoPlayer.GetComponent<VideoPlayer>().SetTargetAudioSource(0, AwsVideoPlayer.GetComponent<AudioSource>());
                AwsVideoPlayer.GetComponent<VideoPlayer>().EnableAudioTrack(0, true);
            }
        }
    }


    private void SetVideoInExternalSpace()
    {
        GetVideo(XANASummitDataContainer.DomeIdForSummit);
        CheckForUrlRefresh(XANASummitDataContainer.DomeIdForSummit);
    }


    IEnumerator GetVideoLinkDirect()
    {
        string incominglink = "Demo";
        checkAgain:
        using (UnityWebRequest www = UnityWebRequest.Get(ConstantsGod.API_BASEURL + ConstantsGod.YOUTUBEVIDEOBYSCENE + WorldItemView.m_EnvName))
        {
            www.timeout = 10;
            www.SendWebRequest();
            while (!www.isDone)
            {
                yield return null;
            }
            if (www.isHttpError || www.isNetworkError)
            {
                _response = null;
            }
            else
            {
                _response = JsonUtility.FromJson<StreamResponse>(www.downloadHandler.text.Trim());
                if (_response != null)
                {
                    if (ksaBgm && !_response.data.isPlaying)
                        ksaBgm.mute = false;

                    if (incominglink == _response.data.link)
                        goto WaitHere;

                    incominglink = _response.data.link;
                    if (!string.IsNullOrEmpty(incominglink))
                    {
                        string videoType;
                        if (_response.data.isLive)
                            videoType = "Live";
                        else
                            videoType = "Prerecorded";

                        PlayVideo(_response.data.isYoutubeURL, videoType, incominglink);
                    }

                    WaitHere:
                    yield return new WaitForSeconds(5f);
                    goto checkAgain;
                }
            }
        }
    }


    async void GetVideo(int DomeId)
    {
        XANASummitDataContainer.DomeGeneralData _DomeGeneralData = new XANASummitDataContainer.DomeGeneralData();
        while(XANASummitDataContainer.summitData.domes.Count==0)
            await Task.Delay(1000);
        _DomeGeneralData = XANASummitDataContainer.GetDomeData(DomeId);

        if (_DomeGeneralData != null)
        {
            if (_DomeGeneralData.domes_media_v2.mediajson != null && _DomeGeneralData.domes_media_v2.mediajson.Length > VideoIndex)
            {
                VideoUrl = _DomeGeneralData.domes_media_v2.mediajson[VideoIndex].mediaUrl;
                PlayVideo(_DomeGeneralData.domes_media_v2.mediajson[VideoIndex].isYoutubeUrl, _DomeGeneralData.domes_media_v2.mediajson[VideoIndex].videoUrlType, _DomeGeneralData.domes_media_v2.mediajson[VideoIndex].mediaUrl);
            }
        }
    }

    async void GetSubWorldVideo(int DomeId, int SubworldId)
    {
        XANASummitDataContainer.DomeGeneralData _DomeGeneralData = new XANASummitDataContainer.DomeGeneralData();
        while (XANASummitDataContainer.summitData.domes.Count == 0)
            await Task.Delay(1000);
        _DomeGeneralData = XANASummitDataContainer.GetSubDomeData(DomeId, SubworldId);

        if (_DomeGeneralData != null)
        {
            if (_DomeGeneralData.domes_media_v2.mediajson != null && _DomeGeneralData.domes_media_v2.mediajson.Length > VideoIndex)
            {
                VideoUrl = _DomeGeneralData.domes_media_v2.mediajson[VideoIndex].mediaUrl;
                PlayVideo(_DomeGeneralData.domes_media_v2.mediajson[VideoIndex].isYoutubeUrl, _DomeGeneralData.domes_media_v2.mediajson[VideoIndex].videoUrlType, _DomeGeneralData.domes_media_v2.mediajson[VideoIndex].mediaUrl);
            }
        }
    }


    async void CheckForUrlRefresh(int DomeId)
    {
        XANASummitDataContainer.SingleDomeData _DomeGeneralData = new XANASummitDataContainer.SingleDomeData();
        while (XANASummitDataContainer.summitData.domes.Count == 0)
            await Task.Delay(1000);
        while (CheckForAutoRefresh)
        {
            await Task.Delay(RefreshTime);
            _DomeGeneralData = await XANASummitDataContainer.GetSingleDomeData(DomeId);
            if (_DomeGeneralData.dome.mediajson!=null && _DomeGeneralData.dome.mediajson.Length > VideoIndex)
            {
                if (VideoUrl != _DomeGeneralData.dome.mediajson[VideoIndex].mediaUrl)
                {
                    YoutubeVideoPlayer.GetComponent<MediaPlayer>().CloseMedia();
                    YoutubeVideoPlayer.GetComponent<MediaPlayer>().ForceDispose();
                    //YoutubeVideoPlayer.GetComponent<ApplyToMesh>().MeshRenderer.

                    VideoUrl = _DomeGeneralData.dome.mediajson[VideoIndex].mediaUrl;
                    PlayVideo(_DomeGeneralData.dome.mediajson[VideoIndex].isYoutubeUrl, _DomeGeneralData.dome.mediajson[VideoIndex].videoUrlType, _DomeGeneralData.dome.mediajson[VideoIndex].mediaUrl);
                }
            }


        }

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
            }
            else
            {
                string _StreamableUrl = await GetPrerecordedVideoUrl(_VideoUrl);
                YoutubeVideoPlayer.GetComponent<MediaPlayer>().MediaPath.Path = _StreamableUrl;
                YoutubeVideoPlayer.GetComponent<MediaPlayer>().Loop = true;
                YoutubeVideoPlayer.GetComponent<MediaPlayer>().OpenMedia(YoutubeVideoPlayer.GetComponent<MediaPlayer>().MediaPath, true);
            }
            BuilderEventManager.ChangeScreenWithPlayerType?.Invoke(true,false);
        }
        else
        {
            YoutubeVideoPlayer.SetActive(false);
            AwsVideoPlayer.SetActive(true);
            AwsVideoPlayer.GetComponent<VideoPlayer>().url = _VideoUrl;
            AwsVideoPlayer.GetComponent<VideoPlayer>().isLooping = true;
            AwsVideoPlayer.GetComponent<VideoPlayer>().Play();
            BuilderEventManager.ChangeScreenWithPlayerType?.Invoke(false, true);
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
                if (ksaBgm)
                    ksaBgm.mute = true;
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
                if (ksaBgm)
                    ksaBgm.mute = true;
                return response.video_url;
            }
            request.Dispose();
        }
        return string.Empty;
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
