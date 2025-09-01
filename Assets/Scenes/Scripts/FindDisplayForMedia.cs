using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(MultiplayerVideoManager))]
public class FindDisplayForMedia : MonoBehaviour
{
    // Start is called before the first frame update
    public bool FindDisplayForVideo()
    {
        GameObject VideoDisplay = GameObject.FindGameObjectWithTag("VideoDisplay");
        if (VideoDisplay == null)
            return false;
        if (VideoDisplay.GetComponent<ApplyToMesh>() == null)
        {
            VideoDisplay.AddComponent<ApplyToMesh>();
            VideoDisplay.GetComponent<ApplyToMesh>().MeshRenderer = VideoDisplay.GetComponent<MeshRenderer>();
            VideoDisplay.GetComponent<ApplyToMesh>().Player = GetComponent<MultiplayerVideoManager>().YoutubeVideoPlayer.GetComponent<MediaPlayer>();

            Debug.LogError("Coming here 0");
            //Set up for AWS player
            GetComponent<MultiplayerVideoManager>().AwsVideoPlayer.GetComponent<VideoPlayer>().targetMaterialRenderer = VideoDisplay.GetComponent<MeshRenderer>();
        }
        else
        {
            VideoDisplay.GetComponent<ApplyToMesh>().MeshRenderer = VideoDisplay.GetComponent<MeshRenderer>();
            VideoDisplay.GetComponent<ApplyToMesh>().Player = GetComponent<MultiplayerVideoManager>().YoutubeVideoPlayer.GetComponent<MediaPlayer>();

            Debug.LogError("Coming here");
            //Set up for AWS player
            GetComponent<MultiplayerVideoManager>().AwsVideoPlayer.GetComponent<VideoPlayer>().targetMaterialRenderer = VideoDisplay.GetComponent<MeshRenderer>();
        }
        
        return true;
    }

    public bool FindDisplayForImage()
    {
        GameObject VideoDisplay = GameObject.FindGameObjectWithTag("VideoDisplay");
        if (VideoDisplay == null)
            return false;
        return true;
    }
    
    public bool FindDisplayForPDF()
    {
        GameObject VideoDisplay=GameObject.FindGameObjectWithTag("VideoDisplay");
        if (VideoDisplay == null)
            return false;
        return true;
    }

}
