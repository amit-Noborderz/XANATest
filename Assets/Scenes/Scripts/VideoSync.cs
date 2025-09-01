using ExitGames.Client.Photon;
using Paroxe.PdfRenderer;
using Photon.Pun;
using Photon.Realtime;
using RenderHeads.Media.AVProVideo;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class VideoSync : MonoBehaviourPunCallbacks
{
    public VideoPlayer videoPlayer;   // The VideoPlayer component

    private bool isPlaying = false;   // Local state for whether video is playing
    private double videoTime = 0f;     // Local time for video playback
    private bool isVideoPlaying = false; // Sync state for whether video is playing


    public MultiplayerVideoManager MultiplayerVideoManager;
    async void OnEnable()
    {
        isVideoPlaying = true;
        PhotonNetwork.AddCallbackTarget(this);

        MediaPresentationManager.SyncAcrossAllPlayers += SyncVideoState;

        while (FilePicker.FilePickerInstance.MediaPresentationManagerInstance == null)
        {
            await Task.Delay(1000);
        }
        gameObject.transform.localScale = FilePicker.FilePickerInstance.MediaPresentationManagerInstance.VideoPlayerParent.transform.localScale;
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        MediaPresentationManager.SyncAcrossAllPlayers -= SyncVideoState;
        BuilderEventManager.StopMediaSharing?.Invoke();
    }


    float GetVideoCurrentTime()
    {
        if (MultiplayerVideoManager.PlayingVideoType == MultiplayerVideoManager.VideoType.Live)
        {
            return (float)MultiplayerVideoManager.YoutubeVideoPlayer.GetComponent<MediaPlayer>().Control.GetCurrentTime();
        }
        else if (MultiplayerVideoManager.PlayingVideoType == MultiplayerVideoManager.VideoType.Prerecorded)
        {
            return (float)MultiplayerVideoManager.YoutubeVideoPlayer.GetComponent<MediaPlayer>().Control.GetCurrentTime();
        }
        else if (MultiplayerVideoManager.PlayingVideoType == MultiplayerVideoManager.VideoType.AWS)
        {
            return (float)MultiplayerVideoManager.AwsVideoPlayer.GetComponent<VideoPlayer>().time;
        }
        else
        {
            Debug.LogError("Video type not found.");
            return 0;
        }
    }

    void SyncVideoCurrentTime(double currentVideoTime)
    {
        if (MultiplayerVideoManager.PlayingVideoType == MultiplayerVideoManager.VideoType.Live)
        {
            MultiplayerVideoManager.YoutubeVideoPlayer.GetComponent<MediaPlayer>().Control.Seek(currentVideoTime);
        }
        else if (MultiplayerVideoManager.PlayingVideoType == MultiplayerVideoManager.VideoType.Prerecorded)
        {
            MultiplayerVideoManager.YoutubeVideoPlayer.GetComponent<MediaPlayer>().Control.Seek(currentVideoTime);
        }
        else if (MultiplayerVideoManager.PlayingVideoType == MultiplayerVideoManager.VideoType.AWS)
        {
            MultiplayerVideoManager.AwsVideoPlayer.GetComponent<VideoPlayer>().time = currentVideoTime;
        }
        else
        {
            Debug.LogError("Video type not found.");
        }
    }

    bool CheckIsVideoplaying()
    {
        if (MultiplayerVideoManager.PlayingVideoType == MultiplayerVideoManager.VideoType.Live)
        {
            return MultiplayerVideoManager.YoutubeVideoPlayer.GetComponent<MediaPlayer>().Control.IsPlaying();
        }
        else if (MultiplayerVideoManager.PlayingVideoType == MultiplayerVideoManager.VideoType.Prerecorded)
        {
            return MultiplayerVideoManager.YoutubeVideoPlayer.GetComponent<MediaPlayer>().Control.IsPlaying();
        }
        else if (MultiplayerVideoManager.PlayingVideoType == MultiplayerVideoManager.VideoType.AWS)
        {
            return MultiplayerVideoManager.AwsVideoPlayer.GetComponent<VideoPlayer>().isPlaying;
        }
        else
        {
            return false;
        }
    }

    void SyncVideoState(FilePicker.MediaType _mediaType, float _currentVideoTime, bool _videoPlay, bool _videoPause, float _pageNumber, float _zoomValue)
    {
        if (_mediaType == FilePicker.MediaType.Video)
        {
            if (_videoPlay)
            {
                if (!videoPlayer.isPlaying)
                    videoPlayer.Play();
            }
            else
            {
                if (videoPlayer.isPlaying)
                    videoPlayer.Pause();
            }
            if ((_currentVideoTime - GetVideoCurrentTime()) > 2)
                SyncVideoCurrentTime(_currentVideoTime);
        }
    }
}
