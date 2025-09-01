using Paroxe.PdfRenderer;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static XANASummitDataContainer;

public class ImageSyncMultiplayer : MonoBehaviourPunCallbacks
{
    public int MediaIndex;
    public RawImage Image;
    public XANASummitDataContainer XANASummitDataContainer;
    public async void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        MediaPresentationManager.LoadMediaAcrossAllPlayers += LoadImageRPC;

        while(FilePicker.FilePickerInstance.MediaPresentationManagerInstance==null)
        {
            await Task.Delay(1000);
        }
        gameObject.transform.localScale = FilePicker.FilePickerInstance.MediaPresentationManagerInstance.ImageParent.transform.localScale;
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        MediaPresentationManager.LoadMediaAcrossAllPlayers -= LoadImageRPC;
        BuilderEventManager.StopMediaSharing?.Invoke();
    }

    public void LoadImageUrl()
    {
        XANASummitDataContainer.DomeGeneralData domeGeneralData = XANASummitDataContainer.GetDomeData(ConstantsHolder.domeId);
        if (domeGeneralData != null)
        {
            if (domeGeneralData.domes_media_v2 != null && domeGeneralData.domes_media_v2.mediajson.Length > 0)
            {
                if (domeGeneralData.domes_media_v2.mediajson[MediaIndex].mediaType == "Image")
                {
                    DownloadImageCoroutine(domeGeneralData.domes_media_v2.mediajson[MediaIndex].mediaUrl);
                }
            }
        }
    }

    public void LoadImageUrl(string mediaUrl, FilePicker.MediaType mediaType)
    {
        StartCoroutine(DownloadImageCoroutine(mediaUrl));
    }

    private IEnumerator DownloadImageCoroutine(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        request.SendWebRequest();
        while (!request.isDone)
        {
            yield return null;
        }
        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Image.texture = texture; // Apply the texture to the RawImage component
            CheckForMutipleDisplays();
        }
        else
        {
            Debug.LogError("Image download failed: " + request.error);
        }
    }

    void CheckForMutipleDisplays()
    {
        GameObject VideoDisplay = GameObject.FindGameObjectWithTag("VideoDisplay");
        if (VideoDisplay != null)
        {
            VideoDisplay.GetComponent<MeshRenderer>().material.mainTexture = Image.texture;
        }
    }

    public void LoadImageRPC(string mediaUrl, FilePicker.MediaType mediaType)
    {
        LoadImageUrl(mediaUrl, mediaType);
    }

}
